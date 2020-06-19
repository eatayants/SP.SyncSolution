using System;
using System.Linq;
using System.Web.UI;
using Microsoft.SharePoint;

namespace Roster.Presentation.WebParts.MakeTemplateButtonWebPart
{
    public partial class MakeTemplateButtonWebPartUserControl : UserControl
    {
        public string CommaSeparatedAccounts { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(CommaSeparatedAccounts))
            {
                try {
                    createTemplatePanel.Visible = SPContext.Current.Web.CurrentUser.ID == SPContext.Current.Site.SystemAccount.ID ||
                        CommaSeparatedAccounts.Split(',').Select(g => g.ToUpper())
                            .Intersect(SPContext.Current.Web.CurrentUser.Groups.Cast<SPGroup>().Select(gr => gr.Name.ToUpper())).Any();
                } catch {
                    createTemplatePanel.Visible = false;
                }
            }
        }
    }
}
