using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.DTO.Encx
{
    public class EncxLevel
    {
        public int LevelId { get; set; } //ID Уровня
        public string Name { get; set; } //Имя уровня
        public int Number { get; set; } //Номер уровня
        public int Timeout { get; set; } //время(в секундах) срабатывания автоперехода, 0 – если нет
        public int TimeoutAward { get; set; } //штраф за автопереход(в секундах), 0 – если нет
        public int TimeoutSecondsRemain { get; set; } //осталось времени до срабатывания автоперехода(в секундах)
        public bool IsPassed { get; set; } //уровень пройден
        public bool Dismissed { get; set; } //уровень снят администратором
        public EncxTime StartTime { get; set; } //время начала уровня для игрока
        public bool HasAnswerBlockRule { get; set; } //есть ли на уровне блокировка ответов
        public int BlockDuration { get; set; } //осталось секунду блокировки; 0 – не активна
        public int BlockTargetId { get; set; } //блокировка установлена для: 0,1 – для игрока; 2 – для команды
        public int AttemtsNumber { get; set; } //количество попыток разрешенных в рамках AttemtsPeriod
        public int AttemtsPeriod { get; set; } //период срабатывания блокировки(в секундах)
        public int RequiredSectorsCount { get; set; } //Количество секторов, которые необходимо отгадать
        public int PassedSectorsCount { get; set; } //Количество отгаданных секторов
        public int SectorsLeftToClose { get; set; } //Количество неотгаданных секторов
        public List<EncxMixedAction> MixedActions { get; set; } //История введенных ответов
        public List<EncxMessage> Messages { get; set; } //Сообщения администратора
        public EncxTask Task { get; set; } //Текст задания
        public List<EncxTask> Tasks { get; set; } //Текст задания
        public List<EncxSector> Sectors { get; set; } //Сектора
        public List<EncxHelp> Helps { get; set; } //Подсказки
        public List<EncxHelp> PenaltyHelps { get; set; } //Штрафные подсказки
        public List<EncxBonus> Bonuses { get; set; } //Бонусные задания
    }
}
