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
    
    public partial class Movimenti
    {
        public int Id { get; set; }
        public string Operazione { get; set; }
        public int Ammontare { get; set; }
        public System.DateTime Data { get; set; }
        public int IdContoCorrente { get; set; }
    
        public virtual ContiCorrente ContiCorrente { get; set; }
    }
}
