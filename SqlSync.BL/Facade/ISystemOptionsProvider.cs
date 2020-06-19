#region

using System;
using System.Collections.Generic;
using Roster.Model.DataContext;

#endregion

namespace Roster.BL.Facade
{
	public interface ISystemOptionProvider : IEntityBaseProvider<SystemOption,String>
	{
		string DatabaseVersion { get; set; }
        int PlannedPublishingDaysAhead { get; set; } 
	}
}