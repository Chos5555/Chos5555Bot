using DAL.Model;

namespace DAL
{
    public class Role
    {
        public int Id { get; set; }
        public ulong DisordId { get; set; }
        public Guild Guild { get; set; }
        public bool resetable { get; set; } = true;
    }
}
