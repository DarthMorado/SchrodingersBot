using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.DTO.Encx
{
    public class EncxMessage
    {
        public int OwnerId { get; set; } //ID Администратора
        public string OwnerLogin { get; set; } //Логин администратора
        public int MessageId { get; set; } //ID Сообщения
        public string MessageText { get; set; } //Оригинальный текст сообщения
        public string WrappedText { get; set; } //Отформатированный текст сообщения с учетом ReplaceNl2Br
        public bool? ReplaceNl2Br { get; set; } //Заменять ли \n на<BR>
    }
}
