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
    internal class ListMetadataProvider : ProviderAbstract<ListMetadata, RosterEntities, Guid>, IListMetadataProvider
	{
        public ListMetadataProvider()
            : base(string.Empty)
        {
        }
        public ListMetadataProvider(string connectionString)
            : base(connectionString)
        {
        }
        public override Expression<Func<ListMetadata, bool>> CompareByIds(IEnumerable<Guid> ids)
		{
			return entity => ids.Contains(entity.Id);
		}

        public override Expression<Func<ListMetadata, bool>> CompareById(Guid id)
		{
			return entity => entity.Id.Equals(id);
		}

        public override Guid GetEntityId(ListMetadata entity)
		{
			return entity.Id;
		}

        public override Expression<Func<ListMetadata, bool>> CompareByValue(ListMetadata entity)
		{
			return existedEntity => existedEntity.Id == entity.Id;
		}

        protected override void OnUpdateInternal(RosterEntities db, ListMetadata newEntity, ListMetadata oldEntity)
        {
            if ((newEntity == null) || (oldEntity == null)) return;
            var merger = new CollectionMerger(db);
            {
                merger.Merge(newEntity.ListMetadataFields, oldEntity.ListMetadataFields, new ListMetadataFieldComparer(),true);
                merger.Merge(newEntity.ListMetadataActions, oldEntity.ListMetadataActions, new ListMetadataActionComparer(), true);
            }
        }
        #region Comparers

        public class ListMetadataFieldComparer : IEqualityComparer<ListMetadataField>
        {
            public bool Equals(ListMetadataField first, ListMetadataField second)
            {
                return first.Id == second.Id;
            }

            public int GetHashCode(ListMetadataField entity)
            {
                return entity.Id.GetHashCode();
            }
        }

        public class ListMetadataActionComparer : IEqualityComparer<ListMetadataAction>
        {
            public bool Equals(ListMetadataAction first, ListMetadataAction second)
            {
                return first.Id == second.Id;
            }

            public int GetHashCode(ListMetadataAction entity)
            {
                return entity.Id.GetHashCode();
            }
        }

        #endregion

	}
}