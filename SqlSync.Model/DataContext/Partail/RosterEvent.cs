// ReSharper disable CheckNamespace

using System.Collections.Generic;
using System.Dynamic;

namespace Roster.Model.DataContext
// ReSharper restore CheckNamespace
{
    public partial class RosterEvent
	{
        public ExpandoObject RosterEventProperties { get; set; }

		public IDictionary<string, object> RosterEventDictionary
	    {
		    get { return RosterEventProperties; }
	    }
	}
}