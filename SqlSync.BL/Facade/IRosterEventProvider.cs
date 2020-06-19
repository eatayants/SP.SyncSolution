#region

using System;
using System.Collections.Generic;
using Roster.Common;
using Roster.Common.Collections;
using Roster.Model.DataContext;

#endregion

namespace Roster.BL.Facade
{
	public interface IRosterEventProvider : IEntityBaseProvider<RosterEvent,Guid>
	{
	    void OnCreate(RosterEvent item);
	    void OnModify(RosterEvent item);
	}
}