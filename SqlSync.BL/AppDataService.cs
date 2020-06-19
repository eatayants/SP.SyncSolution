using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.SharePoint;
using SqlSync.BL.Extentions;
using SqlDynamic.Queries;
using SqlSync.BL.Providers;
using SqlSync.Common;
using SqlSync.Common.Collections;
using SqlSync.Model.DataContext;
using SqlSync.Model.DataContext.Extention;
using Expression = SqlDynamic.Queries.Expression;

namespace SqlSync.BL
{
	public class AppDataService
	{
        private ConnectionInfo _connectionInfo;
        private SystemOptionProvider _systemOptionProvider;
        private void InitProviders(ConnectionInfo connectionInfo)
	    {
            if (connectionInfo == null)
            {
                throw new ArgumentException("connectionInfo");
            }
	        _connectionInfo = connectionInfo;
            _systemOptionProvider = new SystemOptionProvider(connectionInfo.ToString());
        }

        public AppDataService()
        {
           InitProviders(SPContext.Current.GetConnection());
        }

        public AppDataService(SPWeb web)
        {
            InitProviders(web.GetConnection());
        }

        public string ExecuteProcedure(string name, ICollection<Tuple<string,object>> paramerters)
        {
            return _systemOptionProvider.ExecuteProcedure(name, paramerters);
        }

        public List<Tuple<int, ExpandoObject>> TableContent(string table, string key, string fields, List<Tuple<string, string>> whereCriteria = null)
        {
            return _systemOptionProvider.ListTableContent(table, key, fields, whereCriteria);
        }

        public void SaveRows(ListMapping mapping, List<Tuple<int, ExpandoObject>> values)
        {
            using (var providerSqlDynamic = new ProviderSqlDynamic(
                new TableDynamic {PrimaryField = mapping.Key, Name = mapping.TableName}, _connectionInfo.ToString()))
            {
                values.ForEach(item =>
                {
                    SyncRow(providerSqlDynamic, item, mapping);
                });
            }
        }

        public void SaveRow(ListMapping mapping, Tuple<int, ExpandoObject> value)
        {
            using (var providerSqlDynamic = new ProviderSqlDynamic(
                new TableDynamic{PrimaryField = mapping.Key, Name = mapping.TableName}, _connectionInfo.ToString()))
            {
                SyncRow(providerSqlDynamic, value, mapping);
            }
        }

        public int SaveRow(string key, string tableName, ExpandoObject value)
        {
            // !!! Identity increment = 1

            int newRowId = 0;
            using (var providerSqlDynamic = new ProviderSqlDynamic(
                new TableDynamic { PrimaryField = key, Name = tableName }, _connectionInfo.ToString()))
            {
                var valueData = (IDictionary<string, object>)value;
                newRowId = providerSqlDynamic.CreateRowAndReturnID((ExpandoObject)valueData);
            }
            return newRowId;
        }

        private void SyncRow(ProviderSqlDynamic provider, Tuple<int, ExpandoObject> value, ListMapping mapping)
	    {
            var updateData = (IDictionary<string,object>)new ExpandoObject();
            var valueData = (IDictionary<string,object>)value.Item2;
            updateData.Add(mapping.Key, value.Item1);
            mapping.ListMappingFields.ToList().ForEach(item =>
            {
                updateData.Add(item.FieldName, valueData[item.ItemName]);
            });

            if (provider.IsExists(value.Item1))
            {
                provider.UpdateRow((ExpandoObject)updateData);
            }
            else
            {
                provider.CreateRow((ExpandoObject)updateData);
            }
	    }

        public void Delete(string tableName, string primaryField, object key)
        {
            using (var providerSqlDynamic = new ProviderSqlDynamic(new TableDynamic
            {
                PrimaryField = primaryField,
                Name = tableName
            }, _connectionInfo.ToString()))
            {
                providerSqlDynamic.DeleteRow(key);
            }
        }
    }
}