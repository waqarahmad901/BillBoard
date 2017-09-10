using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BillBoardsManagement.Repository;

namespace BillBoardsManagement.Common
{
    public class ContextUser
    {
        public user OUser { get; set; }
        public EnumUserRole EnumRole { get; set; }
        public lk_role Role { get; set; }
        public string GoogleId { get; internal set; }
        public string LinkedInId { get; internal set; }
        public string PhotoPath { get; set; }
        public string FullName { get; set; }
        public string ProfileUrl { get; set; }

    }
}