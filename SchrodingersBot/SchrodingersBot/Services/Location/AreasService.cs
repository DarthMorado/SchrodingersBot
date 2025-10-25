using SchrodingersBot.DB.DBO;
using SchrodingersBot.DB.Repositories;
using SchrodingersBot.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SchrodingersBot.Services.Location
{
    public interface IAreasService
    {
        public Task SetArea(long chatId, RadiusDTO? radius, PolygonDTO? polygon);
        public Task<IArea> GetArea(long chatId);
    }

    public class AreasService  : IAreasService
    {
        private readonly IDbRepository<AreaEntity> _areaRepository;

        public AreasService(IDbRepository<AreaEntity> areaRepository)
        {
            _areaRepository = areaRepository;
        }

        public async Task<IArea> GetArea(long chatId)
        {
            var areas = await _areaRepository.FindAsync(x =>  x.ChatId == chatId);
            
            if (areas == null || !areas.Any()) return null;

            var area = areas.First();
            if (area.CenterLon.HasValue && area.RadiusInMeters.HasValue)
            {
                return new RadiusDTO()
                {
                    Center = new CoordinatesDTO(area.CenterLat.Value, area.CenterLon.Value),
                    RadiusInMeters = area.RadiusInMeters.Value
                };
            }

            if (!String.IsNullOrWhiteSpace(area.PolygonJson))
            {
                return new PolygonDTO()
                {
                    Points = JsonSerializer.Deserialize<List<CoordinatesDTO>>(area.PolygonJson)
                };
            }

            return null;
        }

        public async Task SetArea(long chatId, RadiusDTO? radius, PolygonDTO? polygon)
        {
            AreaEntity area;
            var areas = await _areaRepository.FindAsync(x => x.ChatId == chatId);
            if (areas == null || !areas.Any())
            {
                area = new AreaEntity()
                {
                    ChatId = chatId,
                };
                area = await _areaRepository.CreateAsync(area);
            }
            else
            {
                area = areas.First();
            }

            if (radius != null)
            {
                area.CenterLat = radius.Center.Lat;
                area.CenterLon = radius.Center.Lon;
                area.RadiusInMeters = radius.RadiusInMeters;
            }
            else if (polygon != null) 
            {
                area.PolygonJson = JsonSerializer.Serialize(polygon.Points);
            }

            await _areaRepository.UpdateAsync(area);
        }
    }
}
