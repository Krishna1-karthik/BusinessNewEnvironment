using BusinessNewEnvironment.Data;
using BusinessNewEnvironment.Models;
using BusinessNewEnvironment.Dto;
using BusinessNewEnvironment.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Net.Http;

namespace Business.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessController : ControllerBase
    {
        private readonly BusinessContext _context;
        public ILogger<BusinessController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;
        private IWebHostEnvironment _env;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CachingService _cachingService;

        private readonly string _uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        public BusinessController(ILogger<BusinessController> logger, BusinessContext context, HttpClient httpClient, IConfiguration configuration, IWebHostEnvironment env, IHttpClientFactory httpClientFactory, CachingService cachingService)
        {
            _context = context;
            _logger = logger;
            _apiKey = configuration["GoogleMaps:ApiKey"]; // API key stored in configuration
            _env = env;
            _httpClientFactory = httpClientFactory;
            _cachingService = cachingService;
        }

        [HttpGet("{imageName}")]
        [ResponseCache(Duration = 2592000, Location = ResponseCacheLocation.Any, NoStore = false)] // 30 days
        public IActionResult GetImage(string imageName)
        {
            try
            {
                var filePath = Path.Combine(_uploadsFolder, imageName);
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound();
                }

                // Get MIME type based on file extension
                var extension = Path.GetExtension(filePath).ToLowerInvariant();
                var mimeType = GetMimeType(extension);

                var fileBytes = System.IO.File.ReadAllBytes(filePath);

                // Set ETag for client-side caching validation
                var fileInfo = new FileInfo(filePath);
                var etag = $"\"{fileInfo.LastWriteTimeUtc.Ticks:x}\"";

                Response.Headers.ETag = etag;
                Response.Headers.CacheControl = "public, max-age=2592000, immutable";
                Response.Headers.Add("X-Content-Type-Options", "nosniff");

                // Set proper content disposition to force inline viewing
                Response.Headers.ContentDisposition = $"inline; filename=\"{imageName}\"";

                return File(fileBytes, mimeType, enableRangeProcessing: true);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving image {imageName}: {ex.Message}");
                return StatusCode(500, "Error retrieving image");
            }
        }

        /// <summary>
        /// Returns the appropriate MIME type for an image based on its extension
        /// </summary>
        private string GetMimeType(string extension)
        {
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".avif" => "image/avif",
                ".svg" => "image/svg+xml",
                ".ico" => "image/x-icon",
                _ => "image/jpeg" // default to JPEG
            };
        }

        [HttpPost]
        public async Task<ActionResult<bool>> RegisterBusiness([FromForm] BusinesDto businesDto)
        {
            try
            {
                string? filePath = null;

                if (businesDto.VisitingCard != null)
                {
                    // Ensure the uploads folder exists
                    var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generate a unique file name to prevent conflicts
                    string uniqueFileName = $"{Guid.NewGuid()}_{businesDto.VisitingCard.FileName}";
                    filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await businesDto.VisitingCard.CopyToAsync(stream);
                    }

                    // Convert to a relative path (for storing in the database)
                    filePath = Path.Combine("uploads", uniqueFileName);
                }               

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(businesDto.Password);

                var business = new Busines
                {
                    Name = businesDto.Name,
                    EmailId = businesDto.EmailId,
                    Password = hashedPassword,
                    Description = businesDto.Description,
                    Location = businesDto.Location,
                    Latitude = businesDto.Latitude,
                    Longitude = businesDto.Longitude,
                    VisitingCard = filePath,
                    CategoryID = businesDto.CategoryID,
                    SubCategoryID = businesDto.SubCategoryID,
                    RoleID = 3 // Business role
                };
                _context.Businesses.Add(business);
                int regStatus = await _context.SaveChangesAsync();
                return Ok(true);
               
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }            
        }

        [HttpPut]
        public async Task<ActionResult<bool>> UpdateBusiness([FromForm] BusinesDto businesDto)
        {
            try
            {
                // Find the business by ID
                var existingBusiness = await _context.Businesses.FindAsync(businesDto.BusinessID);
                if (existingBusiness == null)
                {
                    return NotFound(new { message = "Business not found." });
                }

                // Check if the email or business name is being changed and if it's already registered
                bool isDuplicate = await _context.Businesses.AnyAsync(b => b.EmailId == businesDto.EmailId && b.Name == businesDto.Name && b.BusinessID != businesDto.BusinessID);
                if (isDuplicate)
                {
                    return BadRequest(new { message = "Email and/or Business Name already registered." });
                }

                // If a new visiting card is uploaded, update the file path
                if (businesDto.VisitingCard != null)
                {
                    // Delete the old visiting card file if it exists
                    if (System.IO.File.Exists(existingBusiness.VisitingCard))
                    {
                        System.IO.File.Delete(existingBusiness.VisitingCard);
                    }

                    var filePath = Path.Combine(_env.WebRootPath, "uploads");
                    //var filePath = Path.Combine("C:\\Narayana\\moh\\Business+Backend\\Business+Backend\\Business\\Business\\uploads", businesDto.VisitingCard.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await businesDto.VisitingCard.CopyToAsync(stream);
                    }
                    existingBusiness.VisitingCard = filePath;
                }

                // Update the business details
                existingBusiness.Name = businesDto.Name;
                existingBusiness.EmailId = businesDto.EmailId;
                existingBusiness.Description = businesDto.Description;
                existingBusiness.Location = businesDto.Location;
                existingBusiness.Latitude = businesDto.Latitude;
                existingBusiness.Longitude = businesDto.Longitude;
                existingBusiness.CategoryID = businesDto.CategoryID;
                existingBusiness.SubCategoryID = businesDto.SubCategoryID;

                // If password is provided, hash and update it
                if (!string.IsNullOrEmpty(businesDto.Password))
                {
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(businesDto.Password);
                    existingBusiness.Password = hashedPassword;
                }

                // Save the changes to the database
                _context.Businesses.Update(existingBusiness);
                int updateStatus = await _context.SaveChangesAsync();

                return Ok(true);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("updatebusinessdetails")]
        public async Task<IActionResult> UpdateBusinessDetails([FromForm] BusinesDto businessDto)
        {
            var existingBusiness = await _context.Businesses.FindAsync(businessDto.BusinessID);

            if (existingBusiness == null)
            {
                return NotFound("Business not found.");
            }

            // Map the DTO fields to the existing entity
            existingBusiness.Name = businessDto.Name;
            existingBusiness.EmailId = businessDto.EmailId;
            existingBusiness.Description = businessDto.Description;
            existingBusiness.Location = businessDto.Location;
            existingBusiness.SubCategoryID = businessDto.SubCategoryID;
            existingBusiness.CategoryID = businessDto.CategoryID;
            // Add other fields as necessary

            _context.Businesses.Update(existingBusiness);
            await _context.SaveChangesAsync();

            return Ok(true);
        }

        [HttpGet("check-email")]
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any, NoStore = false)] // 5 minutes
        public async Task<ActionResult<bool>> CheckEmailExistsBusiness(string email)
        {
            bool exists = await _context.Businesses
                .AsNoTracking()
                .AnyAsync(u => u.EmailId == email);
            return Ok(exists);
        }

        [HttpGet("GetCategories")]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any, NoStore = false)]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _cachingService.GetOrSetAsync(
                    CachingService.CacheKeys.AllCategories,
                    async () => await _context.Categories
                        .AsNoTracking()
                        .Select(c => new
                        {
                            c.CategoryID,
                            c.CategoryName
                        })
                        .ToListAsync(),
                    TimeSpan.FromHours(1)
                );

                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetSubCategories/{categoryId}")]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any, NoStore = false)]
        public async Task<IActionResult> GetSubCategories(int categoryId)
        {
            try
            {
                var subCategories = await _cachingService.GetOrSetAsync(
                    CachingService.CacheKeys.SubCategories(categoryId),
                    async () => await _context.SubCategories
                        .AsNoTracking()
                        .Where(sc => sc.CategoryID == categoryId)
                        .Select(sc => new
                        {
                            sc.SubCategoryID,
                            sc.SubCategoryName
                        })
                        .ToListAsync(),
                    TimeSpan.FromHours(1)
                );

                return Ok(subCategories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchBusinesses(string category, string subcategory, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                // Validate pagination parameters
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100; // Cap at 100 per page to prevent abuse

                // Build the query
                var query = _context.Businesses
                    .AsNoTracking()
                    .Include(b => b.SubCategory)
                    .ThenInclude(sc => sc.Category)
                    .Include(b => b.BusinessRatings)
                    .Where(b => b.SubCategory.Category.CategoryName == category && 
                                b.SubCategory.SubCategoryName == subcategory);

                // Get total count for pagination metadata
                var totalRecords = await query.CountAsync();

                // Apply pagination
                var businesses = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(b => new BusinessDataShow
                    {
                        BusinessID = b.BusinessID,
                        Name = b.Name,
                        Description = b.Description,
                        Distancekm = b.Latitude + b.Longitude,
                        longitude = b.Longitude,
                        Latitude = b.Latitude,
                        VisitingCard = b.VisitingCard,
                        Location = b.Location,
                        AverageRating = b.BusinessRatings.Any() ? b.BusinessRatings.Average(br => br.Rating) : 0,
                        RoleID = b.RoleID
                    })
                    .ToListAsync();

                // Build paginated response
                var response = new PaginatedResponse<BusinessDataShow>
                {
                    Data = businesses,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalRecords = totalRecords
                };

                // Set cache headers
                Response.Headers.CacheControl = "public, max-age=300"; // Cache for 5 minutes

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Search error: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        
        //[HttpGet("search")]
        //public async Task<IActionResult> SearchBusinesses(string category, string subcategory, int pageNumber = 1, int pageSize = 2)
        //{
        //    try
        //    {
        //        if (pageNumber < 1) pageNumber = 1;
        //        if (pageSize < 1) pageSize = 10;

        //        var query = _context.Businesses
        //            .Include(b => b.SubCategory)
        //            .ThenInclude(sc => sc.Category)
        //            .Where(b => b.SubCategory.Category.CategoryName == category && b.SubCategory.SubCategoryName == subcategory);

        //        // Get total records count
        //        var totalRecords = await query.CountAsync();

        //        // Apply pagination
        //        var businesses = await query
        //            .Skip((pageNumber - 1) * pageSize)
        //            .Take(pageSize)
        //            .Select(b => new BusinessDataShow
        //            {
        //                BusinessID = b.BusinessID,
        //                Name = b.Name,
        //                Description = b.Description,
        //                Distancekm = b.Latitude + b.Longitude,
        //                longitude = b.Longitude,
        //                Latitude = b.Latitude,
        //                VisitingCard = b.VisitingCard,
        //                Location = b.Location
        //            })
        //            .ToListAsync();

        //        // Pagination metadata
        //        var pagination = new
        //        {
        //            TotalRecords = totalRecords,
        //            PageNumber = pageNumber,
        //            PageSize = pageSize,
        //            TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize), //
        //            Data = businesses
        //        };

        //        return Ok(pagination);
        //    }
        //    catch (Exception ex)
        //    {
        ///        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }
        //}

        [HttpGet("getbusinessdetailbyid/{id}")]
        [ResponseCache(Duration = 600, Location = ResponseCacheLocation.Any, NoStore = false)] // 10 minutes
        public async Task<IActionResult> GetBusineesDetailById(int id)
        {
            try
            {
                var businesses = await _context.Businesses
                    .AsNoTracking()
                    .Where(b => b.BusinessID == id)
                    .Select(b => new Busines
                    {
                        BusinessID = b.BusinessID,
                        Name = b.Name,
                        EmailId = b.EmailId,
                        Password = b.Password,
                        Description = b.Description,
                        Location = b.Location,
                        VisitingCard = b.VisitingCard,
                        Latitude = b.Latitude,
                        Longitude = b.Longitude,
                        CategoryID = b.CategoryID,
                        SubCategoryID = b.SubCategoryID,
                        RoleID = b.RoleID
                    })
                    .ToListAsync();

                return Ok(businesses);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting business detail: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }    
}
