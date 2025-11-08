using NotABot.Wrapper;
using SchrodingersBot.DTO;
using SchrodingersBot.Services.Location;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.Commands
{
    public class EmptyCommandHandler : IBotCommandHandler<emptyCommand>
    {
        private readonly ICoordinatesProcessingService _coordinatesProcessingService;
        public EmptyCommandHandler(ICoordinatesProcessingService coordinatesProcessingService)
        {
            _coordinatesProcessingService = coordinatesProcessingService;
        }

        public async Task<Result> Handle(emptyCommand request, CancellationToken cancellationToken)
        {
            var message = request.Message;

            List<CoordinatesDTO> coordinates = _coordinatesProcessingService.Search(message.Text);
            if (coordinates == null) return null;

            StringBuilder sb = new StringBuilder();
            foreach (CoordinatesDTO coordinatesDTO in coordinates)
            {
                sb.AppendLine(await _coordinatesProcessingService.FormatCoordinatesStringAsync(coordinatesDTO, request.Message.ChatId));
            }

            var answerMessage = sb.ToString();

            if (String.IsNullOrEmpty(answerMessage))
            {
                return null;
            }
            else
            {
                return Result.SimpleText(request.Message, sb.ToString(), true, true);
            }
        }
    }
}
