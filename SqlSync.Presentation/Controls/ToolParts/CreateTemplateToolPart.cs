using Microsoft.SharePoint.WebControls;
using Roster.Presentation.WebParts.MakeTemplateButtonWebPart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;

namespace Roster.Presentation.Controls.ToolParts
{
    public class CreateTemplateToolPart : Microsoft.SharePoint.WebPartPages.ToolPart
    {
        PeopleEditor pplEditor;

        protected override void CreateChildControls()
        {
            pplEditor = new PeopleEditor();
            pplEditor.ID = "allowedPrincipalsSelector";
            pplEditor.SelectionSet = "SPGroup";
            pplEditor.MultiSelect = true;

            this.Controls.Add(pplEditor);

            //setting the last set values as current values on custom toolpart
            MakeTemplateButtonWebPart wp = (MakeTemplateButtonWebPart)this.ParentToolPane.SelectedWebPart;
            if (wp != null) {
                this.pplEditor.CommaSeparatedAccounts = wp.CommaSeparatedAccounts;
                this.pplEditor.Validate();
            }

            base.CreateChildControls();
        }

        protected override void RenderToolPart(HtmlTextWriter writer)
        {
            AddAttributesToRender(writer);

            writer.RenderBeginTag(HtmlTextWriterTag.Table); // <table>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            new LiteralControl("Principals: ").RenderControl(writer); // <label/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            pplEditor.RenderControl(writer); // <select/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderEndTag(); // </table>
        }

        public override void ApplyChanges()
        {
            MakeTemplateButtonWebPart wp = (MakeTemplateButtonWebPart)this.ParentToolPane.SelectedWebPart;
            wp.CommaSeparatedAccounts = this.pplEditor.CommaSeparatedAccounts;

            base.ApplyChanges();
        }
    }
}
