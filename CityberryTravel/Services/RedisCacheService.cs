using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using CityberryTravel.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace CityberryTravel.Services
{
    public interface ICacheService
    {
        Task<List<TravelDestination>> GetAllDestinationsAsync();
        Task<TravelDestination> GetDestinationByIdAsync(int id);
        Task CacheDestinationAsync(TravelDestination destination);
        Task CacheAllDestinationsAsync(IEnumerable<TravelDestination> destinations);
        Task RemoveDestinationAsync(int id);
        Task ClearAllDestinationsAsync();
    }

    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<RedisCacheService> _logger;
        private const string AllDestinationsCacheKey = "all_destinations";
        private const string DestinationKeyPrefix = "destination_";

        public RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<List<TravelDestination>> GetAllDestinationsAsync()
        {
            try
            {
                string cachedData = await _cache.GetStringAsync(AllDestinationsCacheKey);
                if (string.IsNullOrEmpty(cachedData))
                {
                    _logger.LogInformation("Önbellekte destinasyon bulunamadı.");
                    return new List<TravelDestination>();
                }

                _logger.LogInformation("Destinasyonlar önbellekten alındı.");
                return JsonSerializer.Deserialize<List<TravelDestination>>(cachedData) ?? new List<TravelDestination>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Destinasyonlar önbellekten alınırken hata oluştu.");
                return new List<TravelDestination>();
            }
        }

        public async Task<TravelDestination> GetDestinationByIdAsync(int id)
        {
            try
            {
                string cacheKey = $"{DestinationKeyPrefix}{id}";
                string cachedData = await _cache.GetStringAsync(cacheKey);
                
                if (string.IsNullOrEmpty(cachedData))
                {
                    _logger.LogInformation($"ID: {id} olan destinasyon önbellekte bulunamadı.");
                    return null;
                }

                _logger.LogInformation($"ID: {id} olan destinasyon önbellekten alındı.");
                return JsonSerializer.Deserialize<TravelDestination>(cachedData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ID: {id} olan destinasyon önbellekten alınırken hata oluştu.");
                return null;
            }
        }

        public async Task CacheDestinationAsync(TravelDestination destination)
        {
            if (destination == null)
                return;

            try
            {
                string cacheKey = $"{DestinationKeyPrefix}{destination.Id}";
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                };

                string serializedData = JsonSerializer.Serialize(destination);
                await _cache.SetStringAsync(cacheKey, serializedData, options);
                _logger.LogInformation($"ID: {destination.Id} olan destinasyon önbelleğe alındı.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ID: {destination.Id} olan destinasyon önbelleğe alınırken hata oluştu.");
            }
        }

        public async Task CacheAllDestinationsAsync(IEnumerable<TravelDestination> destinations)
        {
            if (destinations == null)
                return;

            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                };

                string serializedData = JsonSerializer.Serialize(destinations);
                await _cache.SetStringAsync(AllDestinationsCacheKey, serializedData, options);
                
                foreach (var destination in destinations)
                {
                    await CacheDestinationAsync(destination);
                }
                
                _logger.LogInformation("Tüm destinasyonlar önbelleğe alındı.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tüm destinasyonlar önbelleğe alınırken hata oluştu.");
            }
        }

        public async Task RemoveDestinationAsync(int id)
        {
            try
            {
                string cacheKey = $"{DestinationKeyPrefix}{id}";
                await _cache.RemoveAsync(cacheKey);
                _logger.LogInformation($"ID: {id} olan destinasyon önbellekten kaldırıldı.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ID: {id} olan destinasyon önbellekten kaldırılırken hata oluştu.");
            }
        }

        public async Task ClearAllDestinationsAsync()
        {
            try
            {
                await _cache.RemoveAsync(AllDestinationsCacheKey);
                _logger.LogInformation("Tüm destinasyonlar önbellekten temizlendi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tüm destinasyonlar önbellekten temizlenirken hata oluştu.");
            }
        }
    }
} 