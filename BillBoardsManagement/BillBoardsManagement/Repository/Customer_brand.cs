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
    
    public partial class Customer_brand
    {
        public int Id { get; set; }
        public string Brand { get; set; }
        public Nullable<int> TownId { get; set; }
        public Nullable<int> Year { get; set; }
    
        public virtual lk_town lk_town { get; set; }
    }
}
