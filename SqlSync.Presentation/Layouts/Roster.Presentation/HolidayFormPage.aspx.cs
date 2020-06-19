using System;
using System.Linq;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Roster.Common;
using Roster.Model.DataContext;
using Roster.BL;
using System.Web.UI.WebControls;
using Roster.Presentation.Helpers;
using System.Collections.Generic;
using Microsoft.SharePoint.Utilities;
using System.Web.UI;

namespace Roster.Presentation.Layouts
{
    public partial class HolidayFormPage : LayoutsPageBase
    {
        #region Private variables

        private SPControlMode _mode = SPControlMode.Invalid;
        private HolidayEvent _item;

        private readonly RosterDataService _dataService = new RosterDataService();

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

        public Guid? HolidayId
        {
            get
            {
                return Request.QueryString["ID"].ToNullableGuid();
            }
        }

        public HolidayEvent Item
        {
            get
            {
                return HolidayId.HasValue ? _item ?? (_item = _dataService.Get(this.HolidayId.Value)) : null;
            }
        }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                if (ControlMode == SPControlMode.Invalid) {
                    ControlMode = SPControlMode.Display;
                }

                holidayForm.DefaultMode = (ControlMode == SPControlMode.Display) ? DetailsViewMode.ReadOnly : DetailsViewMode.Edit;

                if (ControlMode != SPControlMode.New)
                {
                    holidayForm.DataSource = new object[] {
                        new {
                            HolidayName = Item.Holiday.Name,
                            HolidayDate = Item.HolidayDate,
                            HolidayType = (ControlMode == SPControlMode.Display) ? Item.Holiday.HolidayType.Name : Item.Holiday.HolidayType.Id.ToString().ToUpper(),
                            Observed = (ControlMode == SPControlMode.Display) ?
                                            String.Join("; ", Item.HolidayObserveds.Select(state => state.State.Name)) :
                                            String.Join("; ", Item.HolidayObserveds.Select(state => state.StateId))
                        }
                    };
                }
                else
                {
                    holidayForm.DataSource = new object[] {
                        new {
                            HolidayName = "",
                            HolidayDate = Request.QueryString["HolidayDate"] != null ?
                                SPUtility.CreateDateTimeFromISO8601DateTimeString(Request.QueryString["HolidayDate"]) :
                                DateTime.Today,
                            HolidayType = "",
                            Observed = ""
                        }
                    };
                }
                holidayForm.DataBind();

                // init Buttons visibility
                btnSave.Visible = btnCancel.Visible = (ControlMode != SPControlMode.Display);
                btnDelete.Visible = (ControlMode != SPControlMode.New);

                // init Page Title
                string pageName = "Holiday Form";
                switch (this.ControlMode)
                {
                    case SPControlMode.New: {
                        pageName = @"New Holiday"; break;
                    }
                    case SPControlMode.Display: {
                        pageName = @"Display Holiday"; break;
                    }
                    case SPControlMode.Edit: {
                        pageName = @"Edit Holiday"; break;
                    }
                }
                lblPageName.Text = pageName;
            }
        }

        protected void holidayForm_DataBound(object sender, EventArgs e) 
        { 
            if (holidayForm.CurrentMode == DetailsViewMode.Edit) 
            {
                ListBox ddlObserved = holidayForm.FindControl("ddlObserved") as ListBox;
                if (ddlObserved != null)
                {
                    ddlObserved.DataTextField = "Name";
                    ddlObserved.DataValueField = "Id";
                    ddlObserved.DataSource = _dataService.TableContent("State", "Id", "Id$Name").Select(x => {
                        var elem = x.Item2 as IDictionary<string, object>;
                        return new { Id = elem["Id"], Name = elem["Name"].ToSafeString() };
                    }); 
                    ddlObserved.DataBind();
                }

                DropDownList ddlType = holidayForm.FindControl("ddlType") as DropDownList;
                if (ddlType != null)
                {
                    ddlType.DataTextField = "Name";
                    ddlType.DataValueField = "Id";
                    ddlType.DataSource = _dataService.TableContent("HolidayType", "Id", "Id$Name").Select(x => {
                        var elem = x.Item2 as IDictionary<string, object>;
                        return new { Id = elem["Id"], Name = elem["Name"].ToSafeString() };
                    });
                    ddlType.DataBind();
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var holidayEvent = (ControlMode == SPControlMode.New) ? new HolidayEvent() { Id = Guid.NewGuid() } : Item;
                if (holidayEvent == null) {
                    throw new Exception("Holiday event is empty");
                }

                if (holidayEvent.Holiday == null) {
                    holidayEvent.Holiday = new Holiday { Id = Guid.NewGuid() };
                }

                // set Holiday Name
                holidayEvent.Holiday.Name = (holidayForm.FindControl("txtName") as TextBox).Text;

                // set Holiday Date
                holidayEvent.HolidayDate = (holidayForm.FindControl("dtHolidayDate") as DateTimeControl).SelectedDate;

                // set Holiday type
                DropDownList ddlType = holidayForm.FindControl("ddlType") as DropDownList;
                holidayEvent.Holiday.TypeId = ddlType.SelectedValue.ToInt();

                // set Observeds
                ListBox ddlObserved = holidayForm.FindControl("ddlObserved") as ListBox;
                holidayEvent.HolidayObserveds.Clear();
                foreach (ListItem li in ddlObserved.Items.Cast<ListItem>().Where(itm => itm.Selected)) {
                    holidayEvent.HolidayObserveds.Add(new HolidayObserved { Id = Guid.NewGuid(), HolidayEventId = holidayEvent.Id, StateId = 6 });
                }

                _dataService.SaveHoliday(holidayEvent);
                Utils.GoBackOnSuccess(this.Page, this.Context);
            }
            catch (Exception ex)
            {
                ErrorHolder.Controls.Add(new Label {
                    Text = ex.ToReadbleSrting("Saving Error"), ForeColor = System.Drawing.Color.Red
                });
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Utils.GoBackOnSuccess(this.Page, this.Context);
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (Item == null) {
                    throw new Exception("Holiday event is empty");
                }

                _dataService.DeleteHoliday(this.HolidayId.Value);

                //_dataService.DeleteHoliday(Item);
                Utils.GoBackOnSuccess(this.Page, this.Context);
            }
            catch (Exception ex)
            {
                ErrorHolder.Controls.Add(new Label {
                    Text = ex.ToReadbleSrting("Deleting Error"), ForeColor = System.Drawing.Color.Red
                });
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (this.ControlMode == SPControlMode.Display) {
                // GET View ribbon buttons
                var ribbonBtns = new List<ListMetadataAction>(new[] {
                    new ListMetadataAction() {
                        LabelText = "Edit", Description = "Edit Holiday", Sequence = 1, Id = Guid.NewGuid(),
                        ImageUrl = "/_layouts/15/images/Roster.Presentation/editItem_32.png",
                        Command = "SP.UI.ModalDialog.OpenPopUpPage('" +
                                        SPContext.Current.Web.ServerRelativeUrl.TrimEnd('/') + Constants.Pages.HOLIDAY_FORM_PAGE_URL + "?Mode=2" + "&ID=" + Item.Id.ToString() + "');" }
                });

                if (ribbonBtns.Any()) {
                    Utils.AddRibbonButtonsToPage(ribbonBtns, this.Page);
                }
            }

            if (Request.QueryString["IsDlg"] == null) {
                btnCancel.Attributes.Add("onClick", "history.back(-1); return false;");
            }
        }

        #region Private methods


        #endregion
    }
}
