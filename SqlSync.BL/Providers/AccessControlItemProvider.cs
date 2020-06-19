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
    internal class AccessControlItemProvider : ProviderAbstract<AccessControlItem, RosterEntities, Guid>, IAccessControlItemProvider
	{
        public AccessControlItemProvider()
            : base(string.Empty)
        {
        }
        public AccessControlItemProvider(string connectionString)
            : base(connectionString)
        {
        }
        public override Expression<Func<AccessControlItem, bool>> CompareByIds(IEnumerable<Guid> ids)
		{
			return entity => ids.Contains(entity.Id);
		}

        public override Expression<Func<AccessControlItem, bool>> CompareById(Guid id)
		{
			return entity => entity.Id.Equals(id);
		}

        public override Guid GetEntityId(AccessControlItem entity)
		{
			return entity.Id;
		}
        public override Expression<Func<AccessControlItem, bool>> CompareByValue(AccessControlItem entity)
		{
			return existedEntity => existedEntity.Id == entity.Id;
		}

        public bool? HasRights(Guid itemId, ICollection<int> trusteeIds, AccessRight accessRights)
        {
            bool? result = null;
            var db = GetObjectContext();
            {
                // first of all check - if any record in Permissions table exist for current ItemId. If no one record exists, method result will be NULL
                var accessItems = db.AccessControlItems.Where(item => item.ItemId == itemId);
                if (accessItems.Any()) 
                {
                    // item has unique permissions - let's check it
                    accessItems = accessItems.Where(item => trusteeIds.Contains(item.TrusteeId));
                    if (accessItems.Any()) 
                    {
                        var maxAccessRight = accessItems.Max(item => item.AccessRight);
                        result = maxAccessRight >= (int) accessRights;
                    } else {
                        result = false; // user does't have permissions
                    }
                }
            }
            return result;
        }
	}
}