//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BillBoardsManagement.Repository
{
    using System;
    using System.Collections.Generic;
    
    public partial class bill
    {
        public int Id { get; set; }
        public string BillId { get; set; }
        public string Brand { get; set; }
        public string CustomerNames { get; set; }
        public string FilePath { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public Nullable<int> DuplicateBillId { get; set; }
        public string AmmendentBill { get; set; }
        public string BrandAddress { get; set; }
        public string TrakingNumber { get; set; }
        public Nullable<int> NumberMonth { get; set; }
        public Nullable<System.DateTime> ShippingDate { get; set; }
        public Nullable<decimal> BillAmountGenerated { get; set; }
        public Nullable<decimal> BillAmountPaid { get; set; }
        public string BrandAddress1 { get; set; }
        public string BrandAddress2 { get; set; }
        public string BrandAddress3 { get; set; }
        public Nullable<System.DateTime> BillDate { get; set; }
        public string ContactPersonName { get; set; }
        public string ContactPersonDesignation { get; set; }
        public string ContactPersonMobile { get; set; }
        public string ContactPersonName1 { get; set; }
        public string ContactPersonDesignation1 { get; set; }
        public string ContactPersonMobile1 { get; set; }
        public Nullable<decimal> Discount { get; set; }
        public Nullable<decimal> ProfessionalTax { get; set; }
        public string BusinessType { get; set; }
    }
}
