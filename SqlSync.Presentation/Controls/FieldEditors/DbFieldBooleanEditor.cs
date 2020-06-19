using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Roster.Presentation.Controls.FieldControls;
using Roster.Presentation.Controls.Fields;

namespace Roster.Presentation.Controls.FieldEditors
{
    public class DbFieldBooleanEditor : CompositeControl, IDbFieldEditor
    {
        #region Private variables

        private DropDownList ddl_DefaultValue;
        private Label label_DefaultValue;

        #endregion

        #region Properties

        public bool DefaultValue
        {
            get
            {
                EnsureChildControls();
                return (ddl_DefaultValue.SelectedValue == "1");
            }
            set
            {
                EnsureChildControls();
                string val = value ? "1" : "0";
                foreach (ListItem li in ddl_DefaultValue.Items)
                    li.Selected = (li.Value.Equals(val));
            }
        }

        #endregion

        protected override void RecreateChildControls()
        {
            EnsureChildControls();
        }

        protected override void CreateChildControls()
        {
            Controls.Clear();

            label_DefaultValue = new Label();
            label_DefaultValue.ID = "lblDefaultValue";
            label_DefaultValue.Text = "Default value:";

            ddl_DefaultValue = new DropDownList();
            ddl_DefaultValue.ID = "ddlDefaultValue";
            ddl_DefaultValue.Items.AddRange(new[] { new ListItem(" Yes", "1"), new ListItem(" No", "0") });

            this.Controls.Add(label_DefaultValue);
            this.Controls.Add(ddl_DefaultValue);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            AddAttributesToRender(writer);

            writer.RenderBeginTag(HtmlTextWriterTag.Table); // <table>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            label_DefaultValue.RenderControl(writer); // <label/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            ddl_DefaultValue.RenderControl(writer); // <select/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderEndTag(); // </table>
        }

        #region Interface methods

        public DbField GetField(string fieldName)
        {
            return new DbFieldBoolean(fieldName) {
                DefaultValue = this.DefaultValue ? "1" : "0"
            };
        }

        #endregion
    }
}
