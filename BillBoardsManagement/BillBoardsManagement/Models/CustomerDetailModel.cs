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
        public decimal billamountgenerated { get; set; }
        public decimal billamountpaid { get; set; }
        public DateTime ShippingDate { get; set; }
        public string BrandAddress { get; set; }
        public string BrandAddress1 { get; set; }
        public string BrandAddress2 { get; set; }
        public string BrandAddress3 { get; set; }
        public bool IsBrand{ get; set; }
        public DateTime BillDate { get; set; }
        public string ContactPersonName { get; set; }
        public string ContactPersonDesignation { get; set; }
        public string ContactPersonMobile { get; set; }
        public string ContactPersonName1 { get; set; }
        public string ContactPersonDesignation1 { get; set; }
        public string ContactPersonMobile1 { get; set; }

        public List<CustomerDetailModel> CustomerDetailList { get; set; }
        public string Billid { get; set; }
    }
}