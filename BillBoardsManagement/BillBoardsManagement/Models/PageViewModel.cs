using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BillBoardsManagement.Models
{
    public class PageViewModel
    {
        public int CurrentPage { get; set; }
        public int RecordsPerPage { get; set; }
        public int TotalRecords { get; set; }
    }
}