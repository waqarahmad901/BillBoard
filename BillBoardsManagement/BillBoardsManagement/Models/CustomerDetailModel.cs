using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BillBoardsManagement.Repository;
using PagedList;

namespace BillBoardsManagement.Models
{
    public class CustomerDetailModel
    {
        public string CustomerName { get; set; }
        public int CustomerId { get; set; }
        public bool Selected { get; set; }
        public List<Customer> Customers { get; set; }
    }

    public class CstomerDetilPageList
    {
        public string Brand { get; set; }
        public string TrakingNumber { get; set; }
        public int NumberMonth { get; set; }
        public DateTime ShippingDate { get; set; }
        public string BrandAddress { get; set; }

        public List<CustomerDetailModel> CustomerDetailList { get; set; }
        public int Billid { get; set; }
    }
}