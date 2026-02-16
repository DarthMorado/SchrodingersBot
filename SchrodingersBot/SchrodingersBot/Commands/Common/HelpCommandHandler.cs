using NotABot.Wrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.Commands
{
    public class HelpCommandHandler : IBotCommandHandler<helpCommand>
    {
        public async Task<Result> Handle(helpCommand request, CancellationToken cancellationToken)
        {
            var txt = @"
Schrodinger's Bot.

<b>***** Список команд: *****</b>
<b>/setarea lat lon rad</b>
Сохраняет радиус игры для чата с центром (lat, lon) и радиусом rad (в метрах)
пример: <b>/setarea 56.123 24.321123 5000</b>

<b>/setarea lat1 lon1 lat2 lon2 lat3 lon3</b>
сохраняет площадь игры для чата в виде полигона с данными координатами

<b>/setarea (без параметров)</b>
удаляет из базы площадь игры.

<b>/game</b>
возвращает домен/id текущей игры

<b>/startgame gameurl login password</b>
подключается к движку en.cx и начинает следить за игрой

<b>/stopgame</b>
отключается от игры

<b>/task</b>
пишет в чат текущее задание + скриншот страницы

<b>/screenshot</b>
скриншот текущего состояния движка en.cx

<b>.code</b>
Вбивает код 'code' в движок
ставит реакцию на сообщение:
❤ — код верный
🎉 — бонус верный
💩 — ответ не верный
👾 / нет реакции — бот не смог вбить код

Если код закрывает уровень, то бот пишет новое задание в чат
";
            return Result.SimpleText(request.Message, txt, true, false);
        }
    }
}
