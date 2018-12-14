using System;
using System.Collections.Generic;

namespace Novel.Reptile.Entities
{
    public partial class ReptileConfig
    {
        public int Id { get; set; }
        public string Source { get; set; }
        public string NameTag { get; set; }
        public string AurthorTag { get; set; }
        public string ImageTag { get; set; }
        public string SummaryTag { get; set; }
        public string ItemTag { get; set; }
    }
}
