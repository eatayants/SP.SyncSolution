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
    internal class AccessControlListProvider : ProviderAbstract<AccessControlList, RosterEntities, Guid>, IAccessControlListProvider
	{
        public AccessControlListProvider()
            : base(string.Empty)
        {
        }
        public AccessControlListProvider(string connectionString)
            : base(connectionString)
        {
        }
        public override Expression<Func<AccessControlList, bool>> CompareByIds(IEnumerable<Guid> ids)
		{
			return entity => ids.Contains(entity.Id);
		}

        public override Expression<Func<AccessControlList, bool>> CompareById(Guid id)
		{
			return entity => entity.Id.Equals(id);
		}

        public override Guid GetEntityId(AccessControlList entity)
		{
			return entity.Id;
		}

        public override Expression<Func<AccessControlList, bool>> CompareByValue(AccessControlList entity)
		{
			return existedEntity => existedEntity.Id == entity.Id;
		}

        public bool HasRights(Guid listId, AccessRight accessRights)
        {           
            return HasRights(listId, GetTrusteeIds(), accessRights);
        }

        public bool HasRights(Guid listId, ICollection<int> trusteeIds, AccessRight accessRights)
        {
            bool result = false;
            var db = GetObjectContext();
            {
                var accessItems = db.AccessControlLists.Where(item => item.ListMetadataId == listId && trusteeIds.Contains(item.TrusteeId));
                if (accessItems.Any()) 
                {
                    var maxAccessRight = accessItems.Max(item => item.AccessRight);
                    result = maxAccessRight >= (int)accessRights;
                }
            }
            return result;
        }
	}
}