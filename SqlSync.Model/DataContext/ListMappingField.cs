//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Roster.Model.DataContext
{
    using System;
    using System.Collections.Generic;
    
    public partial class ListMappingField
    {
        public System.Guid Id { get; set; }
        public System.Guid ListMappingId { get; set; }
        public string ItemName { get; set; }
        public string FieldName { get; set; }
    
        public virtual ListMapping ListMapping { get; set; }
    }
}
