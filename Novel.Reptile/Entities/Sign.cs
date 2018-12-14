using System;
using System.Collections.Generic;

namespace Novel.Reptile.Entities
{
    public partial class Sign
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
    }
}
