using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Roster.Presentation.Controls.Interfaces;

namespace Roster.Presentation.WebParts.ManagerViewFilterWebPart
{
    [ToolboxItemAttribute(false)]
    public class ManagerViewFilterWebPart : Microsoft.SharePoint.WebPartPages.WebPart, IStatusFilter
    {
        [WebBrowsable(true),
         WebDisplayName("WorkerId URL param name"),
         Personalizable(PersonalizationScope.Shared),
         Category("Custom properties")]
        public string WorkerIdParamName { get; set; }

        [WebBrowsable(true),
         WebDisplayName("Status IDs"),
         WebDescription("Status ids to filter rosters"),
         Personalizable(PersonalizationScope.Shared),
         Category("Custom properties")]
        public string StatusIDs { get; set; }

        [WebBrowsable(true),
         WebDisplayName("Status Field name"),
         Personalizable(PersonalizationScope.Shared),
         Category("Custom properties")]
        public string StatusFldName { get; set; }

        [WebBrowsable(true),
         WebDisplayName("Tab name"),
         Personalizable(PersonalizationScope.Shared),
         Category("Custom properties")]
        public string TabName { get; set; }

        [WebBrowsable(true),
         WebDisplayName("Action name: bulk approve"),
         Personalizable(PersonalizationScope.Shared),
         Category("Custom properties")]
        public string ApproveActionName { get; set; }

        [WebBrowsable(true),
         WebDisplayName("Action name: bulk endorse"),
         Personalizable(PersonalizationScope.Shared),
         Category("Custom properties")]
        public string EndorseActionName { get; set; }

        [WebBrowsable(true),
         WebDisplayName("Action name: send to NAV"),
         Personalizable(PersonalizationScope.Shared),
         Category("Custom properties")]
        public string SendNavActionName { get; set; }

        // Visual Studio might automatically update this path when you change the Visual Web Part project item.
        private const string _ascxPath = @"~/_CONTROLTEMPLATES/15/Roster.Presentation.WebParts/ManagerViewFilterWebPart/ManagerViewFilterWebPartUserControl.ascx";

        protected override void CreateChildControls()
        {
            ManagerViewFilterWebPartUserControl control = Page.LoadControl(_ascxPath) as ManagerViewFilterWebPartUserControl;
            control.WorkerIdUrlParamName = this.WorkerIdParamName;
            control.StatusIDs = this.StatusIDs;
            control.TabName = (string)this.TabName ?? "FakeTab";
            control.ApproveActionName = (string)this.ApproveActionName ?? string.Empty;
            control.EndorseActionName = (string)this.EndorseActionName ?? string.Empty;
            control.SendNavActionName = (string)this.SendNavActionName ?? string.Empty;

            Controls.Add(control);
        }

        string IStatusFilter.StatusIDs
        {
            get { return this.StatusIDs; }
        }

        string IStatusFilter.StatusFieldName
        {
            get { return string.IsNullOrEmpty(this.StatusFldName) ? Roster.Common.FieldNames.STATUS_ID : this.StatusFldName; }
        }

        [ConnectionProvider("Status IDs")]
        public IStatusFilter StatusFilterProvider()
        {
            return this;
        }
    }
}
