using System;
using System.Collections.Generic;

namespace Novel.Reptile.Entities
{
    public partial class BookIndex
    {
        public int Id { get; set; }
        public int DataYm { get; set; }
        public DateTime Date { get; set; }
        public int BookId { get; set; }
        public string BookName { get; set; }
        public int ReadVolume { get; set; }
        public int Recommend { get; set; }
    }
}
