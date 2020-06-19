#region

using System;
using System.Collections.Generic;
using System.Data;

using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Remoting.Contexts;
using Roster.BL.Facade;
using Roster.Common;
using Roster.Common.Collections;
using Roster.Model.DataContext;
using Roster.Model.Helpers;

#endregion

namespace Roster.BL.Providers
{
    internal class EventStatusProvider : ProviderAbstract<vwEventStatu, RosterEntities, Guid>, IEventStatusProvider
	{
        public EventStatusProvider()
            : base(string.Empty)
        {
        }
        public EventStatusProvider(string connectionString)
            : base(connectionString)
        {
        }
        public override Expression<Func<vwEventStatu, bool>> CompareByIds(IEnumerable<Guid> ids)
		{
			return entity => ids.Contains(entity.Id);
		}

        public override Expression<Func<vwEventStatu, bool>> CompareByValue(vwEventStatu compare)
        {
            return entity => entity.Id == compare.Id;
        }

        public override Expression<Func<vwEventStatu, bool>> CompareById(Guid id)
		{
			return entity => entity.Id.Equals(id);
		}

        public override Guid GetEntityId(vwEventStatu entity)
		{
			return entity.Id;
		}
	}
}