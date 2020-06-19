using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Roster.Model.DataContext
{
    public partial class ListMetadata
	{
        public ListMetadataContentType DefaultContent
        {
            get { return ListMetadataContentTypes.FirstOrDefault(item => item.IsDefault); }
        }

        public string NewItemUrl
        {
            get { return DefaultContent == null ? string.Empty : DefaultContent.NewItemUrl; }
        }

        public string EditItemUrl
        {
            get { return DefaultContent == null ? string.Empty: DefaultContent.EditItemUrl; }
        }

        public string DispItemUrl
        {
            get { return DefaultContent == null ? string.Empty : DefaultContent.DispItemUrl; }
        }
    }
}
