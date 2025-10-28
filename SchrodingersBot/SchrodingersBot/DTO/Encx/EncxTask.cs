using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.DTO.Encx
{
    public class EncxTask
    {
        public string TaskText { get; set; }
        public string TaskTextFormatted { get; set; }
        public bool? ReplaceNlToBr { get; set; }
    }
}
