using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.DTO.Encx
{
    public class EncxSector
    {
        public int SectorId { get; set; }//ID сектора
        public int Order { get; set; }//пп
        public string Name { get; set; }//Название сектора
        public EncxAnswer Answer { get; set; }//Отгаданный ответ
        public bool? IsAnswered { get; set; }//отгадан / не отгадан
    }
}
