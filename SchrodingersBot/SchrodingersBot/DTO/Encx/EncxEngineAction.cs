using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.DTO.Encx
{
    public class EncxEngineAction
    {
        public int GameId { get; set; }//ID игры
        public int LevelId { get; set; }//ID уровня
        public int LevelNumber { get; set; }//Номер уровня на который был введен ответ
        public EncxLevelAction LevelAction { get; set; }//инфо о результате отправки ответа на уровень и бонус;
        public EncxLevelAction BonusAction { get; set; }//инфо о результате отправки ответа на бонус;
    }
}
