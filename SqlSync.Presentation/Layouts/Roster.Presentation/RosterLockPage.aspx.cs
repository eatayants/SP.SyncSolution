using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Roster.BL;
using System.Collections.Generic;
using Roster.Presentation.Helpers;
using Roster.Common;

namespace Roster.Presentation.Layouts
{
    public partial class RosterLockPage : LayoutsPageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnLock_Click(object sender, EventArgs e)
        {
            try
            {
                var dataService = new RosterDataService();
                {
                    List<Tuple<int, string>> trusteeRights = new List<Tuple<int, string>>();
                    if (chNeedResetRights.Checked) {
                        trusteeRights.AddRange(new [] {
                            new Tuple<int, string>((int)AccessRight.Read, RoleService.ACCOUNT_ID_EVERYONE.ToString()),
                            new Tuple<int, string>((int)AccessRight.Control, new RoleService().GetGroupIdForRole(Constants.Role.RosterAdmins, SPContext.Current.Web).ToString())
                        });
                    }

                    dataService.WorkingLock(
                        txtStoredProcName.Text,
                        dtStart.SelectedDate,
                        dtEnd.SelectedDate,
                        trusteeRights,
                        string.Format("{0} Locked at {1}. Locked by {2}. Lock period [{3} - {4}].",
                            txtReason.Text, DateTime.Now.ToString(), SPContext.Current.Web.CurrentUser.Name, dtStart.SelectedDate.ToString(), dtEnd.SelectedDate.ToString()));
                }

                // close form
                Utils.GoBackOnSuccess(this, this.Context);
            }
            catch (Exception ex)
            {
                ErrorHolder.Controls.Add(new System.Web.UI.WebControls.Label { Text = ex.Message });
            }
        }
    }
}
