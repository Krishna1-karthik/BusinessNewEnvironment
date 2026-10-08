using Business.Controllers;
using BusinessNewEnvironment.Data;
using BusinessNewEnvironment.Service;
using BusinessNewEnvironment.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
            builder => builder.WithOrigins("https://krishna1-karthik.github.io")
            .AllowAnyOrigin()//deployed url https://sasmita2622606.github.io, http://localhost:4200
                              .AllowAnyHeader()
                              .AllowAnyMethod());
});
// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddTransient<EmailService>();
builder.Services.AddTransient<SubAdminServices>();
builder.Services.AddTransient<CachingService>();
builder.Services.AddTransient<DistanceCachingService>();
builder.Services.AddHttpClient<BusinessController>();

// Add distributed caching (in-memory for development, Redis recommended for production)
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDistributedMemoryCache();
}
else
{
    // TODO: Configure Redis connection for production
    // builder.Services.AddStackExchangeRedisCache(options =>
    // {
    //     options.Configuration = builder.Configuration.GetConnectionString("Redis");
    // });
    builder.Services.AddDistributedMemoryCache();
}

// Add response caching for HTTP-level caching
builder.Services.AddResponseCaching();

// Add response compression (gzip, brotli, deflate) to reduce bandwidth
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/json", "text/plain", "text/css", "application/javascript", "text/javascript" }
    );
});

// Configure Gzip compression
builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

//builder.Services.AddDbContext<BusinessContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<BusinessContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Configuration.AddJsonFile(@"C:\inetpub\wwwroot\businessapp\appsettings.json", optional: false, reloadOnChange: true);
builder.Services.AddHttpClient();
var app = builder.Build();

// Serve static files from the 'uploads' directory
app.UseStaticFiles(new StaticFileOptions
{
   
});

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

// Add security headers (must be early)
app.UseSecurityHeaders();

app.UseCors("AllowSpecificOrigin");

// Add response compression before response caching
app.UseResponseCompression();

// Add response caching middleware before static files
app.UseResponseCaching();

// Add image-specific caching headers middleware
app.UseImageCaching();

app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
