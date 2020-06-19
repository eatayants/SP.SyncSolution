using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Roster.Presentation.Controls.Fields;

namespace Roster.Presentation.Controls.FieldEditors
{
    public class DbFieldNoteEditor : CompositeControl, IDbFieldEditor
    {
        #region Private variables

        private RadioButtonList radioList_Required;
        private Label label_Required;

        private TextBox txt_Lines;
        private Label label_Lines;
        private RangeValidator validator_Lines;

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

        public int NumberOfLines
        {
            get
            {
                EnsureChildControls();

                int mLines = 0;
                if (Int32.TryParse(txt_Lines.Text, out mLines))
                    return mLines;
                else
                    return 6; // default value
            }
            set
            {
                EnsureChildControls();

                txt_Lines.Text = value.ToString();
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

            label_Lines = new Label();
            label_Lines.ID = "lblNumberOfLines";
            label_Lines.Text = "Number of lines for editing: ";

            txt_Lines = new TextBox();
            txt_Lines.ID = "txtNumberOfLines";
            txt_Lines.CssClass = "ms-input";
            txt_Lines.TextMode = TextBoxMode.Number;
            txt_Lines.Text = "6"; // default value

            validator_Lines = new RangeValidator();
            validator_Lines.ID = "validatorNumberOfLines";
            validator_Lines.ControlToValidate = txt_Lines.ID;
            validator_Lines.Text = "The value must be from 1 to 60";
            validator_Lines.Display = ValidatorDisplay.Dynamic;
            validator_Lines.MinimumValue = "1";
            validator_Lines.MaximumValue = "60";

            this.Controls.Add(label_Required);
            this.Controls.Add(radioList_Required);
            this.Controls.Add(label_Lines);
            this.Controls.Add(txt_Lines);
            this.Controls.Add(validator_Lines);
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
            label_Lines.RenderControl(writer); // <label/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            txt_Lines.RenderControl(writer); // <input/>
            validator_Lines.RenderControl(writer); // <validator/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderEndTag(); // </table>
        }

        #region Interface methods

        public DbField GetField(string fieldName)
        {
            return new DbFieldNote(fieldName)
            {
                Required = this.Required,
                NumberOfLines = this.NumberOfLines
            };
        }

        #endregion
    }
}
