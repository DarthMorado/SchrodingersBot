using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.DTO.Encx
{
    public class GameDataDTO
    {
        public int Id { get; set; }
        public List<long> ChatIds { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string GuidCookie { get; set; }
        public string StokenCookie { get; set; }
        public string AtokenCookie { get; set; }
        public string Domain { get; set; }
        public string GameId { get; set; }
    }
}
