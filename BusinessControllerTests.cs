using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using Microsoft.Extensions.Caching.Distributed;
using BusinessNewEnvironment.Controllers;
using BusinessNewEnvironment.Data;
using BusinessNewEnvironment.Dto;
using BusinessNewEnvironment.Models;
using BusinessNewEnvironment.Service;

namespace BusinessNewEnvironment.Tests
{
    public class BusinessControllerTests
    {
        private BusinessContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<BusinessContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new BusinessContext(options);
        }

        private BusinessController CreateController(BusinessContext context)
        {
            var loggerMock = new Mock<ILogger<BusinessController>>();
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c[It.Is<string>(s => s == "GoogleMaps:ApiKey")]).Returns("dummy-key");

            var envMock = new Mock<IWebHostEnvironment>();
            envMock.Setup(e => e.WebRootPath).Returns(System.IO.Directory.GetCurrentDirectory());

            var httpClient = new HttpClient();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();

            var distributedCacheMock = new Mock<IDistributedCache>();
            distributedCacheMock
                .Setup(c => c.GetStringAsync(It.IsAny<string>()))
                .ReturnsAsync((string?)null);
            distributedCacheMock
                .Setup(c => c.SetStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>()))
                .Returns(Task.CompletedTask);

            var cachingLogger = new Mock<ILogger<CachingService>>();
            var cachingService = new CachingService(distributedCacheMock.Object, cachingLogger.Object);

            return new BusinessController(
                loggerMock.Object,
                context,
                httpClient,
                configMock.Object,
                envMock.Object,
                httpClientFactoryMock.Object,
                cachingService
            );
        }

        [Fact]
        public async Task CheckEmailExistsBusiness_ReturnsTrue_WhenBusinessExists()
        {
            var context = CreateInMemoryContext(nameof(CheckEmailExistsBusiness_ReturnsTrue_WhenBusinessExists));
            context.Businesses.Add(new Busines { Name = "X", EmailId = "test@example.com", RoleID = 3, SubCategoryID = 1 });
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var actionResult = await controller.CheckEmailExistsBusiness("test@example.com");

            var okResult = Assert.IsType<ActionResult<bool>>(actionResult);
            var result = Assert.IsType<OkObjectResult>(okResult.Result);
            Assert.True((bool)result.Value);
        }

        [Fact]
        public async Task RegisterBusiness_SavesBusiness_WhenValid()
        {
            var context = CreateInMemoryContext(nameof(RegisterBusiness_SavesBusiness_WhenValid));
            var controller = CreateController(context);

            var dto = new BusinesDto
            {
                Name = "MyBiz",
                EmailId = "biz@example.com",
                Password = "PlainSecret",
                Description = "desc",
                Location = "loc",
                Latitude = 1.23,
                Longitude = 4.56,
                CategoryID = 1,
                SubCategoryID = 1
            };

            var actionResult = await controller.RegisterBusiness(dto);

            var okResult = Assert.IsType<ActionResult<bool>>(actionResult);
            var result = Assert.IsType<OkObjectResult>(okResult.Result);
            Assert.True((bool)result.Value);

            var saved = await context.Businesses.FirstOrDefaultAsync(b => b.EmailId == "biz@example.com");
            Assert.NotNull(saved);
            Assert.Equal("MyBiz", saved.Name);
            Assert.NotNull(saved.Password);
            Assert.NotEqual("PlainSecret", saved.Password);
        }

        [Fact]
        public async Task GetBusineesDetailById_ReturnsBusiness_WhenExists()
        {
            var context = CreateInMemoryContext(nameof(GetBusineesDetailById_ReturnsBusiness_WhenExists));
            var biz = new Busines
            {
                Name = "DetailBiz",
                EmailId = "detail@example.com",
                Password = "pwd",
                Description = "d",
                Location = "l",
                Latitude = 0,
                Longitude = 0,
                CategoryID = 1,
                SubCategoryID = 1,
                RoleID = 3
            };
            context.Businesses.Add(biz);
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            var response = await controller.GetBusineesDetailById(biz.BusinessID);

            var ok = Assert.IsType<OkObjectResult>(response);
            Assert.NotNull(ok.Value);
        }
    }
}