using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotABot.Wrapper
{
    public class IncomingMessage
    {
        public string CommandName { get; set; }
        public string UpdateType { get; set; }
        public string Command { get; set; }
        public string Parameter { get; set; }
        public List<string> Parameters { get; set; }
        private string _text;
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _text = value;
                    return;
                }
                _text = value.Trim();

                if (!String.IsNullOrEmpty(_text) && _text.StartsWith("/"))
                {
                    var parts = _text
                                .Substring(1)
                                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                .ToList();
                    if (parts.Any())
                    {
                        // Split command by underscore
                        if (parts[0].Contains('_'))
                        {
                            var subParts = parts[0].Split('_').ToList();
                            parts.RemoveAt(0);
                            parts.InsertRange(0, subParts);
                        }


                        Command = parts
                                    .First()
                                    .ToLower();
                        if (parts.Count > 1)
                        {
                            Parameter = Text.Substring(Command.Length + 1).Trim();
                            Parameters = parts
                                            .Skip(1)
                                            .ToList();
                        }
                    }
                }
            }
        }
        public long ChatId { get; internal set; }
        public int MessageId { get; internal set; }
        public IncomingMessage ReplyToMessage { get; internal set; } //todo
    }
}
