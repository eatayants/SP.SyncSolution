using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Roster.Presentation.Controls.Fields;
using System.Web.UI.WebControls;
using System.Web.UI;
using Roster.Presentation.Helpers;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using Roster.Model.DataContext;
using Roster.BL;
using Roster.Presentation.Extensions;
using Roster.Presentation.ColourCoding;
using RosterCommon = Roster.Common;
using Roster.Common;

namespace Roster.Presentation.Layouts
{
    public partial class DbViewEdit : LayoutsPageBase
    {
        #region Private

        private RosterConfigService configProvider = new RosterConfigService();
        private Guid m_viewId = Guid.Empty;
        private ListMetadata m_list = null;
        private ViewMetadata m_view = null;
        private List<ListMetadataField> m_listFields = null;
        private List<ViewMetadataWhereCritery> m_filterConditions = null;
        private int m_displayedFieldsCount;

        #endregion

        #region Public properties

        public ListMetadata List
        {
            get
            {
                if (m_list == null)
                {
                    this.m_list = this.configProvider.GetList(this.View.ListMetadata.Id);
                }

                return this.m_list;
            }
        }
        public Guid ViewGuid
        {
            get
            {
                if (this.m_viewId == Guid.Empty)
                {
                    Guid _vid;
                    string wId = Request.QueryString["View"];
                    if (!string.IsNullOrEmpty(wId) && Guid.TryParse(wId, out _vid))
                        this.m_viewId = _vid;
                }

                return this.m_viewId;
            }
        }
        public ViewMetadata View
        {
            get
            {
                if (m_view == null)
                {
                    this.m_view = this.configProvider.GetView(this.ViewGuid);
                }

                return this.m_view;
            }
        }
        public List<ListMetadataField> ListFields
        {
            get
            {
                if (m_listFields == null) {
                    this.m_listFields = this.List.ListMetadataFields.ToList();
                }

                return this.m_listFields;
            }
        }

        public string ViewData
        {
            get
            {
                string _data = string.Empty;

                XDocument vDataDoc = new XDocument(
                        new XElement("ViewData",
                            new XElement("FieldRef",
                                new XAttribute("Name", ddlMonthViewTitle.SelectedValue),
                                new XAttribute("Type", "CalendarMonthTitle")),
                            new XElement("FieldRef",
                                new XAttribute("Name", ddlWeekViewTitle.SelectedValue),
                                new XAttribute("Type", "CalendarWeekTitle")),
                            new XElement("FieldRef",
                                new XAttribute("Name", ddlWeekViewSubHeading.SelectedValue),
                                new XAttribute("Type", "CalendarWeekLocation")),
                            new XElement("FieldRef",
                                new XAttribute("Name", ddlDayViewTitle.SelectedValue),
                                new XAttribute("Type", "CalendarDayTitle")),
                            new XElement("FieldRef",
                                new XAttribute("Name", ddlDayViewSubHeading.SelectedValue),
                                new XAttribute("Type", "CalendarDayLocation"))
                            )
                        );
                _data = vDataDoc.ToString(SaveOptions.DisableFormatting);

                return _data;
            }
        }
        public string OrderBy
        {
            get
            {
                string _data = string.Empty;

                if (ddlSortColumn.SelectedIndex != -1)
                {
                    XDocument orderDoc = new XDocument(
                        new XElement("OrderBy",
                            new XElement("FieldRef",
                                new XAttribute("Name", ddlSortColumn.SelectedValue),
                                new XAttribute("Ascending", ddlSortColumn.SelectedValue.Equals("ASC") ? "TRUE" : "FALSE"))
                            )
                        );
                    _data = orderDoc.ToString(SaveOptions.DisableFormatting);
                }

                return _data;
            }
        }
        public int ItemLimit
        {
            get
            {
                int _limit = 30; // default value
                Int32.TryParse(txtViewLimit.Text, out _limit);
                return _limit;
            }
            set
            {
                txtViewLimit.Text = value.ToString();
            }
        }
        public string Scope
        {
            get
            {
                return radioDefaultScope.SelectedValue;
            }
        }
        public bool IsDefault
        {
            get
            {
                return chIsDefault.Checked;
            }
        }
        public RosterCommon.SortDirection SortDir
        {
            get
            {
                return radioSortOrder.SelectedValue.Equals("DESC") ? RosterCommon.SortDirection.Descending : RosterCommon.SortDirection.Ascending;
            }
        }
        public string ViewName
        {
            get
            {
                return txtViewName.Text;
            }
            set
            {
                txtViewName.Text = value;
            }
        }
        public ICollection<ViewMetadataWhereCritery> Filters
        {
            get
            {
                if (m_filterConditions == null)
                {
                    m_filterConditions = new List<ViewMetadataWhereCritery>();

                    if (radioIsWithFilters.SelectedValue.Equals("1") && ddlFilterColumn.SelectedValue != "00000000-0000-0000-0000-000000000000")
                    {
                        m_filterConditions.Add(new ViewMetadataWhereCritery()
                        {
                            Id = Guid.NewGuid(), ListMetadataFieldId = new Guid(ddlFilterColumn.SelectedValue),
                            Value = txtFilterValue.Text,
                            CompareType = Convert.ToInt32(ddlFilterCompareTypes.SelectedValue),
                            ConcateOperator = (int)RosterCommon.ConcateOperator.And // TO-DO :: set correct operator in case of multiple filters!
                        });
                    }
                }

                return m_filterConditions;
            }
            set
            {
                if (value.Any())
                {
                    ViewMetadataWhereCritery filter = value.First(); // TO-DO :: handle multiple filters
                    radioIsWithFilters.SelectedValue = "1";
                    ddlFilterColumn.SelectedValue = filter.ListMetadataFieldId.ToString();
                    txtFilterValue.Text = filter.Value;
                }
            }
        }
        public string StaticColourCodingSettings
        {
            get
            {
                return hidStaticConditions.Value;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    hidStaticConditions.Value = value;
            }
        }
        public string DynamicColourCodingSettings
        {
            get
            {
                JavaScriptSerializer jsSer = new JavaScriptSerializer();
                DynamicColourSettings dynSettings = new DynamicColourSettings() {
                    filterFieldsColl = hidFilterFields.Value,
                    matchingField = hidMatchingField.Value,
                    conditions = jsSer.Deserialize<List<DynamicCondition>>(hidDynamicConditions.Value)
                };
                return jsSer.Serialize(dynSettings);
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    JavaScriptSerializer jsSer = new JavaScriptSerializer();
                    DynamicColourSettings dynSettings = jsSer.Deserialize<DynamicColourSettings>(value);
                    hidDynamicConditions.Value = jsSer.Serialize(dynSettings.conditions);
                    hidMatchingField.Value = dynSettings.matchingField;
                    hidFilterFields.Value = dynSettings.filterFieldsColl;
                }
            }
        }
        public List<ViewMetadataField> CalendarViewFields
        {
            get
            {
                JavaScriptSerializer jsSer = new JavaScriptSerializer();
                List<ViewMetadataField> cvf = new List<ViewMetadataField>();
                List<string> fldNames = new List<string>();

                fldNames.AddRange(new string[] { ddlMonthViewTitle.SelectedValue, ddlWeekViewTitle.SelectedValue,
                                ddlWeekViewSubHeading.SelectedValue, ddlDayViewTitle.SelectedValue, ddlDayViewSubHeading.SelectedValue});

                // also get Fields from Dynamic colour-coding
                DynamicColourSettings dynSettings = jsSer.Deserialize<DynamicColourSettings>(this.DynamicColourCodingSettings);
                if (!string.IsNullOrEmpty(dynSettings.filterFieldsColl)) {
                    fldNames.AddRange(dynSettings.filterFieldsColl.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                }

                // also get Fields from Static colour-coding
                List<StaticCondition> statConds = jsSer.Deserialize<List<StaticCondition>>(this.StaticColourCodingSettings);
                if (statConds.Any()) {
                    fldNames.AddRange(statConds.Select(con => con.Id));
                }

                // const fields for any calendar Event
                fldNames.AddRange(new string[] { RosterCommon.FieldNames.START_DATE,
                    RosterCommon.FieldNames.END_DATE, RosterCommon.FieldNames.ALL_DAY_EVENT, RosterCommon.FieldNames.RECURRENCE});

                var fields = this.ListFields;
                foreach (string fldIntName in fldNames.Where(x => !string.IsNullOrEmpty(x)).Distinct())
                {
                    var fld = fields.FirstOrDefault(f => f.InternalName.Equals(fldIntName) || f.FieldName.Equals(fldIntName));
                    if (fld != null) {
                        cvf.Add(new ViewMetadataField() {
                            Id = Guid.NewGuid(), ListMetadataFieldId = fld.Id, DisplayName = fld.FieldName,
                            Position = 1, OrderCriteria = 0 // for calendar view we do NOT need position and order
                        });
                    }
                }

                return cvf;
            }
        }
        public int DisplayedFieldsCount
        {
            get
            {
                if (m_displayedFieldsCount == 0)
                {
                    int mainFieldsCount = 0;
                    foreach (var fld in this.ListFields)
                    {
                        mainFieldsCount++;
                        if ((fld.DataSourceType == (int)LookupSourceType.Table) || (fld.DataSourceType == (int)LookupSourceType.Query)) {
                            mainFieldsCount += fld.DataSourceField.Split('$').Skip(1).Count();
                        }
                    }
                    this.m_displayedFieldsCount = mainFieldsCount;
                }

                return this.m_displayedFieldsCount;
            }
        }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            List<ListMetadataField> fields = this.ListFields;

            // register JS variable
            this.RegisterFieldInfoAsJSVariable();

            try
            {
                if (!Page.IsPostBack)
                {
                    if (this.View == null)
                        throw new Exception("Cannot find view!");

                    // set View Title
                    this.ViewName = this.View.Name;

                    // show/hide buttons base on 'IsDefault' flag
                    btnRemove.Visible = chIsDefault.Visible = !this.View.IsDefault;
                    lblIsDefaultView.Visible = this.View.IsDefault;

                    // show/hide blocks base on Calendartype
                    this.ShowHideBlocksBaseOnViewType(this.View.IsCalendarView());

                    // init View settings
                    if (this.View.IsCalendarView())
                    {
                        // Init Calendar Columns COntrols
                        this.InitCalendarColumns(this.View.ViewData, fields);
                        // init Dyn settins
                        this.DynamicColourCodingSettings = this.View.DynamicColourCodingSettings;
                        // init static settings
                        this.StaticColourCodingSettings = this.View.StaticColourCodingSettings;

                        // init default scope
                        foreach (ListItem li in radioDefaultScope.Items)
                            li.Selected = (li.Value.Equals(this.View.CalendarScope));

                        // init Tooltip fields coll
                        List<ViewMetadataPopupSetting> tooltipFiedls = this.View.ViewMetadataPopupSettings.ToList();
                        TooltipColumnsGrid.DataSource = fields.Select((fld, index) => new {
                            IsSelected = tooltipFiedls.Any(f => f.ListMetadataFieldId == fld.Id) ? "1" : "0",
                            Position = tooltipFiedls.Where(f => f.ListMetadataFieldId == fld.Id).Select(f => f.Position).FirstOrDefault(),
                            Id = fld.Id,
                            DisplayName = fld.FieldName,
                        }).OrderByDescending(x => x.IsSelected).ThenBy(x => x.Position);
                        TooltipColumnsGrid.DataBind();
                    }
                    else
                    {
                        this.ItemLimit = this.View.ItemLimit;

                        List<ViewMetadataField> viewFiedls = this.View.ViewMetadataFields.ToList();
                        string[] additionalFields = this.View.ExcludeFields == null ? new string[0] : this.View.ExcludeFields.Split(new string[] { ";#" }, StringSplitOptions.RemoveEmptyEntries);
                        this.InitSortBlock(viewFiedls, fields);

                        var ds = new List<ViewField>();
                        foreach (var fld in fields)
                        {
                            var viewFld = viewFiedls.FirstOrDefault(f => f.ListMetadataFieldId == fld.Id);
                            ds.Add(new ViewField {
                                IsSelected = viewFld != null ? "1" : "0",
                                Position = viewFld == null ? 0 : viewFld.Position,
                                Id = fld.Id.ToString(),
                                DisplayName = fld.FieldName
                            });

                            if ((fld.DataSourceType == (int)LookupSourceType.Table) || (fld.DataSourceType == (int)LookupSourceType.Query))
                            {
                                fld.DataSourceField.Split('$').Skip(1).ToList().ForEach(additionalField =>
                                {
                                    ds.Add(new ViewField {
                                        IsSelected = additionalFields.Contains(string.Format("{0}_{1}", fld.DataSource, additionalField)) ? "1" : "0",
                                        Position = viewFld == null ? 0 : viewFld.Position,
                                        Id = string.Format("{0}_{1}", fld.DataSource, additionalField),
                                        DisplayName = string.Format("{0}: {1}", fld.FieldName, additionalField)
                                    });
                                });
                            }
                        }

                        ViewColumnsGrid.DataSource = ds.OrderByDescending(x => x.IsSelected).ThenBy(x => x.Position).ThenBy(x => x.DisplayName);
                        ViewColumnsGrid.DataBind();
                    }

                    // init Filter block
                    ddlFilterColumn.DataSource = fields;
                    ddlFilterColumn.DataBind();
                    this.Filters = this.View.ViewMetadataWhereCriteries;
                }
            }
            catch (Exception ex)
            {
                errorHolder.Controls.Add(new Label() { Text = ex.Message, ForeColor = System.Drawing.Color.Red });
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                ViewMetadata currentView = this.View;
                currentView.Name = txtViewName.Text;
                if (!currentView.IsDefault)
                    currentView.IsDefault = this.IsDefault; // user cannot remove 'IsDefault' flag from Default view

                // Filters
                currentView.ViewMetadataWhereCriteries.Clear();
                if (this.Filters.Any()) {
                    foreach (var filterCrit in this.Filters) {
                        filterCrit.ViewMetadataId = currentView.Id;
                        currentView.ViewMetadataWhereCriteries.Add(filterCrit);
                    }
                }

                if (currentView.IsCalendarView())
                {
                    currentView.ViewData = this.ViewData;
                    currentView.CalendarScope = this.Scope;
                    currentView.StaticColourCodingSettings = this.StaticColourCodingSettings;
                    currentView.DynamicColourCodingSettings = this.DynamicColourCodingSettings;

                    // collect info about ViewFields for CALENDAR view
                    currentView.ViewMetadataFields.Clear();
                    List<ViewMetadataField> calendarViewFields = this.CalendarViewFields;
                    if (calendarViewFields.Any()) {
                        foreach (var vmf in calendarViewFields) {
                            vmf.ViewMetadataId = currentView.Id;
                            currentView.ViewMetadataFields.Add(vmf);
                        }
                    }

                    // collect info about Tooltips columns
                    currentView.ViewMetadataPopupSettings.Clear();
                    foreach (GridViewRow gvRow in TooltipColumnsGrid.Rows)
                    {
                        if (((System.Web.UI.WebControls.CheckBox)gvRow.FindControl("chIsSelected")).Checked) {
                            Guid fldId = new Guid(TooltipColumnsGrid.DataKeys[gvRow.RowIndex].Value.ToString());
                            currentView.ViewMetadataPopupSettings.Add(new ViewMetadataPopupSetting() {
                                Id = Guid.NewGuid(), ListMetadataFieldId = fldId, ViewMetadataId = currentView.Id,
                                Position = Int32.Parse(((System.Web.UI.WebControls.DropDownList)gvRow.FindControl("ddlColumnPosition")).SelectedValue)
                            });
                        }
                    }
                }
                else
                {
                    currentView.ItemLimit = this.ItemLimit;

                    // get sort FieldId
                    Guid orderFieldId = Guid.Empty;
                    Guid.TryParse(ddlSortColumn.SelectedValue, out orderFieldId);

                    // collect info about ViewFields
                    currentView.ViewMetadataFields.Clear();
                    List<string> additionalFields = new List<string>();
                    foreach (GridViewRow gvRow in ViewColumnsGrid.Rows)
                    {
                        // Find checkbox
                        if (((System.Web.UI.WebControls.CheckBox)gvRow.FindControl("chIsSelected")).Checked)
                        {
                            string key = ViewColumnsGrid.DataKeys[gvRow.RowIndex].Value.ToString();
                            Guid fldId = Guid.Empty;
                            if (Guid.TryParse(key, out fldId))
                            {
                                ViewMetadataField _viewField = new ViewMetadataField() {
                                    Id = Guid.NewGuid(), ViewMetadataId = currentView.Id, ListMetadataFieldId = fldId,
                                    DisplayName = this.ListFields.Where(x => x.Id == fldId).FirstOrDefault().FieldName,
                                    Position = Int32.Parse(((System.Web.UI.WebControls.DropDownList)gvRow.FindControl("ddlColumnPosition")).SelectedValue),
                                    OrderCriteria = (orderFieldId != fldId) ? 0 : (int)this.SortDir
                                };
                                currentView.ViewMetadataFields.Add(_viewField);
                            }
                            else
                            {
                                additionalFields.Add(key);
                            }
                        }
                    }
                    currentView.ExcludeFields = String.Join(";#", additionalFields);
                }

                // UPDATE VIEW
                new RosterConfigService().SaveView(currentView);

                // REMOVE IsDefault flag from other views
                if (chIsDefault.Visible && this.IsDefault) {
                    // chIsDefault.Visible == true   means that on PageLoad view was NOT Default
                    this.RemoveDefaultFlagsFromOtherViews(currentView.Id);
                }

                // close form
                Utils.GoBackOnSuccess(this, this.Context);
            }
            catch (Exception ex)
            {
                errorHolder.Controls.Add(new Label() { Text = ex.Message, ForeColor = System.Drawing.Color.Red });
            }
        }

        protected void btnRemove_Click(object sender, EventArgs e)
        {
            try
            {
                this.configProvider.RemoveView(this.ViewGuid);

                // close form
                Utils.GoBackOnSuccess(this, this.Context);
            }
            catch (Exception ex)
            {
                errorHolder.Controls.Add(new Label() { Text = ex.Message, ForeColor = System.Drawing.Color.Red });
            }
        }

        #region Private methods

        /// <summary>
        /// Init grid with View fields
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ViewColumnsGrid_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //Finding the CheckBox control.
                Control ctrl0 = e.Row.FindControl("chIsSelected");
                if (ctrl0 != null) {
                    CheckBox ch = ctrl0 as CheckBox;
                    ch.Checked = (string)DataBinder.Eval(e.Row.DataItem, "IsSelected") == "1";
                }

                //Finding the Dropdown control.
                Control ctrl = e.Row.FindControl("ddlColumnPosition");
                if (ctrl != null)
                {
                    int total = this.DisplayedFieldsCount;
                    DropDownList dd = ctrl as DropDownList;
                    dd.Attributes.Add("onchange", string.Format("Reorder(this,{0},{1})", e.Row.RowIndex, total));
                    dd.CssClass = "ViewOrder" + e.Row.RowIndex;
                    dd.DataSource = Enumerable.Range(1, total);
                    dd.DataBind();
                    dd.SelectedIndex = e.Row.RowIndex;
                }
            }
        }

        /// <summary>
        /// Init OrderBy block
        /// </summary>
        /// <param name="orderBy"></param>
        /// <param name="fields"></param>
        private void InitSortBlock(List<ViewMetadataField> viewFiedls, List<ListMetadataField> listFields)
        {
            ddlSortColumn.DataSource = listFields;
            ddlSortColumn.DataBind();

            ViewMetadataField orderByField = viewFiedls.FirstOrDefault(x => x.OrderCriteria != 0);
            if (orderByField != null) {
                ddlSortColumn.SelectedValue = orderByField.ListMetadataFieldId.ToString();

                radioSortOrder.Items.FindByValue("ASC").Selected = orderByField.OrderCriteria == 1;
                radioSortOrder.Items.FindByValue("DESC").Selected = orderByField.OrderCriteria == 2;
            }
        }
        /// <summary>
        /// Init ViewData block
        /// </summary>
        /// <param name="calCols"></param>
        /// <param name="fields"></param>
        private void InitCalendarColumns(string calCols, List<ListMetadataField> fields)
        {
            ddlMonthViewTitle.DataSource = fields;
            ddlMonthViewTitle.DataBind();
            ddlWeekViewTitle.DataSource = fields;
            ddlWeekViewTitle.DataBind();
            ddlWeekViewSubHeading.DataSource = fields;
            ddlWeekViewSubHeading.DataBind();
            ddlDayViewTitle.DataSource = fields;
            ddlDayViewTitle.DataBind();
            ddlDayViewSubHeading.DataSource = fields;
            ddlDayViewSubHeading.DataBind();

            if (!string.IsNullOrEmpty(calCols))
            {
                XDocument oDoc = XDocument.Parse(calCols);
                var fRefEls = oDoc.Descendants("FieldRef");
                if (fRefEls.Any())
                {
                    foreach(XElement fRef in fRefEls)
                    {
                        XAttribute tAttr = fRef.Attribute("Type");
                        if (tAttr != null)
                        {
                            string fieldId = fRef.Attribute("Name").Value;
                            string fldDataType = tAttr.Value;
                            switch (fldDataType)
                            {
                                case "CalendarMonthTitle":
                                    ddlMonthViewTitle.SelectedValue = fieldId;
                                    break;
                                case "CalendarWeekTitle":
                                    ddlWeekViewTitle.SelectedValue = fieldId;
                                    break;
                                case "CalendarWeekLocation":
                                    ddlWeekViewSubHeading.SelectedValue = fieldId;
                                    break;
                                case "CalendarDayTitle":
                                    ddlDayViewTitle.SelectedValue = fieldId;
                                    break;
                                case "CalendarDayLocation":
                                    ddlDayViewSubHeading.SelectedValue = fieldId;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Show/hide blocks on the page base on View Type
        /// </summary>
        /// <param name="isCalendarView"></param>
        private void ShowHideBlocksBaseOnViewType(bool isCalendarView)
        {
            if (isCalendarView)
            {
                tbodyViewColumnsHeader.Visible = false;
                tbodyViewColumns.Visible = false;
                tbodyViewSortHeader.Visible = false;
                tbodyViewSort.Visible = false;
                tbodyViewLimitHeader.Visible = false;
                tbodyViewLimit.Visible = false;
            }
            else
            {
                tbodyViewCalendarColsHeader.Visible = false;
                tbodyViewCalendarCols.Visible = false;
                tbodyViewScopeHeader.Visible = false;
                tbodyViewScope.Visible = false;
                tbodyViewDynamicColorHeader.Visible = false;
                tbodyViewDynamicColor.Visible = false;
                tbodyViewStaticColorHeader.Visible = false;
                tbodyViewStaticColor.Visible = false;
                tbodyViewTooltipHeader.Visible = false;
                tbodyViewTooltip.Visible = false;
            }
        }
        /// <summary>
        /// Render js variable for Colour-coding
        /// </summary>
        private void RegisterFieldInfoAsJSVariable()
        {
            List<DbField> fields = this.ListFields.Select(x => x.GetDbField()).ToList();
            var availableFields = fields.Where(x => x.InternalName == FieldNames.CONTENT_TYPE_ID || (!x.Hidden &&
                    (x.Type == SPFieldType.Choice || x.Type == SPFieldType.MultiChoice || x.Type == SPFieldType.Lookup || x.Type == SPFieldType.Boolean)))
                    .Select(x => new {
                        fieldTitle = x.DisplayName,
                        fieldName = x.InternalName
                    });
            var availableFields_DynColoring = this.ListFields.Select(x => new {
                fieldTitle = x.FieldName,
                fieldName = x.InternalName
            });

            // register global js variables
            JavaScriptSerializer jsSer = new JavaScriptSerializer();
            Type csType = this.GetType();
            string globalVarKey = "globalVariables";
            string customScript = string.Format("var global_availableFields = {0};var global_availableFields_dynCol = {1};", jsSer.Serialize(availableFields), jsSer.Serialize(availableFields_DynColoring));
            ClientScriptManager csm = Page.ClientScript;
            if (!csm.IsClientScriptBlockRegistered(csType, globalVarKey))
                csm.RegisterClientScriptBlock(csType, globalVarKey, customScript, true);
        }
        /// <summary>
        /// We have just created a new Default view. Ok. Now remove IsDefault flag from previous default view
        /// </summary>
        /// <param name="newDefaultViewId"></param>
        private void RemoveDefaultFlagsFromOtherViews(Guid newDefaultViewId)
        {
            var allViews = new RosterConfigService().GetViews(this.List.Id);
            foreach (ViewMetadata vw in allViews.Where(x => x.Id != newDefaultViewId))
            {
                vw.IsDefault = false;
                new RosterConfigService().SaveView(vw);
            }
        }

        #endregion
    }

    public class ViewField
    {
        public string IsSelected { get; set; }
        public int Position { get; set; }
        public string Id { get; set; }
        public string DisplayName { get; set; }
    }
}
