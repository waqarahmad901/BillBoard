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
    
    public partial class lk_town
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public lk_town()
        {
            this.Customers = new HashSet<Customer>();
            this.Customer_brand = new HashSet<Customer_brand>();
            this.lk_rates = new HashSet<lk_rates>();
        }
    
        public int Id { get; set; }
        public string Town { get; set; }
        public string Contractor { get; set; }
        public string Area { get; set; }
        public string Region { get; set; }
        public string Region_on_pdf { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Customer> Customers { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Customer_brand> Customer_brand { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<lk_rates> lk_rates { get; set; }
    }
}