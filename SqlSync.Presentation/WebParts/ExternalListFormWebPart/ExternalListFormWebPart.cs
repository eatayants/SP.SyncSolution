using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Roster.Presentation.Controls.Interfaces;

namespace Roster.Presentation.WebParts.ExternalListFormWebPart
{
    [ToolboxItemAttribute(false)]
    public class ExternalListFormWebPart : Microsoft.SharePoint.WebPartPages.WebPart
    {
        [WebBrowsable(true),
         WebDisplayName("History link"),
         WebDescription("path to History page"),
         Personalizable(PersonalizationScope.Shared),
         Category("Custom properties")]
        public string HistoryLinkUrl { get; set; }

        // Visual Studio might automatically update this path when you change the Visual Web Part project item.
        private const string _ascxPath = @"~/_CONTROLTEMPLATES/15/Roster.Presentation.WebParts/ExternalListFormWebPart/ExternalListFormWebPartUserControl.ascx";
        private ExternalListFormWebPartUserControl control;

        protected override void CreateChildControls()
        {
            control = Page.LoadControl(_ascxPath) as ExternalListFormWebPartUserControl;
            control.HistoryLinkUrl = this.HistoryLinkUrl;
            Controls.Add(control);
        }

        [ConnectionConsumer("Roster TemplateId Consumer", "TemplateIdConsumer")]
        public void RosterTemplateIdConsumer(IRosterTemplateId Provider)
        {
            if (Provider != null && Provider.TemplateId != Guid.Empty) {
                control.FillInFromTemplate(Provider.TemplateId);
            }
        }
    }
}
