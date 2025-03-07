using System;
using System.Threading.Tasks;
using CityberryTravel.Data.Repositories;
using CityberryTravel.Models;

namespace CityberryTravel.Data.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<TravelDestination> TravelDestinations { get; }
        
        Task<int> CompleteAsync();
    }
} 