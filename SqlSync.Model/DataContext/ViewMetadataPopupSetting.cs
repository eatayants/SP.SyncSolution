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
    
    public partial class ViewMetadataPopupSetting
    {
        public System.Guid Id { get; set; }
        public System.Guid ViewMetadataId { get; set; }
        public System.Guid ListMetadataFieldId { get; set; }
        public int Position { get; set; }
    
        public virtual ViewMetadata ViewMetadata { get; set; }
        public virtual ListMetadataField ListMetadataField { get; set; }
    }
}