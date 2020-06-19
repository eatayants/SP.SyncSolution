using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Roster.Presentation.Controls.Interfaces;

namespace Roster.Presentation.WebParts.TemplateSelectorWebPart
{
    [ToolboxItemAttribute(false)]
    public class TemplateSelectorWebPart : WebPart, IRosterTemplateId
    {
        // Visual Studio might automatically update this path when you change the Visual Web Part project item.
        private const string _ascxPath = @"~/_CONTROLTEMPLATES/15/Roster.Presentation.WebParts/TemplateSelectorWebPart/TemplateSelectorWebPartUserControl.ascx";

        [Personalizable()]
        public Guid TemplateId { get; set; }

        protected override void CreateChildControls()
        {
            TemplateSelectorWebPartUserControl control = Page.LoadControl(_ascxPath) as TemplateSelectorWebPartUserControl;
            control.WebPartInst = this;

            Controls.Add(control);
        }

        [ConnectionProvider("TemplateId Provider for Rosters", "RosterTemplateIdProvider")]
        public IRosterTemplateId TextBoxStringProvider()
        {
            return this;
        }
    }
}
