using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.DTO.Encx
{
    public class EncxGameEngineModel
    {
        //Отражает в каком состоянии находится игра
        public EncxEvent Event { get; set;  }
        //ID игры
        public int GameId { get; set; }
        //номер игры
        public int GameNumber { get; set; }
        //название игры
        public string GameTitle { get; set; }
        //тип последовательнсти: 0 – линейная, 1 –указанная, 2 – случайная, 3 – штурмовая, 4 – динам.случайная
        public int LevelSequence { get; set; }
        //ID игрока
        public int UserId { get; set; }
        //ID команды игрока
        public int TeamId { get; set; }
        //информация о результате последнего запроса игрока
        public EncxEngineAction EngineAction { get; set; }
        //информация о текущем уровне
        public EncxLevel Level { get; set; }
        //список всех уровней
        public List<EncxLevel> Levels { get; set; }

        //время старта игры в UTC
        public string GameDateTimeStart { get; set; }
        //логин игрока
        public string Login { get; set; }
        //имя команды
        public string TeamName { get; set; }
    }
}
