using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Activation;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Client.Services;
using Roster.BL;
using Roster.BL.Extentions;
using Roster.Common;
using Roster.Model.DataContext;
using Roster.Model.Helpers;
using Roster.Presentation.Controls.Fields;
using Roster.Presentation.Extensions;

// ReSharper disable once CheckNamespace
namespace Roster.Services
{
    [BasicHttpBindingServiceMetadataExchangeEndpoint]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class ApiService : BaseService, IApiService
    {
        #region Metadata

        public listView<Entity> MetadataList(string version, QueryDisplayParams query)
        {
            DisableCaching();
            var result = new listView<Entity>();
            try
            {
                result.page = new pager<Entity>();
                var service = new RosterConfigService();
                var content = service.GetLists();
                result.total = content.Count();
                content.Take(query.currentPageSize).Skip(query.currentPageNum * query.currentPageSize).ToList().ForEach(item =>
                {
                    result.page.pageItems.Add(new Entity
                    {
                        Key = item.Id.ToSafeString(),
                        Name = item.Name,
                        Fields = item.ToNamed()
                    });
                });
            }
            catch (Exception ex)
            {
                result.message.message = ex.Message;
                result.message.messageLevel = messageLevelEnum.critical;
                //HandleException(ex);
            }
            return result;
        }

        public listView<Entity> MetadataView(string version, ICollection<abstractSearch> filter, QueryDisplayParams query)
        {
            DisableCaching();
            var result = new listView<Entity>();
            try
            {
                var listId = filter.filterTextValue("listId").ToGuid();
                result.page = new pager<Entity>();
                var service = new RosterConfigService();
                var content = service.GetViews(listId);
                result.total = content.Count();
                content.Take(query.currentPageSize).Skip(query.currentPageNum * query.currentPageSize).ToList().ForEach(item =>
                {
                    result.page.pageItems.Add(new Entity
                    {
                        Key = item.Id.ToSafeString(),
                        Name = item.Name,
                        Fields = item.ToNamed()
                    });
                });
            }
            catch (Exception ex)
            {
                result.message.message = ex.Message;
                result.message.messageLevel = messageLevelEnum.critical;
                //HandleException(ex);
            }
            return result;
        }

        public itemView<Roster.Services.Group> MetadataItem(string version, string id)
        {
            DisableCaching();
            var result = new itemView<Group>();
            try
            {
                var service = new RosterConfigService();
                var content = service.GetLists().FirstOrDefault(_ => _.Id == id.ToGuid());
                if (content != null)
                {
                    result.item = new Group { Key = content.Id.ToSafeString(), Name = content.Name, Fields = new List<Entity>()};
                    content.ListMetadataFields.ToList().ForEach(item =>
                    {
                        result.item.Fields.Add(new Entity { Key = content.Id.ToSafeString(), Name = content.Name, Fields = item.ToNamed()});
                    });
                }
            }
            catch (Exception ex)
            {
                result.message.message = ex.Message;
                result.message.messageLevel = messageLevelEnum.critical;
                //HandleException(ex);
            }
            return result;
        }

        public listView<Entity> ContentTypeList(string version, ICollection<abstractSearch> filter, QueryDisplayParams query)
        {
            DisableCaching();
            var result = new listView<Entity>();
            try
            {
                var listId = filter.filterTextValue("listId").ToGuid();
                result.page = new pager<Entity>();
                var service = new RosterConfigService();
                var content = service.GetContentTypes(listId);
                result.total = content.Count();
                content.Take(query.currentPageSize).Skip(query.currentPageNum * query.currentPageSize).ToList().ForEach(item =>
                {
                    result.page.pageItems.Add(new Entity
                    {
                        Key = item.Id.ToSafeString(),
                        Name = item.Name,
                        Fields = item.ToNamed()
                    });
                });
            }
            catch (Exception ex)
            {
                result.message.message = ex.Message;
                result.message.messageLevel = messageLevelEnum.critical;
                //HandleException(ex);
            }
            return result;
        }

        #endregion Metadata

        #region Dictionary

        public ICollection<named> DictionarySources(string version, int type)
        {
            DisableCaching();
            var result = new List<named>();
            try
            {
                result.AddRange(type == (int)LookupSourceType.SpList ? 
                    SPContext.Current.Web.Lists.Cast<SPList>().Where(l => !l.Hidden)
                    .Select(x => new named(x.ID.ToString("B").ToUpper(), x.Title)).ToList()
                    : new RosterConfigService().DatabaseTables().Select(x => new named(x, x)).ToList());
            }
            catch (Exception ex)
            {
                //HandleException(ex);
            }
            return result;
        }

        public itemView<Entity> DictionaryFields(string version, string sourceId, int type)
        {
            DisableCaching();
            var result = new itemView<Entity>();
            try
            {
                result.item = new Entity() { Key = sourceId, Fields = new List<named>()};
                if (type == (int)LookupSourceType.SpList)
                {
                    var selected = sourceId.ToGuid();
                    var list = SPContext.Current.Web.Lists[selected];
                    if (list != null)
                    {
                        result.item.Fields.AddRange(list.Fields.Cast<SPField>().Where(f => !f.Hidden
                            && f.Type != SPFieldType.Note).Select(x => 
                                new named(x.Id.ToString("B").ToUpper(), x.Title)).ToList());
                    }
                }
                else
                {
                    result.item.Fields.AddRange(new RosterConfigService().TablesFields(sourceId).
                        Select(x => new named(x, x)).ToList());
                }
            }
            catch (Exception ex)
            {
                result.message.message = ex.Message;
                result.message.messageLevel = messageLevelEnum.critical;
                //HandleException(ex);
            }
            return result;
        }

        public listView<Entity> DictionaryList(string version, ICollection<abstractSearch> filter, QueryDisplayParams query)
        {
            DisableCaching();
            var result = new listView<Entity>();
            try
            {
                var source = filter.filterTextValue("source");
                var key = filter.filterTextValue("key");
                var fields = filter.filterTextValue("fields");
                var type = filter.filterIntValue("type");
                var queryTerm = filter.filterTextValue("queryTerm");

                result.page = new pager<Entity>();

                var content = BLExtensions.SourceContent(source, key, fields, type)
                    .Where(i => i.Item2.ToSeparatedString().Contains(queryTerm)).ToList();
                result.total = content.Count();
                content.Take(query.currentPageSize).Skip(query.currentPageNum * query.currentPageSize).ToList().ForEach(item =>
                {
                    result.page.pageItems.Add(new Entity
                    {
                        Key = item.Item1.ToSafeString(),
                        Name = item.Item2.FirstValue(),
                        Fields = new List<named>()
                    });
                });
            }
            catch (Exception ex)
            {
                result.message.message = ex.Message;
                result.message.messageLevel = messageLevelEnum.critical;
                //HandleException(ex);
            }
            return result;
        }

        #endregion Dictionary

        #region Master Roster

        public listView<Entity> ListTemplates(string version, QueryDisplayParams query)
        {
            DisableCaching();
            var result = new listView<Entity>();
            try
            {
                result.page = new pager<Entity>();
                var service = new RosterDataService();
                var content = service.ListTemplates().ToList();
                result.total = content.Count();
                content.Take(query.currentPageSize).Skip(query.currentPageNum * query.currentPageSize).ToList().ForEach(item =>
                {
                    result.page.pageItems.Add(new Entity
                    {
                        Key = item.FindValue("Id").ToSafeString(),
                        Fields = item.ExpandoToNamed()
                    });
                });
            }
            catch (Exception ex)
            {
                //HandleException(ex);
            }
            return result;
        }

        #endregion Master Roster

        #region Roster Events

        public listView<Entity> TimesheetList(string version, ICollection<abstractSearch> filter, QueryDisplayParams query)
        {
            DisableCaching();
            var result = new listView<Entity>();
            try
            {
                var viewId = filter.filterTextValue("viewId").ToGuid();
                var dateFrom = filter.filterDateTimeValue("from");
                var dateTo = filter.filterDateTimeValue("to");
                var period = new Tuple<DateTime, DateTime>(dateFrom, dateTo);
                result.page = new pager<Entity>();
                var service = new RosterDataService();
                var content = service.ViewTimesheetEvents(viewId, null, period).ToList();
                result.total = content.Count();
                content.Take(query.currentPageSize).Skip(query.currentPageNum * query.currentPageSize).ToList().ForEach(item =>
                {
                    result.page.pageItems.Add(new Entity
                    {
                        Key = item.Id.ToSafeString(),
                        Fields = item.RosterEventProperties.ExpandoToNamed()
                    });
                });
            }
            catch (Exception ex)
            {
                result.message.message = ex.Message;
                result.message.messageLevel = messageLevelEnum.critical;
                //HandleException(ex);
            }
            return result;
        }

        public listView<Entity> RosterEventView(string version, ICollection<abstractSearch> filter, QueryDisplayParams query)
        {
            DisableCaching();
            var result = new listView<Entity>();
            try
            {
                var viewId = filter.filterTextValue("viewId").ToGuid();
                result.page = new pager<Entity>();
                var service = new RosterDataService();
                var content = service.ViewRosterEvents(viewId).ToList();
                result.total = content.Count();
                content.Take(query.currentPageSize).Skip(query.currentPageNum * query.currentPageSize).ToList().ForEach(item =>
                {
                    result.page.pageItems.Add(new Entity
                    {
                        Key = item.Id.ToSafeString(),
                        Fields = item.RosterEventProperties.ExpandoToNamed()
                    });
                });
            }
            catch (Exception ex)
            {
                result.message.message = ex.Message;
                result.message.messageLevel = messageLevelEnum.critical;
                //HandleException(ex);
            }
            return result;
        }

        public listView<Entity> RosterEventList(string version, ICollection<abstractSearch> filter, QueryDisplayParams query)
        {
            DisableCaching();
            var result = new listView<Entity>();
            try
            {
                var listId = filter.filterTextValue("listId").ToGuid();
                result.page = new pager<Entity>();
                var service = new RosterDataService();
                var content = service.ListRosterEvents(listId).ToList();
                result.total = content.Count();
                content.Take(query.currentPageSize).Skip(query.currentPageNum * query.currentPageSize).ToList().ForEach(item =>
                {
                    result.page.pageItems.Add(new Entity
                    {
                        Key = item.Id.ToSafeString(),
                        Fields = item.RosterEventProperties.ExpandoToNamed()
                    });
                });
            }
            catch (Exception ex)
            {
                result.message.message = ex.Message;
                result.message.messageLevel = messageLevelEnum.critical;
                //HandleException(ex);
            }
            return result;
        }

        public itemView<Entity> GetRosterEventItem(string version, string itemId, string viewId)
        {
            DisableCaching();
            var result = new itemView<Entity>();
            try
            {
                result.item = new Entity();
                var service = new RosterDataService();
                var content = service.SingleRosterEvent(viewId.ToGuid(), itemId.ToGuid());
                result.item.Key = content.Id.ToSafeString();
                result.item.Fields = content.RosterEventProperties.ExpandoToNamed();
            }
            catch (Exception ex)
            {
                result.message.message = ex.Message;
                result.message.messageLevel = messageLevelEnum.critical;
                //HandleException(ex);
            }
            return result;
        }

        public itemView<Entity> CreateRosterEventItem(string version, string listId, int eventTypeId)
        {
            DisableCaching();
            var result = new itemView<Entity>();
            try
            {
                result.item = new Entity();
                var service = new RosterDataService();
                var content = service.CreateRosterEvent(listId.ToGuid(), eventTypeId);
                result.item.Key = content.Id.ToSafeString();
                result.item.Fields = content.RosterEventProperties.ExpandoToNamed();
            }
            catch (Exception ex)
            {
                result.message.message = ex.Message;
                result.message.messageLevel = messageLevelEnum.critical;
                //HandleException(ex);
            }
            return result;
        }

        public itemView<bool> SaveRosterEventItem(string version, string listId, int eventTypeId, Entity item)
        {
            DisableCaching();
            var result = new itemView<bool>();
            try
            {
                var service = new RosterDataService();
                var config = new RosterConfigService();
                var list = config.GetList(listId.ToGuid());
                var entity = new RosterEvent
                {
                    Id = item.Key.ToGuid(),
                    EventTypeId = eventTypeId,
                    RosterEventProperties = item.Fields.NamedToExpando()
                };
                if (!entity.RosterEventDictionary.ContainsKey("ContentTypeId"))
                {
                    entity.RosterEventDictionary.Add("ContentTypeId",1);
                }
                if (!entity.RosterEventDictionary.ContainsKey("CreatedBy"))
                {
                    entity.RosterEventDictionary.Add("CreatedBy", 
                        DbFieldUser.GetUserRosterId(SPContext.Current.Web.CurrentUser));
                }
                if (!entity.RosterEventDictionary.ContainsKey("Created"))
                {
                    entity.RosterEventDictionary.Add("Created", DateTime.Now);
                }
                if (!entity.RosterEventDictionary.ContainsKey("ModifiedBy"))
                {
                    entity.RosterEventDictionary.Add("ModifiedBy",  
                        DbFieldUser.GetUserRosterId(SPContext.Current.Web.CurrentUser));
                }
                if (!entity.RosterEventDictionary.ContainsKey("Modified"))
                {
                    entity.RosterEventDictionary.Add("Modified", DateTime.Now);
                }
                if (!entity.RosterEventDictionary.ContainsKey("Id"))
                {
                    entity.RosterEventDictionary.Add("Id", entity.Id);
                }
                if (!entity.RosterEventDictionary.ContainsKey("RosterEventId"))
                {
                    entity.RosterEventDictionary.Add("RosterEventId", entity.Id);
                }
                var fields = list.ListMetadataFields.ToList();
                fields.ForEach(_=>
                {
                    if (!entity.RosterEventDictionary.ContainsKey(_.InternalName))
                    {
                        if (_.ClrType == typeof (DateTime))
                        {
                            entity.RosterEventDictionary.Add(_.InternalName, null);
                        }
                        else if (_.ClrType == typeof(Guid))
                        {
                            entity.RosterEventDictionary.Add(_.InternalName, null);
                        }
                        else if (_.ClrType == typeof(Int32))
                        {
                            entity.RosterEventDictionary.Add(_.InternalName, null);
                        }
                        else
                        {
                            entity.RosterEventDictionary.Add(_.InternalName, _.ClrType.DefaultValue());
                        }
                    }
                });
                service.SaveRosterEvent(entity, list.Id);
                result.item = true;
            }
            catch (Exception ex)
            {
                //HandleException(ex);
                result.message.message = ex.Message;
                result.message.messageLevel = messageLevelEnum.critical;
            }
            return result;
        }

        public itemView<bool> DeleteRosterEventItem(string version, string id)
        {
            DisableCaching();
            var result = new itemView<bool>();
            try
            {
                var service = new RosterDataService();
                service.DeleteRosterEvent(id.ToGuid());
                result.item = true;
            }
            catch (Exception ex)
            {
                result.message.message = ex.Message;
                result.message.messageLevel = messageLevelEnum.critical;
                //HandleException(ex);
            }
            return result;
        }

        #endregion Roster Events

        #region Roster Operation

        public itemView<string> EndorseRoster(string version, ICollection<abstractSearch> filter)
        {
            DisableCaching();
            var result = new itemView<string>();
            try
            {
                var listId = filter.filterTextValue("listId").ToGuid();
                var id = filter.filterTextValue("id").ToGuid();
                result.item = new DataHelpers().EndorseRoster(listId, id);
            }
            catch (Exception ex)
            {
                result.message.message = ex.Message;
                result.message.messageLevel = messageLevelEnum.critical;
                //HandleException(ex);
            }
            return result;
        }
        public itemView<string> RejectRoster(string version, ICollection<abstractSearch> filter)
        {
            DisableCaching();
            var result = new itemView<string>();
            try
            {
                var listId = filter.filterTextValue("listId").ToGuid();
                var id = filter.filterTextValue("id").ToGuid();
                var reason = filter.filterTextValue("reason");
                result.item = new DataHelpers().RejectRoster(listId, id, reason);
            }
            catch (Exception ex)
            {
                result.message.message = ex.Message;
                result.message.messageLevel = messageLevelEnum.critical;
                //HandleException(ex);
            }
            return result;
        }

        public itemView<string> ApproveRoster(string version, ICollection<abstractSearch> filter)
        {
            DisableCaching();
            var result = new itemView<string>();
            try
            {
                var listId = filter.filterTextValue("listId").ToGuid();
                var id = filter.filterTextValue("id").ToGuid();
                result.item = new DataHelpers().ApproveRoster(listId, id);
            }
            catch (Exception ex)
            {
                result.message.message = ex.Message;
                result.message.messageLevel = messageLevelEnum.critical;
                //HandleException(ex);
            }

            return result;
        }

        public itemView<string> CancelRoster(string version, ICollection<abstractSearch> filter)
        {
            DisableCaching();
            var result = new itemView<string>();
            try
            {
                var id = filter.filterTextValue("id").ToGuid();
                result.item = new DataHelpers().CancelRoster(id);
            }
            catch (Exception ex)
            {
                result.message.message = ex.Message;
                result.message.messageLevel = messageLevelEnum.critical;
                //HandleException(ex);
            }
            return result;
        }

        #endregion Roster Operation

        #region Roster Execute

        public itemView<string> ExecuteAction(string version, ICollection<abstractSearch> filter)
        {
            DisableCaching();
            var result = new itemView<string>();
            try
            {
                var name = filter.filterTextValue("name");
                var dataService = new RosterDataService();
                {
                    var paramCollection = !filter.Any() ? new List<Tuple<string, object>>() :
                        filter.Select(item => new Tuple<string, object>(item.LabelText, (item as textSearch).searchTerm)).ToList();
                    var currentUserParam = paramCollection.FirstOrDefault(p => p.Item1.Equals("@currentUser"));
                    if (currentUserParam != null)
                    {
                        paramCollection.Remove(currentUserParam);
                        paramCollection.Add(new Tuple<string, object>("@currentUser", DbFieldUser.GetUserRosterId(SPContext.Current.Web.CurrentUser)));
                    }
                    result.item = dataService.ExecuteProcedure(name, paramCollection);
                }
            }
            catch (Exception ex)
            {
                result.message.message = ex.Message;
                result.message.messageLevel = messageLevelEnum.critical;
                //HandleException(ex);
            }
            return result;
        }

        public itemView<bool> PublishRoster(string version, ICollection<abstractSearch> filter)
        {
            DisableCaching();
            var result = new itemView<bool>();
            try
            {
                var id = filter.filterTextValue("id").ToGuid();
                var daysAhead = filter.filterIntValue("daysAhead");
                BLExtensions.PublishRosterEvent(id, daysAhead);
                result.item = true;
            }
            catch (Exception ex)
            {
                //HandleException(ex);
            }
            return result;
        }

        public itemView<string> SubmitTimesheet(string version, ICollection<abstractSearch> filter)
        {
            DisableCaching();
            var result = new itemView<string>();
            try
            {
                var listId = filter.filterTextValue("listId").ToGuid();
                var id = filter.filterTextValue("id").ToGuid();
                result.item = new DataHelpers().SubmitTimesheet(listId, id);
            }
            catch (Exception ex)
            {
                //HandleException(ex);
            }
            return result;
        }

        public itemView<string> SubmitTimesheets(string version, ICollection<abstractSearch> filter)
        {
            DisableCaching();
            var result = new itemView<string>();
            try
            {
                var storedProcedureName = filter.filterTextValue("storedProcedureName");
                var workerId = filter.filterIntValue("workerId");
                var rosterIDs = filter.filterTextValue("rosterIDs");
                var periodStart = filter.filterTextValue("periodStart");
                var periodEnd = filter.filterTextValue("periodEnd");
                result.item = new DataHelpers().SubmitTimesheet(storedProcedureName, workerId, rosterIDs, periodStart, periodEnd);
            }
            catch (Exception ex)
            {
                result.message.message = ex.Message;
                result.message.messageLevel = messageLevelEnum.critical;
                //HandleException(ex);
            }
            return result;
        }

        #endregion Roster Publish
    }
}
