using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CityberryTravel.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CityberryTravel.Services
{
    public class InMemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<InMemoryCacheService> _logger;
        private const string AllDestinationsCacheKey = "all_destinations";
        private const string DestinationKeyPrefix = "destination_";
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(1);

        public InMemoryCacheService(IMemoryCache cache, ILogger<InMemoryCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public Task<List<TravelDestination>> GetAllDestinationsAsync()
        {
            try
            {
                if (_cache.TryGetValue(AllDestinationsCacheKey, out List<TravelDestination> destinations))
                {
                    _logger.LogInformation("Destinasyonlar önbellekten alındı");
                    return Task.FromResult(destinations);
                }

                _logger.LogInformation("Destinasyonlar önbellekte bulunamadı");
                return Task.FromResult(new List<TravelDestination>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Destinasyonlar önbellekten alınırken hata oluştu");
                return Task.FromResult(new List<TravelDestination>());
            }
        }

        public Task<TravelDestination> GetDestinationByIdAsync(int id)
        {
            try
            {
                string cacheKey = $"{DestinationKeyPrefix}{id}";

                if (_cache.TryGetValue(cacheKey, out TravelDestination destination))
                {
                    _logger.LogInformation($"ID: {id} olan destinasyon önbellekten alındı");
                    return Task.FromResult(destination);
                }

                _logger.LogInformation($"ID: {id} olan destinasyon önbellekte bulunamadı");
                return Task.FromResult<TravelDestination>(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ID: {id} olan destinasyon önbellekten alınırken hata oluştu");
                return Task.FromResult<TravelDestination>(null);
            }
        }

        public Task CacheDestinationAsync(TravelDestination destination)
        {
            if (destination == null)
                return Task.CompletedTask;

            try
            {
                string cacheKey = $"{DestinationKeyPrefix}{destination.Id}";
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _cacheExpiration
                };

                _cache.Set(cacheKey, destination, cacheOptions);
                _logger.LogInformation($"ID: {destination.Id} olan destinasyon önbelleğe alındı");

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ID: {destination.Id} olan destinasyon önbelleğe alınırken hata oluştu");
                return Task.CompletedTask;
            }
        }

        public Task CacheAllDestinationsAsync(IEnumerable<TravelDestination> destinations)
        {
            if (destinations == null)
                return Task.CompletedTask;

            try
            {
                var destinationsList = destinations.ToList();
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _cacheExpiration
                };

                _cache.Set(AllDestinationsCacheKey, destinationsList, cacheOptions);

                foreach (var destination in destinationsList)
                {
                    string cacheKey = $"{DestinationKeyPrefix}{destination.Id}";
                    _cache.Set(cacheKey, destination, cacheOptions);
                }

                _logger.LogInformation($"Toplam {destinationsList.Count} destinasyon önbelleğe alındı");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tüm destinasyonlar önbelleğe alınırken hata oluştu");
                return Task.CompletedTask;
            }
        }

        public Task RemoveDestinationAsync(int id)
        {
            try
            {
                string cacheKey = $"{DestinationKeyPrefix}{id}";
                _cache.Remove(cacheKey);
                _logger.LogInformation($"ID: {id} olan destinasyon önbellekten kaldırıldı");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ID: {id} olan destinasyon önbellekten kaldırılırken hata oluştu");
                return Task.CompletedTask;
            }
        }

        public Task ClearAllDestinationsAsync()
        {
            try
            {
                _cache.Remove(AllDestinationsCacheKey);
                _logger.LogInformation("Tüm destinasyonlar önbellekten temizlendi");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tüm destinasyonlar önbellekten temizlenirken hata oluştu");
                return Task.CompletedTask;
            }
        }
    }
} 