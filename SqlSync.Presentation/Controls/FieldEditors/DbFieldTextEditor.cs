using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Roster.Presentation.Controls.Fields;

namespace Roster.Presentation.Controls.FieldEditors
{
    public class DbFieldTextEditor : CompositeControl, IDbFieldEditor
    {
        #region Private variables

        private RadioButtonList radioList_Required;
        private Label label_Required;

        private TextBox txt_MaxLength;
        private Label label_MaxLength;
        private RangeValidator validator_MaxLength;

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

        public int MaxLength
        {
            get
            {
                EnsureChildControls();

                int mLength = 0;
                if (Int32.TryParse(txt_MaxLength.Text, out mLength))
                    return mLength;
                else
                    return 255; // default value
            }
            set
            {
                EnsureChildControls();

                txt_MaxLength.Text = value.ToString();
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

            label_MaxLength = new Label();
            label_MaxLength.ID = "lblMaxLength";
            label_MaxLength.Text = "Maximum number of characters:";

            txt_MaxLength = new TextBox();
            txt_MaxLength.ID = "txtMaxLength";
            txt_MaxLength.CssClass = "ms-input";
            txt_MaxLength.TextMode = TextBoxMode.Number;
            txt_MaxLength.Text = "255"; // default value

            validator_MaxLength = new RangeValidator();
            validator_MaxLength.ID = "validatorMaxLength";
            validator_MaxLength.ControlToValidate = txt_MaxLength.ID;
            validator_MaxLength.Text = "The value must be from 1 to 255";
            validator_MaxLength.Display = ValidatorDisplay.Dynamic;
            validator_MaxLength.MinimumValue = "1";
            validator_MaxLength.MaximumValue = "255";

            label_DefaultValue = new Label();
            label_DefaultValue.ID = "lblDefaultValue";
            label_DefaultValue.Text = "Default value:";

            txt_DefaultValue = new TextBox();
            txt_DefaultValue.ID = "txtDefaultValue";
            txt_DefaultValue.CssClass = "ms-input";
            txt_DefaultValue.MaxLength = 255;

            this.Controls.Add(label_Required);
            this.Controls.Add(radioList_Required);
            this.Controls.Add(label_MaxLength);
            this.Controls.Add(txt_MaxLength);
            this.Controls.Add(validator_MaxLength);
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
            label_MaxLength.RenderControl(writer); // <label/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            txt_MaxLength.RenderControl(writer); // <input/>
            validator_MaxLength.RenderControl(writer); // <validator/>
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
            return new DbFieldText(fieldName)
            {
                Required = this.Required,
                MaxLength = this.MaxLength,
                DefaultValue = this.DefaultValue
            };
        }

        #endregion
    }
}
