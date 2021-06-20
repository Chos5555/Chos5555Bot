using DAL.Model;
using System.Collections.Generic;

namespace DAL
{
    public class Role
    {
        public int Id { get; set; }
        public ulong DisordId { get; set; }
        public virtual Game Game { get; set; }
        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}
