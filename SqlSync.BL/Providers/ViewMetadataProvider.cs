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
	internal class ViewMetadataProvider : ProviderAbstract<ViewMetadata, RosterEntities, Guid>, IViewMetadataProvider
	{
        public ViewMetadataProvider()
            : base(string.Empty)
        {
        }

        public ViewMetadataProvider(string connectionString)
            : base(connectionString)
        {
        }

		public override Expression<Func<ViewMetadata, bool>> CompareByIds(IEnumerable<Guid> ids)
		{
			return entity => ids.Contains(entity.Id);
		}

		public override Expression<Func<ViewMetadata, bool>> CompareById(Guid id)
		{
			return entity => entity.Id.Equals(id);
		}

		public override Guid GetEntityId(ViewMetadata entity)
		{
			return entity.Id;
		}

		public override Expression<Func<ViewMetadata, bool>> CompareByValue(ViewMetadata entity)
		{
			return existedEntity => existedEntity.Id == entity.Id;
		}

        protected override void OnUpdateInternal(RosterEntities db, ViewMetadata newEntity, ViewMetadata oldEntity)
        {
            if ((newEntity == null) || (oldEntity == null)) return;
            var whereMerger = new CollectionMerger(db);
            {
                whereMerger.Merge(newEntity.ViewMetadataWhereCriteries, oldEntity.ViewMetadataWhereCriteries, new ViewMetadataWhereCriteryComparer(), true);
            }
            var fieldMerger = new CollectionMerger(db);
            {
                fieldMerger.Merge(newEntity.ViewMetadataFields, oldEntity.ViewMetadataFields, new ViewMetadataFieldComparer(), true);
            }
            var popupSettingMerger = new CollectionMerger(db);
            {
                popupSettingMerger.Merge(newEntity.ViewMetadataPopupSettings, oldEntity.ViewMetadataPopupSettings, new ViewMetadataPopupSettingComparer(), true);
            }
        }
        #region Comparers

        public class ViewMetadataPopupSettingComparer : IEqualityComparer<ViewMetadataPopupSetting>
        {
            public bool Equals(ViewMetadataPopupSetting first, ViewMetadataPopupSetting second)
            {
                return first.Id == second.Id;
            }

            public int GetHashCode(ViewMetadataPopupSetting entity)
            {
                return entity.Id.GetHashCode();
            }
        }

        public class ViewMetadataFieldComparer : IEqualityComparer<ViewMetadataField>
        {
            public bool Equals(ViewMetadataField first, ViewMetadataField second)
            {
                return first.Id == second.Id;
            }

            public int GetHashCode(ViewMetadataField entity)
            {
                return entity.Id.GetHashCode();
            }
        }

        public class ViewMetadataWhereCriteryComparer : IEqualityComparer<ViewMetadataWhereCritery>
        {
            public bool Equals(ViewMetadataWhereCritery first, ViewMetadataWhereCritery second)
            {
                return first.Id == second.Id;
            }

            public int GetHashCode(ViewMetadataWhereCritery entity)
            {
                return entity.Id.GetHashCode();
            }
        }

        #endregion
	}
}