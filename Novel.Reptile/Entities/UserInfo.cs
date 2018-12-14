using System;
using System.Collections.Generic;

namespace Novel.Reptile.Entities
{
    public partial class UserInfo
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Uesrpwd { get; set; }
        public string UserMoblie { get; set; }
        public string UserEmail { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
    }
}
