using DAL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DAL
{
    public class Role
    {
        public int Id { get; set; }
        public ulong DisordId { get; set; }
        public Game Game { get; set; }
        public List<Room> Rooms { get; set; }
    }
}
