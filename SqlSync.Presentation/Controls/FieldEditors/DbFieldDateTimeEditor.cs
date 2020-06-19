using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Roster.Presentation.Controls.Fields;

namespace Roster.Presentation.Controls.FieldEditors
{
    public class DbFieldDateTimeEditor : CompositeControl, IDbFieldEditor
    {
        #region Private variables

        private RadioButtonList radioList_Required;
        private Label label_Required;

        private RadioButtonList radioList_Format;
        private Label label_Format;

        private RadioButtonList radioList_DefaultValue;
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

        public string Format
        {
            get
            {
                EnsureChildControls();
                return radioList_Format.SelectedValue;
            }
            set
            {
                EnsureChildControls();

                ListItem lItm = radioList_Format.Items.FindByValue(value);
                if (lItm != null)
                    lItm.Selected = true;
            }
        }

        public string DefaultValue
        {
            get
            {
                EnsureChildControls();
                return radioList_DefaultValue.SelectedValue;
            }
            set
            {
                EnsureChildControls();

                foreach (ListItem li in radioList_DefaultValue.Items)
                    li.Selected = (li.Value.Equals(value));
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
            //radioList_Required.SelectedValue = "0";

            label_Format = new Label();
            label_Format.ID = "lblFormat";
            label_Format.Text = "Date and Time Format:";

            radioList_Format = new RadioButtonList();
            radioList_Format.ID = "rListFormat";
            radioList_Format.RepeatDirection = RepeatDirection.Horizontal;
            radioList_Format.Items.AddRange(new[] { new ListItem(" Date Only", "DateOnly"), new ListItem(" Date & Time", "DateTime") });
            radioList_Format.SelectedValue = "DateOnly";

            label_DefaultValue = new Label();
            label_DefaultValue.ID = "lblDefaultValue";
            label_DefaultValue.Text = "Default value:";

            radioList_DefaultValue = new RadioButtonList();
            radioList_DefaultValue.ID = "rListDefaultValue";
            radioList_DefaultValue.RepeatDirection = RepeatDirection.Vertical;
            radioList_DefaultValue.Items.AddRange(new[] { new ListItem(" (None)", ""), new ListItem(" Today's Date", "[today]") });
            radioList_DefaultValue.SelectedValue = "";

            this.Controls.Add(label_Required);
            this.Controls.Add(radioList_Required);
            this.Controls.Add(label_Format);
            this.Controls.Add(radioList_Format);
            this.Controls.Add(label_DefaultValue);
            this.Controls.Add(radioList_DefaultValue);
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
            label_Format.RenderControl(writer); // <label/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            radioList_Format.RenderControl(writer); // <input/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            label_DefaultValue.RenderControl(writer); // <label/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            radioList_DefaultValue.RenderControl(writer); // <input/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderEndTag(); // </table>
        }

        #region Interface methods

        public DbField GetField(string fieldName)
        {
            return new DbFieldDateTime(fieldName)
            {
                Required = this.Required,
                Format = this.Format,
                DefaultValue = this.DefaultValue
            };
        }

        #endregion
    }
}
