using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.DB.DBO
{
    public class ChatParameterEntity : ChatEntity
    {
        public string Code { get; set; }
        public string TextValue { get; set; }
        //public decimal NumberValue { get; set; }
    }
}
