using Roster.Model.DataContext;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Roster.Presentation.Helpers;
using Microsoft.SharePoint;
using Roster.Common;
using Roster.BL;
using Roster.BL.Extentions;
using Roster.Presentation.Controls.NAVproxy;
using System.Net;
using Roster.Presentation.Controls.Fields;
using Roster.Presentation.Extensions;
using System.Reflection;
using System.Text;

namespace Roster.Presentation.WebParts.ManagerViewFilterWebPart
{
    public partial class ManagerViewFilterWebPartUserControl : UserControl
    {
        #region Public properties

        public string WorkerIdUrlParamName { get; set; }
        public string StatusIDs { get; set; }

        // Tabs and Actions
        public string TabName { get; set; }
        public string ApproveActionName { get; set; }
        public string EndorseActionName { get; set; }
        public string SendNavActionName { get; set; }

        public int WorkerId
        {
            get
            {
                return string.IsNullOrEmpty(this.WorkerIdUrlParamName) ? 0 : Request.QueryString[this.WorkerIdUrlParamName].ToInt();
            }
        }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnBulkApprove_Click(object sender, EventArgs e)
        {
            if (this.IsApprovePossible())
            {
                try
                {
                    if (!IsCurrentUserAManagerForWorker())
                        throw new Exception("Only Manager can approve timesheets for current user!");

                    // get timesheets to APPROVE
                    var timesheetRosters = this.GetTimesheetsByStatus(8); // 8 = 'Endorsed'

                    // init StoredProcedure params
                    var paramCollection = new List<Tuple<string, object>>();
                    paramCollection.Add(new Tuple<string, object>("@id", String.Join(",", timesheetRosters.Select(tsr => tsr.Id.ToString()))));
                    paramCollection.Add(new Tuple<string, object>("@reason", string.Format("Bulk approve at {0}", DateTime.Now.ToString())));
                    paramCollection.Add(new Tuple<string, object>("@message", ""));

                    string message = new RosterDataService().ExecuteProcedure("[dbo].[Approved]", paramCollection);
                    txtNotification.Value = "Timesheets approved successfully";
                }
                catch (Exception ex)
                {
                    txtErrorMsg.Value = ex.Message;
                }
            }
        }

        protected void btnBulkSendToNAV_Click(object sender, EventArgs e)
        {
            try
            {
                var config = SPContext.Current.GetNavWebServiceConnection();
                if (config.IsEmpty())
                    throw new Exception("NAV connection info has not been specified!");

                if (!IsCurrentUserAManagerForWorker())
                    throw new Exception("Only Manager can Send timesheets to NAV for current user!");

                // get timesheets to Send to NAV
                var approvedRosters = this.GetTimesheetsByStatus(10); // 10 = 'Approved'
                if (!approvedRosters.Any())
                    throw new Exception("There is nothing to send to NAV");

                // get mapping
                var _mappingSettings = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<List<Roster.Presentation.Layouts.MapElem>>(config.Mapping);

                var calendarPeriod = Utils.GetCalendarViewPeriod(SPContext.Current.Web.GetDateOptions(Request), "week");
                var listDbFields = new RosterConfigService().GetList(TableIDs.TIMESHEET_ROSTERS).ListMetadataFields.Select(fld => fld.GetDbField());

                // init Servicea
                CreateTimeSheetLines_Service srv = new CreateTimeSheetLines_Service();
                srv.Url = config.CreateTimesheetUrl;
                srv.Credentials = new NetworkCredential(config.User, config.Password);
                ProcessTimeSheets procSrv = new ProcessTimeSheets();
                procSrv.Url = config.ProcessTimesheetsUrl;
                procSrv.Credentials = new NetworkCredential(config.User, config.Password);

                Dictionary<Guid, string> errors = new Dictionary<Guid, string>();
                List<Guid> successIDs = new List<Guid>();
                foreach(var roster in approvedRosters)
                {
                    string batchNo = Guid.NewGuid().ToString("N").Substring(0, 20);
                    try
                    {
                        var newLine = new CreateTimeSheetLines();
                        // generic values
                        newLine.Key = "0";
                        newLine.External_Time_Sheet_No = batchNo;
                        newLine.Time_Sheet_Starting_Date = calendarPeriod.Item1.Date;
                        newLine.Time_Sheet_Starting_DateSpecified = true;
                        // fill Line from timesheet roster
                        newLine.FillFromRoster(roster, listDbFields, _mappingSettings);

                        // create entry in NAV temp table
                        srv.Create(ref newLine);
                        // process timesheet entry
                        procSrv.ProcessTimeSheet(batchNo, false);

                        successIDs.Add(roster.Id);
                    }
                    catch(Exception ex)
                    {
                        errors.Add(roster.Id, ex.Message);
                    }
                }

                // init StoredProcedure params
                var paramCollection = new List<Tuple<string, object>>();
                paramCollection.Add(new Tuple<string, object>("@id", String.Join(",", successIDs)));
                paramCollection.Add(new Tuple<string, object>("@reason", string.Format("Bulk submit to NAV at {0}", DateTime.Now.ToString())));
                paramCollection.Add(new Tuple<string, object>("@message", ""));
                // set new STATUS
                string message = new RosterDataService().ExecuteProcedure("[dbo].[SubmitedToNAV]", paramCollection);

                if (errors.Any()) {
                    throw new Exception(String.Join("<br/>", errors.Select(er => string.Format("Error submitting roster '{0}': {1}", er.Key, er.Value))));
                }

                txtNotification.Value = "Success";
            }
            catch (Exception ex)
            {
                txtErrorMsg.Value = ex.Message;
            }
        }

        protected void btnBulkEndorse_Click(object sender, EventArgs e)
        {
            try
            {
                if (!IsCurrentUserATeamLeaderForWorker())
                    throw new Exception("Only TeamLeader can endorse timesheets for current user!");

                // get timesheets to ENDORSE
                var timesheetRosters = this.GetTimesheetsByStatus(7); // 7 = 'Confirmed'

                // init StoredProcedure params
                var paramCollection = new List<Tuple<string, object>>();
                paramCollection.Add(new Tuple<string, object>("@id", String.Join(",", timesheetRosters.Select(tsr => tsr.Id.ToString()))));
                paramCollection.Add(new Tuple<string, object>("@reason", string.Format("Bulk endorse at {0}", DateTime.Now.ToString())));
                paramCollection.Add(new Tuple<string, object>("@message", ""));

                string message = new RosterDataService().ExecuteProcedure("[dbo].[Endorsed]", paramCollection);
                txtNotification.Value = "Timesheets endorsed successfully";
            }
            catch (Exception ex)
            {
                txtErrorMsg.Value = ex.Message;
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Display Ribbon buttons
            //this.ShowRibbonButtons();
            this.RegisterRibbonButtonActions();
        }

        #region Private methods

        private bool IsApprovePossible()
        {
            DateTime weekStart = Utils.GetCalendarViewPeriod(SPContext.Current.Web.GetDateOptions(Request), "week").Item1;

            return this.WorkerId != 0 && new RosterDataService().IsAllowApprove(weekStart, this.WorkerId);
        }

        private bool IsCurrentUserAManagerForWorker()
        {
            string currentUserLogin = SPContext.Current.Web.CurrentUser.LoginName;
            List<string> userLoginAndGroups = new List<string>();
            userLoginAndGroups.Add(currentUserLogin);
            userLoginAndGroups.AddRange(SPContext.Current.Web.CurrentUser.Groups.OfType<SPGroup>().Select(gr => gr.Name));

            List<Tuple<string, string>> whereCriteria = new List<Tuple<string, string>> {
                        new Tuple<string,string>("Id", this.WorkerId.ToString())
                    };
            var workersInfo = new RosterDataService().TableContent("WorkerPerson", "Id", FieldNames.WORKER_MANAGER, whereCriteria).Select(x => {
                var elem = x.Item2 as IDictionary<string, object>;
                return new { WorkerId = x.Item1, Manager = elem[FieldNames.WORKER_MANAGER].ToSafeString() };
            });

            //return workersInfo.Any() && workersInfo.FirstOrDefault().Manager.Equals(currentUserLogin);
            return workersInfo.Any() && userLoginAndGroups.Contains(workersInfo.FirstOrDefault().Manager);
        }

        private bool IsCurrentUserATeamLeaderForWorker()
        {
            string currentUserLogin = SPContext.Current.Web.CurrentUser.LoginName;
            List<string> userLoginAndGroups = new List<string>();
            userLoginAndGroups.Add(currentUserLogin);
            userLoginAndGroups.AddRange(SPContext.Current.Web.CurrentUser.Groups.OfType<SPGroup>().Select(gr => gr.Name));

            List<Tuple<string, string>> whereCriteria = new List<Tuple<string, string>> {
                        new Tuple<string,string>("Id", this.WorkerId.ToString())
                    };
            var workersInfo = new RosterDataService().TableContent("WorkerPerson", "Id", FieldNames.WORKER_TEAMLEADER, whereCriteria).Select(x => {
                var elem = x.Item2 as IDictionary<string, object>;
                return new { WorkerId = x.Item1, TeamLeader = elem[FieldNames.WORKER_TEAMLEADER].ToSafeString() };
            });

            //return workersInfo.Any() && workersInfo.FirstOrDefault().TeamLeader.Equals(currentUserLogin);
            return workersInfo.Any() && userLoginAndGroups.Contains(workersInfo.FirstOrDefault().TeamLeader);
        }

        private List<RosterEvent> GetTimesheetsByStatus(int statusId)
        {
            var dataService = new RosterDataService();
            var query = new QueryParams();
            var listFields = new RosterConfigService().GetList(TableIDs.TIMESHEET_ROSTERS).ListMetadataFields;

            // add filter by WorkerId
            var workerFld = listFields.FirstOrDefault(item => item.InternalName == FieldNames.WORKER_PERSON_ID);
            query.WhereCriteria.Add(new Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>(workerFld, CompareType.Equal, ConcateOperator.And, this.WorkerId, null));

            // add filter by StatusId
            var statusFld = listFields.FirstOrDefault(item => item.InternalName == FieldNames.STATUS_ID);
            query.WhereCriteria.Add(new Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>(statusFld, CompareType.Equal, ConcateOperator.And, statusId, null));

            // add filter according period displayed by Calendar
            var calendarPeriod = Utils.GetCalendarViewPeriod(SPContext.Current.Web.GetDateOptions(Request), "week");
            var startDateFld = listFields.FirstOrDefault(item => item.InternalName == FieldNames.START_DATE);
            var endDateFld = listFields.FirstOrDefault(item => item.InternalName == FieldNames.END_DATE);
            query.WhereCriteria.Add(new Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>(startDateFld, CompareType.LessOrEqual, ConcateOperator.And, calendarPeriod.Item2, null));
            query.WhereCriteria.Add(new Tuple<ListMetadataField, CompareType, ConcateOperator, object, string>(endDateFld, CompareType.MoreOrEqual, ConcateOperator.And, calendarPeriod.Item1, null));

            // get timesheets
            return dataService.ListRosterEvents(TableIDs.TIMESHEET_ROSTERS, query);
        }

        private void RegisterRibbonButtonActions()
        {
            bool isTeamLeader = this.IsCurrentUserATeamLeaderForWorker();
            bool isManager = this.IsCurrentUserAManagerForWorker();
            bool canApprove = this.IsApprovePossible();

            StringBuilder customScript = new StringBuilder();
            customScript.Append("<script type=\"text/javascript\"> function DoBulkEndorse() {");
            customScript.AppendFormat("{0}}} ", isTeamLeader ? Page.GetPostBackEventReference(this.btnBulkEndorse) : "alert('Only TeamLeader can endorse timesheets for current user')");
            customScript.Append("function DoBulkApprove() {");
            customScript.AppendFormat("{0}}} ", isManager ? Page.GetPostBackEventReference(this.btnBulkApprove) : "alert('Only Manager can approve timesheets for current user')");
            customScript.Append("function DoSendToNAV() {");
            customScript.AppendFormat("{0}}} ", isManager ? Page.GetPostBackEventReference(this.btnBulkSendToNAV) : "alert('Only Manager can send to NAV')");
            customScript.Append("function ServerSideHideFunc(tabTitle, tabEl) {");
            customScript.Append("if (tabTitle.toLowerCase()=='" + this.TabName.ToLower() + "') {");
            if (!isTeamLeader) {
                customScript.Append("HideTabAction(tabEl, '" + this.EndorseActionName + "');");
            }
            if (!isManager || !canApprove) {
                customScript.Append("HideTabAction(tabEl, '" + this.ApproveActionName + "');");
            }
            if (!isManager) {
                customScript.Append("HideTabAction(tabEl, '" + this.SendNavActionName + "');");
            }
            customScript.Append("}} </");
            customScript.Append("script>");

            System.Type csType = this.GetType();
            ClientScriptManager csm = Page.ClientScript;
            string globalVarKey = "RosterBulkActions";
            if (!csm.IsClientScriptBlockRegistered(csType, globalVarKey))
                csm.RegisterClientScriptBlock(csType, globalVarKey, customScript.ToString(), false);
        }

        private void ShowRibbonButtons()
        {
            try
            {
                // GET View ribbon buttons
                var ribbonBtns = new List<ListMetadataAction>();

                // this buttons are only visible for worker's TeamLeader
                if (IsCurrentUserATeamLeaderForWorker())
                {
                    ribbonBtns.Add(new ListMetadataAction() {
                        LabelText = "Bulk Endorse", Description = "Endorse all timesheets from current period", Sequence = 1,
                        Id = Guid.NewGuid(),
                        ImageUrl = "/_layouts/15/images/ManageWorkflow32.png",
                        Command = Page.GetPostBackEventReference(this.btnBulkEndorse)                                    
                    });
                }

                // this buttons are only visible for worker's MANAGER
                if (IsCurrentUserAManagerForWorker())
                {
                    if (this.IsApprovePossible())
                    {
                        ribbonBtns.Add(new ListMetadataAction() {
                            LabelText = "Bulk Approve", Description = "Approve all timesheets from current period", Sequence = 2,
                            Id = Guid.NewGuid(),
                            ImageUrl = "/_layouts/15/images/Roster.Presentation/approve_32.png",
                            Command = Page.GetPostBackEventReference(this.btnBulkApprove)
                        });
                    }

                    ribbonBtns.Add(new ListMetadataAction() {
                        LabelText = "Send to NAV", Description = "Send all approved timesheets from current period to NAV", Sequence = 3,
                        Id = Guid.NewGuid(),
                        ImageUrl = "/_layouts/15/images/Roster.Presentation/sendToNav_32.png",
                        Command = SPContext.Current.GetNavWebServiceConnection().IsEmpty() ?
                                    "javascript:alert('NAV connection info has not been specified!')" :
                                    Page.GetPostBackEventReference(this.btnBulkSendToNAV)
                    });
                }

                if (ribbonBtns.Any()) {
                    Utils.AddRibbonButtonsToPage(ribbonBtns, this.Page);
                }

            }
            catch { }
        }

        #endregion
    }
}
