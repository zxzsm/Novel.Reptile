using System;
using System.Collections.Generic;

namespace Novel.Reptile.Entities
{
    public partial class BookContent
    {
        public int CotentId { get; set; }
        public int ItemId { get; set; }
        public string Content { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
    }
}
