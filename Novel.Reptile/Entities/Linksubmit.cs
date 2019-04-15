using System;
using System.Collections.Generic;

namespace Novel.Reptile.Entities
{
    public partial class Linksubmit
    {
        public int Id { get; set; }
        public string WebUrl { get; set; }
        public int Type { get; set; }
        public int CurrentId { get; set; }
        public string Result { get; set; }
        public DateTime? UpdatedTime { get; set; }
    }
}
