using System;
using System.Collections.Generic;

namespace Novel.Reptile.Entities
{
    public partial class BookReptileTask
    {
        public int Id { get; set; }
        public string BookName { get; set; }
        public int? BookId { get; set; }
        public string Url { get; set; }
        public string CurrentRecod { get; set; }
        public int SyncTypeId { get; set; }
        public string Remark { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Updated { get; set; }
    }
}
