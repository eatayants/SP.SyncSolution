#region

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.SharePoint;
using Roster.BL.Providers;
using Roster.Common;
using Roster.Common.Collections;
using Roster.Common.Helpers;
using Roster.Model.DataContext;
using Roster.Model.DataContext.Partail;

#endregion

namespace Roster.BL
{
	public class RosterConfigService
    {
        #region Private variables

        private RosterEventPropertyProvider _rosterEventPropertyProvider;
        private ListMetadataFieldProvider _listMetadataFieldProvider;
        private ContentTypeProvider _contentTypeProvider;
        private ListMetadataProvider _listMetadataProvider;
        private ViewMetadataProvider _viewMetadataProvider;
        private ListMappingProvider _listMappingProvider;
        private DefinitionProvider _definitionProvider;
        private SystemOptionProvider _systemOptionProvider;
        private AccessControlListProvider _accessControlListProvider;
        private AccessControlItemProvider _accessControlItemProvider;

        #endregion

        public RosterConfigService()
        {
            InitProviders(SPContext.Current.GetConnection());
        }
        public RosterConfigService(SPWeb web)
        {
            InitProviders(web.GetConnection());
        }

        public RosterConfigService(ConnectionInfo connectionInfo)
        {
            InitProviders(connectionInfo);
        }

        private void InitProviders(ConnectionInfo connectionInfo = null)
	    {
            if (connectionInfo == null)
	        {
                throw new ArgumentException("connectionInfo");
            }
            _listMetadataProvider = new ListMetadataProvider(connectionInfo.ToString());
            _listMetadataFieldProvider = new ListMetadataFieldProvider(connectionInfo.ToString());
            _viewMetadataProvider = new ViewMetadataProvider(connectionInfo.ToString());
            _rosterEventPropertyProvider = new RosterEventPropertyProvider(connectionInfo.ToString());
            _listMappingProvider = new ListMappingProvider(connectionInfo.ToString());
            _definitionProvider = new DefinitionProvider(connectionInfo.ToString());
            _systemOptionProvider = new SystemOptionProvider(connectionInfo.ToString());
            _contentTypeProvider = new ContentTypeProvider(connectionInfo.ToString());
            _accessControlListProvider = new AccessControlListProvider(connectionInfo.ToString());
            _accessControlItemProvider = new AccessControlItemProvider(connectionInfo.ToString()); 
        }

        #region access control

        public AccessControlItem GetAccessControlItem(Guid id)
        {
            return _accessControlItemProvider.GetById(id);
        }

        public void DeleteAccessControlItem(Guid id)
        {
            _accessControlItemProvider.Delete(id);
        }

        public List<AccessControlItem> GetItemAccessControls(Guid itemId)
        {
            var criteria = new SelectCriteria<AccessControlItem>
            {
                //PageSize = Int32.MaxValue,
                ExpressionSelect = new List<Expression<Func<AccessControlItem, bool>>> { item => item.ItemId == itemId }
            };
            return _accessControlItemProvider.List(criteria);
        }

        public void SaveAccessControlItem(AccessControlItem item)
        {
            if (_accessControlItemProvider.IsExist(item.Id))
            {
                _accessControlItemProvider.Update(item);
            }
            else
            {
                _accessControlItemProvider.Add(item);
            }
        }

        public AccessControlList GetAccessControlList(Guid id)
        {
            return _accessControlListProvider.GetById(id);
        }

        public void DeleteAccessControlList(Guid id)
        {
            _accessControlListProvider.Delete(id);
        }

        public List<AccessControlList> GetListAccessControls(Guid listId)
        {
            var criteria = new SelectCriteria<AccessControlList>
            {
                //PageSize = Int32.MaxValue,
                ExpressionSelect = new List<Expression<Func<AccessControlList, bool>>> { item => item.ListMetadataId == listId}
            };
            return _accessControlListProvider.List(criteria);
        }

        public void SaveAccessControlList(AccessControlList item)
        {
            if (_accessControlListProvider.IsExist(item.Id))
            {
                _accessControlListProvider.Update(item);
            }
            else
            {
                _accessControlListProvider.Add(item);
            }
        }
        
        public bool HasRights(Guid itemId, Guid listId, ICollection<int> trusteeIds, AccessRight accessRights)
        {
            if (trusteeIds.IsEmpty())
            {
                trusteeIds = _accessControlItemProvider.GetTrusteeIds();
            } 
            var result = _accessControlItemProvider.HasRights(itemId, trusteeIds, accessRights);
            if (!result.HasValue)
            {
                result = _accessControlListProvider.HasRights(listId, trusteeIds, accessRights);
            }
            return result.Value;
        }


        /// <summary>
        /// Check for rights of a Current User
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="listId"></param>
        /// <param name="accessRights"></param>
        /// <returns></returns>
        public bool HasRights(Guid itemId, Guid listId, AccessRight accessRights)
        {
            if (SPContext.Current.Web.UserIsSiteAdmin)
            {
                return true; // admins always has FullControl               
            }
            return HasRights(itemId, listId, null, accessRights);
        }

        #endregion

        #region settings
        public bool SqlAgentRunned()
        {
            return _systemOptionProvider.SqlAgentStatus()==4;
        }

        public List<SqlAgentJob> SqlAgentJobs()
        {
            return _systemOptionProvider.SqlAgentJobs();
        }

        public int PlannedDaysAhead()
        {
            return _systemOptionProvider.PlannedPublishingDaysAhead;
        }

        public int PlannedDaysAhead(int daysAhead)
        {
            return _systemOptionProvider.PlannedPublishingDaysAhead = daysAhead;
        }

        #endregion Settings

        #region content type

        public List<ListMetadataContentType> GetContentTypes(Guid listId)
        {
            return _contentTypeProvider.GetContentTypes(listId);
        }

        public ListMetadataContentType GetContentType(int id)
        {
            return _contentTypeProvider.GetById(id);
        }

        public ListMetadataContentType CreateContentType(Guid listId)
        {
            return _contentTypeProvider.Create(listId);
        }

        public void SaveContentType(ListMetadataContentType listMetadataContentType)
        {
            if (_contentTypeProvider.IsExist(listMetadataContentType.Id))
            {
                _contentTypeProvider.Update(listMetadataContentType);
            }
            else
            {
                _contentTypeProvider.Add(listMetadataContentType);
            }
        }

        public void RemoveContentType(int id)
        {
            _contentTypeProvider.Delete(id);
        }

        #endregion content type

        #region mapping

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

        #endregion mapping

        #region list

        public List<ListMetadata> GetLists()
        {
            return _listMetadataProvider.List();
        }

        public ListMetadata GetList(Guid listId)
        {
            return _listMetadataProvider.GetById(listId);
        }

        public void SaveList(ListMetadata listMetadata)
        {
            if (_listMetadataProvider.IsExist(listMetadata.Id))
            {
                _listMetadataProvider.Update(listMetadata);
            }
            else
            {
                _listMetadataProvider.Add(listMetadata);
                var listFields = listMetadata.ListMetadataFields.ToList();
                listFields.ForEach(item =>
                {
                    _rosterEventPropertyProvider.AddField(item);
                });
            }
        }

        public void RemoveList(Guid listId)
        {
            var list = GetList(listId);
            _listMetadataProvider.Delete(listId);
            var listFields = list.ListMetadataFields.ToList();
            listFields.ForEach(item =>
            {
                _rosterEventPropertyProvider.DeleteField(item);
            });
        }


        #endregion

        #region view

        public List<ViewMetadata> GetViews(Guid listId)
        {
            var criteria = new SelectCriteria<ViewMetadata>
                {ExpressionSelect = new List<Expression<Func<ViewMetadata, bool>>> 
                { item => item.ListMetadataId == listId}};
            return _viewMetadataProvider.List(criteria);
        }

        public ViewMetadata GetView(Guid viewId)
        {
            return _viewMetadataProvider.GetById(viewId);
        }

        public void SaveView(ViewMetadata viewMetadata)
        {
            if (_viewMetadataProvider.IsExist(viewMetadata.Id))
            {
                _viewMetadataProvider.Update(viewMetadata);
            }
            else
            {
                _viewMetadataProvider.Add(viewMetadata);
            }
        }

        public void RemoveView(Guid viewId)
        {
            _viewMetadataProvider.Delete(viewId);
        }

        #endregion view
        
        #region field

        public ListMetadataField GetField(Guid fieldId)
        {
            return _listMetadataFieldProvider.GetById(fieldId);
        }

        public void SaveField(ListMetadataField field)
        {
            if (_listMetadataFieldProvider.IsExist(field.Id))
            {
                _listMetadataFieldProvider.Update(field);    
            }
            else
            {
                _listMetadataFieldProvider.Add(field);
                if (!_rosterEventPropertyProvider.IsFieldExists(field))
                {
                    _rosterEventPropertyProvider.AddField(field);
                }
            }
        }

        public void RemoveField(Guid fieldId)
        {
            var refNames = _listMetadataFieldProvider.GetFieldRefrerence(fieldId);
            if (string.IsNullOrWhiteSpace(refNames))
            {
                var field = _listMetadataFieldProvider.GetById(fieldId);
                if (field == null) return;
                _listMetadataFieldProvider.Delete(fieldId);
                _rosterEventPropertyProvider.DeleteField(field);
            }
            else
            {
                throw new Exception(string.Format("Can't delete this field. It has references:{0}", refNames));
            }
        }

        #endregion field

        #region database table

        public List<string> DatabaseTables()
        {
            return _definitionProvider.ListTables();
        }

        public List<string> TablesKeyFields(string table)
        {
            return _definitionProvider.ListKeyFields(table);
        }
        public List<string> TablesFields(string table)
        {
            return _definitionProvider.ListFields(table);
        }

        public void SaveDataSource(string datasource, string statements)
        {
            _definitionProvider.SaveDataSource(datasource, statements);
        }

        public string ReadDataSource(string datasource)
        {
            return _definitionProvider.ReadDataSource(datasource);
        }

        #endregion DatabaseTable
    }
}