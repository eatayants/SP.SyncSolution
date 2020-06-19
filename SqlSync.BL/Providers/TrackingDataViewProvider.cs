#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
    internal class TrackingDataViewProvider : ProviderAbstract<vwRosterEventTrackData, RosterEntities, Guid>, ITrackingDataViewProvider
	{
        public TrackingDataViewProvider()
            : base(string.Empty)
        {
        }
        public TrackingDataViewProvider(string connectionString)
            : base(connectionString)
        {
        }
        public override Expression<Func<vwRosterEventTrackData, bool>> CompareByIds(IEnumerable<Guid> ids)
		{
			return entity => ids.Contains(entity.Id);
		}

        public override Expression<Func<vwRosterEventTrackData, bool>> CompareByValue(vwRosterEventTrackData compare)
        {
            return entity => entity.Id == compare.Id;
        }

        public override Expression<Func<vwRosterEventTrackData, bool>> CompareById(Guid id)
		{
			return entity => entity.Id.Equals(id);
		}

        public override Guid GetEntityId(vwRosterEventTrackData entity)
		{
			return entity.Id;
		}

        public ICollection<vwRosterEventTrackData> GetList(Guid rosterEventId)
        {
            var db = GetContext();
            {
                using (var dbConnection = db.Database.Connection)
                {
                    if (dbConnection.State == ConnectionState.Closed)
                    {
                        dbConnection.Open();
                    }
                    var command = new SqlCommand("dbo.RosterEvents_TrackData", dbConnection as SqlConnection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    var startDateParam = command.Parameters.Add("@rosterEventId", SqlDbType.UniqueIdentifier);
                    startDateParam.Value = rosterEventId;
                    var da = new SqlDataAdapter { SelectCommand = command };
                    var ds = new DataSet("RosterEventTrackData");
                    da.Fill(ds);
                    return ds.Tables[0].AsEnumerable().Select(_ => new vwRosterEventTrackData
                    {
                         Id = _.Field<Guid>("Id"),
                         RosterEventId = _.Field<Guid>("RosterEventId"),
                         CreatedOn = _.Field<DateTime>("CreatedOn"),
                         XmlContent = _.Field<string>("XmlContent"),
                         Version = _.Field<long?>("Version")
                    }).ToList();
                }
            }
        }
	}
}