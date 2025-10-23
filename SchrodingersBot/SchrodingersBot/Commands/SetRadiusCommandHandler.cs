using NotABot.Wrapper;
using SchrodingersBot.Services.Location;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.Commands
{
    public class SetRadiusCommandHandler : IBotCommandHandler<setradiusCommand>
    {
        private readonly ICoordinatesProcessingService _coordinatesProcessingService;

        public SetRadiusCommandHandler(ICoordinatesProcessingService coordinatesProcessingService)
        {
            _coordinatesProcessingService = coordinatesProcessingService;
        }

        public async Task<List<Answer>> Handle(setradiusCommand request, CancellationToken cancellationToken)
        {
            if (request?.Message?.Parameters == null)
            {
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
                if (double.TryParse(parameters[2], out var lengthInMeters))
                {
                    //set center[0] + lengthInMeters
                }
            }

            if (request.Message.Parameters.Count > 3)
            {
                // Polygon
            }

            return null;
        }
    }
}
