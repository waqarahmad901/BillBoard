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
    
    public partial class Customer
    {
        public int Id { get; set; }
        public System.Guid RowGuid { get; set; }
        public int SrNo { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Near { get; set; }
        public string Type { get; set; }
        public string NewType { get; set; }
        public string Size1 { get; set; }
        public string Size2 { get; set; }
        public string Size3 { get; set; }
        public string Size4 { get; set; }
        public string TotalMeasurment { get; set; }
        public string Brand { get; set; }
        public string NewBrand { get; set; }
        public Nullable<System.DateTime> SurveyDate { get; set; }
        public Nullable<int> BookNumber { get; set; }
        public Nullable<System.DateTime> CreatedAt { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public byte[] Picture { get; set; }
        public Nullable<int> test { get; set; }
        public string Catagory { get; set; }
        public Nullable<decimal> Rates { get; set; }
    }
}
