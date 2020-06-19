using Microsoft.SharePoint.WebControls;
using Roster.Presentation.Controls.FieldControls;
using Roster.Presentation.Controls.Fields;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
using Roster.BL;
using Roster.Common;
using Roster.Model.DataContext;
using Roster.Presentation.Extensions;
using Roster.Presentation.Helpers;

namespace Roster.Presentation.WebParts.ExternalListFormWebPart
{
    public partial class ExternalListFormWebPartUserControl : UserControl
    {
		#region Private

        public class RecurenceEventIdentity 
        {
            public RecurenceEventIdentity(object recurence)
            {
                //Guid.0.DateTime
                Valid = false;
                var recurenceDef = recurence.ToSafeString();
                if (String.IsNullOrWhiteSpace(recurenceDef)) return;
                var recurenceList = recurenceDef.Split(new [] { ".0." }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (recurenceList.IsEmpty() || recurenceList.Count() != 2) return;
                ItemId = recurenceList[0].ToNullableGuid();
                ItemTime = recurenceList[1].ToNullableDateTime();
                Valid = true;
            }
            public bool Valid { get; private set; }
            public Guid? ItemId { get; private set; }
            public DateTime? ItemTime { get; private set; }
        }
        private RecurenceEventIdentity _recurence;
		private SPControlMode _mode = SPControlMode.Invalid;
		private readonly RosterConfigService _configService = new RosterConfigService();
		private readonly RosterDataService _dataService = new RosterDataService();
		private ListMetadata _list;
        private ListMetadataContentType _contentType;
		private RosterEvent _item;
		private List<Tuple<Label, DbBaseFieldControl, DbField>> _fieldCtrls;
        private bool? _hasRights = null;

		#endregion

        #region Public Properties

        public SPControlMode ControlMode
        {
            get
            {
	            if (_mode != SPControlMode.Invalid) return _mode;
				_mode = (SPControlMode)Request.QueryString["Mode"].ToInt();
	            return _mode;
            }
            set
            {
				_mode = value;
            }
        }

		public Guid ListId
		{
			get
			{
				if (Request.QueryString["ListId"] == null)
				{
                    throw new Exception(@"ListId is required paramenter");
				}
				return Request.QueryString["ListId"].ToGuid();
			}
		}

        public int DefaultContentTypeId
        {
            get
            {
                if (List.DefaultContent != null)
                {
                    return List.DefaultContent.Id;
                }
                throw new Exception(@"ContentTypeId not found");
            }
        }

        public int ContentTypeId
        {
            get
            {
                if (Request.QueryString["ContentTypeId"] == null)
                {
                    return DefaultContentTypeId;
                }
                return Request.QueryString["ContentTypeId"].ToInt();
            }
        }

        public ListMetadataContentType ContentType
        {
            get { return _contentType ?? (_contentType = _configService.GetContentType(ContentTypeId)); }
        }

        public ListMetadata List
		{
			get { return _list ?? (_list = _configService.GetList(ListId)); }
		}

        public List<Tuple<int,ListMetadataField>> ListMetadataFields
        {
            get
            {
                var result = List.ListMetadataFields.Where(item => item.Hidden).Select(field => new Tuple<int,ListMetadataField>(0,field)).ToList();
                var existing = ContentType.ListMetadataFieldContentTypes.Where(item=>item.ContentTypeId == ContentTypeId).Select(field => 
                                new Tuple<int,ListMetadataField>(field.ItemPosition ?? 0,field.ListMetadataField)).ToList();
                return result.Union(existing, new ListMetadataFieldComparer()).ToList();
            }
        }

        private class ListMetadataFieldComparer : IEqualityComparer<Tuple<int, ListMetadataField>>
        {
            public bool Equals(Tuple<int, ListMetadataField> x, Tuple<int, ListMetadataField> y)
            {
                return x.Item2.Id == y.Item2.Id;
            }

            public int GetHashCode(Tuple<int, ListMetadataField> obj)
            {
                return obj.Item2.Id.GetHashCode();
            }
        }

        public RecurenceEventIdentity Recurence
        {
            get
            {
                return _recurence ?? (_recurence = new RecurenceEventIdentity(Request.QueryString["ID"]));
            }
        }

        public RosterEvent RecurenceItem
        {
            get
            {
                if (_item != null) return _item;
                if (!Recurence.Valid || !Recurence.ItemId.HasValue) return _item;
                _item = _dataService.GetRosterEvent(Recurence.ItemId.Value);
                if (_item == null) return _item;
                if (!Recurence.ItemTime.HasValue) return _item;
                _item.RosterEventDictionary[StaticFields.StartDate] = Recurence.ItemTime.Value;
                _item.RosterEventDictionary[StaticFields.EndDate] = Recurence.ItemTime.Value;
                return _item;
            }
        }

		public Guid? ItemId
		{
			get
			{
				return Request.QueryString["Id"].ToNullableGuid();
			}
		}

		public RosterEvent Item
		{
			get
			{
                return ItemId.HasValue ? _item ?? (_item = _dataService.ListSingleRosterEvent(ListId,ItemId.Value)) : null;
			}
		}

		public RosterEventType EventType
		{
			get
			{
				var eventTypeId = Request.QueryString["EventType"].ToNullableInt();
				if (!eventTypeId.HasValue)
				{
					throw new Exception(@"EventType is requered paramenter");
				}
				if (!Enum.IsDefined(typeof(RosterEventType), eventTypeId))
				{
					throw new Exception(string.Format(@"EventType {0} is not supported", eventTypeId));
				}
				return  (RosterEventType) eventTypeId;
			}
		}

		public List<Tuple<Label, DbBaseFieldControl,DbField>> FieldControls
		{
			get { return _fieldCtrls ?? (_fieldCtrls = new List<Tuple<Label, DbBaseFieldControl, DbField>>()); }
		}

        public bool HasRights
        {
            get
            {
                if (ControlMode == SPControlMode.New)
                    return true; // EVERYBODY can create New

                Guid? itmId = (Recurence.Valid) ? Recurence.ItemId : this.ItemId;

                if (_hasRights == null && itmId.HasValue && this.ListId != Guid.Empty)
                {
                    if (ControlMode == SPControlMode.Invalid || ControlMode == SPControlMode.Display)
                        _hasRights = _configService.HasRights(itmId.Value, this.ListId, AccessRight.Read);
                    else if (ControlMode == SPControlMode.Edit)
                        _hasRights = _configService.HasRights(itmId.Value, this.ListId, AccessRight.Write);
                    else
                        _hasRights = true;
                }

                return _hasRights.HasValue ? _hasRights.Value : false;
            }
        }

        public string HistoryLinkUrl { get; set; }

        #endregion

        #region Public Methods

        protected override void OnInit(EventArgs e)
        {
            if (ControlMode == SPControlMode.Invalid) {
                ControlMode = SPControlMode.Display;
            }
			base.OnInit(e);

            try
            {
                if (!this.HasRights)
                    throw new Exception("You are not allowed to view this page!");

                // History LINK
                if (ControlMode == SPControlMode.Display && !string.IsNullOrEmpty(this.HistoryLinkUrl)) {
                    var historyRow = new TableRow();
                    var titleCell = new TableCell {
                        CssClass = "ms-formlabel", VerticalAlign = VerticalAlign.Top, Width = Unit.Pixel(113), Text = "History"
                    };
                    var bodyCell = new TableCell {
                        CssClass = "ms-formbody", VerticalAlign = VerticalAlign.Top, Width = Unit.Pixel(350)
                    };
                    var linkCtrl = new HyperLink();
                    linkCtrl.Text = "view status history...";
                    linkCtrl.NavigateUrl = this.HistoryLinkUrl + "?RosterId=" + this.ItemId;
                    linkCtrl.Target = "_blank";
                    bodyCell.Controls.Add(linkCtrl);

                    historyRow.Cells.Add(titleCell);
                    historyRow.Cells.Add(bodyCell);
                    FormTable.Rows.AddAt(0, historyRow);
                }

                foreach (var fld in ListMetadataFields.OrderByDescending(item => item.Item1).Select(fieldItem => fieldItem.Item2.GetDbField()))
                {
                    var subcontrols = new List<DbBaseFieldControl>();
                    fld.SubFields().ForEach(item => {
                        var control = ItemRow(item);
                        subcontrols.Add(control);
                    });
                    ItemRow(fld, subcontrols);
                }
            }
            catch (Exception ex)
            {
                ErrorHolder.Controls.Add(new Label {Text = ex.Message, ForeColor = System.Drawing.Color.Red});
                //ErrorHolder.Controls.Add(new Label { Text = ex.StackTrace, ForeColor = System.Drawing.Color.Red });
            }
        }

        protected void Page_Init(object sender, EventArgs e)
        {

        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["IsDlg"] == null) {
                btnCancel.Attributes.Add("onClick", "history.back(-1); return false;");
            }
            if (!Page.IsPostBack) {
                btnSave.Attributes.Add("onclick", "if (!RosterPreSaveItem()) return false;");
            }

            if (!Page.IsPostBack || ControlMode == SPControlMode.Display)
            {
                try
                {
                    foreach (var rowItem in FieldControls)
                    {
                        if (rowItem.Item3.Required && (ControlMode != SPControlMode.Display))
                        {
                            rowItem.Item1.Text += @"<span class='ms-accentText'> *</span>";
                        }
                        rowItem.Item2.ControlMode = ControlMode;
                        if (ControlMode == SPControlMode.New)
                        {
                            rowItem.Item2.Value = Request.QueryString[rowItem.Item3.InternalNameOriginal] ?? 
                                rowItem.Item3.DefaultValue;
                        }
                        else
                        {
                            if (Recurence.Valid)
                            {
                                if (RecurenceItem.RosterEventDictionary.ContainsKey(rowItem.Item3.InternalNameOriginal))
                                {
                                    rowItem.Item2.Value =
                                        RecurenceItem.RosterEventDictionary[rowItem.Item3.InternalNameOriginal];
                                }
                            }
                            else
                            {
                                if (Item.RosterEventDictionary.ContainsKey(rowItem.Item3.InternalNameOriginal))
                                {
                                    rowItem.Item2.Value = Item.RosterEventDictionary[rowItem.Item3.InternalNameOriginal];
                                }
                            }
                            /*
                            rowItem.Item2.Value = Recurence.Valid ?
                                RecurenceItem.RosterEventDictionary[rowItem.Item3.InternalNameOriginal] :
                                Item.RosterEventDictionary[rowItem.Item3.InternalNameOriginal];*/

                            if (ControlMode == SPControlMode.Edit && rowItem.Item3.InternalName == StaticFields.Modified) {
                                rowItem.Item2.Value = "[today]";
                            } else if (ControlMode == SPControlMode.Edit && rowItem.Item3.InternalName == StaticFields.ModifiedBy) {
                                rowItem.Item2.Value = "[me]";
                            }
                        }
                    }

                    // init Buttons visibility
                    btnSave.Visible = btnCancel.Visible = (ControlMode != SPControlMode.Display && this.HasRights);
                }
                catch (Exception ex)
                {
                    ErrorHolder.Controls.Add(new Label() { Text = ex.Message, ForeColor = System.Drawing.Color.Red });
                    //ErrorHolder.Controls.Add(new Label { Text = ex.StackTrace, ForeColor = System.Drawing.Color.Red });
                }
            }

            if (ControlMode != SPControlMode.New) {
                this.AddSystemInfoRow("Created", StaticFields.Created, StaticFields.CreatedBy);
                this.AddSystemInfoRow("Last modified", StaticFields.Modified, StaticFields.ModifiedBy);
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
				var rosterEvent = (ControlMode == SPControlMode.New) ? _dataService.CreateRosterEvent(ListId, (int)EventType): Item;
	            if (rosterEvent == null)
	            {
		            throw new Exception("Roster event is empty");
	            }
				foreach (var field in ListMetadataFields.Select(item=>item.Item2))
				{
					var uiControls = FieldControls.FirstOrDefault(item=>item.Item2.ID==field.InternalName.ToString());
				    if (uiControls == null) continue;
				    if (uiControls.Item3.ReadOnly)
				    {
				        if (Request.QueryString[field.InternalName] != null)
				        {
				            rosterEvent.RosterEventDictionary[field.InternalName] = Request.QueryString[field.InternalName];
				        }
				    }
				    else
				    {
                        rosterEvent.RosterEventDictionary[field.InternalName] = uiControls.Item2.Value;
				    }
				    if (uiControls.Item3.Required && string.IsNullOrWhiteSpace(uiControls.Item2.Value.ToSafeString()))
				    {
				        ErrorHolder.Controls.Add(new Label
				        {
				            Text = @"Please fill out all required fields", ForeColor = System.Drawing.Color.Red
				        });
				        return;
				    }
				}

                // set ContentType
                if (rosterEvent.RosterEventDictionary.ContainsKey(FieldNames.CONTENT_TYPE_ID))
                {
                    if (rosterEvent.RosterEventDictionary[FieldNames.CONTENT_TYPE_ID] == null)
                    {
                        rosterEvent.RosterEventDictionary[FieldNames.CONTENT_TYPE_ID] = ContentTypeId;
                    }
                }
                else
                {
                    rosterEvent.RosterEventDictionary.Add(FieldNames.CONTENT_TYPE_ID,ContentTypeId);
                }

                // set StartDate and EndDate for AllDayEvent
                if (rosterEvent.RosterEventDictionary.ContainsKey(FieldNames.ALL_DAY_EVENT) &&
                    rosterEvent.RosterEventDictionary.ContainsKey(FieldNames.START_DATE) &&
                    rosterEvent.RosterEventDictionary.ContainsKey(FieldNames.END_DATE))
                {
                    var allDayEventObj = rosterEvent.RosterEventDictionary[FieldNames.ALL_DAY_EVENT];
                    if (allDayEventObj != null && allDayEventObj.ToBoolean())
                    {
                        var startDt = rosterEvent.RosterEventDictionary[FieldNames.START_DATE];
                        var endDt = rosterEvent.RosterEventDictionary[FieldNames.END_DATE];
                        if (startDt != null && endDt != null) {
                            rosterEvent.RosterEventDictionary[FieldNames.START_DATE] = ((DateTime)startDt).Date;
                            rosterEvent.RosterEventDictionary[FieldNames.END_DATE] = ((DateTime)endDt).Date.AddMinutes(1439);
                        }
                    }
                }

                // check by Master Roster - if it is a Template - NO new rosters
                var masterRosterId = (rosterEvent.RosterEventDictionary.ContainsKey(FieldNames.MASTER_ROSTER_ID)) ?
                    rosterEvent.RosterEventDictionary[FieldNames.MASTER_ROSTER_ID] : null;
                if (ControlMode == SPControlMode.New && this.ListId == TableIDs.PLANNED_ROSTERS &&
                    masterRosterId != null && _dataService.IsTemplate(masterRosterId.ToInt()))
                {
                    throw new Exception("It is not possible to modify Roster Template!"); // do not allow ADD to Template
                }

				_dataService.SaveRosterEvent(rosterEvent, ListId);
				Utils.GoBackOnSuccess(this.Page, this.Context);
            }
            catch (Exception ex)
            {
                ErrorHolder.Controls.Add(new Label
                {
                    Text = ex.ToReadbleSrting("Saving Error"), ForeColor = System.Drawing.Color.Red
                });
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Utils.GoBackOnSuccess(this.Page, this.Context);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (this.ControlMode == SPControlMode.Display && this.HasRights) {
                this.AppendRibbon();
            }

            if (Request.QueryString["IsDlg"] == null) {
                btnCancel.Attributes.Add("onClick", "history.back(-1); return false;");
            }
        }

        public void FillInFromTemplate(Guid tmplId)
        {
            if (ControlMode == SPControlMode.New && tmplId != Guid.Empty)
            {
                // Templates work ONLY on a NewForm
                try
                {
                    var tmplRoster = new RosterDataService().ListSingleRosterEvent(TableIDs.TEMPLATE_ROSTERS, tmplId);
                    if (tmplRoster != null) 
                    {
                        var tmplRosterProps = tmplRoster.RosterEventDictionary;
                        foreach (var rowItem in FieldControls)
                        {
                            if (!tmplRosterProps.ContainsKey(rowItem.Item3.InternalNameOriginal)) { continue; }
                            rowItem.Item2.Value = tmplRosterProps[rowItem.Item3.InternalNameOriginal];
                        }
                    }
                }
                catch (Exception ex) 
                {
                    ErrorHolder.Controls.Add(new Label() { Text = ex.Message, ForeColor = System.Drawing.Color.Red });
                }
            }
        }

        #endregion

        #region Private Methods

        private DbBaseFieldControl ItemRow(DbField fld, List<DbBaseFieldControl> subcontrols = null)
        {
            var fldRow = new TableRow();
           
            if (fld.Hidden) {
                fldRow.Style.Add("display", "none");
            }
            var titleCell = new TableCell {
                CssClass = "ms-formlabel",
                VerticalAlign = VerticalAlign.Top,
                Width = Unit.Pixel(113)
            };

            var displayName = fld.DisplayName;
            var lookup = fld as DbFieldLookup;
            if (lookup != null)
            {
                if (lookup.ListSource == (int) LookupSourceType.SpList)
                {
                    var list = SPContext.Current.SPList(lookup.ListId.ToGuid());
                    if (list != null)
                    {
                        if (lookup.LookupFields.Count() > 1)
                        {
                            displayName = string.Format("{0} : {1}", fld.DisplayName, 
                                list.FieldTitle(lookup.LookupFields[0].ToGuid()));                          
                        }
                    }
                }
            }

            var fldLabel = new Label {
                ID = fld.InternalName + "_label",
                Text = displayName
            };
            titleCell.Controls.Add(fldLabel);

            var bodyCell = new TableCell {
                CssClass = "ms-formbody",
                VerticalAlign = VerticalAlign.Top,
                Width = Unit.Pixel(350)
            };
            bodyCell.Attributes.Add("fieldname", fld.InternalName);
            var baseCtrl = fld.BaseControl;
            baseCtrl.ID = fld.InternalNameOriginal.ToNormalize();
            baseCtrl.ControlMode = ControlMode;
            if (subcontrols != null) {
                baseCtrl.Subcontrols.AddRange(subcontrols);
            }
            bodyCell.Controls.Add(baseCtrl);

            fldRow.Cells.Add(titleCell);
            fldRow.Cells.Add(bodyCell);

            FormTable.Rows.AddAt(0, fldRow);

            FieldControls.Add(new Tuple<Label, DbBaseFieldControl, DbField>(fldLabel, baseCtrl, fld));
            return baseCtrl;
        }

        private void AppendRibbon()
        {
            try
            {
                // GET View ribbon buttons
                var ribbonBtns = this.List.ListMetadataActions.Where(ca => ca.ContentTypeId == this.ContentTypeId && ca.IsVisibleForUser(SPContext.Current.Web.CurrentUser)).ToList();

                // add static buttons according to Permissions
                if (this.ItemId.HasValue)
                {
                    //if (_configService.HasRights(this.ItemId.Value, this.ListId, AccessRight.Write | AccessRight.Control | AccessRight.Delete))
                    //{
                    //    ribbonBtns.Add(
                    //        new ListMetadataAction()
                    //        {
                    //            LabelText = "Edit Item",
                    //            Description = "Edit this Item",
                    //            Sequence = 1,
                    //            Id = Guid.NewGuid(),
                    //            ImageUrl = "/_layouts/15/images/Roster.Presentation/editItem_32.png",
                    //            Command = string.Format("SP.UI.ModalDialog.ShowPopupDialog('{0}/{1}&ListId={2}&ID={3}');",
                    //                            SPContext.Current.Web.ServerRelativeUrl.TrimEnd('/'),
                    //                            this.ContentType.EditItemUrl, this.ListId.ToString(), this.ItemId.ToString())
                    //        }
                    //    );
                    //}

                    //if (_configService.HasRights(this.ItemId.Value, this.ListId, AccessRight.Control))
                    //{
                    //    ribbonBtns.Add(
                    //        new ListMetadataAction()
                    //        {
                    //            LabelText = "Manage Permissions",
                    //            Description = "Manage Permissions on this Item",
                    //            Sequence = 2,
                    //            Id = Guid.NewGuid(),
                    //            ImageUrl = "/_layouts/15/images/Roster.Presentation/mngPermissions_32.png",
                    //            Command = string.Format("javascript:window.location = '{0}/{1}?ListId={2}&ElemId={3}';",
                    //                            SPContext.Current.Web.ServerRelativeUrl.TrimEnd('/'),
                    //                            Constants.Pages.PERMISSIONS_PAGE_URL, this.ListId.ToString(), this.ItemId.ToString())
                    //        }
                    //    );
                    //}
                }

                if (ribbonBtns.Any()) {
                    Utils.AddRibbonButtonsToPage(ribbonBtns, this.Page);
                }

            }
            catch { }
        }

        private void AddSystemInfoRow(string prefix, string fld1, string fld2)
        {
            Tuple<int, ListMetadataField> userField = null;
            if (this.ItemId.HasValue && Item.RosterEventDictionary.ContainsKey(fld1) && Item.RosterEventDictionary.ContainsKey(fld2) &&
                    Item.RosterEventDictionary[fld1] != null && Item.RosterEventDictionary[fld2] != null &&
                    (userField = ListMetadataFields.FirstOrDefault(fld => fld.Item2.InternalName.Equals(fld2))) != null)
            {
                var createdRow = new TableRow();
                var createdCell = new TableCell {
                    Text = String.Format("{0} at {1} by {2}", prefix, ((DateTime)Item.RosterEventDictionary[fld1]).ToString("dd/MM/yyyy HH:mm"),
                        (userField.Item2.GetDbField() as DbFieldUser).EnsureUser(Item.RosterEventDictionary[fld2].ToInt()).DisplayText),
                    CssClass = "ms-descriptiontext"
                };
                createdRow.Cells.Add(createdCell);
                tblCreatedModified.Rows.Add(createdRow);
            }
        }

        #endregion
    }
}
