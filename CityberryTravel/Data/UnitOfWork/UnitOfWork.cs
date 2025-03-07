using System;
using System.Threading.Tasks;
using CityberryTravel.Data.Repositories;
using CityberryTravel.Models;

namespace CityberryTravel.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IRepository<TravelDestination> _travelDestinations;

        public UnitOfWork(ApplicationDbContext context, IRepository<TravelDestination> travelDestinations)
        {
            _context = context;
            _travelDestinations = travelDestinations;
        }

        public IRepository<TravelDestination> TravelDestinations => 
            _travelDestinations ??= new Repository<TravelDestination>(_context);
        
        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }
        
        public void Dispose()
        {
            _context.Dispose();
        }
    }
} 