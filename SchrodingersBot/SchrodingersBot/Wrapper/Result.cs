using NotABot.Wrapper;
using SchrodingersBot.Services.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NotABot.Wrapper.Answer;

namespace NotABot.Wrapper
{
    public class Result : List<Answer>
    {
        public Result()
        {

        }

        public static Result SimpleText(IncomingMessage message, string text, bool isHtml = false, bool isReply = false)
        {
            return new Result()
            {
                Answer.SimpleText(message, text, isHtml,isReply)
            };
        }

        public static Result Reaction(IncomingMessage message, ReactionType reactionType)
        {
            return new Result()
            {
                Answer.Reaction(message, reactionType)
            };
        }
    }
}
