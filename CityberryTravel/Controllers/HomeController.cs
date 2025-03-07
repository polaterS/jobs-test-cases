using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CityberryTravel.Models;
using CityberryTravel.Data.UnitOfWork;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace CityberryTravel.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var destinations = await _unitOfWork.TravelDestinations.GetAllAsync();
            _logger.LogInformation($"Veritabanından {destinations.Count()} seyahat destinasyonu alındı.");
            
            foreach (var dest in destinations)
            {
                _logger.LogInformation($"Destinasyon: {dest.Id} - {dest.Name} - {dest.Price:C2}");
                if (dest.AvailableDates != null)
                {
                    _logger.LogInformation($"  Mevcut tarih sayısı: {dest.AvailableDates.Count}");
                }
                else
                {
                    _logger.LogInformation("  Mevcut tarih bulunamadı (null)");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Veritabanından seyahat destinasyonları alınırken hata oluştu");
            // return RedirectToAction(nameof(Error)); 
        }
        
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var errorModel = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        };

        _logger.LogInformation($"Error sayfası gösteriliyor. RequestId: {errorModel.RequestId}, ShowRequestId: {errorModel.ShowRequestId}");
        
        return View(errorModel);
    }
}
