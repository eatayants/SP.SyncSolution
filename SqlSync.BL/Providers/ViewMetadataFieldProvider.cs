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
    internal class ViewMetadataFieldProvider : ProviderAbstract<ViewMetadataField, RosterEntities, Guid>, IViewMetadataFieldProvider
	{        
        public ViewMetadataFieldProvider()
            : base(string.Empty)
        {
        }
        public ViewMetadataFieldProvider(string connectionString)
            : base(connectionString)
        {
        }
        public override Expression<Func<ViewMetadataField, bool>> CompareByIds(IEnumerable<Guid> ids)
		{
			return entity => ids.Contains(entity.Id);
		}

        public override Expression<Func<ViewMetadataField, bool>> CompareById(Guid id)
		{
			return entity => entity.Id.Equals(id);
		}

        public override Guid GetEntityId(ViewMetadataField entity)
		{
			return entity.Id;
		}

        public override Expression<Func<ViewMetadataField, bool>> CompareByValue(ViewMetadataField entity)
		{
			return existedEntity => existedEntity.Id == entity.Id;
		}
	}
}