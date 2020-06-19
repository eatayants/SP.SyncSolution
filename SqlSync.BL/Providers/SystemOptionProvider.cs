#region

using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;
using Roster.BL.Facade;
using Roster.Common;
using Roster.Model.DataContext;
using Roster.Model.DataContext.Partail;

#endregion

namespace Roster.BL.Providers
{
	internal class SystemOptionProvider : ProviderAbstract<SystemOption, RosterEntities, String>, ISystemOptionProvider
	{
        public SystemOptionProvider()
            : base(string.Empty)
        {
        }
        public SystemOptionProvider(string connectionString)
            : base(connectionString)
        {
        }
		#region System Methods

		private string GetOptionValue(string optionName)
		{
			var result = string.Empty;
			var db = GetObjectContext();
			{
				var entity = db.SystemOptions.FirstOrDefault(item => item.OptionName == optionName);
				if (entity != null)
				{
					result = entity.OptionValue;
				}
			}
			return result;
		}

		private void SetOptionValue(string optionName, string optionValue)
		{
			var db = GetObjectContext();
			{
				var entity = (from option in db.SystemOptions
				              where option.OptionName == optionName
				              select option).FirstOrDefault();
				if (entity == null)
				{
					entity = new SystemOption
					{
						OptionName = optionName, 
						OptionValue = optionValue
					};
					db.SystemOptions.Add(entity);
				}
				else
				{
					entity.OptionValue = optionValue;
				}
				db.SaveChanges();
			}
		}

		#endregion System Methods

		#region EntityBase

		public override Expression<Func<SystemOption, bool>> CompareByIds(IEnumerable<String> ids)
		{
            return entity => ids.Contains(entity.OptionName);
		}

        public override Expression<Func<SystemOption, bool>> CompareById(String id)
        {
            return entity => String.Compare(entity.OptionName, id, StringComparison.InvariantCultureIgnoreCase)==0;
        }

		public override Expression<Func<SystemOption, bool>> CompareByValue(SystemOption entity)
		{
			return existedEntity => (existedEntity.OptionName != entity.OptionName);
		}

		public override String GetEntityId(SystemOption entity)
		{
            return entity.OptionName;
		}

		#endregion EntityBase

        public int PlannedPublishingDaysAhead
        {
            get
            {
                return GetOptionValue(SystemSettingName.PlannedPublishingDaysAhead).ToInt();
            }
            set
            {
                SetOptionValue(SystemSettingName.PlannedPublishingDaysAhead, value.ToString());
            }
        }

		public string DatabaseVersion
		{
			get
			{
				return GetOptionValue(SystemSettingName.DatabaseVersion);
			}
			set
			{
				SetOptionValue(SystemSettingName.DatabaseVersion, value);
			}
		}

        public int SqlAgentStatus()
        {
            int result;
            var db = GetObjectContext();
            {
                var status = new ObjectParameter("status", typeof(Int32));
                db.SqlServerAgent_Status(status);
                result = (int)status.Value;
            }
            return result;
        }

        public List<SqlAgentJob> SqlAgentJobs()
        {
            var result = new List<SqlAgentJob>();
            var db = GetObjectContext();
            {
                var dbResult = db.SqlServerAgent_JobList().Select(item=>item).ToList();
                if (!dbResult.IsEmpty())
                {
                    result.AddRange(dbResult.Select(item => new SqlAgentJob
                    {
                        Id = item.JobID,
                        JobName = item.JobName,
                        JobDescription = item.JobDescription,
                        IsScheduled = item.IsScheduled,
                        IsEnabled = item.IsEnabled,
                        LastRunDateTime = item.LastRunDateTime,
                        LastRunDuration = item.LastRunDuration__HH_MM_SS_,
                        LastRunStatus = item.LastRunStatus,
                        LastRunStatusMessage = item.LastRunStatusMessage,
                        NextRunDateTime = item.NextRunDateTime
                    }));
                }
            }
            return result;
        }
    }
}