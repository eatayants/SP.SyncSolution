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
    
    public partial class ListMetadataContentType
    {
        public ListMetadataContentType()
        {
            this.ListMetadataFieldContentTypes = new HashSet<ListMetadataFieldContentType>();
            this.ListMetadataActions = new HashSet<ListMetadataAction>();
        }
    
        public int Id { get; set; }
        public System.Guid ListMetadataId { get; set; }
        public string Name { get; set; }
        public string NewItemUrl { get; set; }
        public string EditItemUrl { get; set; }
        public string DispItemUrl { get; set; }
        public bool IsOnNewAction { get; set; }
        public bool IsDefault { get; set; }
    
        public virtual ListMetadata ListMetadata { get; set; }
        public virtual ICollection<ListMetadataFieldContentType> ListMetadataFieldContentTypes { get; set; }
        public virtual ICollection<ListMetadataAction> ListMetadataActions { get; set; }
    }
}
