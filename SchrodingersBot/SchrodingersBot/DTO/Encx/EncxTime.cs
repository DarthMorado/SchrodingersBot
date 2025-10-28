using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.DTO.Encx
{
    public class EncxTime
    {
        public long Value { get; set; }
        public long Timestamp { get; set; }

        public DateTime Date => new DateTime(Timestamp);
    }
}
