using NotABot.Wrapper;
using SchrodingersBot.DB.Repositories;
using SchrodingersBot.DTO;
using SchrodingersBot.Services.Location;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.Commands
{
    public class SetAreaCommandHandler : IBotCommandHandler<setareaCommand>
    {
        private readonly ICoordinatesProcessingService _coordinatesProcessingService;
        private readonly IAreasService _areasService;

        public SetAreaCommandHandler(ICoordinatesProcessingService coordinatesProcessingService,
            IAreasService areasService)
        {
            _coordinatesProcessingService = coordinatesProcessingService;
            _areasService = areasService;
        }

        public async Task<List<Answer>> Handle(setareaCommand request, CancellationToken cancellationToken)
        {
            if (request?.Message?.Parameters == null)
            {
                await _areasService.SetArea(request.Message.ChatId, null, null);
                return null;
            }

            var parameters = request.Message.Parameters;

            if (parameters.Count < 3)
            {
                return null;
            }

            if (parameters.Count == 3)
            {
                // Radius
                var center = _coordinatesProcessingService.Search($"{parameters[0]} {parameters[1]}");
                if (center == null || !center.Any())
                {
                    return null;
                }
                if (double.TryParse(parameters[2], out var radiusInMeters))
                {
                    var radius = new RadiusDTO()
                    {
                        Center = center.First(),
                        RadiusInMeters = radiusInMeters
                    };
                    await _areasService.SetArea(request.Message.ChatId, radius, null);
                }
            }

            if (request.Message.Parameters.Count > 3)
            {
                if (request.Message.Parameters.Count % 2 == 1)
                {
                    return null;
                }

                PolygonDTO polygon = new();
                
                for(int i = 0; i < request.Message.Parameters.Count; i += 2)
                {
                    var lat = request.Message.Parameters[i];
                    var lon = request.Message.Parameters[i+1];
                    var cord = _coordinatesProcessingService.Convert(lat, lon);
                    if (cord == null) return null;
                    polygon.Points.Add(cord);
                }

                await _areasService.SetArea(request.Message.ChatId, null, polygon);
            }

            return null;
        }
    }
}
