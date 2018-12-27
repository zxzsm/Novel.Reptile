using System;
using System.Collections.Generic;

namespace Novel.Reptile.Entities
{
    public partial class BookThumbsup
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string Ip { get; set; }
        public int? UserId { get; set; }
        public DateTime Date { get; set; }
    }
}
