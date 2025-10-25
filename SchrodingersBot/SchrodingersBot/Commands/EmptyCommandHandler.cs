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

        public async Task<List<Answer>> Handle(emptyCommand request, CancellationToken cancellationToken)
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
                return new()
                {
                    new()
                    {
                        AnswerType = "text",
                        ChatId = message.ChatId,
                        Text = sb.ToString(),
                        IsHtml = true,
                        DisableWebPagePreview = true,
                        ReplyToMessageId = request.Message.MessageId
                    }
                };
            }
        }
    }
}
