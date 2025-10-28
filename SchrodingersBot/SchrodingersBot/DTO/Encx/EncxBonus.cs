using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.DTO.Encx
{
    public class EncxBonus
    {
        public int BonusId { get; set; }//ID Бонуса
        public string Name { get; set; }//Название
        public int Number { get; set; }//Номер пп
        public string Task { get; set; }//Задание
        public string Help { get; set; }//Бонусная подсказка
        public bool IsAnswered { get; set; }//Разгадан / не разгадан
        public bool Expired { get; set; }//Время на выполнение истекло
        public int SecondsToStart { get; set; }//Будет доступен через
        public int SecondsLeft { get; set; }//Будет еще доступен
        public int AwardTime { get; set; }//Начисленный бонус в секндах
        public bool Negative { get; set; }//"Отрицательность" начисляемого времени
    }
}
