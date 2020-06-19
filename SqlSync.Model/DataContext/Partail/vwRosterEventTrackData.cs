// ReSharper disable CheckNamespace

using System.Collections.Generic;
using System.Dynamic;

namespace Roster.Model.DataContext
// ReSharper restore CheckNamespace
{
    public partial class vwRosterEventTrackData
	{
        public ExpandoObject TrackDataProperties { get; set; }
        public IDictionary<string, object> TrackDataDictionary
	    {
            get { return TrackDataProperties; }
	    }
	}
}