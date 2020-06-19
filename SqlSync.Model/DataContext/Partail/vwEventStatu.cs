// ReSharper disable CheckNamespace

using System.Collections.Generic;
using System.Dynamic;

namespace Roster.Model.DataContext
// ReSharper restore CheckNamespace
{
    public partial class vwEventStatu
	{
        public ExpandoObject EventStatusProperties { get; set; }
        public IDictionary<string, object> EventStatusDictionary
	    {
            get { return EventStatusProperties; }
	    }
	}
}