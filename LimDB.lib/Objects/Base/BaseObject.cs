namespace LimDB.lib.Objects.Base
{
    public class BaseObject
    {
        public int Id { get; set; }

        public bool Active { get; set; }

        public DateTime Modified { get; set; }

        public DateTime Created { get; set; }
    }
}