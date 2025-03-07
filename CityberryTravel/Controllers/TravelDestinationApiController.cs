using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CityberryTravel.Data.UnitOfWork;
using CityberryTravel.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CityberryTravel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TravelDestinationApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TravelDestinationApiController> _logger;

        public TravelDestinationApiController(IUnitOfWork unitOfWork, ILogger<TravelDestinationApiController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TravelDestination>>> GetTravelDestinations()
        {
            try
            {
                var destinations = await _unitOfWork.TravelDestinations.GetAllAsync();
                return Ok(destinations);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Destinasyonlar getirilirken hata oluştu: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Veritabanı işlemi sırasında bir hata oluştu");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TravelDestination>> GetTravelDestination(int id)
        {
            try
            {
                var destination = await _unitOfWork.TravelDestinations.GetByIdAsync(id);

                if (destination == null)
                {
                    return NotFound($"ID: {id} ile destinasyon bulunamadı");
                }

                return Ok(destination);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ID: {id} olan destinasyon getirilirken hata oluştu: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Veritabanı işlemi sırasında bir hata oluştu");
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TravelDestination>> CreateTravelDestination([FromBody] TravelDestination destination)
        {
            try
            {
                if (destination == null)
                {
                    return BadRequest("Destinasyon verisi null olamaz");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _unitOfWork.TravelDestinations.AddAsync(destination);
                await _unitOfWork.CompleteAsync();

                return CreatedAtAction(nameof(GetTravelDestination), new { id = destination.Id }, destination);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Destinasyon oluşturulurken hata oluştu: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Veritabanı işlemi sırasında bir hata oluştu");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTravelDestination(int id, [FromBody] TravelDestination destination)
        {
            try
            {
                if (destination == null || id != destination.Id)
                {
                    return BadRequest("Geçersiz ID veya destinasyon verisi");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingDestination = await _unitOfWork.TravelDestinations.GetByIdAsync(id);
                if (existingDestination == null)
                {
                    return NotFound($"ID: {id} ile destinasyon bulunamadı");
                }

                _unitOfWork.TravelDestinations.Update(destination);
                await _unitOfWork.CompleteAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"ID: {id} olan destinasyon güncellenirken hata oluştu: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Veritabanı işlemi sırasında bir hata oluştu");
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTravelDestination(int id)
        {
            try
            {
                var destination = await _unitOfWork.TravelDestinations.GetByIdAsync(id);
                if (destination == null)
                {
                    return NotFound($"ID: {id} ile destinasyon bulunamadı");
                }

                _unitOfWork.TravelDestinations.Remove(destination);
                await _unitOfWork.CompleteAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"ID: {id} olan destinasyon silinirken hata oluştu: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Veritabanı işlemi sırasında bir hata oluştu");
            }
        }
    }
} 