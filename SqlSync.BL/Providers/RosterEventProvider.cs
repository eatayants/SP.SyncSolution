#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Remoting.Contexts;
using Roster.BL.Facade;
using Roster.Common;
using Roster.Common.Collections;
using Roster.Common.Helpers;
using Roster.Model.DataContext;

#endregion

namespace Roster.BL.Providers
{
	internal class RosterEventProvider : ProviderAbstract<RosterEvent, RosterEntities, Guid>, IRosterEventProvider
	{
        public RosterEventProvider()
            : base(string.Empty)
        {
        }
        public RosterEventProvider(string connectionString)
            : base(connectionString)
        {
        }
		public override Expression<Func<RosterEvent, bool>> CompareByIds(IEnumerable<Guid> ids)
		{
			return entity => ids.Contains(entity.Id);
		}

		public override Expression<Func<RosterEvent, bool>> CompareById(Guid id)
		{
			return entity => entity.Id.Equals(id);
		}

		public override Guid GetEntityId(RosterEvent entity)
		{
			return entity.Id;
		}

		public override Expression<Func<RosterEvent, bool>> CompareByValue(RosterEvent entity)
		{
			return existedEntity => existedEntity.Id == entity.Id;
		}

        public void OnModify(RosterEvent item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            var db = GetObjectContext();
            {              
                try
                {
                    var message = new ObjectParameter("message", typeof(string));
                    db.RosterEvents_SetProperties(item.Id, message);
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Error("RorterEvents On Modify", ex);
                    throw new DataException("RorterEvents On Modify", ex);
                }
            }
        }

        public void OnCreate(RosterEvent item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            var db = GetObjectContext();
            {
                var message = new ObjectParameter("message", typeof (string));
                try
                {
                    switch (item.EventTypeId)
                    {
                        case 0://Planned
                            db.RorterEvents_PlannedCreate(item.Id.ToString(), message);
                            break;
                        case 1://Working
                            db.RorterEvents_WorkingCreate(item.Id.ToString(), message);
                            break;
                        case 2://Tempalate
                            //db.RorterEvents_WorkingCreate(item.Id.ToString(), message);
                            break;
                        case 3://Timesheet
                            db.RorterEvents_TimesheetCreate(item.Id.ToString(), message);
                            break;
                    }
                    db.RosterEvents_SetProperties(item.Id, message);
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Error("RorterEvents On Create", ex);
                    throw new DataException("RorterEvents On Create", ex);
                }
            }
        }

        private static DataTable GetDataTable(List<Tuple<int, string>> values) 
        {
            var result = new DataTable();
            result.Columns.Add(new DataColumn {DataType = Type.GetType("System.Int32"), ColumnName = "Code"});
            result.Columns.Add(new DataColumn { DataType = Type.GetType("System.String"), ColumnName = "Value" });
            if (values.IsEmpty()) return result;
            foreach (var item in values)
            {
                var row = result.NewRow();
                row["Code"] = item.Item1;
                row["Value"] = item.Item2;
                result.Rows.Add(row);
            }
            return result;
        }
        public bool IsAllowApprove(DateTime startWeekDate, Guid id)
        {
            var db = GetObjectContext();
            {
                var message = new ObjectParameter("message", typeof(string)) { Value = "" };
                bool result;
                try
                {
                    var allow = new ObjectParameter("allow", typeof(bool)) { Value = false };
                    db.RorterEvents_TimesheetCheckApprove(id,startWeekDate, allow, message);
                    if (!string.IsNullOrEmpty(message.Value.ToString()))
                        throw new Exception(message.Value.ToString());
                    var spResult = (allow.Value as bool?);
                    result = spResult ?? false;
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Error("Timesheet event is allow approve", ex);
                    throw new DataException(ex.Message, ex);
                }
                return result;
            }
        }
        public bool IsAllowApprove(DateTime startWeekDate, int workerId)
        {
            var db = GetObjectContext();
            {
                var message = new ObjectParameter("message", typeof (string));
                bool result;
                try
                {
                    var allow = new ObjectParameter("allow", typeof (bool)) {Value = false};
                    db.RorterEvents_TimesheetsCheckApprove(startWeekDate, workerId, allow, message);
                    var spResult = (allow.Value as bool?);
                    result = spResult ?? false;
                }
                catch (Exception ex)
                {
                    const string exMessage = "Timesheet event is allow approve";
                    LogHelper.Instance.Error(exMessage, ex);
                    throw new DataException(exMessage, ex);
                }
                return result;
            }
        }

        public void WorkingLock(string spName, DateTime startDate, DateTime endDate, List<Tuple<int, string>> trusteeRights, string message)
        {
            var db = GetContext();
            {
                using (var dbConnection = db.Database.Connection)
                {
                    if (dbConnection.State == ConnectionState.Closed) {
                        dbConnection.Open();
                    }
                    var command = new SqlCommand(spName, dbConnection as SqlConnection) {
                        CommandType = CommandType.StoredProcedure
                    };
                    var startDateParam = command.Parameters.Add("@startDate", SqlDbType.DateTime);
                    startDateParam.Value = startDate;
                    var endDateParam = command.Parameters.Add("@endDate", SqlDbType.DateTime);
                    endDateParam.Value = endDate;
                    var messageParam = command.Parameters.Add("@message", SqlDbType.NVarChar);
                    messageParam.Value = message;
                    var trusteeRightsParams = command.Parameters.AddWithValue("@persissions", GetDataTable(trusteeRights));
                    trusteeRightsParams.SqlDbType = SqlDbType.Structured;
                    trusteeRightsParams.TypeName = "dbo.ParamCollection";
                    command.ExecuteNonQuery();
                }
            }
        }
	}
}