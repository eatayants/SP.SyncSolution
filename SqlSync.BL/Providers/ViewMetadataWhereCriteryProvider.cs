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

#endregion

namespace Roster.BL.Providers
{
    internal class ViewMetadataWhereCriteryProvider : ProviderAbstract<ViewMetadataWhereCritery, RosterEntities,Guid>, IViewMetadataWhereCriteryProvider
	{
        public ViewMetadataWhereCriteryProvider()
            : base(string.Empty)
        {
        }
        public ViewMetadataWhereCriteryProvider(string connectionString)
            : base(connectionString)
        {
        }
        public override Expression<Func<ViewMetadataWhereCritery, bool>> CompareByIds(IEnumerable<Guid> ids)
		{
			return entity => ids.Contains(entity.Id);
		}

        public override Expression<Func<ViewMetadataWhereCritery, bool>> CompareById(Guid id)
		{
			return entity => entity.Id.Equals(id);
		}

        public override Guid GetEntityId(ViewMetadataWhereCritery entity)
		{
			return entity.Id;
		}

        public override Expression<Func<ViewMetadataWhereCritery, bool>> CompareByValue(ViewMetadataWhereCritery entity)
		{
			return existedEntity => existedEntity.Id == entity.Id;
		}
	}
}