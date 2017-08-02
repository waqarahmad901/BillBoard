using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BillBoardsManagement.Repository;

namespace BillBoardsManagement.Models
{
    public class CustomerDetailModel
    {
        public string CustomerName { get; set; }
        public List<Customer> Customers { get; set; }
    }
}