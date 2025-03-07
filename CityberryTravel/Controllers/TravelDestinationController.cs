using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CityberryTravel.Models;
using CityberryTravel.Data.UnitOfWork;
using CityberryTravel.Services;

namespace CityberryTravel.Controllers
{
    public class TravelDestinationController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TravelDestinationController> _logger;
        private readonly ICacheService _cacheService;

        public TravelDestinationController(
            IUnitOfWork unitOfWork, 
            ILogger<TravelDestinationController> logger,
            ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _cacheService = cacheService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var cachedDestinations = await _cacheService.GetAllDestinationsAsync();
                
                if (cachedDestinations != null && cachedDestinations.Any())
                {
                    _logger.LogInformation("Destinasyonlar önbellekten alındı");
                    return View(cachedDestinations);
                }
                
                var destinations = await _unitOfWork.TravelDestinations.GetAllAsync();
                
                if (destinations != null && destinations.Any())
                {
                    await _cacheService.CacheAllDestinationsAsync(destinations);
                    _logger.LogInformation("Destinasyonlar veritabanından alındı ve önbelleğe kaydedildi");
                }
                
                return View(destinations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Destinasyonlar getirilirken hata oluştu");
                return View(new List<TravelDestination>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning($"Geçersiz ID: {id}");
                    return BadRequest("Geçersiz ID");
                }
                
                var cachedDestination = await _cacheService.GetDestinationByIdAsync(id);
                
                if (cachedDestination != null)
                {
                    _logger.LogInformation($"ID: {id} olan destinasyon önbellekten alındı");
                    return View(cachedDestination);
                }
                
                var destination = await _unitOfWork.TravelDestinations.GetByIdAsync(id);

                if (destination == null)
                {
                    _logger.LogWarning($"ID: {id} ile destinasyon bulunamadı");
                    return NotFound($"ID: {id} ile destinasyon bulunamadı");
                }
                
                await _cacheService.CacheDestinationAsync(destination);
                _logger.LogInformation($"ID: {id} olan destinasyon veritabanından alındı ve önbelleğe kaydedildi");
                
                return View(destination);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ID: {id} ile destinasyon detayı alınırken hata oluştu");
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult Create()
        {
            return View(new TravelDestination { 
                Name = "", 
                Description = "", 
                Price = 0, 
                AvailableDates = new List<DateTime>() 
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TravelDestination destination)
        {
            try
            {
                if (destination == null)
                {
                    _logger.LogWarning("Destinasyon oluşturulurken null model gönderildi");
                    ModelState.AddModelError("", "Destinasyon verileri boş olamaz");
                    return View(new TravelDestination { 
                        Name = "", 
                        Description = "", 
                        Price = 0, 
                        AvailableDates = new List<DateTime>() 
                    });
                }
                
                if (destination.AvailableDates == null)
                {
                    destination.AvailableDates = new List<DateTime>();
                }

                if (ModelState.IsValid)
                {
                    await _unitOfWork.TravelDestinations.AddAsync(destination);
                    await _unitOfWork.CompleteAsync();
                    _logger.LogInformation($"Yeni destinasyon oluşturuldu: {destination.Name}");
                    
                    await _cacheService.CacheDestinationAsync(destination);
                    
                    await _cacheService.ClearAllDestinationsAsync();
                    
                    return RedirectToAction(nameof(Index));
                }
                
                return View(destination ?? new TravelDestination { 
                    Name = "", 
                    Description = "", 
                    Price = 0, 
                    AvailableDates = new List<DateTime>() 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Destinasyon oluşturulurken hata oluştu");
                
                // Hata tipine göre daha açıklayıcı mesajlar ekleyelim
                string errorMessage = "Destinasyon oluşturulurken bir hata oluştu. ";
                
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, "İç hata detayı");
                    errorMessage += ex.InnerException.Message;
                }
                
                // Veritabanı bağlantı hatası mı kontrol edelim
                if (ex.Message.Contains("connection") || 
                    (ex.InnerException != null && ex.InnerException.Message.Contains("connection")))
                {
                    errorMessage = "Veritabanına bağlanırken bir sorun oluştu. Lütfen daha sonra tekrar deneyin.";
                }
                
                // Model doğrulama hatası var mı?
                if (!ModelState.IsValid)
                {
                    errorMessage += " Lütfen tüm gerekli alanları doldurun.";
                    foreach (var state in ModelState)
                    {
                        foreach (var error in state.Value.Errors)
                        {
                            _logger.LogWarning($"ModelState Error for {state.Key}: {error.ErrorMessage}");
                        }
                    }
                }
                
                ModelState.AddModelError("", errorMessage);
                return View(destination ?? new TravelDestination { 
                    Name = "", 
                    Description = "", 
                    Price = 0, 
                    AvailableDates = new List<DateTime>() 
                });
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning($"Geçersiz ID ile Edit sayfası istendi: {id}");
                    return BadRequest("Geçersiz ID");
                }
                
                var cachedDestination = await _cacheService.GetDestinationByIdAsync(id);
                
                if (cachedDestination != null)
                {
                    _logger.LogInformation($"ID: {id} olan destinasyon önbellekten alındı (Edit sayfası için)");
                    return View(cachedDestination);
                }
                
                var destination = await _unitOfWork.TravelDestinations.GetByIdAsync(id);

                if (destination == null)
                {
                    _logger.LogWarning($"ID: {id} ile düzenlenecek destinasyon bulunamadı");
                    return NotFound($"ID: {id} ile destinasyon bulunamadı");
                }
                
                if (destination.AvailableDates == null)
                {
                    destination.AvailableDates = new List<DateTime>();
                }

                return View(destination);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ID: {id} ile destinasyon düzenleme sayfası açılırken hata oluştu");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TravelDestination destination)
        {
            if (id != destination?.Id)
            {
                _logger.LogWarning($"Edit POST metodunda ID uyuşmazlığı: URL ID: {id}, Model ID: {destination?.Id}");
                return NotFound();
            }

            try
            {
                var availableDates = new List<DateTime>();
                foreach (var key in Request.Form.Keys.Where(k => k.StartsWith("AvailableDates[")))
                {
                    string strDate = Request.Form[key].ToString();
                    _logger.LogInformation($"Tarih string değeri: {key} = {strDate}");
                    
                    if (DateTime.TryParseExact(strDate, "yyyy-MM-ddTHH:mm", 
                        System.Globalization.CultureInfo.InvariantCulture, 
                        System.Globalization.DateTimeStyles.None, 
                        out DateTime parsedDate))
                    {
                        _logger.LogInformation($"Tarih HTML5 formatında başarıyla işlendi: {parsedDate}");
                        availableDates.Add(parsedDate);
                    }
                    else if (DateTime.TryParse(strDate, out parsedDate))
                    {
                        _logger.LogInformation($"Tarih genel format ile işlendi: {parsedDate}");
                        availableDates.Add(parsedDate);
                    }
                    else
                    {
                        _logger.LogWarning($"Tarih ayrıştırılamadı: {strDate}");
                    }
                }
                
                if (availableDates.Count > 0)
                {
                    destination.AvailableDates = availableDates;
                    _logger.LogInformation($"Toplam {availableDates.Count} tarih bulundu ve destination objesine eklendi");
                }
                else if (destination.AvailableDates == null)
                {
                    destination.AvailableDates = new List<DateTime>();
                    _logger.LogWarning("Hiç tarih bulunamadı veya ayrıştırılamadı, boş liste kullanılıyor");
                }
                
                if (ModelState.IsValid)
                {
                    var existingDestination = await _unitOfWork.TravelDestinations.GetByIdAsync(id);

                    if (existingDestination == null)
                    {
                        _logger.LogWarning($"ID: {id} ile güncellenecek destinasyon bulunamadı");
                        return NotFound($"ID: {id} ile destinasyon bulunamadı");
                    }

                    _unitOfWork.TravelDestinations.Update(destination);
                    await _unitOfWork.CompleteAsync();
                    _logger.LogInformation($"Destinasyon güncellendi: {destination.Name} (ID: {destination.Id})");
                    
                    await _cacheService.CacheDestinationAsync(destination);
                    
                    await _cacheService.ClearAllDestinationsAsync();
                    
                    return RedirectToAction(nameof(Index));
                }
                
                if (!ModelState.IsValid)
                {
                    foreach (var state in ModelState)
                    {
                        foreach (var error in state.Value.Errors)
                        {
                            _logger.LogWarning($"ModelState Error for {state.Key}: {error.ErrorMessage}");
                        }
                    }
                }
                
                return View(destination);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ID: {id} ile destinasyon güncellenirken hata oluştu");
                ModelState.AddModelError("", "Değişiklikler kaydedilemedi. Lütfen tekrar deneyin.");
                return View(destination);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning($"Geçersiz ID ile Delete sayfası istendi: {id}");
                    return BadRequest("Geçersiz ID");
                }
                
                var cachedDestination = await _cacheService.GetDestinationByIdAsync(id);
                
                if (cachedDestination != null)
                {
                    _logger.LogInformation($"ID: {id} olan destinasyon önbellekten alındı (Delete sayfası için)");
                    return View(cachedDestination);
                }
                
                var destination = await _unitOfWork.TravelDestinations.GetByIdAsync(id);

                if (destination == null)
                {
                    _logger.LogWarning($"ID: {id} ile silinecek destinasyon bulunamadı");
                    return NotFound($"ID: {id} ile destinasyon bulunamadı");
                }

                return View(destination);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ID: {id} ile destinasyon silme sayfası açılırken hata oluştu");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var destination = await _unitOfWork.TravelDestinations.GetByIdAsync(id);
                
                if (destination == null)
                {
                    _logger.LogWarning($"ID: {id} ile silinecek destinasyon bulunamadı");
                    return NotFound($"ID: {id} ile destinasyon bulunamadı");
                }

                _unitOfWork.TravelDestinations.Remove(destination);
                await _unitOfWork.CompleteAsync();
                
                await _cacheService.RemoveDestinationAsync(id);
                
                await _cacheService.ClearAllDestinationsAsync();
                
                _logger.LogInformation($"ID: {id} olan destinasyon silindi");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ID: {id} ile destinasyon silinirken hata oluştu");
                return RedirectToAction(nameof(Index));
            }
        }
    }
} 