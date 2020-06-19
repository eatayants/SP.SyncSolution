#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using Microsoft.SharePoint;
using Microsoft.SharePoint.ApplicationPages.Calendar.Exchange;
using Roster.Common;
using Roster.Model.DataContext;
using Roster.Model.DataContext.Extention;
using SqlDynamic.Providers;
using SqlDynamic.Queries;
using QueryCriteria = Roster.BL.Extentions.QueryCriteria;

#endregion

namespace Roster.BL.Providers
{
    public abstract class ProviderSqlAbstract<TTable> : ProviderBase,IDisposable
        where TTable : TableDynamic, new()
    {
        private const string RowNumFieldAlias = "RowNum";
        private const string PagingSubqueryAlias = "Paging";

        protected ProviderSqlAbstract(string connectionString)
            : base(connectionString)
        {
        }

        #region Abstract Methods

        public abstract TTable GetTableDef();
        public abstract List<string> SystemFields();

        #endregion

        #region Internal Methods


        private ExpandoObject SqlDataReaderToExpando(IDataRecord reader)
        {
            var expandoObject = new ExpandoObject();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                try {
                    ((IDictionary<string, object>)expandoObject).Add(reader.GetName(i), reader[i]);
                } catch (ArgumentException) { }
            }
            return expandoObject;
        }

        #endregion

        #region Service Members

        private RosterEntities _dataContextInstance;
		private bool _isDisposed;
        public DbContext GetContext()
        {
            return CreateObjectContext();
        }
		#endregion

		#region IDisposable

		public void Dispose()
		{
			if (_dataContextInstance != null && !_isDisposed)
			{
				_dataContextInstance.Dispose();
				_dataContextInstance = null;
				_isDisposed = true;
			}
		}

		#endregion

		#region Members

		public virtual List<IExpression> GetDefaultFields()
		{
			return new List<IExpression>();
		}

        public ExpandoObject Get(object id)
        {
            var dbContext = CreateObjectContext();
            {
                using (IDbConnection dbConnection = dbContext.Database.Connection)
                {
					if (dbConnection.State == ConnectionState.Closed)
					{
						dbContext.Database.Connection.Open();
					}
                    var tableDef = GetTableDef();
                    var query = tableDef.GetQuery().Where(Expression.Eq(tableDef.GetPrimaryField(), Expression.Const(id)));
                    var command = Provider.GetCommand(query);
                    command.Connection = dbConnection;
                    var reader = command.ExecuteReader();
                    return reader.Read() ? SqlDataReaderToExpando(reader as DbDataReader) : null;
                }
            }
        }

        public ExpandoObject GetByParentId(object id)
        {
            var dbContext = CreateObjectContext();
            {
                using (IDbConnection dbConnection = dbContext.Database.Connection)
                {
					if (dbConnection.State == ConnectionState.Closed)
					{
						dbContext.Database.Connection.Open();
					}
                    var tableDef = GetTableDef();
                    var query = tableDef.GetQuery().Where(Expression.Eq(tableDef.GetParentField(), Expression.Const(id)));
                    var command = Provider.GetCommand(query);
                    command.Connection = dbConnection;
                    var reader = command.ExecuteReader();
                    return reader.Read() ? SqlDataReaderToExpando(reader as DbDataReader) : null;
                }
            }
        }
        
        public bool IsExists(object id)
        {
            var dbContext = CreateObjectContext();
            {
                using (IDbConnection dbConnection = dbContext.Database.Connection)
                {
					if (dbConnection.State == ConnectionState.Closed)
					{
						dbConnection.Open();
					}
                    var tableDef = GetTableDef();
                    var query = tableDef.GetCount().Where(Expression.Eq(tableDef.GetPrimaryField(), Expression.Const(id)));
                    var command = Provider.GetCommand(query);
                    command.Connection = dbConnection;
                    var rowCount = (Int32) command.ExecuteScalar();
                    return rowCount > 0;
                }
            }
        }

        private Query AccessControl(Query query, QueryCriteria queryCriteria)
        {
            if (!queryCriteria.AccessCriteria) return query;
            if (queryCriteria.TrusteeIds.IsEmpty())
            {
                queryCriteria.TrusteeIds.AddRange(GetTrusteeIds());
            }
            var tableDef = GetTableDef();
            var accessTableDef = new TableSource(new Table("AccessControlItem"));
            var trusteeIds = queryCriteria.TrusteeIds.Select(item=>item as object).AsEnumerable();
            var subQuery = Query.From(accessTableDef).Select(accessTableDef.Field("ItemId")).
                Where(Expression.Ba(accessTableDef.Field("AccessRight"), (int)AccessRight.Read)).
                Where(Expression.And(Expression.In(accessTableDef.Field("TrusteeId"), trusteeIds)));
            query = query.Where(Expression.And(Expression.In(tableDef.GetPrimaryField(), new SubQuery(subQuery))));
            return query;  
        }

        private Query WhereBuilder(Query query, QueryCriteria queryCriteria)
        {
            var tableDef = GetTableDef();

            queryCriteria.InternalWhere.ForEach(item => { query = query.Where(item); });

            if (queryCriteria.WhereCriteria.IsEmpty()) return query;
            
            queryCriteria.WhereCriteria.ForEach(item =>
            {
                IExpression value;
                Field field;
                var concateOperator = item.Item3;
                if (item.Item1.SqlDbType == SqlDbType.Xml)
                {
                    value = Expression.Const(1);
                    field = tableDef.TableSource().Field(item.Item1.InternalName,
                       string.Format(@"exist('/Items/Value[.=""{0}""]')", Convert.ToString(item.Item4)));
                    if (!item.Item2.In(CompareType.NotEqual,CompareType.Equal))
                    {
                        throw new Exception(string.Format(
                            "Comparison operator '{0}' not supported for type XML", item.Item2.GetStringValue()));
                    }
                }
                else
                {
                    value = Expression.Const(item.Item4);
                    field = tableDef.TableSource().Field(item.Item1.InternalName);
                    if (item.Item1.DataSourceType.In((int)LookupSourceType.Query,(int) LookupSourceType.Table))
                    {
                        if (!string.IsNullOrWhiteSpace(item.Item5))
                        {
                            field = new TableSource(new Table(item.Item1.DataSource)).Field(item.Item5);
                        }
                    }
                }

                switch (item.Item2)
                {
                    case CompareType.Contains:
                    case CompareType.NotContains:
                    {
                        value = Expression.Const("%" + Convert.ToString(item.Item4) + "%");
                        var expression = (concateOperator == ConcateOperator.Or)
                            ? Expression.Or(Expression.Like(field, value))
                            : (IBoolean) Expression.And(Expression.Like(field, value));
                        if (item.Item2 == CompareType.NotContains)
                        {
                            expression = Expression.Not(expression);
                        }
                        query = query.Where(expression);
                        break;
                    }
                    case CompareType.InValue:
                    case CompareType.NotInValue:
                    {
                        var inValue = (item.Item4 as IEnumerable<object>);
                        if (inValue != null)
                        {
                            var expression = (concateOperator == ConcateOperator.Or)
                                ? Expression.Or(Expression.In(field, inValue))
                                : (IBoolean)Expression.And(Expression.In(field, inValue));
                            if (item.Item2 == CompareType.NotInValue)
                            {
                                expression = Expression.Not(expression);
                            }
                            query = query.Where(expression);
                        }
                        break;
                    }
                    case CompareType.Equal:
                    case CompareType.NotEqual:
                    {
                        var expression = (concateOperator == ConcateOperator.Or)
                            ? Expression.Or(Expression.Eq(field, value))
                            : (IBoolean)Expression.And(Expression.Eq(field, value));
                        if (item.Item2 == CompareType.NotEqual)
                        {
                            expression = Expression.Not(expression);
                        }
                        query = query.Where(expression);
                        break;
                    }
                    case CompareType.Less:
                    {
                        query = query.Where((concateOperator == ConcateOperator.Or)
                            ? Expression.Or(Expression.Lt(field, value))
                            : (IBoolean) Expression.And(Expression.Lt(field, value)));
                        break;
                    }
                    case CompareType.LessOrEqual:
                    {
                        query = query.Where((concateOperator == ConcateOperator.Or)
                            ? Expression.Or(Expression.Le(field, value))
                            : (IBoolean) Expression.And(Expression.Le(field, value)));
                        break;
                    }
                    case CompareType.More:
                    {
                        query = query.Where((concateOperator == ConcateOperator.Or)
                            ? Expression.Or(Expression.Gt(field, value))
                            : (IBoolean) Expression.And(Expression.Gt(field, value)));
                        break;
                    }
                    case CompareType.MoreOrEqual:
                    {
                        query = query.Where((concateOperator == ConcateOperator.Or)
                            ? Expression.Or(Expression.Ge(field, value))
                            : (IBoolean) Expression.And(Expression.Ge(field, value)));
                        break;
                    }
                }
            });
            query = AccessControl(query, queryCriteria);
            return query;
        }

        public List<ExpandoObject> List(QueryCriteria queryCriteria)
        {
            var result = new List<ExpandoObject>();
			var dbContext = GetContext();
            {
                using (IDbConnection dbConnection = dbContext.Database.Connection)
                {
	                if (dbConnection.State == ConnectionState.Closed)
	                {
						dbContext.Database.Connection.Open();
	                }
                    var tableDef = GetTableDef();
					var query = tableDef.GetQueryDef();
	                if (!queryCriteria.SelectCriteria.IsEmpty())
	                {
						queryCriteria.SelectCriteria.ForEach(item =>
						{
							var field = (item as Field);
							if (field != null)
							{
								var dataSource = (field.TabularDataSource as TableSource);
								query = dataSource != null && dataSource.Table.Name != tableDef.Name ? 
                                    query.Select(field.As(string.Format("[{0}_{1}]", dataSource.Table.Name, field.Name))) : 
                                    query.Select(field);
							}
							else
							{
								query = query.Select(item);
							}
						});
	                }
	                else
	                {
		                query = query.SelectAll();
	                }
                    if (queryCriteria.Dictinct)
                    {
                        query = query.Distinct();
                    }
                    if (!queryCriteria.JoinCriteria.IsEmpty())
                    {
						queryCriteria.JoinCriteria.ForEach(item => query = 
                            query.Join(item.Item1, item.Item2, item.Item3));
                    }                   
                    query = WhereBuilder(query, queryCriteria);
                    if (!queryCriteria.OrderCriteria.IsEmpty())
                    {
                        queryCriteria.OrderCriteria.ForEach(item =>
                        {
                            var sortType = item.Item2 == SortDirection.Descending ? Order.Descending : Order.Ascending;
                            var field = tableDef.TableSource().Field(item.Item1.InternalName);
                            if (item.Item1.DataSourceType.In((int) LookupSourceType.Query,(int) LookupSourceType.Table))
                            {
                                field =  new TableSource(new Table(item.Item1.DataSource)).Field(item.Item3);
                            }
                            query = query.OrderBy(field, sortType);
                        });
                    }
                    else
                    {
                        query = query.OrderBy(tableDef.GetPrimaryField());
                    }

                    IDbCommand command;
                    if (queryCriteria.TakeRows > 0)
                    {
                        query = query.Select(new RowNumber(query.OrderByClause, queryCriteria.Dictinct).As(RowNumFieldAlias));
                        query = query.ClearOrderBy();
                        var subQuery = Query.From(query.As(PagingSubqueryAlias)).SelectAll();
                        subQuery = subQuery.OrderBy(new TableSource(new Table(PagingSubqueryAlias)).Field(RowNumFieldAlias));

                        subQuery = subQuery.Where(Expression.Gt(Field.Get(PagingSubqueryAlias,RowNumFieldAlias),
                            Expression.Const((queryCriteria.SkipRows))));
                        subQuery = subQuery.Where(Expression.Le(Field.Get(PagingSubqueryAlias, RowNumFieldAlias),
                            Expression.Const((queryCriteria.SkipRows + queryCriteria.TakeRows))));
                        command = Provider.GetCommand(subQuery);
                    }
                    else
                    {
                        command = Provider.GetCommand(query);
                    }
                    command.Connection = dbConnection;
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        result.Add(SqlDataReaderToExpando(reader as DbDataReader));
                    }
                }
            }
            return result;
        }

		public int Count(QueryCriteria queryCriteria)
        {
            var result = 0;
			var dbContext = GetContext();
            {
                using (IDbConnection dbConnection = dbContext.Database.Connection)
                {
	                if (dbConnection.State == ConnectionState.Closed)
	                {
						dbContext.Database.Connection.Open(); 
	                }
                    var tableDef = GetTableDef();
	                var query = tableDef.GetCount();

                    if (!queryCriteria.JoinCriteria.IsEmpty())
                    {
						queryCriteria.JoinCriteria.ForEach(item => query = query.Join(item.Item1, item.Item2, item.Item3));
                    }
                    query = WhereBuilder(query, queryCriteria);
                    var command = Provider.GetCommand(query);
                    command.Connection = dbConnection;
					result = (Int32)command.ExecuteScalar();
                }
            }
            return result;
        }

        public ExpandoObject Create(ListMetadata listMetadata, Guid? parentId = null)
        {
            var result = new ExpandoObject();
            var listMetadataFields = listMetadata.ListMetadataFields.ToList();
            var valuesCollection = ((IDictionary<string,object>) result);
			listMetadataFields.ForEach(item => { valuesCollection[item.InternalName] = null; });
			var tableDef = GetTableDef();
            valuesCollection[tableDef.PrimaryField] = Guid.NewGuid();
			valuesCollection[tableDef.ParentField] = parentId;
            return result;
        }

        public void CreateRow(ExpandoObject item)
        {
            var itemValues = ((IDictionary<string, object>)item);
            var tableDef = GetTableDef();
            var dbContext = GetContext();
            {
                using (IDbConnection dbConnection = dbContext.Database.Connection)
                {
                    if (dbConnection.State == ConnectionState.Closed)
                    {
                        dbContext.Database.Connection.Open();
                    }
                    var insertClause = new List<string>();
                    var valuesClause = new List<string>();
                    foreach (var keyValue in itemValues)
                    {
                        insertClause.Add(string.Format("[{0}]", keyValue.Key));
                        valuesClause.Add(string.Format("@{0}", keyValue.Key));   
                    }
                    var sqlCommand = string.Format(SqlTemplate.Row_Insert, tableDef.Name,
                        String.Join(", ", insertClause.ToArray()), String.Join(", ", valuesClause.ToArray()));
                    var command = new SqlCommand(sqlCommand, dbConnection as SqlConnection)
                    {
                        CommandType = CommandType.Text
                    };
                    command.Parameters.AddRange(itemValues.Select(keyValue => 
                        new SqlParameter(string.Format("@{0}", keyValue.Key), keyValue.Value ?? DBNull.Value)).ToArray());
                    command.ExecuteNonQuery();
                }
            }

        }

        public int CreateRowAndReturnID(ExpandoObject item)
        {
            int newRowId = 0;
            var itemValues = ((IDictionary<string, object>)item);
            var tableDef = GetTableDef();
            var dbContext = GetContext();
            {
                using (IDbConnection dbConnection = dbContext.Database.Connection)
                {
                    if (dbConnection.State == ConnectionState.Closed) {
                        dbContext.Database.Connection.Open();
                    }
                    var insertClause = new List<string>();
                    var valuesClause = new List<string>();
                    foreach (var keyValue in itemValues) {
                        insertClause.Add(string.Format("[{0}]", keyValue.Key));
                        valuesClause.Add(string.Format("@{0}", keyValue.Key));
                    }
                    string insertCmdTemplate = SqlTemplate.Row_Insert.Replace("VALUES", "OUTPUT INSERTED.Id VALUES");
                    var sqlCommand = string.Format(insertCmdTemplate, tableDef.Name,
                        String.Join(", ", insertClause.ToArray()), String.Join(", ", valuesClause.ToArray()));
                    var command = new SqlCommand(sqlCommand, dbConnection as SqlConnection) {
                        CommandType = CommandType.Text
                    };
                    command.Parameters.AddRange(itemValues.Select(keyValue =>
                        new SqlParameter(string.Format("@{0}", keyValue.Key), keyValue.Value ?? DBNull.Value)).ToArray());
                    newRowId = (Int32)command.ExecuteScalar();
                }
            }
            return newRowId;
        }

        public void UpdateRow(ExpandoObject item)
        {
            var itemValues = ((IDictionary<string, object>)item);
            var tableDef = GetTableDef();
            var dbContext = GetContext();
            {
                using (IDbConnection dbConnection = dbContext.Database.Connection)
                {
                    if (dbConnection.State == ConnectionState.Closed)
                    {
                        dbContext.Database.Connection.Open();
                    }
                    var updateClause = new List<string>();
                    var whereClause = string.Empty;
                    foreach (var keyValue in itemValues)
                    {
                        if (string.Equals(keyValue.Key,tableDef.PrimaryField, StringComparison.InvariantCultureIgnoreCase))
                        {
                            whereClause = string.Format("[{0}]=@{1}", keyValue.Key, keyValue.Key);
                        }
                        else
                        {
                            updateClause.Add(string.Format("[{0}]=@{1}", keyValue.Key, keyValue.Key));
                        }
                    }
                    var sqlCommand = string.Format(SqlTemplate.Row_Update, tableDef.Name, 
                                            String.Join(", ", updateClause.ToArray()), whereClause);
                    var command = new SqlCommand(sqlCommand, dbConnection as SqlConnection)
                    {
                        CommandType = CommandType.Text
                    };
                    command.Parameters.AddRange(itemValues.Select(keyValue =>
                            new SqlParameter(string.Format("@{0}", keyValue.Key), keyValue.Value ?? DBNull.Value)).ToArray());
                    command.ExecuteNonQuery();
                }
            }

        }

        public void DeleteRow(object key)
        {
            var tableDef = GetTableDef();
            var dbContext = GetContext();
            {
                using (IDbConnection dbConnection = dbContext.Database.Connection)
                {
                    if (dbConnection.State == ConnectionState.Closed)
                    {
                        dbContext.Database.Connection.Open();
                    }
                    var whereClause = string.Format("[{0}]={1}", tableDef.PrimaryField, key);
                    var sqlCommand = string.Format(SqlTemplate.Row_Delete, tableDef.Name, whereClause);
                    var command = new SqlCommand(sqlCommand, dbConnection as SqlConnection)
                    {
                        CommandType = CommandType.Text
                    };
                    command.Parameters.Add(new SqlParameter(string.Format("@{0}", tableDef.PrimaryField), key ?? DBNull.Value));
                    command.ExecuteNonQuery();
                }
            }
        }
        
        public void Save(ListMetadata listMetadata, ExpandoObject item)
        {
            var itemValues = ((IDictionary<string,object>) item);
	        var systemFields = SystemFields();
            var listMetadataFields = listMetadata.ListMetadataFields.ToList();
            var tableDef = GetTableDef();
            string sqlCommand;
	        var sqlParamList = new List<SqlParameter>();
			listMetadataFields.ForEach(field =>
			{
                if (systemFields.Any(fieldDef => fieldDef == field.InternalName)) return;
				switch (field.SqlDbType)
				{
                    case SqlDbType.DateTime:
                        itemValues[field.InternalName] = itemValues[field.InternalName].ToNullableDateTime();
                        break;
				    case SqlDbType.UniqueIdentifier:
				        itemValues[field.InternalName] = itemValues[field.InternalName].ToNullableGuid();
				        break;
				    case SqlDbType.Int:
				        itemValues[field.InternalName] = itemValues[field.InternalName].ToNullableInt();
				        break;
				}
				var parameter = new SqlParameter(string.Format("@{0}", field.InternalName), field.SqlDbType)
				{
					Value = itemValues[field.InternalName] ?? DBNull.Value,
				};
				sqlParamList.Add(parameter);
			});
			systemFields.ForEach(field =>
			{
				if (itemValues.All(kvp => kvp.Key != field)) return;
			    var systemFieldValue = itemValues[field].ToNullableGuid();
				var parameter = new SqlParameter(string.Format("@{0}", field), SqlDbType.UniqueIdentifier)
				{
                    Value = systemFieldValue.HasValue ? (object)systemFieldValue.Value : DBNull.Value,
				};
				sqlParamList.Add(parameter);
			});
			if (IsExists((Guid)itemValues[tableDef.PrimaryField]))
            {
                var updateClause = new List<string>();
                var whereClause = string.Empty;
				systemFields.ForEach(field =>
				{
					if (itemValues.All(kvp => kvp.Key != field)) return;
					if (field == tableDef.PrimaryField)
					{
						whereClause = string.Format("[{0}]=@{1}", field, field);
					}
					else
					{
						updateClause.Add(string.Format("[{0}]=@{1}", field, field));
					}
				});
                listMetadataFields.ForEach(field =>
                {
                    if (systemFields.Any(fieldDef => fieldDef == field.InternalName)) return;
                    if (field.InternalName == tableDef.PrimaryField)
					{
						whereClause = string.Format("[{0}]=@{1}", field.InternalName, field.InternalName);
					}
					else
					{
						updateClause.Add(string.Format("[{0}]=@{1}", field.InternalName, field.InternalName));
					}
                });
				sqlCommand = string.Format(SqlTemplate.Row_Update, tableDef.Name, 
					String.Join(", ", updateClause.ToArray()), whereClause);
            }
            else
            {
                var insertClause = new List<string>();
                var valuesClause = new List<string>();
				systemFields.ForEach(field =>
				{
					if (itemValues.All(kvp => kvp.Key != field)) return;
					insertClause.Add(string.Format("[{0}]", field));
					valuesClause.Add(string.Format("@{0}", field));
				});

                listMetadataFields.ForEach(field =>
                {
                    if (systemFields.Any(fieldDef => fieldDef == field.InternalName)) return;
                    insertClause.Add(string.Format("[{0}]",field.InternalName));
                    valuesClause.Add(string.Format("@{0}", field.InternalName));
                });
				sqlCommand = string.Format(SqlTemplate.Row_Insert, tableDef.Name, 
					String.Join(", ", insertClause.ToArray()), String.Join(", ", valuesClause.ToArray()));
            };
			var dbContext = GetContext();
            {
                using (IDbConnection dbConnection = dbContext.Database.Connection)
                {
					if (dbConnection.State == ConnectionState.Closed)
					{
						dbContext.Database.Connection.Open();
					}
                    var command = new SqlCommand(sqlCommand, dbConnection as SqlConnection)
                    {
                        CommandType = CommandType.Text
                    };
					command.Parameters.AddRange(sqlParamList.ToArray());
                    command.ExecuteNonQuery();
                }
            }
        }

        #endregion

        #region Field Operations

        public virtual bool IsFieldExists(ListMetadataField field)
        {
            var dbContext = GetContext();
            {
                using (IDbConnection dbConnection = dbContext.Database.Connection)
                {
                    if (dbConnection.State == ConnectionState.Closed)
                    {
                        dbConnection.Open();
                    }
                    var sqlTable = GetTableDef();
                    var sqlRaw = string.Format(SqlTemplate.Field_Exists, sqlTable.Name, field.InternalName);
                    return dbContext.Database.SqlQuery<int>(sqlRaw).First() > 0;
                }
            }
        }

        public virtual void AddField(ListMetadataField field)
		{
            if (IsFieldExists(field)) return;
            var context = GetContext();
		    {
		        var sqlTable = GetTableDef();
		        var sqlRaw = string.Format(SqlTemplate.Field_Create, sqlTable.Name, field.InternalName, field.FieldType);
				context.Database.ExecuteSqlCommand(sqlRaw);
		        if (field.SqlDbType == SqlDbType.Xml)
		        {
		            var sqlPrimaryIndexRaw = string.Format(SqlTemplate.XmlPrimaryIndex_Create, sqlTable.Name,field.InternalName);
		            context.Database.ExecuteSqlCommand(sqlPrimaryIndexRaw);
		            var sqlPropertyIndexRaw = string.Format(SqlTemplate.XmlPropertyIndex_Create, sqlTable.Name,field.InternalName);
		            context.Database.ExecuteSqlCommand(sqlPropertyIndexRaw);
		        }
		        else
		        {
                    if (field.SqlDbType.In(SqlDbType.Xml, SqlDbType.VarBinary, SqlDbType.Text,
		                SqlDbType.Binary, SqlDbType.Image) || field.FieldLenght > 200 || 
                        field.FieldType.Trim().ToLowerInvariant() == "nvarchar(max)") return;
		            var sqlIndexRaw = string.Format(SqlTemplate.Index_Create, sqlTable.Name, field.InternalName);
		            context.Database.ExecuteSqlCommand(sqlIndexRaw);
		        }
		    }
		}

        public virtual void DeleteField(ListMetadataField field)
        {
            if (!IsFieldExists(field)) return;
            var context = GetContext();
            {
                var sqlTable = GetTableDef();
                var sqlIndexRaw = string.Format(SqlTemplate.Index_Delete, sqlTable.Name, field.InternalName);
                context.Database.ExecuteSqlCommand(sqlIndexRaw);
                sqlIndexRaw = string.Format(SqlTemplate.XmlPrimaryIndex_Drop, sqlTable.Name, field.InternalName);
                context.Database.ExecuteSqlCommand(sqlIndexRaw);
                var sqlRaw = string.Format(SqlTemplate.Field_Delete, sqlTable.Name, field.InternalName);
                context.Database.ExecuteSqlCommand(sqlRaw);
            }
        }

        #endregion
    }
}