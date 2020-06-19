using System;
using System.Linq;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Web.UI.WebControls;
using Roster.BL;
using System.Collections.Generic;
using System.Web.UI;
using Microsoft.SharePoint.Utilities;
using System.Web;
using Roster.Model.DataContext;
using Roster.Common;
using System.Runtime.Serialization;
using Roster.Presentation.Controls.Fields;
using Roster.Presentation.Extensions;
using System.Text;

namespace Roster.Presentation.Layouts
{
    public partial class ListMigrationPage : LayoutsPageBase
    {
        #region Private variables

        private readonly RosterConfigService _configService = new RosterConfigService();
        private readonly string[] _excludeListFields = new string[] { "AppAuthor", "AppEditor", "Attachments", "ContentType", "Edit",
            "FolderChildCount", "ID", "ItemChildCount", "LinkTitleNoMenu", "LinkTitle", "DocIcon", "_UIVersionString", "Facilities", "fRecurrence" };
        private const string LIST_FIELD_EDIT_URL_TEMPLATE = "javascript:SP.UI.ModalDialog.OpenPopUpPage('{0}/_layouts/15/FldEdit.aspx?List={1}&Field={2}');";
        private const string LIST_CT_EDIT_URL_TEMPLATE = "javascript:SP.UI.ModalDialog.OpenPopUpPage('{0}/_layouts/15/ManageContentType.aspx?List={1}&ctype={2}');";
        private const string LIST_FIELD_TOOLTIP_TEMPLATE = "Type: {0}; UsedIn: {1}";
        private const int LIST_ROW_LIMIT = 999;
        private readonly Dictionary<string, string> _predefinedMapping = new Dictionary<string, string>() {
            { "fAllDayEvent", "AllDayEvent" },
            { "Created", "Created" },
            { "Author", "CreatedBy" },
            { "Modified", "Modified" },
            { "Editor", "ModifiedBy" },
            { "EndDate", "EndDate" },
            { "EventDate", "StartDate" },
            { "RecurrenceData", "Recurrence" }
        };

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                try
                {                    
                    // init SharePoint Lists
                    ddlSharePointLists.DataSource =
                        SPContext.Current.Web.GetListsOfType(SPBaseType.GenericList).OfType<SPList>().Where(lst => !lst.Hidden).Select(item => new { Id = item.ID, Title = item.Title });
                    ddlSharePointLists.DataBind();

                    // init FIELDS repeater
                    this.RebindRepeater(ddlSharePointLists.SelectedValue, ddlRosterTables.SelectedValue);
                }
                catch (Exception ex)
                {
                    pnlError.Controls.Add(new Label { Text = ex.Message, ForeColor = System.Drawing.Color.Red });
                }
            }
        }

        #region Protected methods

        protected void ddlShPLists_SelectedIndexChanged(object sender, EventArgs e)
        {
            // re-init repeater
            this.RebindRepeater((sender as DropDownList).SelectedValue, ddlRosterTables.SelectedValue);
        }
        protected void ddlRosterTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            // re-init repeater
            this.RebindRepeater(ddlSharePointLists.SelectedValue, (sender as DropDownList).SelectedValue);
        }

        protected void ColumnsMapperRep_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var ddlCtrl = e.Item.FindControl("ddlTableFields");
                var dd = ddlCtrl as DropDownList;
                if (dd == null) return;
                dd.Items.AddRange(_configService.GetList(new Guid(ddlRosterTables.SelectedValue)).ListMetadataFields.OrderBy(fld => fld.FieldName)
                    .Select(fld => new ListItem(fld.FieldName, fld.InternalName)).ToArray());

                string predefinedColumnName = (string)DataBinder.Eval(e.Item.DataItem, "DBColumnIntName");
                if (!string.IsNullOrEmpty(predefinedColumnName)) {
                    dd.SelectedValue = predefinedColumnName;
                    dd.Enabled = (dd.SelectedIndex <= 0);
                }
            }
        }
        protected void ContentTypesMapperRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var ddlCtrl = e.Item.FindControl("ddlTableContentTypes");
                var dd = ddlCtrl as DropDownList;
                if (dd == null) return;
                dd.Items.AddRange(_configService.GetList(new Guid(ddlRosterTables.SelectedValue)).ListMetadataContentTypes.OrderByDescending(ct => ct.IsDefault)
                    .Select(ct => new ListItem(ct.Name, ct.Id.ToString())).ToArray());
            }
        }

        #endregion

        #region Web methods

        [System.Web.Services.WebMethod]
        public static MigrateResult MigrateItems(MigrateQuery query)
        {
            MigrateResult result = new MigrateResult { Success = true };

            try
            {
                bool needValidation = (string.IsNullOrEmpty(query.PaginInfo));
                RosterConfigService _configSrv = new RosterConfigService();
                SPList shpList = SPContext.Current.Web.Lists[new Guid(query.Mapping.ListId)];
                ListMetadata dbList = _configSrv.GetList(new Guid(query.Mapping.TableId));
                var dbList_Fields = dbList.ListMetadataFields;
                var _globalMapping = _configSrv.GetMapping();

                // collect and validate
                MigrationMapping mapping = new MigrationMapping(shpList, dbList);
                mapping.ContentTypeMapping.AddRange(query.Mapping.ContentTypeMapping.Select(m => new MigrationCtMappingElem {
                    ListCtId = m.ListCtId, TableCtId = m.TableCtId.ToInt()
                }));
                foreach (var mItm in query.Mapping.FieldMapping)
                {
                    ListMetadataField dbField = dbList_Fields.FirstOrDefault(fld => fld.InternalName.Equals(mItm.TableColumnName));
                    SPField listField = shpList.Fields.GetFieldByInternalName(mItm.ListFieldName);

                    if (needValidation && dbField.DataSourceType == (int)LookupSourceType.Table && !(listField is SPFieldUser)) {
                        SPFieldLookup shpFieldLookup = listField as SPFieldLookup;
                        if (shpFieldLookup == null) {
                            throw new Exception("You cannot map non-lookup field " + mItm.ListFieldName + " to Lookup column");
                        }
                        var _lookupFieldMapping = _globalMapping.FirstOrDefault(m => m.ListName == new Guid(shpFieldLookup.LookupList).ToString());
                        if (_lookupFieldMapping == null || _lookupFieldMapping.TableName != dbField.DataSource) {
                            throw new Exception(listField.Title + " field error. Mapping does not exist or Lookup lists don't match!");
                        }
                    }

                    bool listField_isMultiple =
                        (listField.Type == SPFieldType.Lookup   && (listField as SPFieldLookup).AllowMultipleValues) ||
                        (listField.Type == SPFieldType.User     && (listField as SPFieldLookup).AllowMultipleValues) ||
                        (listField.TypeAsString == "DualLookup" && (listField as SPFieldLookup).AllowMultipleValues);
                    mapping.FieldMapping.Add(new MigrationMappingElem {
                        ListField = listField, AllowMultipleValues = listField_isMultiple,
                        TableColumn = dbField, TableDbColumn = dbField.GetDbField()
                    });
                }

                if (needValidation) {
                    var repeatedShPFields = mapping.FieldMapping.GroupBy(x => x.TableColumn.InternalName).Where(gr => gr.Count() > 1).Select(gr => gr.Key);
                    if (repeatedShPFields.Any()) {
                        throw new Exception("Following DB columns selected more than once: " + string.Join(",", repeatedShPFields));
                    }
                }

                double step = Math.Round(((LIST_ROW_LIMIT * 100) / (double)mapping.List.ItemCount), 2);
                string _pagingInfo = Migrate(query.PaginInfo, mapping);
                result.PagingInfo = (string)_pagingInfo ?? string.Empty;
                result.CurrentIndex = (_pagingInfo == null) ? 100 : query.CurrentIndex + step;
            }
            catch (Exception ex) {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        #endregion

        #region Private methods

        private static string Migrate(string pagingInfo, MigrationMapping mapping)
        {
            string _pagingInfo = null;
            SPQuery query = new SPQuery {
                RowLimit = LIST_ROW_LIMIT,
                ViewFields = "<FieldRef Name='ContentTypeId'/>" + String.Join("", mapping.FieldMapping.Select(m => string.Format("<FieldRef Name='{0}'/>", m.ListField.InternalName))),
                Query = "<OrderBy><FieldRef Name='ID'/></OrderBy>"
            };
            if (!string.IsNullOrEmpty(pagingInfo)) {
                query.ListItemCollectionPosition = new SPListItemCollectionPosition(pagingInfo);
            }
            Dictionary<int, int> usersCache = new Dictionary<int, int>();
            usersCache.Add(1073741823, 1); // System Account !!!!!!!

            int eventType = (mapping.Table.Id == TableIDs.PLANNED_ROSTERS) ? 0 : 1;
            RosterDataService _dataService = new RosterDataService();
            SPSecurity.RunWithElevatedPrivileges(delegate() {
                using (SPSite eSite = new SPSite(mapping.List.ParentWeb.Url))
                using (SPWeb eWeb = eSite.OpenWeb())
                {
                    Dictionary<int, string> errors = new Dictionary<int, string>();

                    SPListItemCollection items = eWeb.Lists[mapping.List.ID].GetItems(query);
                    foreach (SPListItem itm in items)
                    {
                        var rosterEvent = _dataService.CreateRosterEvent(mapping.Table.Id, eventType);

                        try
                        {
                            foreach (var mapEl in mapping.FieldMapping)
                            {
                                if (itm[mapEl.ListField.InternalName] == null) { continue; }

                                object _val = null;
                                if (mapEl.ListField.Type == SPFieldType.Lookup || mapEl.ListField.TypeAsString == "DualLookup") {
                                    var lookupVal = (mapEl.AllowMultipleValues) ? itm.GetLookupValueCollection(mapEl.ListField)[0] : itm.GetLookupValue(mapEl.ListField);
                                    _val = (lookupVal.LookupId.ToInt() == 0 || lookupVal.LookupValue == null) ? null : (int?)lookupVal.LookupId;
                                } else if (mapEl.ListField.Type == SPFieldType.User) {
                                    SPFieldUserValue userVal = (mapEl.AllowMultipleValues) ?
                                        (itm[mapEl.ListField.InternalName] as SPFieldUserValueCollection)[0] : new SPFieldUserValue(eWeb, itm[mapEl.ListField.InternalName].ToString());
                                    if (usersCache.ContainsKey(userVal.LookupId)) {
                                        // get value from cache
                                        _val = usersCache[userVal.LookupId];
                                    } else {
                                        var dbUserFld = mapEl.TableDbColumn as DbFieldUser;
                                        if (dbUserFld == null) {
                                            _val = userVal.LookupValue;
                                        } else {
                                            int _valInt = dbUserFld.EnsureUser(userVal).RosterLookupId;
                                            usersCache.Add(userVal.LookupId, _valInt);
                                            _val = _valInt;
                                        }
                                    }
                                } else if (mapEl.ListField.Type == SPFieldType.MultiChoice) {
                                    SPFieldMultiChoiceValue vals = new SPFieldMultiChoiceValue(itm[mapEl.ListField.InternalName].ToString());
                                    List<string> choiceVals = new List<string>();
                                    for (int k = 0; k < vals.Count; k++) {
                                        choiceVals.Add(vals[k]);
                                    }
                                    _val = choiceVals.ListToXml();
                                } else {
                                    _val = itm[mapEl.ListField.InternalName];
                                }
                                rosterEvent.RosterEventDictionary[mapEl.TableColumn.InternalName] = _val;
                            }

                            rosterEvent.RosterEventDictionary[FieldNames.CONTENT_TYPE_ID] =
                                mapping.ContentTypeMapping.Where(ct => ct.ListCtId == itm["ContentTypeId"].ToString()).Select(ct => ct.TableCtId).FirstOrDefault();
                            _dataService.SaveRosterEvent(rosterEvent, mapping.Table.Id);
                        }
                        catch (Exception ex)
                        {
                            //StringBuilder exMsg = new StringBuilder();
                            //exMsg.AppendFormat("Exception: {0};\nMessage: {1};\nStackTrace: {2}", ex.GetType().Name, ex.Message, ex.StackTrace);

                            //Exception innerEx = ex;
                            //while (innerEx.InnerException != null) {
                            //    innerEx = innerEx.InnerException;
                            //    exMsg.AppendFormat("\n\nInnerException: {0};\nMessage: {1};\nStackTrace: {2}", innerEx.GetType().Name, innerEx.Message, innerEx.StackTrace);
                            //}

                            errors.Add(itm.ID, "Error: " + ex.Message + ". Dict: " +
                                String.Join(";", rosterEvent.RosterEventDictionary.Select(x => string.Format("[{0}]:{1}", x.Key, x.Value))));
                        }
                    }

                    if (errors.Any()) {
                        throw new Exception("Errors:<br/>" +
                            String.Join("<br/>", errors.Select(er => string.Format("  ListItemID: {0}. {1}", er.Key, er.Value))));
                    }

                    _pagingInfo = items.ListItemCollectionPosition == null ? null : items.ListItemCollectionPosition.PagingInfo; //Paged=TRUE&p_ID=100
                }
            });

            return _pagingInfo;
        }

        private void RebindRepeater(string list, string table)
        {
            SPList shpList = SPContext.Current.Web.Lists[new Guid(list)];
            var contentTypes = shpList.ContentTypes.OfType<SPContentType>();
            var fields = shpList.Fields.OfType<SPField>().Where(fld => (!fld.Hidden || fld.InternalName.Equals("RecurrenceData")) && !_excludeListFields.Contains(fld.InternalName));
            ColumnsMapperRep.DataSource = fields.Select(item => new
            {
                ListColumnTitle = item.Title,
                ListColumnInternalName = item.InternalName,
                ListColumnType = item.TypeAsString,
                ListColumnIsSupported = IsSharePointFieldSupportMapping(item),
                ListColumnEditPageUrl = string.Format(LIST_FIELD_EDIT_URL_TEMPLATE,
                    SPContext.Current.Web.Url.TrimEnd('/'), shpList.ID.ToString(), item.InternalName),
                ListColumnToolTip = string.Format(LIST_FIELD_TOOLTIP_TEMPLATE,
                    item.TypeAsString, String.Join(", ", contentTypes.Where(ct => ct.FieldLinks[item.Id] != null).Select(ct => ct.Name))),
                DBColumnIntName = (_predefinedMapping.ContainsKey(item.InternalName)) ? _predefinedMapping[item.InternalName] : ""
            }).OrderBy(item => item.ListColumnTitle);
            ColumnsMapperRep.DataBind();

            // init ContentTypes repeater
            ContentTypesMapperRepeater.DataSource = shpList.ContentTypes.OfType<SPContentType>().Select(ct => new {
                ListCtId = ct.Id.ToString(),
                ListCtTitle = ct.Name,
                ListCtEditPageUrl = string.Format(LIST_CT_EDIT_URL_TEMPLATE,
                    SPContext.Current.Web.Url.TrimEnd('/'), shpList.ID.ToString(), ct.Id.ToString()),
            });
            ContentTypesMapperRepeater.DataBind();
        }

        private static bool IsSharePointFieldSupportMapping(SPField fld)
        {
            if (fld.Type == SPFieldType.Attachments || fld.Type == SPFieldType.Calculated || fld.Type == SPFieldType.Computed || fld.Type == SPFieldType.ContentTypeId ||
                fld.Type == SPFieldType.CrossProjectLink || fld.Type == SPFieldType.File || fld.Type == SPFieldType.Geolocation || fld.Type == SPFieldType.GridChoice ||
                fld.Type == SPFieldType.MaxItems || fld.Type == SPFieldType.ModStat || fld.Type == SPFieldType.PageSeparator || fld.Type == SPFieldType.ThreadIndex ||
                fld.Type == SPFieldType.Threading || fld.Type == SPFieldType.WorkflowEventType)
            {
                return false;
            }
            else if (fld.Type == SPFieldType.Lookup)
            {
                return !(fld as SPFieldLookup).IsDependentLookup;
            }
            else
            {
                return true;
            }
        }

        private void GotoBackToSettingsPage()
        {
            SPUtility.Redirect("settings.aspx", SPRedirectFlags.Static | SPRedirectFlags.RelativeToLayoutsPage, HttpContext.Current, "");
        }

        #endregion
    }

    #region Dependand classes

    public class MigrationMapping
    {
        public SPList List { get; set; }
        public ListMetadata Table { get; set; }
        public List<MigrationMappingElem> FieldMapping { get; set; }
        public List<MigrationCtMappingElem> ContentTypeMapping { get; set; }

        public MigrationMapping(SPList list, ListMetadata table)
        {
            this.List = list;
            this.Table = table;
            this.FieldMapping = new List<MigrationMappingElem>();
            this.ContentTypeMapping = new List<MigrationCtMappingElem>();
        }
    }
    public class MigrationMappingElem
    {
        public SPField ListField { get; set; }
        public ListMetadataField TableColumn { get; set; }
        public DbField TableDbColumn { get; set; }
        public bool AllowMultipleValues { get; set; }
    }
    public class MigrationCtMappingElem
    {
        public string ListCtId { get; set; }
        public int TableCtId { get; set; }
    }

    [DataContract]
    public class MigrateResult
    {
        [DataMember]
        public bool Success { get; set; }
        [DataMember]
        public double CurrentIndex { get; set; }
        [DataMember]
        public string PagingInfo { get; set; }
        [DataMember]
        public string ErrorMessage { get; set; }
    }
    [DataContract]
    public class MigrateQuery
    {
        [DataMember]
        public string PaginInfo { get; set; }
        [DataMember]
        public double CurrentIndex { get; set; }
        [DataMember]
        public ClientMigrationMapping Mapping { get; set; }
    }

    [DataContract]
    public class ClientMigrationMapping
    {
        public string ListId { get; set; }
        public string TableId { get; set; }
        public List<ClientMigrationMappingElem> FieldMapping { get; set; }
        public List<ClientMigrationCtMappingElem> ContentTypeMapping { get; set; }

        public ClientMigrationMapping()
        {
            this.FieldMapping = new List<ClientMigrationMappingElem>();
            this.ContentTypeMapping = new List<ClientMigrationCtMappingElem>();
        }
        public ClientMigrationMapping(string listId, string tableId) : this()
        {
            this.ListId = listId;
            this.TableId = tableId;
        }
    }
    [DataContract]
    public class ClientMigrationMappingElem
    {
        public string ListFieldName { get; set; }
        public string TableColumnName { get; set; }
    }
    [DataContract]
    public class ClientMigrationCtMappingElem
    {
        public string ListCtId { get; set; }
        public string TableCtId { get; set; }
    }

    #endregion
}
