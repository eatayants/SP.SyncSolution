#region

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.SharePoint;
using SqlSync.BL.Providers;
using SqlSync.Common;
using SqlSync.Common.Collections;
using SqlSync.Common.Helpers;
using SqlSync.Model.DataContext;

#endregion

namespace SqlSync.BL
{
	public class AppConfigService
    {
        #region Private variables
        private ListMappingProvider _listMappingProvider;
        private SystemOptionProvider _systemOptionProvider;

        #endregion

        public AppConfigService()
        {
            InitProviders(SPContext.Current.GetConnection());
        }
        public AppConfigService(SPWeb web)
        {
            InitProviders(web.GetConnection());
        }

        public AppConfigService(ConnectionInfo connectionInfo)
        {
            InitProviders(connectionInfo);
        }

        private void InitProviders(ConnectionInfo connectionInfo = null)
	    {
            if (connectionInfo == null)
	        {
                throw new ArgumentException("connectionInfo");
            }
            _listMappingProvider = new ListMappingProvider(connectionInfo.ToString());
            _systemOptionProvider = new SystemOptionProvider(connectionInfo.ToString());
        }

        public List<ListMapping> GetMapping()
	    {
            return _listMappingProvider.List();
	    }

        public ListMapping GetMapping(Guid mappingId)
        {
            return _listMappingProvider.GetById(mappingId);
        }

        public ListMapping GetMappingByList(string list)
        {
            var criteria = new SelectCriteria<ListMapping> 
            { ExpressionSelect = new List<Expression<Func<ListMapping, bool>>>
                {
                    item => item.ListName == list.ToString()
                }
            };
            return _listMappingProvider.List(criteria).FirstOrDefault();
        }

        public void SaveMapping(ListMapping mapping)
        {
            if (_listMappingProvider.IsExist(mapping.Id))
            {
                _listMappingProvider.Update(mapping);
            }
            else
            {
                _listMappingProvider.Add(mapping);
            }
        }

        public void RemoveMapping(Guid mappingId)
        {
            _listMappingProvider.Delete(mappingId);
        }
        
        public List<string> DatabaseTables()
        {
            return _systemOptionProvider.ListTables();
        }

        public List<string> TablesKeyFields(string table)
        {
            return _systemOptionProvider.ListKeyFields(table);
        }
        public List<string> TablesFields(string table)
        {
            return _systemOptionProvider.ListFields(table);
        }

        public void SaveDataSource(string datasource, string statements)
        {
            _systemOptionProvider.SaveDataSource(datasource, statements);
        }

        public string ReadDataSource(string datasource)
        {
            return _systemOptionProvider.ReadDataSource(datasource);
        }
    }
}