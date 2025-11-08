using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace NotABot.Wrapper
{
    public class Answer
    {
        public AnswerTypes AnswerType { get; set; }
        public string Text { get; set; }
        public long ChatId { get; internal set; }
        public int MessageId { get; internal set; }
        public bool IsHtml { get; internal set; }
        public bool DisableWebPagePreview { get; internal set; }
        public int? ReplyToMessageId { get; set; }

        public enum AnswerTypes
        {
            Text,
            Image,
            Sticker,
            Reaction,
            Location
        }

        public enum ReactionType
        {
            Unknown,
            Yes,
            No,
            Fire,
            Heart,
            Shit,
            Celebration,
            Cup
        }

        private static string ReactionString(ReactionType reactionType)
        {
            return reactionType switch
            {
                ReactionType.Yes => "👍",
                ReactionType.No => "👎",
                ReactionType.Fire => "🔥",
                ReactionType.Heart => "❤",
                ReactionType.Shit => "💩",
                ReactionType.Celebration => "🎉",
                ReactionType.Cup => "🏆",
                _ => "👾"
            };
        }

        public static Answer Reaction(IncomingMessage message, ReactionType reactionType)
        {
            return new Answer()
            {
                ChatId = message.ChatId,
                AnswerType = AnswerTypes.Reaction,
                Text = ReactionString(reactionType),
                ReplyToMessageId = message.MessageId
            };
        }

        public static Answer SimpleText(IncomingMessage message, string text, bool isHtml = false, bool isReply = false)
        {
            return new Answer()
            {
                ChatId = message.ChatId,
                    AnswerType = AnswerTypes.Text,
                    IsHtml = isHtml,
                    DisableWebPagePreview = true,
                    Text = text,
                    ReplyToMessageId = isReply ? message.MessageId : null
            }
            ;
        }
    }
}
