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
    public class SetPrefixCommandHandler : IBotCommandHandler<setprefixCommand>
    {
        private readonly IDbChatRepository<ChatParameterEntity> _parametersRepository;
        private const string PrefixParameterCode = "PREFIX";

        public SetPrefixCommandHandler(IDbChatRepository<ChatParameterEntity> parametersRepository)
        {
            _parametersRepository = parametersRepository;
        }


        public async Task<Result> Handle(setprefixCommand request, CancellationToken cancellationToken)
        {
            var chatId = request.Message.ChatId;

            var dbPrefixData = await _parametersRepository.FindAsync(x => x.ChatId == chatId && x.Code == PrefixParameterCode);


            if (String.IsNullOrEmpty(request?.Message?.Parameter))
            {
                //Delete Parameter
                if (dbPrefixData != null && dbPrefixData.Any())
                {
                    foreach (ChatParameterEntity parameter in dbPrefixData)
                    {
                        await _parametersRepository.DeleteAsync(parameter);
                    }
                }
                return null;
            }

            var newPrefix = request.Message.Parameters[0];

            if (dbPrefixData == null || !dbPrefixData.Any())
            {
                await _parametersRepository.CreateAsync(new ChatParameterEntity
                {
                    ChatId = chatId,
                    Code = PrefixParameterCode,
                    TextValue = newPrefix
                });
            }
            else
            {
                var dbPrefix = dbPrefixData.First();
                dbPrefix.TextValue = newPrefix;

                await _parametersRepository.UpdateAsync(dbPrefix);
            }
            return null;
        }
    }
}
