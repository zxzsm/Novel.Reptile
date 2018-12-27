using System;
using System.Collections.Generic;

namespace Novel.Reptile.Entities
{
    public partial class BookItem
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int BookId { get; set; }
        public int? ParentItemId { get; set; }
        public string Content { get; set; }
        public int ItemLevel { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
        public int Pri { get; set; }
    }
}
