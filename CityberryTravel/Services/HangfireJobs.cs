using System;
using System.Linq;
using System.Threading.Tasks;
using CityberryTravel.Data.UnitOfWork;
using Microsoft.Extensions.Logging;

namespace CityberryTravel.Services
{
    public class HangfireJobs
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly ILogger<HangfireJobs> _logger;

        public HangfireJobs(
            IUnitOfWork unitOfWork,
            ICacheService cacheService,
            ILogger<HangfireJobs> logger)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task RefreshAllDestinationsCache()
        {
            try
            {
                _logger.LogInformation("Tüm destinasyonlar için önbellek yenileme işi başlatıldı");
                var destinations = await _unitOfWork.TravelDestinations.GetAllAsync();
                
                if (destinations == null || !destinations.Any())
                {
                    _logger.LogWarning("Önbelleğe alınacak destinasyon bulunamadı");
                    return;
                }
                
                await _cacheService.CacheAllDestinationsAsync(destinations);
                _logger.LogInformation($"Toplam {destinations.Count()} destinasyon önbelleğe alındı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Destinasyonları önbelleğe alma işinde hata oluştu");
            }
        }

        public async Task CleanupExpiredDates()
        {
            try
            {
                _logger.LogInformation("Geçmiş tarihleri temizleme işi başlatıldı");
                var destinations = await _unitOfWork.TravelDestinations.GetAllAsync();
                var today = DateTime.Today;
                var modified = false;
                
                foreach (var destination in destinations)
                {
                    var expiredDates = destination.AvailableDates
                        .Where(d => d.Date < today)
                        .ToList();
                        
                    if (expiredDates.Any())
                    {
                        foreach (var date in expiredDates)
                        {
                            destination.AvailableDates.Remove(date);
                        }
                        
                        _unitOfWork.TravelDestinations.Update(destination);
                        modified = true;
                        
                        _logger.LogInformation(
                            $"ID: {destination.Id} olan destinasyondan {expiredDates.Count} adet eski tarih kaldırıldı");
                    }
                }
                
                if (modified)
                {
                    await _unitOfWork.CompleteAsync();
                    await RefreshAllDestinationsCache();
                    _logger.LogInformation("Tüm geçmiş tarihler temizlendi");
                }
                else
                {
                    _logger.LogInformation("Temizlenecek geçmiş tarih bulunamadı");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Geçmiş tarihleri temizleme işinde hata oluştu");
            }
        }

        public async Task GeneratePopularDestinationsReport()
        {
            try
            {
                _logger.LogInformation("Popüler destinasyonlar raporu oluşturuluyor");
                var destinations = await _unitOfWork.TravelDestinations.GetAllAsync();
                
                var popularDestinations = destinations
                    .OrderByDescending(d => d.Price)
                    .Take(3)
                    .ToList();
                
                foreach (var destination in popularDestinations)
                {
                    _logger.LogInformation($"Popüler Destinasyon: {destination.Name}, Fiyat: {destination.Price:C2}");
                }
                
                _logger.LogInformation("Popüler destinasyonlar raporu oluşturuldu");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Popüler destinasyonlar raporu oluşturulurken hata oluştu");
            }
        }
    }
} 