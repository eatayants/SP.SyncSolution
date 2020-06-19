#region

using System;
using System.Collections.Generic;
using System.Data;

using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Remoting.Contexts;
using SqlSync.BL.Facade;
using SqlSync.Common;
using SqlSync.Common.Collections;
using SqlSync.Model.DataContext;
using SqlSync.Model.Helpers;

#endregion

namespace SqlSync.BL.Providers
{
    internal class ListMappingProvider : ProviderAbstract<ListMapping, SqlSyncEntities,Guid>, IListMappingProvider
	{
        public ListMappingProvider()
            : base(string.Empty)
        {
        }
        public ListMappingProvider(string connectionString)
            : base(connectionString)
        {
        }
        public override Expression<Func<ListMapping, bool>> CompareByIds(IEnumerable<Guid> ids)
		{
			return entity => ids.Contains(entity.Id);
		}

        public override Expression<Func<ListMapping, bool>> CompareById(Guid id)
		{
			return entity => entity.Id.Equals(id);
		}

        public override Guid GetEntityId(ListMapping entity)
		{
			return entity.Id;
		}

        public override Expression<Func<ListMapping, bool>> CompareByValue(ListMapping entity)
		{
			return existedEntity => existedEntity.Id == entity.Id;
		}

        protected override void OnUpdateInternal(SqlSyncEntities db, ListMapping newEntity, ListMapping oldEntity)
        {
            if ((newEntity == null) || (oldEntity == null)) return;
            var merger = new CollectionMerger(db);
            {
                merger.Merge(newEntity.ListMappingFields, oldEntity.ListMappingFields, new ListMappingFieldComparer(),true);
            }
        }
        #region Comparers

        public class ListMappingFieldComparer : IEqualityComparer<ListMappingField>
        {
            public bool Equals(ListMappingField first, ListMappingField second)
            {
                return first.Id == second.Id;
            }

            public int GetHashCode(ListMappingField entity)
            {
                return entity.Id.GetHashCode();
            }
        }

        #endregion

	}
}