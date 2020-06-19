#region

using System;
using System.Collections.Generic;
using Roster.Common;
using Roster.Common.Collections;
using Roster.Model.DataContext;

#endregion

namespace Roster.BL.Facade
{
    public interface ITrackingDataViewProvider : IEntityBaseProvider<vwRosterEventTrackData, Guid>
    {
        ICollection<vwRosterEventTrackData> GetList(Guid rosterEventId);
    }
}