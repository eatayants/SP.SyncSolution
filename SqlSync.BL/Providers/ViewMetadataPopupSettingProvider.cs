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
    internal class ViewMetadataPopupSettingProvider : ProviderAbstract<ViewMetadataPopupSetting, RosterEntities, Guid>, IViewMetadataPopupSettingProvider
	{
        public ViewMetadataPopupSettingProvider()
            : base(string.Empty)
        {
        }
        public ViewMetadataPopupSettingProvider(string connectionString)
            : base(connectionString)
        {
        }
        public override Expression<Func<ViewMetadataPopupSetting, bool>> CompareByIds(IEnumerable<Guid> ids)
		{
			return entity => ids.Contains(entity.Id);
		}

        public override Expression<Func<ViewMetadataPopupSetting, bool>> CompareById(Guid id)
		{
			return entity => entity.Id.Equals(id);
		}

        public override Guid GetEntityId(ViewMetadataPopupSetting entity)
		{
			return entity.Id;
		}

        public override Expression<Func<ViewMetadataPopupSetting, bool>> CompareByValue(ViewMetadataPopupSetting entity)
		{
			return existedEntity => existedEntity.Id == entity.Id;
		}
	}
}