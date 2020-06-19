using System;
using System.Linq;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Roster.Presentation.Helpers;
using Roster.Common;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using Roster.Model.DataContext;
using Roster.BL;
using System.Web.Script.Serialization;
using Roster.Presentation.Controls.NAVproxy;
using Roster.Presentation.Extensions;
using Roster.Presentation.Controls.Fields;

namespace Roster.Presentation.Layouts
{
    public partial class WebServiceSettingsPage : LayoutsPageBase
    {
        #region Private variables

        private Dictionary<string, DropDownList> _mapInfo;
        private List<ListMetadataField> m_listFields = null;

        #endregion

        #region Public properties

        public Dictionary<string, DropDownList> MapInfo
        {
            get
            {
                if (_mapInfo == null)
                {
                    _mapInfo = new Dictionary<string, DropDownList>() {
                        { EnumHelper.GetEnumDescription(CreateTimeSheetLines_Fields.Resource_No), ddlResourceNo},
                        { EnumHelper.GetEnumDescription(CreateTimeSheetLines_Fields.Type), ddlType },
                        { EnumHelper.GetEnumDescription(CreateTimeSheetLines_Fields.Job_No), ddlJobNo },
                        { EnumHelper.GetEnumDescription(CreateTimeSheetLines_Fields.Job_Task_No), ddlJobTaskNo },
                        { EnumHelper.GetEnumDescription(CreateTimeSheetLines_Fields.Cause_of_Absence_Code), ddlCauseOfAbsence },
                        { EnumHelper.GetEnumDescription(CreateTimeSheetLines_Fields.Description), ddlDescription },
                        { EnumHelper.GetEnumDescription(CreateTimeSheetLines_Fields.Work_Type_Code), ddlWorkType },
                        { EnumHelper.GetEnumDescription(CreateTimeSheetLines_Fields.Chargeable), ddlChargeable },
                        { EnumHelper.GetEnumDescription(CreateTimeSheetLines_Fields.Non_Roster_Day), ddlNonRosterDay },
                        { EnumHelper.GetEnumDescription(CreateTimeSheetLines_Fields.Start_Time), ddlStartTime },
                        { EnumHelper.GetEnumDescription(CreateTimeSheetLines_Fields.End_Time), ddlEndTime }
                    };
                }

                return _mapInfo;
            }
        }

        public List<ListMetadataField> TimesheetFields
        {
            get
            {
                if (m_listFields == null) {
                    this.m_listFields = new RosterConfigService().GetList(TableIDs.TIMESHEET_ROSTERS).ListMetadataFields.ToList();
                }

                return this.m_listFields;
            }
        }

        public string MappingSettings
        {
            get
            {
                var _mappingSet = this.MapInfo.Where(m => !string.IsNullOrEmpty(m.Value.SelectedValue)).Select(m => new MapElem { WebParam = m.Key, FieldName = m.Value.SelectedValue });
                return new JavaScriptSerializer().Serialize(_mappingSet);
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    JavaScriptSerializer jsSer = new JavaScriptSerializer();
                    var _mappingSet = jsSer.Deserialize<List<MapElem>>(value);

                    foreach (string key in this.MapInfo.Keys)
                    {
                        var mapEl = _mappingSet.FirstOrDefault(s => s.WebParam == key);
                        if (mapEl == null) { continue; }
                        DropDownList ddl = this.MapInfo[key];
                        ddl.SelectedValue = mapEl.FieldName;
                    }
                }
            }
        }

        #endregion

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Utils.GoBackOnSuccess(this, this.Context);
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var connectionInfo = new NavConnectionInfo
                {
                    CreateTimesheetUrl = txtCreateTimesheetUrl.Text,
                    ProcessTimesheetsUrl = txtProcessTimesheetsUrl.Text,
                    User = txtUser.Text,
                    Password = txtPassword.Text,
                    Mapping = this.MappingSettings
                };
                //connectionInfo.ValidateConnection();
                SPContext.Current.SetNavWebServiceConnection(connectionInfo);

                // go back
                Utils.GoBackOnSuccess(this, this.Context);
            }
            catch (Exception ex)
            {
                pannelError.Controls.Add(new Label { Text = ex.Message, ForeColor = System.Drawing.Color.Red });
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                try
                {
                    // init MAPPING
                    Dictionary<string, string> fieldsInfo = new Dictionary<string, string>();
                    foreach(ListMetadataField fld in this.TimesheetFields.OrderBy(fld => fld.FieldName))
                    {
                        fieldsInfo.Add(fld.InternalName, fld.FieldName);

                        if (fld.FieldType() == SPFieldType.Lookup) {
                            var lookupFld = fld.GetDbField() as DbFieldLookup;
                            lookupFld.LookupFields.ForEach(item => {
                                fieldsInfo.Add(string.Format("{0}${1}", fld.Id.ToString(), item), string.Format("{0}: {1}", fld.FieldName, item));
                            });
                        } else if (fld.FieldType() == SPFieldType.User) {
                            var userFld = fld.GetDbField() as DbFieldUser;
                            userFld.LookupFields.ForEach(item => {
                                fieldsInfo.Add(string.Format("{0}${1}", fld.Id.ToString(), item), string.Format("{0}: {1}", fld.FieldName, item));
                            });
                        }
                    }
                    foreach (string key in this.MapInfo.Keys) {
                        DropDownList ddl = this.MapInfo[key];
                        ddl.DataSource = fieldsInfo;
                        ddl.DataBind();
                    }

                    // connection info
                    var conInfo = SPContext.Current.GetNavWebServiceConnection();
                    txtCreateTimesheetUrl.Text = conInfo.CreateTimesheetUrl;
                    txtProcessTimesheetsUrl.Text = conInfo.ProcessTimesheetsUrl;
                    txtUser.Text = conInfo.User;
                    txtPassword.Text = conInfo.Password;
                    this.MappingSettings = conInfo.Mapping;
                }
                catch (Exception ex)
                {
                    pannelError.Controls.Add(new Label { Text = ex.Message, ForeColor = System.Drawing.Color.Red });
                }
            }
        }
    }

    public class MapElem
    {
        public string WebParam { get; set; }
        public string FieldName { get; set; }
    }
}
