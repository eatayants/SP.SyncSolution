using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;
using Roster.BL;
using Roster.Common;
using Roster.Model.DataContext;
using Roster.Presentation.ColourCoding;
using System;
using System.Linq;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Roster.Presentation.Extensions;
using System.Text;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Globalization;
using Roster.Presentation.Helpers;
using Roster.Presentation.Controls;
using Roster.Presentation.Controls.Fields;
using Roster.Presentation.ColourCoding.Recurrence;
using System.Data;
using System.Collections.Specialized;
using Roster.BL.Extentions;
using Roster.Common.Helpers;

namespace Roster.Presentation.WebParts.ExternalListViewWebPart
{
    public partial class ExternalListViewWebPartUserControl : UserControl
    {
        #region Private variables

        private RosterConfigService configProvider = new RosterConfigService();
        private RosterDataService dataProvider = new RosterDataService();
        private ListMetadata m_list = null;
        private ViewMetadata m_view = null;
        private RosterClientContext clientContext = new RosterClientContext();
        private QueryParams _qsFilterParams = null;

        private DateOptions _currentDateOptions;

        #endregion

        protected Panel viewPanel;
        protected SPCalendarView spCalendarView;
        protected SPGridView2 spGridView;

        #region Public properties

        /// <summary>
        /// View Guid
        /// </summary>
        public string ViewId { get; set; }
        /// <summary>
        /// Displayed DB table
        /// </summary>
        public ListMetadata List
        {
            get
            {
                if (m_list == null && this.View != null) {
                    this.m_list = this.configProvider.GetList(this.View.ListMetadata.Id);
                }

                return this.m_list;
            }
        }
        /// <summary>
        /// Displayed View
        /// </summary>
        public ViewMetadata View
        {
            get
            {
                if (m_view == null && !string.IsNullOrEmpty(this.ViewId))
                {
                    this.m_view = this.configProvider.GetView(new Guid(this.ViewId));
                }

                return this.m_view;
            }
        }
        /// <summary>
        /// Filter expression from 'URL Filter WebPart'
        /// </summary>
        public string ConnectionFilterExpression { get; set; }
        /// <summary>
        /// Status IDs for filtering rosters
        /// </summary>
        public int[] StatusIDsForFiltering { get; set; }
        /// <summary>
        /// Filter expression from URL params
        /// </summary>
        public QueryParams FilterExpression
        {
            get
            {
                if (_qsFilterParams == null)
                {
                    NameValueCollection queryString = this.Request.QueryString;
                    var viewFields = this.List.ListMetadataFields;
                    _qsFilterParams = new QueryParams();

                    if (queryString != null && queryString.HasKeys())
                    {
                        Dictionary<int, string> filterFields = new Dictionary<int, string>();
                        Dictionary<int, string> filterValues = new Dictionary<int, string>();
                        foreach(string key in queryString.AllKeys)
                        {
                            string filterFieldName = string.Empty;
                            if (string.IsNullOrEmpty(key)) {
                                continue;
                            } else if (key.StartsWith("FilterFields")) {
                                int num = int.Parse(key.Substring(12), CultureInfo.InvariantCulture);
                                if (!filterFields.ContainsKey(num)) {
                                    filterFields.Add(num, queryString.GetValues(key)[0]);
                                }
                            } else if (key.StartsWith("FilterField")) {
                                int num = int.Parse(key.Substring(11), CultureInfo.InvariantCulture);
                                if (!filterFields.ContainsKey(num)) {
                                    filterFields.Add(num, queryString.GetValues(key)[0]);
                                }
                            } else if (key.StartsWith("FilterValues")) {
                                int num = int.Parse(key.Substring(12), CultureInfo.InvariantCulture);
                                if (!filterValues.ContainsKey(num)) {
                                    filterValues.Add(num, queryString.GetValues(key)[0]);
                                }
                            } else if (key.StartsWith("FilterValue")) {
                                int num = int.Parse(key.Substring(11), CultureInfo.InvariantCulture);
                                if (!filterValues.ContainsKey(num)) {
                                    filterValues.Add(num, queryString.GetValues(key)[0]);
                                }
                            }
                        }

                        // get information about how to concat values inside ONE filter
                        var concatOperationsInsideOnefilter = this.FilterOperators;
                        
                        foreach(int key in filterFields.Keys)
                        {
                            string[] filterField_nameParts = filterFields[key].Split('$');
                            string fldName = filterField_nameParts[0];
                            string fldFilterPart = (filterField_nameParts.Length == 2) ? filterField_nameParts[1] : null;
                            var filterFld = viewFields.Where(f => f.InternalName == fldName || f.Id == fldName.ToGuid()).FirstOrDefault();
                            string filterVal = filterValues.ContainsKey(key) ? filterValues[key] : string.Empty;

                            string[] _vals = filterVal.Split(new string[] { ";#" }, StringSplitOptions.RemoveEmptyEntries);

                            var _dbFieldType = filterFld.GetDbField().Type;
                            object queryVal = null;
                            if (_vals.Length > 1)
                            {
                                queryVal = _vals.Select(v => SPHttpUtility.UrlKeyValueDecode(v) as object);
                                if (_dbFieldType == SPFieldType.DateTime)
                                    queryVal = _vals.Select(v => DateTime.Parse(v) as object);
                                else if (_dbFieldType == SPFieldType.Boolean)
                                    queryVal = _vals.Select(v => (v == "Yes") as object);
                            }
                            else
                            {
                                queryVal = SPHttpUtility.UrlKeyValueDecode(filterVal);
                                if (_dbFieldType == SPFieldType.DateTime)
                                    queryVal = DateTime.ParseExact(queryVal.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                                else if (_dbFieldType == SPFieldType.Boolean)
                                    queryVal = (filterVal == "Yes");
                            }

                            bool fOperExists = concatOperationsInsideOnefilter.ContainsKey(filterFld.InternalName);
                            if (fOperExists && _vals.Length > 1 && concatOperationsInsideOnefilter[filterFld.InternalName] == FieldFilterOperator.AND)
                            {
                                // applied to multi-value fields, when need to find all filter occurence in field value
                                (queryVal as IEnumerable<object>).ToList().ForEach(v => _qsFilterParams.WhereCriteria.Add(
                                        new Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>(filterFld, CompareType.Equal, ConcateOperator.And, v, fldFilterPart)));
                            }
                            else if (fOperExists && concatOperationsInsideOnefilter[filterFld.InternalName] == FieldFilterOperator.NOT)
                            {
                                // filter like: NOT(a) and NOT(b) and NOT(c)
                                var enumVal = queryVal as IEnumerable<object>;
                                CompareType compType = enumVal == null ? CompareType.NotEqual : CompareType.NotInValue;
                                if (compType == CompareType.NotInValue && filterFld.FieldType() == SPFieldType.Choice) {
                                    // NOT-IN for Choice fields
                                    enumVal.ToList().ForEach(v => _qsFilterParams.WhereCriteria.Add(
                                        new Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>(filterFld, CompareType.NotEqual, ConcateOperator.And, v, fldFilterPart)));
                                } else {
                                    _qsFilterParams.WhereCriteria.Add(new Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>(
                                            filterFld, compType, ConcateOperator.And, queryVal, fldFilterPart));
                                }
                            }
                            else
                            {
                                if (_dbFieldType == SPFieldType.DateTime)
                                {
                                    // set period for filter by DateTime field
                                    _qsFilterParams.WhereCriteria.Add(new Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>(
                                        filterFld, CompareType.MoreOrEqual, ConcateOperator.And, queryVal, null));
                                    _qsFilterParams.WhereCriteria.Add(new Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>(
                                        filterFld, CompareType.LessOrEqual, ConcateOperator.And, ((DateTime)queryVal).AddDays(1).AddMilliseconds(-1), null));
                                }
                                else
                                {
                                    // general logic: multiple values compared by IN; single value compared by EQUALS
                                    CompareType compType = _vals.Length > 1 ? CompareType.InValue : CompareType.Equal;
                                    if (compType == CompareType.InValue && filterFld.FieldType() == SPFieldType.Choice) {
                                        // IN operator for Choice fields
                                        (queryVal as IEnumerable<object>).ToList().ForEach(v => _qsFilterParams.WhereCriteria.Add(
                                            new Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>(filterFld, CompareType.Equal, ConcateOperator.Or, v, fldFilterPart)));
                                    } else {
                                        _qsFilterParams.WhereCriteria.Add(new Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>(
                                            filterFld, compType, ConcateOperator.And, queryVal, fldFilterPart));
                                    }
                                }
                            }
                        }
                    }

                    #region Add filter from URL param (by WebPart connector). It is a filter by parent element

                    if (!string.IsNullOrEmpty(this.ConnectionFilterExpression)) {
                        _qsFilterParams.WhereCriteria.AddRange(Utils.GetWhereCriteriaFromFilterExpression(this.m_view, this.ConnectionFilterExpression));
                    }

                    #endregion

                    #region Add filter ONLY for Dynamic colour-coding

                    if (!string.IsNullOrEmpty(this.RelationField) && this.FilteredIDs != null && this.FilteredIDs.Any())
                    {
                        var relationFld = this.m_list.ListMetadataFields.FirstOrDefault(item => item.InternalName == this.RelationField);
                        if (relationFld == null)
                            throw new Exception(string.Format("Relation field '{0}' is incorrect. Please set correct name in WebPart props.", this.RelationField));

                        _qsFilterParams.WhereCriteria.Add(new Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>
                            (relationFld, CompareType.InValue, ConcateOperator.And, FilteredIDs.Select(x => x as object), null));
                    }

                    #endregion

                    #region Add filters by StatusID - from ManagerViewFilterWebPart

                    if (this.StatusIDsForFiltering.Length > 0)
                    {
                        var statusFld = viewFields.Where(f => f.InternalName == FieldNames.STATUS_ID).FirstOrDefault();
                        if (statusFld != null) {
                            _qsFilterParams.WhereCriteria.Add(new Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>(
                                            statusFld, CompareType.InValue, ConcateOperator.And, this.StatusIDsForFiltering.Select(v => v as object), null));
                        }
                    }

                    #endregion
                }

                return _qsFilterParams;
            }
        }
        /// <summary>
        /// Filter concat operations from SQL filter webPart
        /// </summary>
        public Dictionary<string, FieldFilterOperator> FilterOperators { get; set; }

        /// <summary>
        /// Original Roster ID - used only on dynamic colour-coding
        /// </summary>
        public Guid OriginalRosterId
        {
            get
            {
                return Request.QueryString["OrigSchiftId"] == null ? Guid.Empty : Request.QueryString["OrigSchiftId"].ToGuid();
            }
        }
        public int[] FilteredIDs
        {
            get
            {
                string param = Page.Request.QueryString["FilteredIDs"];

                if (!string.IsNullOrEmpty(param)) {
                    var res = Page.Request.QueryString["FilteredIDs"].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    return res.Select(x => Int32.Parse(x)).ToArray();
                } else {
                    return new int[0];
                }
            }
        }
        public string RelationField { get; set; }

        public int CalendarHeight { get; set; }
        public int CalendarWidth { get; set; }
        public string CalendarPeriod
        {
            get
            {
                if (this.View.ListMetadataId == Roster.Common.TableIDs.TIMESHEET_ROSTERS)
                    return "week"; // !!! timesheets always displayed in a WEEK period !!!

                string viewType = SPUtility.GetViewType(Request);
                if (string.IsNullOrEmpty(viewType)) {
                    viewType = !string.IsNullOrEmpty(this.View.CalendarScope) ? this.View.CalendarScope.ToLower() : "month";
                }
                return viewType;
            }
        }

        public bool EditMode { get; set; }
        public bool DisplayPrepopulatedRosters { get; set; }
        
        public List<QuickEditColumnProperties> QuickEditSettings
        {
            get
            {
                List<QuickEditColumnProperties> settings = new List<QuickEditColumnProperties>();

                if (!this.EditMode)
                    return settings; // generic Mode -> no settings

                var listFields = this.List.ListMetadataFields.ToList();
                var fields = this.View.ViewMetadataFields.OrderBy(x => x.Position).Select(x => listFields.First(f => f.Id == x.ListMetadataFieldId).GetDbField());

                int dataIndex = 0;
                foreach (var fld in fields)
                {
                    var qecp = new QuickEditColumnProperties { name = fld.InternalName, readOnly = (fld.ReadOnly || fld.Hidden), data = dataIndex++ };

                    switch (fld.Type)
                    {
                        case SPFieldType.Lookup:
                            var lookupFld = fld as DbFieldLookup;
                            qecp.EditorType = Constants.EditableType.Select2;
                            var parentObject =  settings.FirstOrDefault(item => item.name == lookupFld.DependentParent);
                            qecp.select2LookupOptions = new Select2LookupOptions(lookupFld);
                            qecp.select2LookupOptions.ParentKeyValue = "";
                            qecp.select2LookupOptions.ParentData = parentObject == null ? -1 : parentObject.data;
                            qecp.renderer = "lookupRenderer";
                            // hide additional fields from Quick-Edit view
                            dataIndex += lookupFld.LookupFields.Skip(1).Count();
                            break;
                        case SPFieldType.User:
                            var userFld = fld as DbFieldUser;
                            qecp.EditorType = Constants.EditableType.UserOrGroup;
                            qecp.userLookupOptions = new UserLookupOptions(userFld);
                            qecp.renderer = "lookupRenderer";
                            // hide additional fields from Quick-Edit view
                            dataIndex += userFld.LookupFields.Skip(1).Count();
                            break;
                        case SPFieldType.Number:
                            qecp.EditorType = Constants.EditableType.Numeric;
                            qecp.renderer = qecp.editor;
                            qecp.DataType = Constants.EditableType.Number;
                            break;
                        case SPFieldType.Boolean:
                            qecp.DataType = Constants.EditableType.Checkbox;
                            qecp.EditorType = Constants.EditableType.Checkbox;
                            qecp.renderer = qecp.editor;
                            qecp.checkedTemplate = "Yes";
                            qecp.uncheckedTemplate = "No";
                            break;
                        case SPFieldType.Choice:
                            var choiceFld = fld as DbFieldChoice;
                            qecp.selectOptions = choiceFld.Choices;
                            qecp.EditorType = (choiceFld.ControlType != "Checkboxes") ? Constants.EditableType.Select : Constants.EditableType.Checklist;
                            qecp.renderer = "text";
                            break;
                        case SPFieldType.DateTime:
                            var dateFld = fld as DbFieldDateTime;
                            qecp.EditorType = Constants.EditableType.Date;
                            qecp.datePickerConfig = new DatePickerOptions { showTime = (dateFld.Format == "DateTime") };
                            qecp.dateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.Replace("yyyy", "YYYY").Replace("d", "D"); // replace for 'moment.js' format
                            if (dateFld.Format == "DateTime") {
                                qecp.dateFormat += " " + CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.Replace("tt", "A");
                            }
                            qecp.renderer = "autocomplete";
                            break;
                        case SPFieldType.Recurrence:
                            qecp.readOnly = true;
                            break;
                        default:
                            qecp.EditorType = Constants.EditableType.Text;
                            break;
                    }

                    settings.Add(qecp);
                }

                return settings;
            }
        }

        public Tuple<DateTime, DateTime> CalendarViewPeriod
        {
            get
            {
                return Utils.GetCalendarViewPeriod(SPContext.Current.Web.GetDateOptions(Request), this.CalendarPeriod);
            }
        }

        public string ParentWebPartId { get; set; }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack && hidFiltersHistory.Value == "filterAction") {
                // reset paging AFTER filtering
                hidFiltersHistory.Value = "";
                spGridView.PageIndex = 0;
            }
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            try
            {
                if (this.View == null)
                    throw new WebPartConfigException("Open the tool pane to select List and View.");

                SPWeb web = SPContext.Current.Web;

                viewPanel = new Panel { ID = "rosterViewPanel", CssClass = "view-main-panel" };
                Controls.Add(viewPanel);

                #region Init 'New Item' button

                object _parentElemId = null;
                Dictionary<string, string> additionalUrlParam = new Dictionary<string,string>();
                if (!string.IsNullOrEmpty(this.ConnectionFilterExpression)) {
                    string[] fVals = this.ConnectionFilterExpression.Split(new string[] { " = " }, StringSplitOptions.None);
                    if (fVals.Length > 1) {
                        _parentElemId = fVals[1].Trim(new char[] { '\'' });
                        additionalUrlParam.Add(fVals[0], _parentElemId.ToSafeString());
                    }
                }

                // login of this button will be used FROM CUSTOM RIBBON BUTTON
                DbNewItemButton addNewLink = new DbNewItemButton() { List = this.List, AdditionalUrlParams = additionalUrlParam, ParentElemId = _parentElemId, CssClass = "always-hidden" };
                addNewLink.Visible = !string.IsNullOrEmpty(this.ConnectionFilterExpression) &&
                    (this.List.Id != TableIDs.STATUS_HISTORY); // visible only if Connection to parent exists and NOT Status list

                #endregion

                if (this.View.IsCalendarView())
                {
                    // DISPLAY CALENDAR VIEW
                    short calendarType = web.RegionalSettings.CalendarType;

                    #region Init Calendar

                    spCalendarView = new SPCalendarView();
                    spCalendarView.EnableViewState = true;
                    spCalendarView.EnableV4Rendering = web.UIVersion >= 4;
                    spCalendarView.NewItemFormUrl = this.List.NewItemUrl;
                    spCalendarView.EditItemFormUrl = this.List.EditItemUrl;
                    spCalendarView.DisplayItemFormUrl = this.List.DispItemUrl;

                    string viewType = this.CalendarPeriod;
                    string selectedDate = SPUtility.GetSelectedDate(Request, web);

                    spCalendarView.ViewType = viewType;
                    spCalendarView.SelectedDate = selectedDate;

                    // Bind the datasource to the SPCalendarView
                    SPGridView2DataSource calendarDS = new SPGridView2DataSource(this.View, this.FilterExpression, this.OriginalRosterId, this.DisplayPrepopulatedRosters);
                    object rostersForColoring = null;
                    spCalendarView.DataSource = calendarDS.SelectDataForCalendar(calendarType, this.CalendarViewPeriod, viewType, out rostersForColoring);
                    spCalendarView.DataBind();

                    // register CSS before adding CalendarControl
                    this.RegisterCssStylesForColourCoding(this.View);
                    this.RegisterColourCodingSettings(this.View, rostersForColoring);
                    this.ResizeCalendar(this.CalendarWidth, this.CalendarHeight);

                    #endregion

                    #region TIMESHEET addons

                    if (this.List.Id == TableIDs.TIMESHEET_ROSTERS) {
                        viewPanel.CssClass += " timesheet-holder"; // hide Day/Week/Month selector
                    }

                    #endregion

                    viewPanel.Controls.Add(addNewLink);
                    viewPanel.Controls.Add(new HtmlGenericControl("br"));
                    viewPanel.Controls.Add(spCalendarView);
                }
                else
                {
                    // DISPLAY GRID VIEW

                    #region Init Grid columns

                    var listFields = this.List.ListMetadataFields;
                    var fields = this.View.ViewMetadataFields.OrderBy(x => x.Position).Select(x => listFields.First(f => f.Id == x.ListMetadataFieldId).GetDbField());
                    var additionalFields = this.View.ExcludeFields == null ? new string[0] : this.View.ExcludeFields.Split(new string[] { ";#" }, StringSplitOptions.RemoveEmptyEntries);

                    spGridView = new SPGridView2();
                    spGridView.ID = "myRostersGridView";
                    spGridView.EnableViewState = false;
                    spGridView.AutoGenerateColumns = false;
                    spGridView.EmptyDataText = @"There are no items to show in this view";
                    spGridView.HeaderStyle.HorizontalAlign = HorizontalAlign.Left;
                    spGridView.AllowSorting = true;
                    spGridView.PageSize = this.View.ItemLimit;
                    spGridView.AllowPaging = true;
                    spGridView.Sorting += new GridViewSortEventHandler(spGridView_Sorting);
                    spGridView.ShowHeaderWhenEmpty = true;
                    spGridView.RowDataBound += new GridViewRowEventHandler(GridView_RowDataBound);
                    spGridView.AllowFiltering = true;
                    spGridView.FilterDataFields = String.Join(",", fields.SelectMany(f => {
                        if (f.Type == SPFieldType.Note || f.Type == SPFieldType.Recurrence) {
                            return new [] { string.Empty };
                        } else if (f.Type == SPFieldType.Lookup) {
                            var lookupFld = f as DbFieldLookup;
                            return lookupFld.LookupFields
                                .Select((item, index) => new { Item = string.Format("{0}${1}", f.Id.ToString(), item), Position = index, Name = item })
                                .Where(x => x.Position == 0 || (x.Position > 0 && additionalFields.Contains(lookupFld.ListId + "_" + x.Name)))
                                .Select(x => x.Item);
                        } else if (f.Type == SPFieldType.User) {
                            var userFld = f as DbFieldUser;
                            return userFld.LookupFields
                                .Select((item, index) => new { Item = string.Format("{0}${1}", f.Id.ToString(), item), Position = index, Name = item })
                                .Where(x => x.Position == 0 || (x.Position > 0 && additionalFields.Contains(userFld.ListId + "_" + x.Name)))
                                .Select(x => x.Item);
                        } else {
                            return new [] { f.InternalName };
                        }
                    }).ToArray());

                    if (this.EditMode) { spGridView.FilterDataFields += ","; } // add no-filter for last ID column

                    int fldIndex = 1;
                    var mappingCollForLookups = configProvider.GetMapping();
                    foreach(var fld in fields)
                    {
                        string sortExp = (fld.Type == SPFieldType.Note || fld.Type == SPFieldType.Recurrence) ? string.Empty : fld.InternalName;

                        if (fldIndex == 1 && fld.Type != SPFieldType.Lookup && fld.Type != SPFieldType.User && !this.EditMode && (this.List.Id != TableIDs.STATUS_HISTORY))
                        {
                            #region Add menu ONLY on first column

                            var listIdParam = "ListId=" + SPEncode.UrlEncode(List.Id.ToString("B").ToUpper());

                            var colMenu = new SPMenuField {
                                HeaderText = fld.DisplayName,
                                TextFields = fld.InternalName,
                                SortExpression = sortExp,
                                MenuTemplateId = "EventListMenu",
                                NavigateUrlFields = "ID",
                                NavigateUrlFormat = string.Format("{0}/{1}&{2}&{3}",
                                    web.ServerRelativeUrl.TrimEnd('/'), List.DispItemUrl.TrimStart('/'), listIdParam, "ID={0}"),
                                TokenNameAndValueFields = "RosterId=ID"
                            };

                            MenuTemplate menuTempl = new MenuTemplate { ID = "EventListMenu" };
                            var viewItem = new MenuItemTemplate("View Item") {
                                ClientOnClickNavigateUrl = string.Format("javascript:SP.UI.ModalDialog.ShowPopupDialog('{0}/{1}&{2}&{3}')",
                                    web.ServerRelativeUrl.TrimEnd('/'), List.DispItemUrl.TrimStart('/'), listIdParam, "ID=%RosterId%")
                            };
                            menuTempl.Controls.Add(viewItem);
                            var editItem = new MenuItemTemplate("Edit Item") {
                                ClientOnClickNavigateUrl = string.Format("javascript:SP.UI.ModalDialog.ShowPopupDialog('{0}/{1}&{2}&{3}')",
                                    web.ServerRelativeUrl.TrimEnd('/'), List.EditItemUrl.TrimStart('/'), listIdParam, "ID=%RosterId%")
                            };
                            menuTempl.Controls.Add(editItem);

                            spGridView.Columns.Add(colMenu);
                            this.Controls.Add(menuTempl);
                            fldIndex++;

                            #endregion
                        }
                        else
                        {
                            if (fld.Type == SPFieldType.Lookup)
                            {
                                #region Init lookup fields

                                var lookupFld = fld as DbFieldLookup;
                                if (lookupFld == null) continue;
                                var listId = lookupFld.ListId.ToGuid();
                                var spList = lookupFld.ListSource == (int) LookupSourceType.SpList ? 
                                        SPContext.Current.SPList(listId) : null;
                                
                                if (!lookupFld.ListId.IsGuid())
                                {
                                    var map = mappingCollForLookups.FirstOrDefault(m => m.TableName == lookupFld.ListId);
                                    if (map != null)
                                    {
                                        listId = map.ListName.ToGuid();
                                    }
                                }
                                if (this.EditMode)
                                {
                                    // plain lookup value [ {id};#{value} ]
                                    lookupFld.LookupFields.ForEach(item =>
                                    {
                                        var dataField = string.Format("{0}_{1}", lookupFld.ListId, item);
                                        var subDisplayName = spList == null ? item : spList.FieldTitle(item.ToGuid());
                                        spGridView.Columns.Add(new TemplateField {
                                            HeaderText = lookupFld.LookupFields.Count() == 1 ? fld.DisplayName : string.Format("{0} : {1}", fld.DisplayName, subDisplayName),
                                            ItemTemplate = new PlainLookupFieldTemplate(fld.InternalName, dataField),
                                            SortExpression = string.Format("{0}${1}", lookupFld.Id.ToString(), item)
                                        });
                                    });
                                }
                                else if (listId == Guid.Empty)
                                {
                                    // Lookup field withOUT mapping
                                    int num = 1;
                                    foreach (var item in lookupFld.LookupFields)
                                    //lookupFld.LookupFields.ForEach(item =>
                                    {
                                        if (num > 1 && !additionalFields.Contains(lookupFld.ListId + "_" + item)) { continue; }

                                        var dataField = string.Format("{0}_{1}", lookupFld.ListId, item);
                                        var subDisplayName = spList == null ? item : spList.FieldTitle(item.ToGuid());
                                        spGridView.Columns.Add(new BoundField {
                                            HeaderText = lookupFld.LookupFields.Count() == 1 ? fld.DisplayName : string.Format("{0} : {1}", fld.DisplayName, subDisplayName),
                                            DataField = dataField,
                                            SortExpression = string.Format("{0}${1}", lookupFld.Id.ToString(), item)
                                        });
                                        num++;
                                    }
                                }
                                else
                                {
                                    // Lookup field with mapping
                                    lookupFld.LookupFields.ForEach(item =>
                                    {
                                        var dataField = string.Format("{0}_{1}", lookupFld.ListId, item);
                                        var subDisplayName = spList == null ? item : spList.FieldTitle(item.ToGuid());
                                        spGridView.Columns.Add(new HyperLinkField {
                                            HeaderText = lookupFld.LookupFields.Count() == 1 ? fld.DisplayName : string.Format("{0} : {1}", fld.DisplayName, subDisplayName),
                                            DataTextField = dataField,
                                            DataNavigateUrlFields = new[] { fld.InternalName },
                                            DataNavigateUrlFormatString = web.Lists[listId].Forms[PAGETYPE.PAGE_DISPLAYFORM].ServerRelativeUrl + "?ID={0}",
                                            SortExpression = string.Format("{0}${1}", lookupFld.Id.ToString(), item)
                                        });
                                    });
                                }

                                #endregion
                            }
                            else if (fld.Type == SPFieldType.User)
                            {
                                #region Init User fields

                                var userFld = fld as DbFieldUser;
                                if (userFld == null) continue;

                                if (this.EditMode)
                                {
                                    // plain lookup value [ {id};#{value} ]
                                    int num = 1;
                                    foreach (var item in userFld.LookupFields)
                                    //userFld.LookupFields.ForEach(item =>
                                    {
                                        if (num > 1 && !additionalFields.Contains(userFld.ListId + "_" + item)) { continue; }

                                        var dataField = string.Format("{0}_{1}", userFld.ListId, item);
                                        spGridView.Columns.Add(new TemplateField {
                                            HeaderText = userFld.LookupFields.Count() == 1 ? fld.DisplayName : string.Format("{0} : {1}", fld.DisplayName, item),
                                            ItemTemplate = new PlainLookupFieldTemplate(fld.InternalName, dataField),
                                            SortExpression = string.Format("{0}${1}", userFld.Id.ToString(), item)
                                        });
                                        num++;
                                    };
                                }
                                else
                                {
                                    userFld.LookupFields.ForEach(item =>
                                    {
                                        var dataField = string.Format("{0}_{1}", userFld.ListId, item);
                                        spGridView.Columns.Add(new BoundField {
                                            HeaderText = userFld.LookupFields.Count() == 1 ? fld.DisplayName : string.Format("{0} : {1}", fld.DisplayName, item),
                                            DataField = dataField,
                                            SortExpression = string.Format("{0}${1}", userFld.Id.ToString(), item)
                                        });
                                    });
                                }

                                #endregion
                            }
                            else if (fld.Type == SPFieldType.DateTime)
                            {
                                spGridView.Columns.Add(new BoundField {
                                    HeaderText = fld.DisplayName,
                                    DataField = fld.InternalName,
                                    SortExpression = sortExp,
                                    DataFormatString = ((fld as DbFieldDateTime).Format == "DateOnly") ? "{0:d}" : "{0:g}"
                                });
                            }
                            else
                            {
                                spGridView.Columns.Add(new BoundField {
                                    HeaderText = fld.DisplayName,
                                    DataField = fld.InternalName,
                                    SortExpression = sortExp
                                });
                            }
                        }
                    }

                    // add ID column for Edit mode
                    if (this.EditMode) {
                        spGridView.Columns.Add(new BoundField { HeaderText = "ID", DataField = "ID" });
                    }

                    #endregion

                    viewPanel.Controls.Add(addNewLink);
                    viewPanel.Controls.Add(new HtmlGenericControl("br"));
                    viewPanel.Controls.Add(spGridView);

                    #region QuickEditMode additions

                    if (this.EditMode) {
                        viewPanel.CssClass += " tbl-original-hidden"; // hide original GridView using css

                        viewPanel.Controls.Add(new HtmlGenericControl("div") { ID = "QuickEditTable", ClientIDMode = System.Web.UI.ClientIDMode.Static });
                    }

                    #endregion

                    #region DataSource AND Pager AND Default View Filter

                    var gridDS = spGridView.SetObjectDataSource("rostersGridDS", "SelectData", "SelectRowsCount", new SPGridView2DataSource(this.View, new QueryParams()), this.View);
                    spGridView.GridFilterExpression = this.FilterExpression;
                    viewPanel.Controls.Add(gridDS);

                    SPGridViewPager pager = new SPGridViewPager();
                    pager.ID = "gridViewMyPager";
                    pager.GridViewId = spGridView.ID;
                    viewPanel.Controls.Add(pager);

                    #endregion
                }

                // add Roster client Contex to the Page
                this.AddRosterJsContext();
            }
            catch (WebPartConfigException ex)
            {
                Controls.Add(ex.GetHtmlControl(this.ParentWebPartId));

                if (this.View != null && !this.View.IsCalendarView()) {
                    // add empty DataSource
                    var emptyDS = spGridView.SetObjectDataSource("rostersGridDS", "SelectEmptyData", "SelectEmptyDataCount", new SPGridView2DataSource(this.View, new QueryParams()), this.View);
                    viewPanel.Controls.Add(emptyDS);
                }
            }
            catch (Exception ex)
            {
                Controls.Add(new Label() { Text = ex.Message, ForeColor = System.Drawing.Color.Red });
                Controls.Add(new Label() { Text = " StackTrace: " + ex.StackTrace, ForeColor = System.Drawing.Color.Red });

                if (this.View != null && !this.View.IsCalendarView()) {
                    // add empty DataSource
                    var emptyDS = spGridView.SetObjectDataSource("rostersGridDS", "SelectEmptyData", "SelectEmptyDataCount", new SPGridView2DataSource(this.View, new QueryParams()), this.View);
                    viewPanel.Controls.Add(emptyDS);
                }
            }
        }

        protected void spGridView_Sorting(object sender, GridViewSortEventArgs e)
        {
            string lastExpression = ViewState["SortExpression"] == null ? "" : ViewState["SortExpression"].ToString();
            string lastDirection = ViewState["SortDirection"] == null ? "asc" : ViewState["SortDirection"].ToString();

            string newDirection = e.SortExpression != lastExpression ? "asc" : ((lastDirection == "asc") ? "desc" : "asc");

            ViewState["SortExpression"] = e.SortExpression;
            ViewState["SortDirection"] = newDirection;
        }
        protected void GridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (sender == null || e.Row.RowType != DataControlRowType.Header || this.FilterExpression == null) {
                return;
            }

            // Show icon on filtered columns
            SPGridView grid = sender as SPGridView;
            for (int i = 0; i < grid.Columns.Count; i++)
            {
                DataControlField field = grid.Columns[i];
                string[] filterFieldParts = field.SortExpression.Split('$');
                bool doesFieldHasFilter = filterFieldParts.Length == 2 ?
                    this.FilterExpression.WhereCriteria.Any(wcFld => wcFld.Item1.Id == filterFieldParts[0].ToGuid() && wcFld.Item5 == filterFieldParts[1]) :
                    this.FilterExpression.WhereCriteria.Any(wcFld => wcFld.Item1.InternalName == field.SortExpression);

                if (doesFieldHasFilter) {
                    try {
                        Image filterIcon = new Image();
                        filterIcon.ImageUrl = "/_layouts/15/images/filter.gif";
                        filterIcon.Style[HtmlTextWriterStyle.MarginLeft] = "2px";

                        Literal headerText = new Literal();
                        headerText.Text = field.HeaderText;

                        PlaceHolder panel = new PlaceHolder();
                        panel.Controls.Add(headerText);
                        panel.Controls.Add(filterIcon);
                        e.Row.Cells[i].Controls[0].Controls.Add(panel);
                    }
                    catch { }
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            //if (this.List != null && this.List.Id == TableIDs.TIMESHEET_ROSTERS) {
            //    this.AppendRibbon();
            //}
        }

        #region Private methods

        private void RegisterCssStylesForColourCoding(ViewMetadata view)
        {
            JavaScriptSerializer jsSer = new JavaScriptSerializer();
            LiteralControl ltrStyle = new LiteralControl();

            List<ColorInfo> colorSettings = new List<ColorInfo>();
            StaticColourSettings staticSettings = view.GetDerializedStaticColourSettings();
            if (!staticSettings.IsEmpty)
            {
                // Static colour-coding
                colorSettings.AddRange(staticSettings.Conditions.Select(x => new ColorInfo { color = x.color, fontColor = x.fontColor }));
            }
            else
            {
                // Dynamic colour-coding
                DynamicColourSettings dynSettings = view.GetDerializedDynamicColourSettings();
                if (!dynSettings.IsEmpty) {
                    colorSettings.AddRange(dynSettings.conditions.Select(x => new ColorInfo { color = x.color, fontColor = x.fontColor }));
                }
            }

            if (colorSettings.Any())
            {
                StringBuilder styleBuilder = new StringBuilder();
                foreach (ColorInfo ci in colorSettings)
                {
                    styleBuilder.AppendFormat(Constants.ColourCodingSettings.STYLE_BACKGROUND_CLASS_TEMPLATE, ci.color.Replace("#", ""), ci.color);
                    styleBuilder.AppendLine();
                    styleBuilder.AppendFormat(Constants.ColourCodingSettings.STYLE_COLOR_CLASS_TEMPLATE, ci.color.Replace("#", ""), ci.fontColor);
                    styleBuilder.AppendLine();
                }

                ltrStyle.Text = string.Format(Constants.ColourCodingSettings.STYLE_ELEMENT_TEMPLATE, styleBuilder.ToString());
                viewPanel.Controls.Add(ltrStyle);
            }
        }
        private void RegisterColourCodingSettings(ViewMetadata view, object ds)
        {
            StaticColourSettings staticSettings = view.GetDerializedStaticColourSettings();
            if (!staticSettings.IsEmpty && ds != null)
            {
                #region Static colour-coding
                
                var listFields = this.List.ListMetadataFields;
                Dictionary<string, string> statSet = new Dictionary<string, string>();
                IEnumerable<RosterEvent> staticDS = ds as IEnumerable<RosterEvent>;
                foreach (RosterEvent re in staticDS)
                {
                    string key = Convert.ToString(re.Id);
                    if (statSet.ContainsKey(key)) { continue; }

                    var props = (IDictionary<string, object>)re.RosterEventProperties;
                    foreach (StaticCondition sc in staticSettings.Conditions)
                    {
                        List<string> cellValues = new List<string> { props[sc.Id].ToSafeString() };

                        var conditionField = listFields.FirstOrDefault(fld => fld.InternalName == sc.Id || fld.Id == sc.Id.Split('$')[0].ToGuid());
                        if ((conditionField.DataSourceType == (int)LookupSourceType.Table) || (conditionField.DataSourceType == (int)LookupSourceType.Query)) {
                            conditionField.DataSourceField.Split('$').ToList().ForEach(itemField => {
                                cellValues.Add(props[string.Format("{0}_{1}", conditionField.DataSource, itemField)].ToSafeString());
                            });
                        }
                        else if (conditionField.FieldType() == SPFieldType.Choice) {
                            cellValues.AddRange(props[sc.Id].XmlToList());
                        }
                        
                        bool stopConditionLoop = false;
                        foreach(string cellValue in cellValues)
                        {
                            bool needRecolor = false;
                            if (cellValue == null && sc.camlOperator != "IsNull") {
                                continue;
                            } else if (sc.camlOperator == "Eq") {
                                needRecolor = (sc.filterValue == cellValue);
                            } else if (sc.camlOperator == "Contains") {
                                needRecolor = (cellValue.StartsWith(sc.filterValue));
                            } else if (sc.camlOperator == "Neq") {
                                needRecolor = (sc.filterValue != cellValue);
                            } else if (sc.camlOperator == "BeginsWith") {
                                needRecolor = (cellValue.IndexOf(sc.filterValue) == 0);
                            } else if (sc.camlOperator == "IsNull") {
                                needRecolor = string.IsNullOrEmpty(cellValue);
                            } else if (sc.camlOperator == "IsNotNull") {
                                needRecolor = !string.IsNullOrEmpty(cellValue);
                            }

                            if (needRecolor) {
                                if (!statSet.ContainsKey(key)) {
                                    statSet.Add(key, sc.color);
                                }
                                stopConditionLoop = true;
                                break;
                            }
                        }
                        if (stopConditionLoop) { break; }
                    }
                }

                #endregion

                clientContext.ColorSettings = statSet.GroupBy(x => x.Value).Select(g => new { color = g.Key, ids = g.Select(t => t.Key) });
            }
            else
            {
                #region Dynamic colour-coding
                
                DynamicColourSettings dynSettings = view.GetDerializedDynamicColourSettings();
                if (!dynSettings.IsEmpty && ds != null)
                {
                    IEnumerable<ExpandedRosterEvent> dynamicDS = ds as IEnumerable<ExpandedRosterEvent>;
                    Dictionary<string, string> queryParams = new Dictionary<string, string>();

                    #region Init replacement variables

                    // get original replacement shift
                    if (this.OriginalRosterId != Guid.Empty) {
                        RosterEvent origRoster = dataProvider.GetRosterEvent(this.OriginalRosterId);

                        queryParams.Add("Param:ReplacementShiftStartDate", Utils.GetDateAsSqlString(origRoster.GetStartDate()));
                        queryParams.Add("Param:ReplacementShiftEndDate", Utils.GetDateAsSqlString(origRoster.GetEndDate()));
                        queryParams.Add("Param:ReplacementShiftID", origRoster.Id.ToString());
                    }

                    // init from url params
                    foreach (var queryParamKey in Page.Request.QueryString.AllKeys)
                        queryParams.Add("UrlParam:" + queryParamKey, Page.Request.QueryString[queryParamKey]); // adding url param

                    #endregion

                    Dictionary<string, string> dynSet = new Dictionary<string, string>();
                    DataTable displayedShiftsDT = SPGridView2DataSource.GetRostersAsDataTable(this.View, dynamicDS);
                    foreach (DynamicCondition dc in dynSettings.conditions)
                    {
                        // clear query (replace holders with real values)
                        string clearedQuery = this.ReplaceParamsInCamlQuery(dc.camlQuery, queryParams);

                        // get shifts by condition query
                        DataRow[] shiftsCollAr = displayedShiftsDT.Select(clearedQuery);
                        if (shiftsCollAr != null)
                        {
                            foreach(DataRow row in shiftsCollAr)
                            {
                                string _key = row[dynSettings.matchingField].ToString();
                                if (!dynSet.ContainsKey(_key)) {
                                    dynSet.Add(_key, dc.color);
                                }
                            }
                        }
                    }

                    var enumSchifts = displayedShiftsDT.AsEnumerable();
                    clientContext.ColorSettings = dynSet.Select(dSet => new {
                        color = dSet.Value,
                        ids = enumSchifts.Where(p => p.Field<string>(dynSettings.matchingField) == dSet.Key).Select(r => r.Field<string>("ID").Substring(0, 36)).Distinct()
                    });
                }

                #endregion
            }
        }
        private string ReplaceParamsInCamlQuery(string conditionCamlQuery, Dictionary<string, string> queryParams)
        {
            string clearedQuery = conditionCamlQuery;

            foreach (string paramKey in queryParams.Keys) {
                clearedQuery = clearedQuery.Replace(paramKey, queryParams[paramKey]);
            }

            return clearedQuery;
        }
        private void SetGridFilterManually()
        {
            spGridView.GridDataSource.FilterExpression = (string)ViewState["FilterExpression"];

            string[] fVals = ViewState["FilterExpression"].ToString().Split(new string[] { " = " }, StringSplitOptions.None);
            if (fVals.Length > 1) {
                spGridView.Attributes.Add("filterFieldName", fVals[0]);
                spGridView.Attributes.Add("filterFieldValue", fVals[1].Replace("%3b", ";").Replace("%25", "%").Trim(new char[] { '\'' }));
            }
        }
        private void AddRosterJsContext()
        {
            string webUrl = SPContext.Current.Web.ServerRelativeUrl.TrimEnd('/');

            clientContext.viewId = this.View.Id;
            clientContext.listId = this.View.ListMetadataId;
            clientContext.listDispForm = webUrl + this.List.DispItemUrl;
            clientContext.listEditForm = webUrl + this.List.EditItemUrl;
            clientContext.WithTooltips = this.View.ViewMetadataPopupSettings.Any();
            clientContext.FilterOperators = FieldFilterOperatorsLayer.GetOperatorsAsString(this.FilterOperators);
            clientContext.QuickEditMode = this.EditMode;
            clientContext.QuickEditSettings = this.QuickEditSettings;
            clientContext.ShortDatePattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
            clientContext.ContentTypes = this.List.ListMetadataContentTypes.Select(x => new ContentTypeInfo {
                Id = x.Id,
                DispFormUrl = webUrl + x.DispItemUrl,
                EditFormUrl = webUrl + x.EditItemUrl
            }).ToList();

            // generate NewForm url
            clientContext.listNewForm = webUrl + this.List.NewItemUrl;
            if (!string.IsNullOrEmpty(this.ConnectionFilterExpression)) {
                string[] fVals = this.ConnectionFilterExpression.Split(new string[] { " = " }, StringSplitOptions.None);
                if (fVals.Length > 1) {
                    clientContext.listNewForm += string.Format("&{0}={1}", fVals[0], fVals[1].Trim(new char[] { '\'' }).ToSafeString());
                }
            }

            // add calendar period to client context
            if (this.View.IsCalendarView()) {
                var calPeriod = this.CalendarViewPeriod;
                clientContext.CalendarProps = new CalendarProperties { PeriodStartDate = calPeriod.Item1.Date, PeriodEndDate = calPeriod.Item2.Date };
            }

            // add filter from Connected WebPart to client context
            if (!string.IsNullOrEmpty(this.ConnectionFilterExpression)) {
                var filterExp = Utils.GetWhereCriteriaFromFilterExpression(this.m_view, this.ConnectionFilterExpression).FirstOrDefault();
                if (filterExp != null) {
                    clientContext.FilterFromConnectedWebPart = new ViewFilter { Field = filterExp.Item1.InternalName, Value = filterExp.Item4.ToSafeString() };
                }
            }

            JavaScriptSerializer jsSer = new JavaScriptSerializer();
            Type csType = this.GetType();
            ClientScriptManager csm = Page.ClientScript;

            string globalVarKey = "RosterContext";
            string customScript = string.Format("RosterContext = {0};", jsSer.Serialize(clientContext));
            if (!csm.IsClientScriptBlockRegistered(csType, globalVarKey))
                csm.RegisterClientScriptBlock(csType, globalVarKey, customScript, true);
        }
        /// <summary>
        /// Add styles to reside Calendar webpart (if required)
        /// </summary>
        private void ResizeCalendar(int WidthInPx, int HeightInPx)
        {
            int _width = WidthInPx;
            int _height = HeightInPx;
            if (_width != 0 && _height != 0)
            {
                LiteralControl lblResizeCalendar = new LiteralControl();

                // calculate font-size
                int fontSize = 13; // default value
                if (_width < 500)
                    fontSize = 10;
                else if (_width >= 500 && _width < 700)
                    fontSize = 11;
                else if (_width >= 700 && _width < 900)
                    fontSize = 12;

                // calculate height
                int rowHeight = 0;
                string ddiv_padding = "3px 2px 2px"; // default=standart value for padding
                string th_detailtime_padding = "1px 5px 2px";
                if (this.CalendarPeriod != "month")
                {
                    rowHeight = _height / 36;

                    if (rowHeight < 20) {
                        fontSize = 9;
                        ddiv_padding = "0px";
                        th_detailtime_padding = "0px";
                    }
                    if (rowHeight < 10) {
                        fontSize = rowHeight - 2;
                    }
                }
                else
                {
                    rowHeight = _height / 5;
                }

                lblResizeCalendar.Text = string.Format(
                        @"<style type='text/css'>
                            .view-main-panel {{
                                width: {0}px !important;
                            }}
                            .ms-acal-summary-itemrow td div {{
                                height: {1}px;
                            }}
                            div.ms-acal-sdiv, div.ms-acal-ddiv, th.ms-acal-detailtime {{
                                font-size: {2}px;
                            }}
                            tr.ms-acal-hour00 > td, tr.ms-acal-hour30 > td {{
                                height: {1}px;
                            }}
                            div.ms-acal-ddiv {{
                                padding: {3}
                            }}
                            th.ms-acal-detailtime {{
                                padding: {4}
                            }}
                          </style>", _width, rowHeight, fontSize, ddiv_padding, th_detailtime_padding);
                viewPanel.Controls.Add(lblResizeCalendar);
            }
        }
        private void AppendRibbon()
        {
            try
            {
                // GET View ribbon buttons
                var ribbonBtns = new List<ListMetadataAction>();

                ribbonBtns.Add(new ListMetadataAction() {
                                LabelText = "Submit", Description = "Submit this Item",
                                Sequence = 1, Id = Guid.NewGuid(),
                                ImageUrl = "/_layouts/15/images/DLDSLN32.PNG",
                                Command = "javascript:Roster.CustomActions.SubmitTimesheet();"
                            });

                if (ribbonBtns.Any()) {
                    Utils.AddRibbonButtonsToPage(ribbonBtns, this.Page);
                }

            }
            catch { }
        }

        #endregion
    }
}
