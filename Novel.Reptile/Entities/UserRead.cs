using System;
using System.Collections.Generic;

namespace Novel.Reptile.Entities
{
    public partial class UserRead
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? CotentId { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
