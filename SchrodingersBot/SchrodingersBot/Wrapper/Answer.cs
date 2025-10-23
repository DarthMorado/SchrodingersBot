using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotABot.Wrapper
{
    public class Answer
    {
        public string AnswerType { get; set; }
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
    }
}
