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
    
        public virtual admin_user admin_user { get; set; }
    }
}
