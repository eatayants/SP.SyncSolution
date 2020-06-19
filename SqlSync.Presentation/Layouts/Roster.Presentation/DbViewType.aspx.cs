using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Microsoft.SharePoint.Utilities;

namespace Roster.Presentation.Layouts
{
    public partial class DbViewType : LayoutsPageBase
    {
        private Guid m_listId = Guid.Empty;

        public Guid ListGuid
        {
            get
            {
                if (this.m_listId == Guid.Empty)
                {
                    Guid _lid;
                    string lstId = Request.QueryString["List"];
                    if (!string.IsNullOrEmpty(lstId) && Guid.TryParse(lstId, out _lid))
                        this.m_listId = _lid;
                }

                return this.m_listId;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                string encodedListId = SPEncode.UrlEncode(this.ListGuid.ToString("B").ToUpper());
                linkNewView.NavigateUrl += "List=" + encodedListId;
                linkNewViewCalendar.NavigateUrl += "List=" + encodedListId;
            }
        }
    }
}
