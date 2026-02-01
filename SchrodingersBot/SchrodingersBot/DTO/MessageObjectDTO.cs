using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.DTO
{
    public class MessageObjectDTO
    {
        public byte[] Content { get; set; }
        public string Text { get; set; }

        public bool IsImage { get; set; } = false;
    }
}
