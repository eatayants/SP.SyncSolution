using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Roster.Presentation.Controls.Fields;

namespace Roster.Presentation.Controls.FieldEditors
{
    public class DbFieldChoiceEditor : CompositeControl, IDbFieldEditor
    {
        #region Private variables

        private RadioButtonList radioList_Required;
        private Label label_Required;

        private TextBox txt_Choices;
        private Label label_Choices;

        private RadioButtonList radioList_ControlType;
        private Label label_ControlType;

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

                string val = value ? "1" : "0";
                foreach (ListItem li in radioList_Required.Items)
                    li.Selected = (li.Value.Equals(val));
            }
        }

        public string[] Choices
        {
            get
            {
                EnsureChildControls();

                return txt_Choices.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            }
            set
            {
                EnsureChildControls();

                if (value.Length > 0)
                    txt_Choices.Text = string.Join("\r\n", value);
            }
        }

        public string ControlType
        {
            get
            {
                EnsureChildControls();
                return radioList_ControlType.SelectedValue;
            }
            set
            {
                EnsureChildControls();

                foreach (ListItem li in radioList_ControlType.Items)
                    li.Selected = (li.Value.Equals(value));
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

            label_Choices = new Label();
            label_Choices.ID = "lblChoices";
            label_Choices.Text = "Type each choice on a separate line:";

            txt_Choices = new TextBox();
            txt_Choices.ID = "txtChoices";
            txt_Choices.CssClass = "ms-input";
            txt_Choices.TextMode = TextBoxMode.MultiLine;
            txt_Choices.Rows = 4;
            txt_Choices.Columns = 40;
            txt_Choices.Text = "Enter Choice #1\r\nEnter Choice #2\r\nEnter Choice #3"; // default value

            label_ControlType = new Label();
            label_ControlType.ID = "lblControlType";
            label_ControlType.Text = "Display choices using:";

            radioList_ControlType = new RadioButtonList();
            radioList_ControlType.ID = "rListControlType";
            radioList_ControlType.RepeatDirection = RepeatDirection.Vertical;
            radioList_ControlType.Items.AddRange(new[] { new ListItem("Drop-Down Menu", "Dropdown"), new ListItem("Radio Buttons", "RadioButtons"), new ListItem("Checkboxes (allow multiple selections)", "Checkboxes") });
            radioList_ControlType.SelectedValue = "Dropdown";

            label_DefaultValue = new Label();
            label_DefaultValue.ID = "lblDefaultValue";
            label_DefaultValue.Text = "Default value:";

            txt_DefaultValue = new TextBox();
            txt_DefaultValue.ID = "txtDefaultValue";
            txt_DefaultValue.CssClass = "ms-input";
            txt_DefaultValue.MaxLength = 255;

            this.Controls.Add(label_Required);
            this.Controls.Add(radioList_Required);
            this.Controls.Add(label_Choices);
            this.Controls.Add(txt_Choices);
            this.Controls.Add(label_ControlType);
            this.Controls.Add(radioList_ControlType);
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
            label_Choices.RenderControl(writer); // <label/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            txt_Choices.RenderControl(writer); // <textarea/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            label_ControlType.RenderControl(writer); // <label/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            radioList_ControlType.RenderControl(writer); // <radio/>
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
            return new DbFieldChoice(fieldName)
            {
                Required = this.Required,
                Choices = this.Choices,
                ControlType = this.ControlType,
                DefaultValue = this.DefaultValue
            };
        }

        #endregion
    }
}
