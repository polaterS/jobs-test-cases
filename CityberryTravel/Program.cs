using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using CityberryTravel.Data;
using CityberryTravel.Data.UnitOfWork;
using CityberryTravel.Data.Repositories;
using Microsoft.OpenApi.Models;
using CityberryTravel.Models;
using CityberryTravel.Services;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// Database reset development
bool resetDatabase = builder.Configuration.GetValue<bool>("ResetDatabase", false);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Register the UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Add Memory Cache instead of Redis
builder.Services.AddMemoryCache();

// Register in-memory cache service
builder.Services.AddScoped<ICacheService, InMemoryCacheService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddControllers();

// Add Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Cityberry Travel API", 
        Version = "v1",
        Description = "A simple API for managing travel destinations",
        Contact = new OpenApiContact
        {
            Name = "Cityberry",
            Email = "support@cityberry.com",
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cityberry Travel API v1"));
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); 

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        if (resetDatabase && app.Environment.IsDevelopment())
        {
            logger.LogWarning("Veritabanı sıfırlanıyor! Bu işlem tüm verileri silecek!");
            context.Database.EnsureDeleted();
            logger.LogInformation("Veritabanı silindi.");
        }
        
        logger.LogInformation("Veritabanı migrasyonu başlatılıyor...");
        context.Database.Migrate();
        logger.LogInformation("Veritabanı migrasyonu tamamlandı.");
        
        if (!context.TravelDestinations.Any())
        {
            logger.LogInformation("Veritabanında seyahat destinasyonu bulunamadı. Seed verileri ekleniyor...");
            
            var destinations = new List<TravelDestination>
            {
                new TravelDestination
                {
                    Name = "Paris, France",
                    Description = "Experience the romance of the City of Light. Visit iconic landmarks like the Eiffel Tower, Louvre Museum, and Notre-Dame Cathedral.",
                    Price = 1299.99m,
                    AvailableDates = new List<DateTime>
                    {
                        DateTime.Now.AddDays(30),
                        DateTime.Now.AddDays(60),
                        DateTime.Now.AddDays(90)
                    }
                },
                new TravelDestination
                {
                    Name = "Tokyo, Japan",
                    Description = "Discover the perfect blend of traditional culture and cutting-edge technology in this vibrant metropolis.",
                    Price = 1599.99m,
                    AvailableDates = new List<DateTime>
                    {
                        DateTime.Now.AddDays(45),
                        DateTime.Now.AddDays(75),
                        DateTime.Now.AddDays(105)
                    }
                },
                new TravelDestination
                {
                    Name = "Santorini, Greece",
                    Description = "Relax on stunning beaches and enjoy breathtaking sunsets over the Aegean Sea on this beautiful Greek island.",
                    Price = 1199.99m,
                    AvailableDates = new List<DateTime>
                    {
                        DateTime.Now.AddDays(15),
                        DateTime.Now.AddDays(45),
                        DateTime.Now.AddDays(75)
                    }
                }
            };
            
            context.TravelDestinations.AddRange(destinations);
            context.SaveChanges();
            logger.LogInformation($"{destinations.Count} adet seyahat destinasyonu başarıyla eklendi.");
            
            var cacheService = services.GetRequiredService<ICacheService>();
            cacheService.CacheAllDestinationsAsync(destinations).Wait();
        }
        else
        {
            logger.LogInformation("Veritabanında zaten seyahat destinasyonu mevcut. Seed işlemi atlandı.");
            var count = context.TravelDestinations.Count();
            logger.LogInformation($"Mevcut seyahat destinasyonu sayısı: {count}");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanı seed işlemi sırasında hata oluştu.");
    }
}

app.Run();
