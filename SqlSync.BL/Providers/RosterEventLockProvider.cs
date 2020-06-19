#region

using System;
using System.Collections.Generic;
using System.Data;

using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Remoting.Contexts;
using Microsoft.SharePoint.ApplicationPages.Calendar.Exchange;
using Roster.BL.Facade;
using Roster.Common;
using Roster.Common.Collections;
using Roster.Common.Helpers;
using Roster.Model.DataContext;

#endregion

namespace Roster.BL.Providers
{
    internal class RosterEventLockProvider : ProviderAbstract<RosterEventLock, RosterEntities,Guid>, IRosterEventLockProvider
	{
        public RosterEventLockProvider()
            : base(string.Empty)
        {
        }
        public RosterEventLockProvider(string connectionString)
            : base(connectionString)
        {
        }
        public override Expression<Func<RosterEventLock, bool>> CompareByIds(IEnumerable<Guid> ids)
		{
			return entity => ids.Contains(entity.Id);
		}

        public override Expression<Func<RosterEventLock, bool>> CompareByValue(RosterEventLock entity)
        {
            return existedEntity => existedEntity.Id == entity.Id;
        }

        public override Expression<Func<RosterEventLock, bool>> CompareById(Guid id)
		{
			return entity => entity.Id.Equals(id);
		}

        public override Guid GetEntityId(RosterEventLock entity)
		{
			return entity.Id;
		}

        public bool IsExist(int workerId, DateTime startDate, DateTime endDate)
        {
            var result = false;
            var db = GetObjectContext();
            {
                try
                {
                    result = (db.RosterEventLocks.Any(
                            item => item.WorkerPersonId != null && item.WorkerPersonId.Value == workerId
                            && item.StartDate == startDate.Date && item.EndDate == endDate.Date));
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Error("RosterEventLockProvider - IsExists", ex);
                }
                return result;
            }
        }

        public void Save(int workerId, DateTime startDate, DateTime endDate)
        {
            try
            {
                if (IsExist(workerId, startDate, endDate)) return;
                var rosterEventLock = new RosterEventLock()
                {
                    Id = Guid.NewGuid(),WorkerPersonId = workerId,
                    StartDate = startDate,EndDate = endDate,
                };
                Add(rosterEventLock);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Error("RosterEventLockProvider - Save", ex);
            }
        }
    }
}