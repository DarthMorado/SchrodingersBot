using NotABot.Wrapper;
using SchrodingersBot.DB.DBO;
using SchrodingersBot.DB.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.Commands
{
    public class PrefixCommandHandler : IBotCommandHandler<prefixCommand>
    {
        private readonly IDbChatRepository<ChatParameterEntity> _parametersRepository;
        private const string PrefixParameterCode = "PREFIX";

        public PrefixCommandHandler(IDbChatRepository<ChatParameterEntity> parametersRepository)
        {
            _parametersRepository = parametersRepository;
        }


        public async Task<Result> Handle(prefixCommand request, CancellationToken cancellationToken)
        {
            var chatId = request.Message.ChatId;

            var dbPrefixData = await _parametersRepository.FindAsync(x => x.ChatId == chatId && x.Code == PrefixParameterCode);

            string result;

            if (dbPrefixData == null || !dbPrefixData.Any())
            {
                result = "Prefix is not set";
            }
            else
            {
                result = $"Prefix:{dbPrefixData.First().TextValue}";
            }

            return Result.SimpleText(request.Message, result);
        }
    }
}
