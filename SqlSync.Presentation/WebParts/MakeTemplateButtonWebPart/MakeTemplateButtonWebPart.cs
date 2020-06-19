using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Collections;
using Roster.Presentation.Controls.ToolParts;

namespace Roster.Presentation.WebParts.MakeTemplateButtonWebPart
{
    [ToolboxItemAttribute(false)]
    public class MakeTemplateButtonWebPart : Microsoft.SharePoint.WebPartPages.WebPart
    {
        // Visual Studio might automatically update this path when you change the Visual Web Part project item.
        private const string _ascxPath = @"~/_CONTROLTEMPLATES/15/Roster.Presentation.WebParts/MakeTemplateButtonWebPart/MakeTemplateButtonWebPartUserControl.ascx";
        public string CommaSeparatedAccounts { get; set; }

        protected override void CreateChildControls()
        {
            MakeTemplateButtonWebPartUserControl control = Page.LoadControl(_ascxPath) as MakeTemplateButtonWebPartUserControl;
            control.CommaSeparatedAccounts = this.CommaSeparatedAccounts;

            Controls.Add(control);
        }

        public override Microsoft.SharePoint.WebPartPages.ToolPart[] GetToolParts()
        {
            Microsoft.SharePoint.WebPartPages.ToolPart[] allToolParts = new Microsoft.SharePoint.WebPartPages.ToolPart[3];

            //Toolpart for Default properties for every webpart
            Microsoft.SharePoint.WebPartPages.WebPartToolPart standardToolPart = new Microsoft.SharePoint.WebPartPages.WebPartToolPart();
            //Toolpart for Custom properties for every webpart
            Microsoft.SharePoint.WebPartPages.CustomPropertyToolPart customToolParts = new Microsoft.SharePoint.WebPartPages.CustomPropertyToolPart();
            allToolParts[0] = standardToolPart;
            allToolParts[1] = customToolParts;
            //our custom toolpart
            allToolParts[2] = new CreateTemplateToolPart();

            return allToolParts;
        }
    }
}
