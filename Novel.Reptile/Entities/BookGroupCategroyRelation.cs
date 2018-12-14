using System;
using System.Collections.Generic;

namespace Novel.Reptile.Entities
{
    public partial class BookGroupCategroyRelation
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int CategoryId { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
    }
}
