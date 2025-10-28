using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.DTO.Encx
{
    public class EncxMixedAction
    {
        public int ActionId { get; set; }
        public int LevelId { get; set; } //ID уровня к которому был введен ответ
        public int LevelNumber { get; set; } //Номер уровня к которому был введен ответ
        public int UserId { get; set; } //ID игрока который ввел ответ
        public int Kind { get; set; } //1 – ответ к уровню, 2 – ответ к бонусу
        public string Login { get; set; } //Логин игрока который ввел ответ
        public string Answer { get; set; } //Текст ответа
        public string AnswForm { get; set; } //Текст ответа с подсветкой русских букв
        public object EnterDateTime { get; set; } //Время ввода ответа(UTC+0)
        public string LocDateTime { get; set; } //Локализованное время ввода ответа
        public bool? IsCorrect { get; set; } //Верен/неверен
    }
}
