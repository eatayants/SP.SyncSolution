#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using SqlSync.Common;
using SqlSync.Model.DataContext;
using SqlSync.Model.DataContext.Extention;
using SqlDynamic.Providers;
using SqlDynamic.Queries;

#endregion

namespace SqlSync.BL.Providers
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

        private SqlSyncEntities _dataContextInstance;
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

        #endregion
    }
}