//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Bankomat
{
    using System;
    using System.Collections.Generic;
    
    public partial class ContiCorrente
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ContiCorrente()
        {
            this.Clienti = new HashSet<Clienti>();
            this.Movimenti = new HashSet<Movimenti>();
        }
    
        public int Id { get; set; }
        public double Saldo { get; set; }
        public int IdBanca { get; set; }
        public Nullable<System.DateTime> DataUltimoVersamento { get; set; }
    
        public virtual Banche Banche { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Clienti> Clienti { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Movimenti> Movimenti { get; set; }
    }
}