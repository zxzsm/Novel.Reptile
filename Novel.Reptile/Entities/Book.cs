using System;
using System.Collections.Generic;

namespace Novel.Reptile.Entities
{
    public partial class Book
    {
        public int BookId { get; set; }
        public string BookName { get; set; }
        public string BookAuthor { get; set; }
        public DateTime? BookReleaseTime { get; set; }
        public string BookImage { get; set; }
        public string BookSummary { get; set; }
        public int? BookState { get; set; }
        public int ReadVolume { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
    }
}
