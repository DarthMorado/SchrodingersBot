using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.DTO.Encx
{
    public enum EncxEvent
    {
        OK = 0//Игра в нормальном состоянии
        , GameDoesNotExist = 2//Игра с указанным ID не существует
        , WrongEngine = 3//Запрошенная игра не соответствует запрошенному Engine
        , PlayerNotLoggedIn = 4//Игрок не залогинен на сайте
        , GameNotStarted = 5//Игра не началась
        , GameEnded = 6//Игра закончилась
        , PlayerNotParticipationg = 7//Не подана заявка (игроком)
        , TeamNotParticipationg = 8//Не подана заявка (командой)
        , PlayerNotAccepted = 9//Игрок еще не принят в игру
        , PlayerHasNoTeam = 10//У игрока нет команды (в командной игре)
        , PlayerNotActiveInTeam = 11//Игрок не активен в команде (в командной игре)
        , GameHasNoLevels = 12//В игре нет уровней
        , ExceededPlayerLimitInGame = 13//Превышено количество участников в команде (в командной игре)
        , LevelDismissed16 = 16//Уровень снят
        , GameFinished = 17//Игра закончена
        , LevelDismissed18 = 18//Уровень снят
        , LevelAutoCompleted = 19//Уровень пройден автопереходом
        , AllSectorsDone = 20//Все сектора отгаданы
        , LevelDismissed21 = 21//Уровень снят
        , LevelTimeout = 22//Таймаут уровня
    }
}
