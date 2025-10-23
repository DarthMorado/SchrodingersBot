using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.DTO
{
    public interface IArea
    {
        public Task<bool> InArea(CoordinatesDTO coordinates);
    }
}
