using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.DTO.Encx
{
    public class EncxHelp
    {
        public int HelpId { get; set; }//ID подсказки
        public int Number { get; set; }//Номер пп
        public string HelpText { get; set; }//Текст подсказки
        public bool IsPenalty { get; set; }//Штрафная/Обычная
        public int Penalty { get; set; }//Штраф в секундах(для штрафной)
        public string PenaltyComment { get; set; }//Описание подсказки(для штрафной)
        public bool RequestConfirm { get; set; }//Требует дополнительного подтверждения(для штрафной)
        public int PenaltyHelpState { get; set; }//Состояние, 0 - не открыта; 2 -  открыта(для штрафной)
        public int RemainSeconds { get; set; }//осталось секуд до того как подскзка будет доступна для игрока
    }
}
