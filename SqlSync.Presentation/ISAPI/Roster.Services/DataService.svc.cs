using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel.Activation;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Roster.BL;
using Roster.BL.Extentions;
using Roster.Common;
using Roster.Model.DataContext;
using Roster.Model.Helpers;
using Roster.Presentation.Extensions;
using Roster.Model.DataContext.Extention;
using Roster.Presentation.Controls.CustomFields;
using Roster.Presentation.Controls.Fields;
using Roster.Presentation.Helpers;
using System.Xml.Linq;

// ReSharper disable once CheckNamespace
namespace Roster.Services
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class DataService : BaseService, IDataService
    {
        private const int PageSize = 10;

        public RosterEventResult RosterEvents(RosterEventQuery query)
        {
            DisableCaching();
            var result = new RosterEventResult
            {
                Page = query.Page,
                Items = new List<RosterEventResultItem>()
            };
            try
            {
                var list = new RosterConfigService().GetList(query.ListId);
                if (list == null) {
                    throw new Exception(string.Format("List '{0}' not found", query.ListId));
                }

                var fieldId = list.ListMetadataFields.FirstOrDefault(item => item.InternalName == FieldNames.ROSTER_EVENT_ID);
                if (fieldId == null) {
                    throw new Exception("Field 'RosterEventId' not found");
                }

                var fieldTitle = list.ListMetadataFields.FirstOrDefault(item => item.InternalName == FieldNames.ROSTER_EVENT_TITLE);
                if (fieldTitle == null) {
                    throw new Exception("Field 'Title' not found");
                }
                result.Page = query.Page;
                var queryParams = new QueryParams { SkipRows = 0, TakeRows = PageSize };
                queryParams.SelectCriteria.AddRange(new[] { fieldId, fieldTitle });
                queryParams.OrderCriteria.Add(new Tuple<ListMetadataField, SortDirection, string>
                    (fieldTitle, SortDirection.Ascending, fieldTitle.InternalName));
                queryParams.WhereCriteria.Add(new Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>
                    (fieldTitle, CompareType.Contains, ConcateOperator.And, query.Query, null));
                var content = new RosterDataService().ListRosterEventProperties(list.Id, queryParams);
                foreach (var properties in content)
                {
                    var id = properties.First(item => item.Key == fieldId.InternalName).Value;
                    var value = properties.First(item => item.Key == fieldTitle.InternalName).Value;
                    result.Items.Add(new RosterEventResultItem { id = id.ToSafeString(), text = value.ToSafeString() });
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return result;
        }

        public ListResult List(ListQuery query)
        {
            DisableCaching();
            var result = new ListResult
            {
                Page = query.Page,
                Items = new List<ListResultItem>()
            };
            try
            {
                var view = new RosterConfigService().GetView(query.ViewId);
                var field = view.ListMetadata.ListMetadataFields.FirstOrDefault(item => item.Id == query.FieldId);
                if (field == null)
                {
                    throw  new Exception(string.Format("Field {0} not found",query.FieldId));
                }
                if (field.FieldType() == SPFieldType.Choice)
                {
                    result.Items.AddRange(field.FieldValues().Where(
                        item=>item.Contains(query.Query)).Select(item=> new ListResultItem { id = item, text = item }).ToList());
                }
                else
                {
                    result.Page = query.Page;
                    var queryParams = new QueryParams { SkipRows = 0, TakeRows = 1, Dictinct = true};
                    queryParams.SelectCriteria.Add(field);
                    queryParams.OrderCriteria.Add(new Tuple<ListMetadataField, SortDirection, string>
                        (field, SortDirection.Ascending, query.DisplayField.ExtractField()));
                    queryParams.WhereCriteria.Add(new Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>
                        (field, CompareType.Contains, ConcateOperator.And, query.Query, query.DisplayField.ExtractField()));
                    var content = new RosterDataService().ListRosterEventProperties(view.ListMetadataId, queryParams);
                    foreach (var value in content.Select(properties => properties.First(item => item.Key == query.DisplayField).Value))
                    {
                        result.Items.Add(new ListResultItem { id = value.ToSafeString(), text = value.ToSafeString() });
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return result;
        }

        private List<Tuple<string, string>> GetCriteria(LookupQuery query)
        {
            var whereCriteria = new List<Tuple<string, string>>();
            if (query.ParentKeyValue.ToInt() != 0)
            {
                var rosterConfigService = new RosterConfigService();
                var field = rosterConfigService.GetField(query.MetadataId.ToGuid());
                if (field == null)
                {
                    throw new Exception(string.Format("Field '{0}' not found", query.MetadataId));
                }
                var dbfield = field.GetDbField() as DbFieldLookup;
                if (dbfield != null)
                {
                    var list = rosterConfigService.GetList(field.ListMetadataId);
                    if (list == null)
                    {
                        throw new Exception(string.Format("List '{0}' not found", field.ListMetadataId));
                    }
                    var parentField =
                        list.ListMetadataFields.FirstOrDefault(item => item.InternalName == dbfield.DependentParent);
                    if (parentField != null)
                    {
                        if (dbfield.DependentParentField == parentField.DataSourceKey)
                        {
                            whereCriteria.Add(new Tuple<string, string>(dbfield.FilterByField, query.ParentKeyValue.ToSafeString()));
                        }
                        else
                        {
                            var parentResults = BLExtensions.SourceContent(parentField.DataSource,
                                parentField.DataSourceKey.ToSafeString(),
                                parentField.DataSourceField, parentField.DataSourceType,
                                new List<Tuple<string, string>>
                                {
                                    new Tuple<string, string>(parentField.DataSourceKey.ToSafeString(),
                                        query.ParentKeyValue.ToSafeString())
                                });
                            if (!parentResults.IsEmpty())
                            {
                                var firstOrDefault = parentResults.FirstOrDefault();
                                if (firstOrDefault != null)
                                {
                                    var parentResult = (firstOrDefault.Item2 as IDictionary<string, object>);
                                    if (parentResult.ContainsKey(dbfield.DependentParentField))
                                    {
                                        whereCriteria.Add(new Tuple<string, string>(dbfield.FilterByField, parentResult[dbfield.DependentParentField].ToSafeString()));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return whereCriteria;
        }

        public LookupFieldResult LookupField(LookupFieldQuery query)
        {
            DisableCaching();
            var result = new LookupFieldResult
            {
                Page = query.Page,
                Items = new List<LookupFieldResultItem>()
            };
            try
            {
                result.Page = query.Page;
                var queryTerm = query.Query.ToSafeString().ToLowerInvariant();
                var field = (SPContext.Current.SPList(query.ListId.ToGuid()).FieldById(query.FieldId.ToGuid()) as RosterTableLookupField);
                if (field != null)
                {
                    var content = new RosterDataService().TableContent(field.TableName, field.KeyName, field.FieldName).
                      Where(i => i.Item2.ToSeparatedString().ToLowerInvariant().Contains(queryTerm)).OrderBy(e => e.Item2.ToSeparatedString());
                    content.Take(PageSize).Skip(PageSize * result.Page).ToList().ForEach(item =>
                    {
                        var resultItem = new LookupFieldResultItem
                        {
                            id = item.Item1,
                            name = item.Item2.FirstValue(),
                        };
                        ((List<LookupFieldResultItem>)result.Items).Add(resultItem);
                    });                  
                }
            }
            catch (SqlException ex)
            {
                if (!ex.Message.Contains("Conversion failed"))
                {
                    HandleException(ex);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return result;
        }      

        public LookupResult Lookup(LookupQuery query)
        {
            DisableCaching();
            var result = new LookupResult
            {
                Page = query.Page,
                Items = new List<LookupResultItem>()
            };
            try
            {
                result.Page = query.Page;
                var queryTerm = query.Query.ToSafeString().ToLowerInvariant();
                var content = BLExtensions.SourceContent(query.Source, query.Key.ToSafeString(), query.Fields,
                    query.ListType, GetCriteria(query))
                    .OrderBy(e => e.Item2.FirstValue())
                    .Where(i => i.Item2.ToSeparatedString().ToLowerInvariant().Contains(queryTerm));
                content.Take(PageSize).Skip(PageSize*result.Page).ToList().ForEach(item =>
                {
                    var resultItem = new LookupResultItem
                    {
                        id = item.Item1,
                        name = item.Item2.FirstValue(),
                        description = string.Empty,
                        property = new List<LookupResultItemProperty>(),
                    };
                    foreach (var itemList in item.Item2)
                    {
                        resultItem.property.Add(new LookupResultItemProperty
                        {
                            Name = string.Format("{0}_{1}", query.Source, itemList.Key),
                            Value = itemList.Value.ToSafeString()
                        });
                    }
                    ((List<LookupResultItem>) result.Items).Add(resultItem);
                });
            }
            catch (SqlException ex)
            {
                if (!ex.Message.Contains("Conversion failed"))
                {
                    HandleException(ex);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return result;
        }

        public PopupContentResult PopupContent(PopupContentQuery query)
        {
            DisableCaching();
            var result = new PopupContentResult();
            try
            {
                result.Items = new List<PopupContentItem>();

                var view = (query.ViewId == Guid.Empty && query.ListId == TableIDs.WORKING_ROSTERS) ?
                    new RosterConfigService().GetList(query.ListId).ViewMetadatas.FirstOrDefault(vw => vw.Name == ViewNames.WORKING_ROSTERS_FOR_TIMESHEETS) :
                    new RosterConfigService().GetView(query.ViewId);

                if (view != null)
                {
                    var item = new RosterDataService().ListSingleRosterEvent(view.ListMetadataId, query.ItemId);
                    if (item != null)
                    {
                        var popupSettings = view.ViewMetadataPopupSettings.OrderBy(i=>i.Position).ToList();

                        // add ContentType ID for links
                        if (item.RosterEventDictionary.ContainsKey(FieldNames.CONTENT_TYPE_ID)) {
                            result.Items.Add(new PopupContentItem {
                                Name = "ContentTypeInternalId",
                                Text = item.RosterEventDictionary[FieldNames.CONTENT_TYPE_ID].ToSafeString()
                            });
                        }

                        popupSettings.ForEach(field =>
                        {
                            var listField = field.ListMetadataField;
                            if (listField.DataSourceType != (int) LookupSourceType.None)
                            {
                                var dataSourceFieldlistFields = listField.DataSourceField.Split('$').ToList();
                                dataSourceFieldlistFields.ForEach(fieldItem =>
                                {
                                    try {
                                        var fieldName = string.Format("{0}_{1}", listField.DataSource, fieldItem);
                                            result.Items.Add(new PopupContentItem {
                                                Name = dataSourceFieldlistFields.Count() == 1 ? listField.FieldName :
                                                        string.Format("{0}: {1}", listField.FieldName, fieldItem),
                                            Text = item.RosterEventDictionary[fieldName].ToSafeString()
                                        });
                                    } catch { }
                                });
                            }
                            else
                            {
                                result.Items.Add(new PopupContentItem
                                {
                                    Name = listField.FieldName,
                                    Text = item.RosterEventDictionary[listField.InternalName].ToSafeString()
                                });
                            }
                        });
                    }
                }
            }
            catch (Exception ex) 
            {
                HandleException(ex);
            }

            return result;
        }

        public void PublishPlannedRoster(PublishQuery query)
        {
            DisableCaching();
            try
            {
                BLExtensions.PublishRosterEvent(query.ItemId, query.DaysAhead);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        public void SaveRosterItemData(SaveItemDataQuery query)
        {
            DisableCaching();
            try
            {
                if (query == null) {
                    throw new ArgumentNullException("query");
                }

                var rosterConfigService = new RosterConfigService();
                if (!rosterConfigService.HasRights(query.ItemId, query.ListId, AccessRight.Write)) {
                    throw new Exception("You don't have permissions to modify current roster");
                }

                var rosterDataService = new RosterDataService();
                rosterDataService.SaveRosterEventItem(query.ListId, query.ItemId, query.FieldName, query.Value);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        public ExecuteActionResult ExecuteAction(ExecuteActionQuery query)
        {
            DisableCaching();
            var result = new ExecuteActionResult(); 
            try
            {
                if (query == null)
                {
                    throw new ArgumentNullException("query");
                }
                var dataService = new RosterDataService();
                {
                    var paramCollection = query.Parameters == null ? new List<Tuple<string, object>>():
                        query.Parameters.Select(item => new Tuple<string, object>(item.Name, item.Value)).ToList();

                    var currentUserParam = paramCollection.FirstOrDefault(p => p.Item1.Equals("@currentUser"));
                    if (currentUserParam != null) {
                        // change :: sharepoint user id -> roster user id
                        paramCollection.Remove(currentUserParam);
                        paramCollection.Add(new Tuple<string, object>("@currentUser", DbFieldUser.GetUserRosterId(SPContext.Current.Web.CurrentUser)));
                    }

                    result.Message = dataService.ExecuteProcedure(query.Name, paramCollection);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return result;
        }

        public ItemDataResult RosterItemData(ItemDataQuery query)
        {
            DisableCaching();
            var result = new ItemDataResult {
                Props = new List<ItemDataElem> ()
            };

            try
            {
                var roster = new RosterDataService().ListSingleRosterEvent(query.ListId, query.ItemId);

                if(roster != null)
                {
                    var origRosterProps = roster.RosterEventDictionary;
                    foreach(string fldKey in origRosterProps.Keys) {
                        bool isFilterByLookupId = false;
                        object val = origRosterProps[fldKey];
                        string valAsStr = string.Empty;
                        if (val is DateTime) {
                            valAsStr = ((DateTime)val).ToString("yyyy-MM-dd");
                        } else if (val is Boolean) {
                            valAsStr = (bool)val ? "1" : "0";
                        } else if (val is int) {
                            valAsStr = ((int)val).ToString("N" + 13);
                        } else {
                            valAsStr = val.ToSafeString();

                            if (valAsStr.StartsWith("<Items>")) {
                                valAsStr = string.Join(";#", valAsStr.XmlToList()); // choice values
                            }

                            // LookupId ?
                            int dummy;
                            isFilterByLookupId = val.ToSafeString().Split(',').Where(x => Int32.TryParse(x, out dummy)).Any();
                        }

                        ((List<ItemDataElem>)result.Props).Add(new ItemDataElem {
                            FieldName = fldKey,
                            FieldValue = SPHttpUtility.UrlKeyValueEncode(valAsStr),
                            FilterLookupId = isFilterByLookupId
                        });
                    }
                }
            }
            catch (Exception ex) {
                HandleException(ex);
            }

            return result;
        }

        public void SaveRosterAsTemplate(ItemDataQuery query)
        {
            DisableCaching();

            try
            {
                // init source Roster
                var sourceRoster = new RosterDataService().ListSingleRosterEvent(query.ListId, query.ItemId);
                var sourceRosterProps = sourceRoster.RosterEventDictionary;

                // create empty Template Roster
                RosterDataService _dataService = new RosterDataService();
                var newRosterTemplate = _dataService.CreateRosterEvent(Roster.Common.TableIDs.TEMPLATE_ROSTERS, (int)RosterEventType.TemplateRosterEvent);

                // fill template Roster properties
                var rosterTemplateFields = new RosterConfigService().GetList(Roster.Common.TableIDs.TEMPLATE_ROSTERS).ListMetadataFields;
                foreach (var field in rosterTemplateFields)
                {
                    if (field.InternalName == FieldNames.ROSTER_EVENT_ID || field.InternalName == FieldNames.ID ||
                        !sourceRosterProps.ContainsKey(field.InternalName)) { continue; }

                    newRosterTemplate.RosterEventDictionary[field.InternalName] = sourceRosterProps[field.InternalName];
                }

                // save template Roster
                _dataService.SaveRosterEvent(newRosterTemplate, Roster.Common.TableIDs.TEMPLATE_ROSTERS);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        public void RemoveRoster(ItemDataQuery query)
        {
            DisableCaching();
            try
            {
                if (!new RosterConfigService().HasRights(query.ItemId, query.ListId, AccessRight.Delete))
                    throw new Exception("You do not have permissions to remove current roster!");

                new RosterDataService().DeleteRosterEvent(query.ItemId);
            }
            catch (Exception ex) {
                HandleException(ex);
            }
        }

        public ListTemplatesResult ListTemplates(ListTemplatesQuery query)
        {
            DisableCaching();
            var result = new ListTemplatesResult {
                Page = query.Page,
                Items = new List<ListTemplatesResultItem>()
            };

            try
            {
                var templates = new RosterDataService().ListTemplates();
                string field_ID = new MasterRoster().PrimaryField;
                string field_Description = new MasterRoster().Description;

                foreach (var properties in templates) {
                    var id = properties.First(item => item.Key == field_ID).Value;
                    string value = properties.First(item => item.Key == field_Description).Value.ToSafeString();
                    if (value.ContainsIgnoreCase(query.Query)) {
                        result.Items.Add(new ListTemplatesResultItem { id = id.ToSafeString(), text = value });
                    }
                }
            }
            catch (Exception ex) {
                HandleException(ex);
            }

            return result;
        }

        public void SaveMasterAsTemplate(SaveAsTemplateQuery query)
        {
            DisableCaching();

            try 
            {
                // set flag IsTemplate for Master roster
                var _confService = new RosterConfigService();
                var _dataService = new RosterDataService();
                _dataService.SetAsTemplate(query.MasterId);

                // init filter by MasterRosterId
                var plannedRostersList = _confService.GetList(TableIDs.PLANNED_ROSTERS);
                var queryPar = new QueryParams();
                var masterRosterFld = plannedRostersList.ListMetadataFields.FirstOrDefault(item => item.InternalName == FieldNames.MASTER_ROSTER_ID);
                queryPar.WhereCriteria.Add(new Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>(masterRosterFld, CompareType.Equal, ConcateOperator.And, query.MasterId, null));

                // get Planned rosters by MasterRosterId
                var plannedRosters = _dataService.ListRosterEvents(plannedRostersList.Id, queryPar);

                // limit access to Planned rosters inside Template
                int rosterAdminGroupId = new RoleService().GetGroupIdForRole(Roster.Presentation.Constants.Role.RosterAdmins, SPContext.Current.Web);
                foreach(RosterEvent re in plannedRosters)
                {
                    List<AccessControlItem> reRights = _confService.GetItemAccessControls(re.Id);

                    // clear old
                    if (reRights != null && reRights.Any()) {
                        reRights.ForEach(ipItm => _confService.DeleteAccessControlItem(ipItm.Id));
                    }

                    // add 'Edit' to RosterAdmins
                    _confService.SaveAccessControlItem(new AccessControlItem {
                        AccessRight = (int)AccessRight.Write, Id = Guid.NewGuid(),
                        ItemId = re.Id, ListMetadataId = plannedRostersList.Id,
                        TrusteeId = rosterAdminGroupId
                    });

                    // add 'Read' to Everyone
                    _confService.SaveAccessControlItem(new AccessControlItem
                    {
                        AccessRight = (int)AccessRight.Read, Id = Guid.NewGuid(),
                        ItemId = re.Id, ListMetadataId = plannedRostersList.Id,
                        TrusteeId = RoleService.ACCOUNT_ID_EVERYONE
                    });
                }

            } catch (Exception ex) {
                HandleException(ex);
            }
        }

        public ExecuteActionResult SubmitTimesheet(SubmitTimesheetQuery query)
        {
            DisableCaching();
            var result = new ExecuteActionResult(); 
            try
            {
                result.Message = new DataHelpers().SubmitTimesheet(query.StoredProcedureName, query.WorkerId,query.RosterIDs, query.PeriodStart, query.PeriodEnd);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return result;
        }

        public ExecuteActionResult SubmitSingleTimesheet(ItemDataQuery query)
        {
            DisableCaching();
            var result = new ExecuteActionResult();
            try
            {
                result.Message = new DataHelpers().SubmitTimesheet(query.ListId, query.ItemId);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return result;
        }

        public GetContentTypeResult GetContentTypeByRosterId(GetContentTypeQuery query)
        {
            DisableCaching();
            var result = new GetContentTypeResult();
            try
            {
                var dataService = new RosterDataService();
                {
                    var roster = dataService.GetRosterEvent(query.RosterId);
                    if (roster != null)
                    {
                        var rosterProps = roster.RosterEventDictionary;
                        if (rosterProps.ContainsKey(FieldNames.CONTENT_TYPE_ID))
                        {
                            int ctId = rosterProps[FieldNames.CONTENT_TYPE_ID].ToInt();

                            var ct = new RosterConfigService().GetContentType(ctId);
                            if (ct != null)
                            {
                                result.DispItemUrl = ct.DispItemUrl;
                                result.EditItemUrl = ct.EditItemUrl;
                                result.NewItemUrl = ct.NewItemUrl;
                                result.Name = ct.Name;
                                result.Id = ct.Id;
                                result.IsDefault = ct.IsDefault;
                                result.IsOnNewAction = ct.IsOnNewAction;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return result;
        }

        public ExecuteActionResult EndorseRoster(ItemDataQuery query)
        {
            DisableCaching();
            var result = new ExecuteActionResult(); 
            try
            {
                 result.Message = new DataHelpers().EndorseRoster(query.ListId, query.ItemId);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return result;
        }

        public ExecuteActionResult RejectRoster(RejectRosterQuery query)
        {
            DisableCaching();
            var result = new ExecuteActionResult();
            try
            {
                result.Message = new DataHelpers().RejectRoster(query.ListId, query.ItemId, query.Reason);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return result;
        }

        public ExecuteActionResult ApproveRoster(ItemDataQuery query)
        {
            DisableCaching();
            var result = new ExecuteActionResult();
            try
            {
                result.Message = new DataHelpers().ApproveRoster(query.ListId, query.ItemId);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return result;
        }

        public ExecuteActionResult CancelRoster(ItemDataQuery query)
        {
            DisableCaching();
            var result = new ExecuteActionResult();
            try
            {
                result.Message = new DataHelpers().CancelRoster(query.ItemId);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return result;
        }

        public EnsureUserResult EnsureUser(EnsureUserQuery query)
        {
            DisableCaching();
            var result = new EnsureUserResult {
                RosterLookupId = -1,
                Key = "",
                DisplayText = ""
            };

            try {
                RosterConfigService configProvider = new RosterConfigService();
                ListMetadataField field = configProvider.GetField(query.FieldMetedataId.ToGuid());

                DbFieldUser dbFld = field.GetDbField() as DbFieldUser;
                var user = dbFld.EnsureUser(XDocument.Parse(query.XmlText));

                result.DisplayText = user.DisplayText;
                result.Key = user.Key;
                result.RosterLookupId = user.RosterLookupId;
            }
            catch (Exception ex) {
                HandleException(ex);
            }

            return result;
        }
    }
}
