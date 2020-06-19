using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Roster.Presentation.Controls.Fields;

namespace Roster.Presentation.Controls.FieldEditors
{
    public class DbFieldNumberEditor : CompositeControl, IDbFieldEditor
    {
        #region Private variables

        private RadioButtonList radioList_Required;
        private Label label_Required;

        private Label label_MaxMin;

        private TextBox txt_MaxValue;
        private Label label_MaxValue;

        private TextBox txt_MinValue;
        private Label label_MinValue;

        private DropDownList ddl_DecimalPlaces;
        private Label label_DecimalPlaces;

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

        public double MaxValue
        {
            get
            {
                EnsureChildControls();

                double mMval = 0;
                if (Double.TryParse(txt_MaxValue.Text, out mMval))
                    return mMval;
                else
                    return 0; // default value
            }
            set
            {
                EnsureChildControls();

                txt_MaxValue.Text = value.ToString();
            }
        }
        public double MinValue
        {
            get
            {
                EnsureChildControls();

                double mMval = 0;
                if (Double.TryParse(txt_MinValue.Text, out mMval))
                    return mMval;
                else
                    return 0; // default value
            }
            set
            {
                EnsureChildControls();

                txt_MinValue.Text = value.ToString();
            }
        }

        public int DecimalPlaces
        {
            get
            {
                EnsureChildControls();
                return Int32.Parse(ddl_DecimalPlaces.SelectedValue);
            }
            set
            {
                EnsureChildControls();

                if (value >= -1 && value <= 5) {
                    foreach (ListItem li in ddl_DecimalPlaces.Items)
                        li.Selected = (li.Value.Equals(value.ToString()));
                }
            }
        }

        public double? DefaultValue
        {
            get
            {
                EnsureChildControls();
                double dv = 0;
                if (Double.TryParse(txt_DefaultValue.Text, out dv))
                    return dv;
                else
                    return null;
            }
            set
            {
                EnsureChildControls();
                txt_DefaultValue.Text = value.ToString();
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

            label_MaxMin = new Label();
            label_MaxMin.ID = "lblMaxMin";
            label_MaxMin.Text = "You can specify a minimum and maximum allowed value:";

            label_MaxValue = new Label();
            label_MaxValue.ID = "lblMaxValue";
            label_MaxValue.Text = "  Max:  ";

            txt_MaxValue = new TextBox();
            txt_MaxValue.ID = "txtMaxValue";
            txt_MaxValue.CssClass = "ms-input";
            txt_MaxValue.TextMode = TextBoxMode.Number;
            txt_MaxValue.Width = Unit.Pixel(80);

            label_MinValue = new Label();
            label_MinValue.ID = "lblMinValue";
            label_MinValue.Text = "Min:  ";

            txt_MinValue = new TextBox();
            txt_MinValue.ID = "txtMinValue";
            txt_MinValue.CssClass = "ms-input";
            txt_MinValue.TextMode = TextBoxMode.Number;
            txt_MinValue.Width = Unit.Pixel(80);

            label_DecimalPlaces = new Label();
            label_DecimalPlaces.ID = "lblDecimalPlaces";
            label_DecimalPlaces.Text = "Number of decimal places:";

            ddl_DecimalPlaces = new DropDownList();
            ddl_DecimalPlaces.ID = "ddlDecimalPlaces";
            ddl_DecimalPlaces.Items.AddRange(new[] { new ListItem("Automatic", "-1"), new ListItem("0", "0"), new ListItem("1", "1"), new ListItem("2", "2"), new ListItem("3", "3"), new ListItem("4", "4"), new ListItem("5", "5") });
            ddl_DecimalPlaces.SelectedValue = "-1";

            label_DefaultValue = new Label();
            label_DefaultValue.ID = "lblDefaultValue";
            label_DefaultValue.Text = "Default value:";

            txt_DefaultValue = new TextBox();
            txt_DefaultValue.ID = "txtDefaultValue";
            txt_DefaultValue.CssClass = "ms-input";
            txt_DefaultValue.MaxLength = 255;

            this.Controls.Add(label_Required);
            this.Controls.Add(radioList_Required);
            this.Controls.Add(label_MaxMin);
            this.Controls.Add(label_MaxValue);
            this.Controls.Add(txt_MaxValue);
            this.Controls.Add(label_MinValue);
            this.Controls.Add(txt_MinValue);
            this.Controls.Add(label_DecimalPlaces);
            this.Controls.Add(ddl_DecimalPlaces);
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
            label_MaxMin.RenderControl(writer); // <label/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            label_MinValue.RenderControl(writer); // <label/>
            txt_MinValue.RenderControl(writer); // <input/>
            label_MaxValue.RenderControl(writer); // <label/>
            txt_MaxValue.RenderControl(writer); // <input/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            label_DecimalPlaces.RenderControl(writer); // <label/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            ddl_DecimalPlaces.RenderControl(writer); // <select/>
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
            return new DbFieldNumber(fieldName)
            {
                Required = this.Required,
                MinValue = this.MinValue,
                MaxValue = this.MaxValue,
                DecimalPlaces = this.DecimalPlaces,
                DefaultValue = this.DefaultValue.HasValue ? this.DefaultValue.Value.ToString() : ""
            };
        }

        #endregion
    }
}
