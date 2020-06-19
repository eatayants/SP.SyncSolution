using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Roster.Presentation.Controls.Fields;

namespace Roster.Presentation.Controls.FieldEditors
{
    public class DbFieldGuidEditor : CompositeControl, IDbFieldEditor
    {
        #region Private variables

        private RadioButtonList radioList_Required;
        private Label label_Required;

        private TextBox txt_DefaultValue;
        private Label label_DefaultValue;

        #endregion

        #region Properties

        public bool Required
        {
            get
            {
                EnsureChildControls();
                return (radioList_Required.SelectedValue == "1");
            }
            set
            {
                EnsureChildControls();

                string selectedValue = value ? "1" : "0";
                foreach (ListItem li in radioList_Required.Items)
                    li.Selected = (li.Value.Equals(selectedValue));
            }
        }

        public string DefaultValue
        {
            get
            {
                EnsureChildControls();
                return txt_DefaultValue.Text;
            }
            set
            {
                EnsureChildControls();
                txt_DefaultValue.Text = value;
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

            label_Required = new Label();
            label_Required.ID = "lblRequired";
            label_Required.Text = "Require that this column contains information:";

            radioList_Required = new RadioButtonList();
            radioList_Required.ID = "rListRequired";
            radioList_Required.RepeatDirection = RepeatDirection.Horizontal;
            radioList_Required.Items.AddRange(new[] { new ListItem("Yes", "1"), new ListItem("No", "0") });
            radioList_Required.SelectedValue = "0";

            label_DefaultValue = new Label();
            label_DefaultValue.ID = "lblDefaultValue";
            label_DefaultValue.Text = "Default value:";

            txt_DefaultValue = new TextBox();
            txt_DefaultValue.ID = "txtDefaultValue";
            txt_DefaultValue.CssClass = "ms-input";
            txt_DefaultValue.MaxLength = 255;

            this.Controls.Add(label_Required);
            this.Controls.Add(radioList_Required);
            this.Controls.Add(label_DefaultValue);
            this.Controls.Add(txt_DefaultValue);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            AddAttributesToRender(writer);

            writer.RenderBeginTag(HtmlTextWriterTag.Table); // <table>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            label_Required.RenderControl(writer); // <label/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            radioList_Required.RenderControl(writer); // <radioList/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            label_DefaultValue.RenderControl(writer); // <label/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            txt_DefaultValue.RenderControl(writer); // <input/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderEndTag(); // </table>
        }

        #region Interface methods

        public DbField GetField(string fieldName)
        {
            return new DbFieldGuid(fieldName)
            {
                Required = this.Required,
                DefaultValue = this.DefaultValue
            };
        }

        #endregion
    }
}
