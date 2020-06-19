#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SqlSync.BL.Facade;
using SqlSync.Common;
using SqlSync.Model.DataContext;
using System.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Text;
using System.Dynamic;
using SqlDynamic.Providers;
using SqlDynamic.Queries;
using System.Data.Entity;
using SqlSync.Model.Helpers;

#endregion

namespace SqlSync.BL.Providers
{
    internal class SystemOptionProvider : ProviderAbstract<SystemOption, SqlSyncEntities, String>, ISystemOptionProvider
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

        private string MappedTables(DbContext db)
        {
            var result = string.Empty;
            var mapped = db.MappedTables();
            if (!mapped.IsEmpty())
            {
                result = String.Join(",", mapped.Select(item => string.Format("'{0}'", item)));
            }
            if (!string.IsNullOrWhiteSpace(result))
            {
                result = string.Format(SqlTemplate.ListTable_WhereClause, result);
            }
            return result;
        }

        public List<string> ListTables()
        {
            var result = new List<string>();
            var db = GetContext();
            {
                using (var dbConnection = db.Database.Connection)
                {
                    if (dbConnection.State == ConnectionState.Closed)
                    {
                        db.Database.Connection.Open();
                    }
                    var query = string.Format(SqlTemplate.ListTables_SelectClause, MappedTables(db));
                    var command = new SqlCommand(query) as IDbCommand;
                    command.Connection = dbConnection;
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        result.Add(reader[0].ToString());
                    }
                }
            }
            return result;
        }

        public List<Tuple<int, ExpandoObject>> ListTableContent(string table, string key, string fields, List<Tuple<string, string>> whereCriteria = null)
        {
            var result = new List<Tuple<int, ExpandoObject>>();
            var db = GetContext();
            {
                using (var dbConnection = db.Database.Connection)
                {
                    if (dbConnection.State == ConnectionState.Closed)
                    {
                        db.Database.Connection.Open();
                    }
                    var tableDef = new Table(table);
                    var query = Query.From(tableDef).Select(tableDef.Field(key));
                    fields.Split('$').ToList().ForEach(item =>
                    {
                        query = query.Select(tableDef.Field(item));
                        query = query.OrderBy(tableDef.Field(item));
                    });
                    if (!whereCriteria.IsEmpty())
                    {
                        whereCriteria.ForEach(item =>
                        {
                            if (!string.IsNullOrWhiteSpace(item.Item1))
                            {
                                query = query.Where(SqlDynamic.Queries.Expression.And(
                                    SqlDynamic.Queries.Expression.Eq(tableDef.Field(item.Item1), 
                                    SqlDynamic.Queries.Expression.Const(item.Item2))));
                            }
                        });
                    }
                    var command = Provider.GetCommand(query);
                    command.Connection = dbConnection;
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var keyValue = reader[key].ToInt();
                        var value = (IDictionary<string, object>)new ExpandoObject();
                        fields.Split('$').ToList().ForEach(item =>
                        {
                            value.Add(item, reader[item].ToSafeString());
                        });
                        result.Add(new Tuple<int, ExpandoObject>(keyValue, (ExpandoObject)value));
                    }
                }
            }
            return result;
        }
        public List<string> ListKeyFields(string table)
        {
            var result = new List<string>();
            var db = GetContext();
            {
                using (var dbConnection = db.Database.Connection)
                {
                    if (dbConnection.State == ConnectionState.Closed)
                    {
                        db.Database.Connection.Open();
                    }
                    var query = string.Format(SqlTemplate.ListKeyFields_Query, table);
                    var command = new SqlCommand(query) as IDbCommand;
                    command.Connection = dbConnection;
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        result.Add(reader[0].ToString());
                    }
                }
            }
            return result;
        }


        public List<string> ListFields(string table)
        {
            var result = new List<string>();
            var db = GetContext();
            {
                using (var dbConnection = db.Database.Connection)
                {
                    if (dbConnection.State == ConnectionState.Closed)
                    {
                        db.Database.Connection.Open();
                    }
                    var query = string.Format(SqlTemplate.ListFields_Query, table);
                    var command = new SqlCommand(query) as IDbCommand;
                    command.Connection = dbConnection;
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        result.Add(reader[0].ToString());
                    }
                }
            }
            return result;
        }

        public void SaveDataSource(string datasource, string statements)
        {
            var db = GetContext();
            {
                using (var dbConnection = db.Database.Connection)
                {
                    if (dbConnection.State == ConnectionState.Closed)
                    {
                        dbConnection.Open();
                    }
                    var queryExists = string.Format(SqlTemplate.ExistsStatements_Query, datasource);
                    var commandExists = new SqlCommand(queryExists, dbConnection as SqlConnection)
                    {
                        CommandType = CommandType.Text
                    };
                    var querySave = string.Format(((Int32)commandExists.ExecuteScalar() > 0)
                        ? SqlTemplate.AlterStatements_Query
                        : SqlTemplate.CreateStatements_Query, datasource, statements);
                    var commandSave = new SqlCommand(querySave, dbConnection as SqlConnection)
                    {
                        CommandType = CommandType.Text
                    };
                    commandSave.ExecuteScalar();
                }
            }
        }

        public string ReadDataSource(string datasource)
        {
            var result = new StringBuilder();
            var db = GetContext();
            {
                using (var dbConnection = db.Database.Connection)
                {
                    if (dbConnection.State == ConnectionState.Closed)
                    {
                        dbConnection.Open();
                    }
                    var queryRead = string.Format(SqlTemplate.ReadStatements_Query, datasource);
                    var command = new SqlCommand(queryRead, dbConnection as SqlConnection)
                    {
                        CommandType = CommandType.Text
                    };
                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        var value = reader[0].ToSafeString();
                        if (String.IsNullOrWhiteSpace(value)) continue;
                        result.Append(value);
                    }
                }
            }
            return result.ToString().Replace(string.Format(
                SqlTemplate.CreateStatements_Query, datasource, string.Empty), string.Empty);
        }

        public string ExecuteProcedure(string name, ICollection<Tuple<string, object>> parameters)
        {
            var result = string.Empty;
            var db = GetContext();
            {
                using (var dbConnection = db.Database.Connection)
                {
                    if (dbConnection.State == ConnectionState.Closed)
                    {
                        dbConnection.Open();
                    }
                    using (var command = new SqlCommand(name, dbConnection as SqlConnection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        SqlCommandBuilder.DeriveParameters(command);
                        foreach (SqlParameter item in command.Parameters)
                        {
                            var paramValue = parameters.FirstOrDefault(i => String.Equals(i.Item1, item.ParameterName, StringComparison.InvariantCultureIgnoreCase));
                            if (paramValue != null)
                            {
                                switch (item.DbType)
                                {
                                    case DbType.Date:
                                    case DbType.DateTime:
                                    case DbType.DateTime2:
                                        string dtValue = paramValue.Item2.ToString();
                                        if (!item.IsNullable && string.IsNullOrEmpty(dtValue))
                                            throw new Exception(string.Format("Please specify '{0}' required parameter!", paramValue.Item1));

                                        item.Value = new DateTime(Convert.ToInt32(dtValue.Substring(0, 4)), Convert.ToInt32(dtValue.Substring(5, 2)), Convert.ToInt32(dtValue.Substring(8, 2)),
                                            Convert.ToInt32(dtValue.Substring(11, 2)), Convert.ToInt32(dtValue.Substring(14, 2)), Convert.ToInt32(dtValue.Substring(17, 2)), new GregorianCalendar());
                                        //item.Value = SPUtility.CreateDateTimeFromISO8601DateTimeString(paramValue.Item2.ToString());
                                        break;
                                    default:
                                        item.Value = paramValue.Item2;
                                        break;
                                }
                            }
                        }
                        command.ExecuteNonQuery();
                        result = command.Parameters.Cast<SqlParameter>().Where(item =>
                            item.Direction == ParameterDirection.Output).Aggregate(result, (current, item) => current + item.Value);
                    }
                }
            }
            return result;
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
    }
}