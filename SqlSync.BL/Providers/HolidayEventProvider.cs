#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;
using Roster.BL.Facade;
using Roster.Common;
using Roster.Common.Helpers;
using Roster.Model.DataContext;
using Roster.Model.Helpers;

#endregion

namespace Roster.BL.Providers
{
	internal class HolidayEventProvider : ProviderAbstract<HolidayEvent, RosterEntities,Guid>, IHolidayEventProvider
	{
        public HolidayEventProvider()
            : base(string.Empty)
        {
        }
        public HolidayEventProvider(string connectionString)
            : base(connectionString)
        {
        }
		public override Expression<Func<HolidayEvent, bool>> CompareByIds(IEnumerable<Guid> ids)
		{
			return entity => ids.Contains(entity.Id);
		}

        public override Expression<Func<HolidayEvent, bool>> CompareById(Guid id)
		{
			return entity => entity.Id.Equals(id);
		}

        public override Guid GetEntityId(HolidayEvent entity)
		{
			return entity.Id;
		}

        public override Expression<Func<HolidayEvent, bool>> CompareByValue(HolidayEvent entity)
		{
			return existedEntity => existedEntity.Id == entity.Id;
		}

        protected override void OnUpdateInternal(RosterEntities db, HolidayEvent newEntity, HolidayEvent oldEntity)
        {
            if ((newEntity == null) || (oldEntity == null)) return;
            var merger = new CollectionMerger(db);
            {
                merger.Merge(newEntity.Holiday, oldEntity.Holiday, new HolidayComparer());
                merger.Merge(newEntity.HolidayObserveds, oldEntity.HolidayObserveds, new HolidayObservedComparer());
            }
        }

        public List<Holiday> Holidays(int? typeId)
        {
            var db = GetObjectContext();
            {
                try
                {
                    return typeId.HasValue ? 
                        db.Holidays.Where(item => item.TypeId == typeId.Value).OrderBy(item=>item.Name).ToList(): db.Holidays.OrderBy(item=>item.Name).ToList();
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Error("Holidays", ex);
                    return null;
                }
            }
        }

        public class HolidayObservedComparer : IEqualityComparer<HolidayObserved>
        {
            public bool Equals(HolidayObserved first, HolidayObserved second)
            {
                return first.StateId == second.StateId && first.HolidayEventId == second.HolidayEventId;
            }

            public int GetHashCode(HolidayObserved entity)
            {
                return entity.StateId.GetHashCode() * entity.HolidayEventId.GetHashCode();
            }
        }

        public class HolidayComparer : IEqualityComparer<Holiday>
        {
            public bool Equals(Holiday first, Holiday second)
            {
                return first.Id == second.Id;
            }

            public int GetHashCode(Holiday entity)
            {
                return entity.Id.GetHashCode();
            }
        }

        public DateTime GetNextDate(DateTime checkDate, int city)
        {
            return checkDate;
        }

        public void Populate(bool clear = false)
        {
            var db = GetObjectContext();
            {
                try
                {
                    db.Holiday_PopulateNextYear(clear);
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Error("Holiday_PopulateNextYear", ex);
                }
            }
        }
    }
}