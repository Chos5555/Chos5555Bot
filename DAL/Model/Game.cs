using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Model
{
    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Emote { get; set; }
        public ulong MessageId { get; set; }
    }
}
