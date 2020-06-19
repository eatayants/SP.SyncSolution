#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Roster.BL.Facade;
using Roster.Common;
using Roster.Common.Collections;
using Roster.Model.DataContext;
using Roster.Model.Helpers;
using SqlDynamic.Providers;
using SqlDynamic.Queries;
using System.Globalization;
using Expression = SqlDynamic.Queries.Expression;

#endregion

namespace Roster.BL.Providers
{
	internal class DefinitionProvider : ProviderAbstract<Definition, RosterEntities, Guid>, IDefinitionProvider
	{
        public DefinitionProvider()
            : base(string.Empty)
        {
        }
        public DefinitionProvider(string connectionString)
            : base(connectionString)
        {
        }
		public override Expression<Func<Definition, bool>> CompareByIds(IEnumerable<Guid> ids)
		{
			return entity => ids.Contains(entity.Id);
		}

		public Dictionary<string, string> ListBoolean()
		{
			return (new Dictionary<string, string>
				{
					{Boolean.FalseString, Boolean.FalseString},
					{Boolean.TrueString, Boolean.TrueString},
				}).ToDictionary(p => p.Key, p => p.Value);
		}

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
					var query = string.Format(SqlTemplate.ListTables_SelectClause,MappedTables(db));
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
                                query = query.Where(Expression.And(Expression.Eq(tableDef.Field(item.Item1), Expression.Const(item.Item2))));
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
                    var querySave = string.Format(((Int32) commandExists.ExecuteScalar() > 0)
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
                SqlTemplate.CreateStatements_Query, datasource, string.Empty),string.Empty);
        }

		public List<Definition> List(DefTypes defTypes)
		{
			var criteria = new SelectCriteria<Definition>
			{
				PageSize = Int32.MaxValue,
				ExpressionSort = new List<Expression<Func<Definition, dynamic>>> { item => item.Code, item => item.Name },
				ExpressionSelect = new List<Expression<Func<Definition, bool>>> 
						{ 
							item => item.TypeId == (int)defTypes, 
							item => item.Status == (int)DefStatus.Active
						}
			};
			return List(criteria);
		}

		public List<DefType> GetByUserTypes()
		{
			List<DefType> result;
			var db = GetObjectContext();
			{
				var query = from entity in db.DefTypes 
							orderby entity.Name 
							where !entity.Hidden
				select entity;
				result = query.ToList();
			}
			return result;
		}

		public DefType GetByType(DefTypes current)
		{
			DefType result;
			var db = GetObjectContext();
			{
				var query = from entity in db.DefTypes
							where entity.Id == (int)current
							select entity;
				result = query.FirstOrDefault();
			}
			return result;
		}

        public Definition GetDefinityionIdByName(string name,DefTypes type)
        {
            var db = GetObjectContext();
            {
              return  db.Definitions.FirstOrDefault(d=>d.TypeId == (int)type && d.Name == name);
            }
        }

		public override Expression<Func<Definition, bool>> CompareById(Guid id)
		{
			return entity => entity.Id.Equals(id);
		}

		public override Guid GetEntityId(Definition entity)
		{
			return entity.Id;
		}

		public override string GetEntityDisplayName(Definition entity)
		{
			var result = string.Empty;
			if (entity == null)
			{
				return result;
			}
			result = string.IsNullOrEmpty(entity.Code) ? entity.Name : string.Format("{0}-{1}", entity.Code, entity.Name);
			return result;
		}

		public override Expression<Func<Definition, bool>> CompareByValue(Definition entity)
		{
			return existedEntity => existedEntity.Id == entity.Id;
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
    }
}