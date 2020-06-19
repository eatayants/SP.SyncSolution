#region

using System;
using System.Collections.Generic;
using SqlSync.Common;
using SqlSync.Common.Collections;
using SqlSync.Model.DataContext;

#endregion

namespace SqlSync.BL.Facade
{
    public interface IListMappingProvider : IEntityBaseProvider<ListMapping,Guid>
	{
	}
}