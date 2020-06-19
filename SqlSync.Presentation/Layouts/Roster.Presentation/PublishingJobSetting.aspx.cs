using System;
using System.Drawing;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;
using Roster.BL;
using Roster.Common;
using Roster.Model.DataContext;
using Roster.Presentation.Helpers;
using Label = Microsoft.SharePoint.Mobile.Controls.Label;

namespace Roster.Presentation.Layouts
{
    public partial class PublishingJobSetting : LayoutsPageBase
    {
        private readonly RosterConfigService _configProvider = new RosterConfigService();

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!Page.IsPostBack)
                {
                    txtDaysAhead.Text = _configProvider.PlannedDaysAhead().ToSafeString();
                    pnAgentStatus.Controls.Add(_configProvider.SqlAgentRunned()
                        ? new Label { Text = "SQLServer Agent is runned", ForeColor = Color.Green }
                        : new Label { Text = "SQLServer Agent is stopped", ForeColor = Color.Red });
                    JobSettingGrid.DataSource = _configProvider.SqlAgentJobs();
                    JobSettingGrid.DataBind();
                }
            }
            catch (Exception ex)
            {
                pannelError.Controls.Add(new Label{ Text = ex.Message, ForeColor = Color.Red });
            }
        }

        
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                Utils.GoBackOnSuccess(this, Context);
            }
            catch (Exception ex)
            {
                pannelError.Controls.Add(new Label { Text = ex.Message, ForeColor = Color.Red });
            }
        }
        
        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // update data in DB
                _configProvider.PlannedDaysAhead(txtDaysAhead.Text.ToInt());
                Utils.GoBackOnSuccess(this, Context);
            }
            catch (Exception ex)
            {
                pannelError.Controls.Add(new Label { Text = ex.Message, ForeColor = Color.Red });
            }
        }
        protected void JobSettingGrid_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                
            }
        }
    }
}
