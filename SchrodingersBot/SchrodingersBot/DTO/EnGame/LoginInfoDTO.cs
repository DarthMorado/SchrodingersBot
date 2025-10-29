using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.DTO.EnGame
{
    public class LoginInfoDTO
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Atoken { get; set; }
        public string Stoken { get; set; }
        public string Guid { get; set; }
    }
}
