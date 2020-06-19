using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.SharePoint;
using Roster.BL.Extentions;
using SqlDynamic.Queries;
using Roster.BL.Providers;
using Roster.Common;
using Roster.Common.Collections;
using Roster.Model.DataContext;
using Roster.Model.DataContext.Extention;
using Expression = SqlDynamic.Queries.Expression;
using QueryCriteria = Roster.BL.Extentions.QueryCriteria;

namespace Roster.BL
{
	public class RosterDataService
	{
        private ViewMetadataProvider _viewMetadataProvider;
        private ListMetadataProvider _listMetadataProvider;
        private RosterEventProvider _rosterEventProvider;
        private HolidayEventProvider _holydayProvider;
        private EventStatusProvider _eventStatusProvider;
        private EventStatusPropertyProvider _eventStatusPropertyProvider;
        private RosterEventPropertyProvider _rosterEventPropertyProvider;
        private MasterRosterProvider _masterRosterProvider;
        private DefinitionProvider _definitionProvider;
        private ConnectionInfo _connectionInfo;
        private RosterEventLockProvider _rosterEventLockProvider;
        private AccessControlListProvider _accessControlListProvider;
        private TrackingDataViewProvider _trackingDataProvider;

	    private void InitProviders(ConnectionInfo connectionInfo)
	    {
            if (connectionInfo == null)
            {
                throw new ArgumentException("connectionInfo");
            }
	        _connectionInfo = connectionInfo;
            _listMetadataProvider = new ListMetadataProvider(connectionInfo.ToString());
            _viewMetadataProvider = new ViewMetadataProvider(connectionInfo.ToString());
            _eventStatusProvider = new EventStatusProvider(connectionInfo.ToString());
            _eventStatusPropertyProvider = new EventStatusPropertyProvider(connectionInfo.ToString());
            _rosterEventProvider = new RosterEventProvider(connectionInfo.ToString());
            _rosterEventPropertyProvider = new RosterEventPropertyProvider(connectionInfo.ToString());
            _definitionProvider = new DefinitionProvider(connectionInfo.ToString());
            _holydayProvider = new HolidayEventProvider(connectionInfo.ToString());
            _masterRosterProvider = new MasterRosterProvider(connectionInfo.ToString());
            _rosterEventLockProvider = new RosterEventLockProvider(connectionInfo.ToString());
            _accessControlListProvider = new AccessControlListProvider(connectionInfo.ToString());
            _trackingDataProvider = new TrackingDataViewProvider(connectionInfo.ToString());
	    }

        public RosterDataService()
        {
           InitProviders(SPContext.Current.GetConnection());
        }

        public RosterDataService(SPWeb web)
        {
            InitProviders(web.GetConnection());
        }

        #region Private methods

	    private bool IncludeAccessCiteria(Guid listId)
	    {
            if (SPContext.Current.Web.UserIsSiteAdmin)
            {
                return false; // admins always has FullControl               
            }
            return !_accessControlListProvider.HasRights(listId, AccessRight.Read);
	    }

        private QueryCriteria QueryCriteriaByView(ViewMetadata view)
        {
            var queryCriteria = new QueryCriteria {AccessCriteria = IncludeAccessCiteria(view.ListMetadataId)};
            var tableDef = _rosterEventPropertyProvider.GetTableDef();
	        if (!string.IsNullOrWhiteSpace(view.ListMetadata.DataSource))
	        {
	            tableDef.ParentName = view.ListMetadata.DataSource;
	        }
			queryCriteria.TakeRows = queryCriteria.TakeRows == 0? view.ItemLimit : queryCriteria.TakeRows;
			queryCriteria.ListMetadataFields.AddRange(view.ListMetadata.ListMetadataFields);
			queryCriteria.SelectCriteria.AddRange(_rosterEventPropertyProvider.GetDefaultFields());
			queryCriteria.JoinCriteria.Add(new Tuple<JoinType, TableSource, IBoolean>(JoinType.Inner,tableDef.ParentTableSource(),
										Expression.Eq(tableDef.GetParentField(), tableDef.GetParentPrimariField())));
            var viewMetadataFields = view.ViewMetadataFields.ToList();
			viewMetadataFields.ForEach(item =>
            {
                var fieldName = item.ListMetadataField.InternalName;
                if (!queryCriteria.SelectCriteria.Any(field => (field as Field) != null && ((Field) field).Name == fieldName))
                {
                    queryCriteria.SelectCriteria.Add(tableDef.TableSource().Field(fieldName));
                }
	            if ((item.ListMetadataField.DataSourceType.In((int)LookupSourceType.Table,(int)LookupSourceType.Query)))
	            {
		            var table = new Table(item.ListMetadataField.DataSource);
                    item.ListMetadataField.DataSourceField.Split('$').ToList().ForEach(itemField =>
                    {
                        queryCriteria.SelectCriteria.Add(table.Field(itemField));
                    });
					queryCriteria.JoinCriteria.Add(new Tuple<JoinType, TableSource, IBoolean>(JoinType.LeftOuter,new TableSource(table),
                                        Expression.Eq(tableDef.GetTable().Field(item.ListMetadataField.InternalName), 
                                        table.Field(item.ListMetadataField.DataSourceKey))));
	            }
                if (item.OrderCriteria > 0)
                {
                    var sortPart = item.ListMetadataField.InternalName;
                    if ((item.ListMetadataField.DataSourceType == (int) LookupSourceType.Table) ||
                        (item.ListMetadataField.DataSourceType == (int) LookupSourceType.Query))
                    {
                        var dataSourceFields = item.ListMetadataField.DataSourceField.Split('$').ToList();
                        sortPart = dataSourceFields.IsEmpty() ? string.Empty : dataSourceFields[0];
                    }
					queryCriteria.OrderCriteria.Add(new Tuple<ListMetadataField, SortDirection, string>
                        (item.ListMetadataField, (SortDirection)item.OrderCriteria,sortPart));
                }
            });

            var viewMetadataWhereCriteries = view.ViewMetadataWhereCriteries.ToList();
            viewMetadataWhereCriteries.ForEach(item =>
            {
				queryCriteria.WhereCriteria.Add(new Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>(
					item.ListMetadataField, (CompareType)item.CompareType, (ConcateOperator)item.ConcateOperator, item.Value, null));
            });

	        return queryCriteria;
	    }

        private QueryCriteria QueryCriteriaByList(ListMetadata list)
        {
            var queryCriteria = new QueryCriteria { AccessCriteria = IncludeAccessCiteria(list.Id) };
            var tableDef = _rosterEventPropertyProvider.GetTableDef();
	        if (!string.IsNullOrWhiteSpace(list.DataSource))
	        {
                tableDef.ParentName = list.DataSource;
	        }
            queryCriteria.ListMetadataFields.AddRange(list.ListMetadataFields);
			queryCriteria.SelectCriteria.AddRange(_rosterEventPropertyProvider.GetDefaultFields());
			queryCriteria.JoinCriteria.Add(new Tuple<JoinType, TableSource, IBoolean>(JoinType.Inner,tableDef.ParentTableSource(),
										Expression.Eq(tableDef.GetParentField(), tableDef.GetParentPrimariField())));
			queryCriteria.ListMetadataFields.ForEach(item =>
            {
                var fieldName = item.InternalName;
                queryCriteria.SelectCriteria.Add(tableDef.TableSource().Field(fieldName));
	            if ((item.DataSourceType == (int)LookupSourceType.Table)||
                (item.DataSourceType == (int)LookupSourceType.Query))
	            {
		            var table = new Table(item.DataSource);
                    item.DataSourceField.Split('$').ToList().ForEach(itemField =>
                    {
                        queryCriteria.SelectCriteria.Add(table.Field(itemField));
                    });
					queryCriteria.JoinCriteria.Add(new Tuple<JoinType, TableSource, IBoolean>(JoinType.LeftOuter,new TableSource(table),
                                        Expression.Eq(tableDef.GetTable().Field(item.InternalName), table.Field(item.DataSourceKey))));	            }
            });
	        return queryCriteria;
	    }

        private QueryCriteria QueryCriteriaByView_Status(ViewMetadata view)
        {
            var queryCriteria = new QueryCriteria();
            var tableDef = _eventStatusPropertyProvider.GetTableDef();
            if (!string.IsNullOrWhiteSpace(view.ListMetadata.DataSource))
            {
                tableDef.ParentName = view.ListMetadata.DataSource;
            }
            queryCriteria.TakeRows = queryCriteria.TakeRows == 0 ? view.ItemLimit : queryCriteria.TakeRows;
            queryCriteria.ListMetadataFields.AddRange(view.ListMetadata.ListMetadataFields);
            queryCriteria.SelectCriteria.AddRange(_eventStatusPropertyProvider.GetDefaultFields());
            queryCriteria.JoinCriteria.Add(new Tuple<JoinType, TableSource, IBoolean>(JoinType.Inner, tableDef.ParentTableSource(),
                                        Expression.Eq(tableDef.GetParentField(), tableDef.GetParentPrimariField())));
            var viewMetadataFields = view.ViewMetadataFields.ToList();
            viewMetadataFields.ForEach(item =>
            {
                var fieldName = item.ListMetadataField.InternalName;
                queryCriteria.SelectCriteria.Add(tableDef.TableSource().Field(fieldName));
                if ((item.ListMetadataField.DataSourceType.In((int)LookupSourceType.Table, (int)LookupSourceType.Query)))
                {
                    var table = new Table(item.ListMetadataField.DataSource);
                    item.ListMetadataField.DataSourceField.Split('$').ToList().ForEach(itemField =>
                    {
                        queryCriteria.SelectCriteria.Add(table.Field(itemField));
                    });
                    queryCriteria.JoinCriteria.Add(new Tuple<JoinType, TableSource, IBoolean>(JoinType.LeftOuter, new TableSource(table),
                                        Expression.Eq(tableDef.GetTable().Field(item.ListMetadataField.InternalName),
                                        table.Field(item.ListMetadataField.DataSourceKey))));
                }
                if (item.OrderCriteria > 0)
                {
                    var sortPart = item.ListMetadataField.InternalName;
                    if ((item.ListMetadataField.DataSourceType == (int)LookupSourceType.Table) ||
                        (item.ListMetadataField.DataSourceType == (int)LookupSourceType.Query))
                    {
                        var dataSourceFields = item.ListMetadataField.DataSourceField.Split('$').ToList();
                        sortPart = dataSourceFields.IsEmpty() ? string.Empty : dataSourceFields[0];
                    }
                    queryCriteria.OrderCriteria.Add(new Tuple<ListMetadataField, SortDirection, string>
                        (item.ListMetadataField, (SortDirection)item.OrderCriteria, sortPart));
                }
            });

            var viewMetadataWhereCriteries = view.ViewMetadataWhereCriteries.ToList();
            viewMetadataWhereCriteries.ForEach(item =>
            {
                queryCriteria.WhereCriteria.Add(new Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>(
                    item.ListMetadataField, (CompareType)item.CompareType, (ConcateOperator)item.ConcateOperator, item.Value, null));
            });

            return queryCriteria;
        }

        private QueryCriteria QueryCriteriaByList_Status(ListMetadata list)
        {
            var queryCriteria = new QueryCriteria();
            var tableDef = _eventStatusPropertyProvider.GetTableDef();
            if (!string.IsNullOrWhiteSpace(list.DataSource))
            {
                tableDef.ParentName = list.DataSource;
            }
            queryCriteria.ListMetadataFields.AddRange(list.ListMetadataFields);
            queryCriteria.SelectCriteria.AddRange(_eventStatusPropertyProvider.GetDefaultFields());
            queryCriteria.JoinCriteria.Add(new Tuple<JoinType, TableSource, IBoolean>(JoinType.Inner, tableDef.ParentTableSource(),
                                        Expression.Eq(tableDef.GetParentField(), tableDef.GetParentPrimariField())));
            queryCriteria.ListMetadataFields.ForEach(item =>
            {
                var fieldName = item.InternalName;
                queryCriteria.SelectCriteria.Add(tableDef.TableSource().Field(fieldName));
                if ((item.DataSourceType == (int)LookupSourceType.Table) ||
                (item.DataSourceType == (int)LookupSourceType.Query))
                {
                    var table = new Table(item.DataSource);
                    item.DataSourceField.Split('$').ToList().ForEach(itemField =>
                    {
                        queryCriteria.SelectCriteria.Add(table.Field(itemField));
                    });
                    queryCriteria.JoinCriteria.Add(new Tuple<JoinType, TableSource, IBoolean>(JoinType.LeftOuter, new TableSource(table),
                                        Expression.Eq(tableDef.GetTable().Field(item.InternalName), table.Field(item.DataSourceKey))));
                }
            });
            return queryCriteria;
        }

        #endregion

        #region Events Status History

        public List<vwEventStatu> ViewEventStatuses(Guid viewId)
        {
            return ViewEventStatuses(viewId, null);
        }

        public List<vwEventStatu> ViewEventStatuses(Guid viewId, QueryParams queryParams)
        {
            var result = new List<vwEventStatu>();
            var tableDef = _eventStatusPropertyProvider.GetTableDef();
            var properties = ViewEventStatusProperties(viewId, queryParams);
            var parentKeys = properties.Select(item => item.FirstOrDefault(value =>
                value.Key == tableDef.ParentField)).Select(parentKey => parentKey.Value.ToString().ToGuid()).ToList();
            var eventStatuses = _eventStatusProvider.GetByIds(parentKeys);
            properties.ForEach(item =>
            {
                var parentId = item.FindValue(tableDef.ParentField).ToString().ToNullableGuid();
                if (!parentId.HasValue) return;
                var eventStatus = eventStatuses.FirstOrDefault(re => re.Id == parentId.Value);
                if (eventStatus == null) return;
                eventStatus.EventStatusProperties = item;
                result.Add(eventStatus);
            });
            return result;
        }

        public List<ExpandoObject> ViewEventStatusProperties(Guid viewId, QueryParams queryParams)
        {
            var view = _viewMetadataProvider.GetById(viewId);
            var tableDef = _eventStatusPropertyProvider.GetTableDef();
            var criteria = QueryCriteriaByView_Status(view).Join(queryParams);
            if (!queryParams.SelectCriteria.IsEmpty())
            {
                criteria.SelectCriteria.Clear();
                queryParams.SelectCriteria.ForEach(item =>
                {
                    criteria.SelectCriteria.Add(tableDef.TableSource().Field(item.InternalName));
                    if (item.DataSourceType.In((int)LookupSourceType.Table, (int)LookupSourceType.Query))
                    {
                        var table = new Table(item.DataSource);
                        item.DataSourceField.Split('$').ToList().ForEach(itemField =>
                        {
                            criteria.SelectCriteria.Add(table.Field(itemField));
                        });
                    }
                });
            }
            return _eventStatusPropertyProvider.List(criteria).MapList(view);
        }

        public List<vwEventStatu> ListEventStatuses(Guid listId)
        {
            return ListEventStatuses(listId, null);
        }

        public List<vwEventStatu> ListEventStatuses(Guid listId, QueryParams queryParams)
        {
            var result = new List<vwEventStatu>();
            var tableDef = _eventStatusPropertyProvider.GetTableDef();
            var properties = ListEventStatusProperties(listId, queryParams);
            var parentKeys = properties.Select(item => item.FirstOrDefault(value =>
                value.Key == tableDef.ParentField)).Select(parentKey => parentKey.Value.ToString().ToGuid()).ToList();
            var eventStatuses = _eventStatusProvider.GetByIds(parentKeys);
            properties.ForEach(item =>
            {
                var parentId = item.FindValue(tableDef.ParentField).ToString().ToNullableGuid();
                if (!parentId.HasValue) return;
                var eventStatus = eventStatuses.FirstOrDefault(re => re.Id == parentId.Value);
                if (eventStatus == null) return;
                eventStatus.EventStatusProperties = item;
                result.Add(eventStatus);
            });
            return result;
        }

        public List<ExpandoObject> ListEventStatusProperties(Guid listId, QueryParams queryParams)
        {
            var list = _listMetadataProvider.GetById(listId);
            var tableDef = _eventStatusPropertyProvider.GetTableDef();
            var criteria = QueryCriteriaByList_Status(list).Join(queryParams);
            if (!queryParams.SelectCriteria.IsEmpty())
            {
                criteria.SelectCriteria.Clear();
                queryParams.SelectCriteria.ForEach(item =>
                {
                    criteria.SelectCriteria.Add(tableDef.TableSource().Field(item.InternalName));
                    if (item.DataSourceType.In((int)LookupSourceType.Query, (int)LookupSourceType.Table))
                    {
                        var table = new Table(item.DataSource);
                        item.DataSourceField.Split('$').ToList().ForEach(itemField =>
                        {
                            criteria.SelectCriteria.Add(table.Field(itemField));
                        });
                    }
                });
            }
            return _eventStatusPropertyProvider.List(criteria).MapList(list);
        }

        public int CountEventStatuses(Guid viewId, QueryParams queryParams)
        {
            var view = _viewMetadataProvider.GetById(viewId);
            var criteria = QueryCriteriaByView_Status(view).Join(queryParams);
            return _eventStatusPropertyProvider.Count(criteria);
        }

        public vwEventStatu SingleEventStatus(Guid viewId, Guid itemId)
        {
            var view = _viewMetadataProvider.GetById(viewId);
            var criteria = QueryCriteriaByView_Status(view);
            var field = view.ListMetadata.ListMetadataFields.FirstOrDefault(
                item => item.InternalName == _eventStatusPropertyProvider.GetTableDef().ParentField);
            criteria.WhereCriteria.Add(
                new Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>(
                field, CompareType.Equal, ConcateOperator.And, itemId, null));
            var properties = _eventStatusPropertyProvider.List(criteria).MapList(view);
            var result = _eventStatusProvider.GetById(itemId);
            result.EventStatusProperties = properties.FirstOrDefault();
            return result;
        }

        public vwEventStatu ListSingleEventStatus(Guid listId, Guid itemId)
        {
            var list = _listMetadataProvider.GetById(listId);
            var tableDef = _eventStatusPropertyProvider.GetTableDef();
            var criteria = QueryCriteriaByList_Status(list).Join(new QueryParams { TakeRows = 1 });

            var field = list.ListMetadataFields.FirstOrDefault(item => item.InternalName == tableDef.ParentField);
            criteria.WhereCriteria.Add(
                new Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>(
                field, CompareType.Equal, ConcateOperator.And, itemId, null));

            criteria.SelectCriteria.Clear();
            criteria.SelectCriteria.Add(tableDef.GetPrimaryField());
            list.ListMetadataFields.ToList().ForEach(item =>
            {
                criteria.SelectCriteria.Add(tableDef.TableSource().Field(item.InternalName));
                if (item.DataSourceType.In((int)LookupSourceType.Table, (int)LookupSourceType.Query))
                {
                    var table = new Table(item.DataSource);
                    item.DataSourceField.Split('$').ToList().ForEach(itemField =>
                    {
                        criteria.SelectCriteria.Add(table.Field(itemField));
                    });
                }
            });

            var properties = _eventStatusPropertyProvider.List(criteria).MapList(list);
            var result = _eventStatusProvider.GetById(itemId);
            result.EventStatusProperties = properties.FirstOrDefault();
            return result;
        }

        public vwEventStatu GetEventStatus(Guid id)
        {
            var result = _eventStatusProvider.GetById(id);
            if (result != null)
            {
                result.EventStatusProperties = _eventStatusPropertyProvider.GetByParentId(result.Id);
            }
            return result;
        }

        #endregion

        #region Get Roster Events

        public List<RosterEvent> ViewRosterEvents(Guid viewId)
        {
			return ViewRosterEvents(viewId, null);
        }

        public List<RosterEvent> ViewRosterEvents(Guid viewId, QueryParams queryParams)
        {
            var result = new List<RosterEvent>();
            var tableDef = _rosterEventPropertyProvider.GetTableDef();
            var properties = ViewRosterEventProperties(viewId, queryParams);
            var parentKeys = properties.Select(item => item.FirstOrDefault(value =>
                value.Key == tableDef.ParentField)).Select(parentKey => parentKey.Value.ToString().ToGuid()).ToList();
            var rosterEvents = _rosterEventProvider.GetByIds(parentKeys);
            properties.ForEach(item =>
            {
                var parentId = item.FindValue(tableDef.ParentField).ToString().ToNullableGuid();
                if (!parentId.HasValue) return;
                var rosterEvent = rosterEvents.FirstOrDefault(re => re.Id == parentId.Value);
                if (rosterEvent == null) return;
                rosterEvent.RosterEventProperties = item;
                result.Add(rosterEvent);
            });
            return result;
        }

        public List<ExpandoObject> ViewRosterEventProperties(Guid viewId, QueryParams queryParams)
        {
            var view = _viewMetadataProvider.GetById(viewId);
            var tableDef = _rosterEventPropertyProvider.GetTableDef();
            var criteria = QueryCriteriaByView(view).Join(queryParams);
            if (queryParams != null && !queryParams.SelectCriteria.IsEmpty())
            {
                criteria.SelectCriteria.Clear();
                queryParams.SelectCriteria.ForEach(item =>
                {
                    criteria.SelectCriteria.Add(tableDef.TableSource().Field(item.InternalName));
                    if (item.DataSourceType.In((int)LookupSourceType.Table,(int)LookupSourceType.Query))
                    {
                        var table = new Table(item.DataSource);
                        item.DataSourceField.Split('$').ToList().ForEach(itemField =>
                        {
                            criteria.SelectCriteria.Add(table.Field(itemField));
                        });
                    }
                });
            }
            return _rosterEventPropertyProvider.List(criteria).MapList(view);
        }

        public List<RosterEvent> ListRosterEvents(Guid listId)
        {
            return ListRosterEvents(listId, null);
        }

        public List<Tuple<Guid, List<RosterEvent>>> ListRosterEvents(List<Guid> listIds, QueryParams queryParams)
	    {
	        var result = new List<Tuple<Guid, List<RosterEvent>>>();
            if (listIds.IsEmpty())
            {
                listIds.ForEach(item =>
                {
                    result.Add(new Tuple<Guid, List<RosterEvent>>(item,ListRosterEvents(item, queryParams)));
                });                
            }
	        return result;
	    }

        public List<RosterEvent> ListRosterEvents(Guid listId, QueryParams queryParams)
        {
            var result = new List<RosterEvent>();
            var tableDef = _rosterEventPropertyProvider.GetTableDef();
            var properties = ListRosterEventProperties(listId, queryParams);
            var parentKeys = properties.Select(item => item.FirstOrDefault(value =>
                value.Key == tableDef.ParentField)).Select(parentKey => parentKey.Value.ToString().ToGuid()).ToList();
            var rosterEvents = _rosterEventProvider.GetByIds(parentKeys);
            properties.ForEach(item =>
            {
                var parentId = item.FindValue(tableDef.ParentField).ToString().ToNullableGuid();
                if (!parentId.HasValue) return;
                var rosterEvent = rosterEvents.FirstOrDefault(re => re.Id == parentId.Value);
                if (rosterEvent == null) return;
                rosterEvent.RosterEventProperties = item;
                result.Add(rosterEvent);
            });
            return result;
        }

        public List<ExpandoObject> ListRosterEventProperties(Guid listId, QueryParams queryParams)
        {
            var list = _listMetadataProvider.GetById(listId);
            var tableDef = _rosterEventPropertyProvider.GetTableDef();
            var criteria = QueryCriteriaByList(list).Join(queryParams);
            if (queryParams != null && !queryParams.SelectCriteria.IsEmpty())
            {
                criteria.SelectCriteria.Clear();
                queryParams.SelectCriteria.ForEach(item =>
                {
                    criteria.SelectCriteria.Add(tableDef.TableSource().Field(item.InternalName));
                    if (item.DataSourceType.In((int)LookupSourceType.Query, (int)LookupSourceType.Table))
                    {
                        var table = new Table(item.DataSource);
                        item.DataSourceField.Split('$').ToList().ForEach(itemField =>
                        {
                            criteria.SelectCriteria.Add(table.Field(itemField));
                        });
                    }
                });
            }
            return _rosterEventPropertyProvider.List(criteria).MapList(list);
        }

		public int CountRosterEvents(Guid viewId, QueryParams queryParams)
		{
            var view = _viewMetadataProvider.GetById(viewId);
            var criteria = QueryCriteriaByView(view).Join(queryParams);
            return _rosterEventPropertyProvider.Count(criteria);
		}

        public RosterEvent SingleRosterEvent(Guid viewId, Guid itemId)
        {
            var view = _viewMetadataProvider.GetById(viewId);
            var criteria = QueryCriteriaByView(view);
            var field = view.ListMetadata.ListMetadataFields.FirstOrDefault(
                item => item.InternalName == _rosterEventPropertyProvider.GetTableDef().ParentField);
            criteria.WhereCriteria.Add(
                new Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>(
                field, CompareType.Equal, ConcateOperator.And, itemId, null));
            var properties = _rosterEventPropertyProvider.List(criteria).MapList(view);
            var result = _rosterEventProvider.GetById(itemId);
            result.RosterEventProperties = properties.FirstOrDefault();
            return result;
        }

        public RosterEvent ListSingleRosterEvent(Guid listId, Guid itemId)
        {
            var list = _listMetadataProvider.GetById(listId);
            var tableDef = _rosterEventPropertyProvider.GetTableDef();
            var criteria = QueryCriteriaByList(list).Join(new QueryParams { TakeRows = 1 });

            var field = list.ListMetadataFields.FirstOrDefault(item => item.InternalName == tableDef.ParentField);
            criteria.WhereCriteria.Add(
                new Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>(
                field, CompareType.Equal, ConcateOperator.And, itemId, null));

            criteria.SelectCriteria.Clear();
            criteria.SelectCriteria.Add(tableDef.GetPrimaryField());
            list.ListMetadataFields.ToList().ForEach(item => {
                criteria.SelectCriteria.Add(tableDef.TableSource().Field(item.InternalName));
                if (item.DataSourceType.In((int)LookupSourceType.Table,(int)LookupSourceType.Query))
                {
                    var table = new Table(item.DataSource);
                    item.DataSourceField.Split('$').ToList().ForEach(itemField => {
                        criteria.SelectCriteria.Add(table.Field(itemField));
                    });
                }
            });

            var properties = _rosterEventPropertyProvider.List(criteria).MapList(list);
            var result = _rosterEventProvider.GetById(itemId);
            result.RosterEventProperties = properties.FirstOrDefault();
            return result;
        }

        public RosterEvent GetRosterEvent(Guid id)
        {
            var result = _rosterEventProvider.GetById(id);
            if (result != null)
            {
                result.RosterEventProperties = _rosterEventPropertyProvider.GetByParentId(result.Id);
            }
            return result;
        }

        public List<RosterEvent> GetRosterEvents(List<Guid> ids, bool withProperties)
        {
            var result = _rosterEventProvider.GetByIds(ids);
            if (withProperties && result != null && result.Any()) {
                foreach (RosterEvent ri in result) {
                    ri.RosterEventProperties = _rosterEventPropertyProvider.GetByParentId(ri.Id);
                }
            }
            return result;
        }

	    public ExpandoObject GetParentEntity(object id)
	    {
	        ExpandoObject result = null;
            var rosterEvent = GetRosterEvent(id.ToGuid());
	        if (rosterEvent != null)
	        {
	            result = rosterEvent.RosterEventProperties;
	        }
	        else
	        {
	            var materRoster = _masterRosterProvider.Get(id.ToInt());
	            if (materRoster != null)
	            {
	                result = materRoster;
	            }
	        }
	        return result;
	    }

        #endregion Get Roster Events

        #region Get Timesheet events

        public bool IsAllowApprove(DateTime weekStart, int workerId)
        {
            return _rosterEventProvider.IsAllowApprove(weekStart, workerId);
        }

        public bool IsAllowApprove(DateTime weekStart, Guid rosterId)
        {
            return _rosterEventProvider.IsAllowApprove(weekStart, rosterId);
        }

        public List<RosterEvent> ViewTimesheetEvents(Guid viewId, QueryParams queryParams, Tuple<DateTime, DateTime> calendarPeriod)
        {
            // GET timesheet events
            var result = new List<RosterEvent>();
            var tableDef = _rosterEventPropertyProvider.GetTableDef();
            var properties = ViewRosterEventProperties(viewId, queryParams);
            var parentKeys = properties.Select(item => item.FirstOrDefault(value =>
                value.Key == tableDef.ParentField)).Select(parentKey => parentKey.Value.ToString().ToGuid()).ToList();
            var rosterEvents = _rosterEventProvider.GetByIds(parentKeys);
            properties.ForEach(item =>
            {
                var parentId = item.FindValue(tableDef.ParentField).ToString().ToNullableGuid();
                if (!parentId.HasValue) return;
                var rosterEvent = rosterEvents.FirstOrDefault(re => re.Id == parentId.Value);
                if (rosterEvent == null) return;
                rosterEvent.RosterEventProperties = item;
                result.Add(rosterEvent);
            });

            // CHECK if Where criterias contain 'WorkerPersonId' param
            if (queryParams == null)
            {
                return result; // return only timesheets, without pre-populated working rosters (cause no WorkerId exists)                
            }
            var workerIdFilter = queryParams.WhereCriteria.Where(cr => cr.Item1.InternalName == Roster.Common.FieldNames.WORKER_PERSON_ID).FirstOrDefault();
            if (workerIdFilter == null)
            {
                return result; // return only timesheets, without pre-populated working rosters (cause no WorkerId exists)                
            }
            // CHECK if period already SUBMITTED
            var workerId = Convert.ToInt32(workerIdFilter.Item4);
            if (IsExistLockPeriod(workerId, calendarPeriod.Item1.Date, calendarPeriod.Item2.Date))
                return result; // period already submitted -> means Display only Timesheet rosters
            
            // CHECK if pre-population is ON for current user
            var workersInfo = this.TableContent("WorkerPerson", "Id", Roster.Common.FieldNames.USE_TIMESHEET).Select(x => {
                var elem = x.Item2 as IDictionary<string, object>;
                return new { WorkerId = x.Item1, UseTimesheet = elem[Roster.Common.FieldNames.USE_TIMESHEET].ToBoolean() };
            });
            if (workersInfo != null && workersInfo.Any() &&
                workersInfo.Where(wi => wi.WorkerId == workerId && wi.UseTimesheet).Any())
            {
                // PRE-POPULATE events FROM Working rosters

                // get view from 'Working rosters' list by CONST name
                var workingRosterView = _viewMetadataProvider.List(new SelectCriteria<ViewMetadata> {
                    ExpressionSelect = new List<Expression<Func<ViewMetadata, bool>>> { item => item.Name == Roster.Common.ViewNames.WORKING_ROSTERS_FOR_TIMESHEETS }
                }).FirstOrDefault();

                if (workingRosterView != null)
                {
                    var _wrkField = workingRosterView.ListMetadata.ListMetadataFields.FirstOrDefault(item => item.InternalName == FieldNames.WORKER_PERSON_ID);
                    var _wrkParams = new QueryParams();
                    _wrkParams.WhereCriteria.Add(new Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>(
                                                                _wrkField, CompareType.Equal, ConcateOperator.And, workerId, null));
                    var _wrkResult = this.ViewRosterEvents(workingRosterView.Id, _wrkParams);
                    result = result.Concat(_wrkResult).ToList();
                }
            }

            return result;
        }

        #endregion

        #region Roster Events Modification

        public void WorkingLock(string spName, DateTime startDate, DateTime endDate, List<Tuple<int, string>> trusteeRights, string message)
        {
            _rosterEventProvider.WorkingLock(spName, startDate, endDate, trusteeRights, message);
        }

        public void SaveLockPeriod(int workerId, DateTime startDate,  DateTime endDate)
        {
            _rosterEventLockProvider.Save(workerId, startDate, endDate);
        }

        public bool IsExistLockPeriod(int workerId, DateTime startDate, DateTime endDate)
        {
            return _rosterEventLockProvider.IsExist(workerId, startDate, endDate);
        }

		public RosterEvent CreateRosterEvent(Guid listId, int eventTypeId)
        {
            var listMetadata = _listMetadataProvider.GetById(listId);
            var item = new RosterEvent
            {
                Id = Guid.NewGuid(),
				EventTypeId = eventTypeId
            };
			item.RosterEventProperties = _rosterEventPropertyProvider.Create(listMetadata, item.Id);
            return item;
        }

        public void SaveRosterEvent(RosterEvent rosterEvent, Guid listId)
        {
            var list = _listMetadataProvider.GetById(listId);
            if (_rosterEventProvider.IsExist(rosterEvent.Id))
            {
                _rosterEventProvider.Update(rosterEvent);
                _rosterEventPropertyProvider.Save(list, rosterEvent.RosterEventProperties);
                _rosterEventProvider.OnModify(rosterEvent);
            }
            else
            {
                _rosterEventProvider.Add(rosterEvent);
                _rosterEventPropertyProvider.Save(list, rosterEvent.RosterEventProperties);
                _rosterEventProvider.OnCreate(rosterEvent);
            }
        }

	    public void SaveRosterEventItem(Guid listId, Guid itemId, string fieldName, string value)
	    {
	        var listMetadata = _listMetadataProvider.GetById(listId);
	        if (listMetadata == null)
	        {
	            return;
	        }
            var rosterEvent = GetRosterEvent(itemId);
	        if (rosterEvent != null && rosterEvent.RosterEventProperties != null)
	        {
	            if (!rosterEvent.RosterEventDictionary.ContainsKey(fieldName)) return;
	            rosterEvent.RosterEventDictionary[fieldName] = value;
	            _rosterEventPropertyProvider.Save(listMetadata,rosterEvent.RosterEventProperties);
	        }
	    }

        public void DeleteRosterEvent(Guid rosterEventId)
        {
            if (_rosterEventProvider.IsExist(rosterEventId))
            {
                _rosterEventProvider.Delete(rosterEventId);
            }
        }

        #endregion Create Roster Events

        #region DatabaseTable

        public string ExecuteProcedure(string name, ICollection<Tuple<string,object>> paramerters)
        {
            return _definitionProvider.ExecuteProcedure(name, paramerters);
        }

        public List<Tuple<int, ExpandoObject>> TableContent(string table, string key, string fields, List<Tuple<string, string>> whereCriteria = null)
        {
            return _definitionProvider.ListTableContent(table, key, fields, whereCriteria);
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

	    #endregion

        #region Master Roster

        public List<ExpandoObject> ListTemplates()
        {
            var tableDef = _masterRosterProvider.GetTableDef();
            var queryCriteria = new QueryCriteria();
            queryCriteria.InternalWhere.Add(Expression.Eq(tableDef.GetTable().Field(tableDef.IsTemplate), Expression.Const(true)));
            return _masterRosterProvider.List(queryCriteria);
        }

	    public void SetAsTemplate(int masterRosterId)
	    {
	        var masterRoster = (_masterRosterProvider.Get(masterRosterId) as IDictionary<string, object>);
            var tableDef = _masterRosterProvider.GetTableDef();
	        masterRoster[tableDef.IsTemplate] = true;
            _masterRosterProvider.UpdateRow((ExpandoObject)masterRoster);
        }

        public bool IsTemplate(int masterRosterId)
        {
            var masterRoster = (_masterRosterProvider.Get(masterRosterId) as IDictionary<string, object>);
            var tableDef = _masterRosterProvider.GetTableDef();
            return (masterRoster[tableDef.IsTemplate] == DBNull.Value) ? false : Convert.ToBoolean(masterRoster[tableDef.IsTemplate]);
        }

        public void RemoveFormTemplate(int masterRosterId)
        {
            var masterRoster = (_masterRosterProvider.Get(masterRosterId) as IDictionary<string, object>);
            var tableDef = _masterRosterProvider.GetTableDef();
            masterRoster[tableDef.IsTemplate] = false;
            _masterRosterProvider.UpdateRow((ExpandoObject)masterRoster);
        }

        #endregion Master Roster

        #region Holiday

        public void SaveHoliday(HolidayEvent item)
        {
            if (_holydayProvider.IsExist(item))
            {
                _holydayProvider.Update(item);
            }
            else
            {
                _holydayProvider.Add(item);
            }
        }

        public void DeleteHoliday(Guid itemId)
        {
            _holydayProvider.Delete(itemId);
        }

        public void PopulateHolidayNextYear(bool clear)
        {
            _holydayProvider.Populate(clear);
        }

        public HolidayEvent Get(Guid itemId)
        {
            return _holydayProvider.GetById(itemId);
        }

        public List<Holiday> Holidays(int? typeId)
        {
            return _holydayProvider.Holidays(typeId);
        }

        public List<HolidayEvent> ListHolidayEvents()
        {
            return _holydayProvider.List();
        }

        #endregion

        #region Tracking

	    public List<vwRosterEventTrackData> ListTrackData(Guid rosterEventId)
	    {
            var result = _trackingDataProvider.GetList(rosterEventId).OrderByDescending(i => i.Version).ToList();
	        result.ForEach(item =>
	        {
	            item.TrackDataProperties = new DocParser(item.XmlContent).GetElements("*");
	        });
	        return result;
	    }
	    #endregion
    }
}