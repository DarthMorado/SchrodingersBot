using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.DTO.Encx
{
    public class EncxLevelAction
    {
        //Введенный ответ
        public string Answer { get; set; }
        //null – ответа не было, false – неправильный ответ; true – правильный ответ;
        public bool? IsCorrectAnswer { get; set; }
    }
}
