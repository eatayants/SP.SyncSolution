using Microsoft.SharePoint.WebControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using Roster.Presentation.Controls.Fields;

namespace Roster.Presentation.Controls.FieldControls
{
    public class DbNumberControl : DbBaseFieldControl
    {
        private TextBox ctrl_Texbox;
        private RequiredFieldValidator ctrl_TexboxValidator;
        private RangeValidator ctrl_TextboxRangeValidator;

        private Label ctrl_Label;

        protected override void RecreateChildControls()
        {
            EnsureChildControls();
        }

        protected override void CreateChildControls()
        {
            Controls.Clear();

            if (Field != null)
            {
                if (base.ControlMode == SPControlMode.Display)
                {
                    ctrl_Label = new Label();
                    ctrl_Label.ID = Field.InternalName;
                    this.Controls.Add(ctrl_Label);
                }
                else
                {
                    ctrl_Texbox = new TextBox();
                    ctrl_Texbox.ID = Field.InternalName;
                    ctrl_Texbox.CssClass = "ms-input";
                    ctrl_Texbox.TextMode = TextBoxMode.Number;
                    ctrl_Texbox.Width = Unit.Pixel(110);
                    this.Controls.Add(ctrl_Texbox);

                    if (Field.Required) {
                        ctrl_TexboxValidator = new RequiredFieldValidator();
                        ctrl_TexboxValidator.ID = Field.InternalName + "Validator";
                        ctrl_TexboxValidator.ControlToValidate = ctrl_Texbox.ID;
                        ctrl_TexboxValidator.Text = "You can't leave this blank";
                        ctrl_TexboxValidator.Display = ValidatorDisplay.Dynamic;
                        ctrl_TexboxValidator.CssClass = "ms-formvalidation";
                        this.Controls.Add(ctrl_TexboxValidator);
                    }

                    DbFieldNumber _fld = Field as DbFieldNumber;
                    if (_fld.MinValue != 0 || _fld.MaxValue != 0) {
                        ctrl_TextboxRangeValidator = new RangeValidator();
                        ctrl_TextboxRangeValidator.ID = Field.InternalName + "RangeValidator";

                        string errMsg = "Incorrect number";
                        if (_fld.MinValue != 0 && _fld.MaxValue != 0) {
                            ctrl_TextboxRangeValidator.MaximumValue = _fld.MaxValue.ToString();
                            ctrl_TextboxRangeValidator.MinimumValue = _fld.MinValue.ToString();
                            errMsg = string.Format("The value must be from {0} to {1}", _fld.MinValue, _fld.MaxValue);
                        } else if (_fld.MinValue != 0) {
                            ctrl_TextboxRangeValidator.MinimumValue = _fld.MinValue.ToString();
                            ctrl_TextboxRangeValidator.MaximumValue = (Int32.MaxValue-1).ToString(); // auto set max value
                            errMsg = string.Format("The value must be greater than {0}", _fld.MinValue);
                        } else if (_fld.MaxValue != 0) {
                            ctrl_TextboxRangeValidator.MinimumValue = "0";
                            ctrl_TextboxRangeValidator.MaximumValue = _fld.MaxValue.ToString();
                            errMsg = string.Format("The value must be less than {0}", _fld.MaxValue);
                        }

                        ctrl_TextboxRangeValidator.Text = errMsg;
                        ctrl_TextboxRangeValidator.ControlToValidate = ctrl_Texbox.ID;
                        ctrl_TextboxRangeValidator.Display = ValidatorDisplay.Dynamic;
                        ctrl_TextboxRangeValidator.CssClass = "ms-formvalidation";
                        this.Controls.Add(ctrl_TextboxRangeValidator);
                    }
                }
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            AddAttributesToRender(writer);

            if (base.ControlMode == SPControlMode.Display)
            {
                ctrl_Label.RenderControl(writer); // <label/>
            }
            else
            {
                ctrl_Texbox.RenderControl(writer); // <input/>
                if (Field.Required)
                    ctrl_TexboxValidator.RenderControl(writer); // <validator/>
                if (ctrl_TextboxRangeValidator != null)
                    ctrl_TextboxRangeValidator.RenderControl(writer);
            }
        }

        public override object Value
        {
            get
            {
                EnsureChildControls();
                double _val = 0;
                Double.TryParse(ctrl_Texbox.Text, out _val);
                return _val;
            }
            set
            {
                EnsureChildControls();
                if (value != null)
                {
                    string _val = string.Empty;
                    DbFieldNumber _fld = Field as DbFieldNumber;
                    if (_fld.DecimalPlaces == -1) {
                        _val = value.ToString();
                    } else {
                        double d = 0;
                        if (Double.TryParse(value.ToString(), out d)) {
                            _val = string.Format("{0:N" + Math.Abs(_fld.DecimalPlaces) + "}", d);
                        }
                    }

                    if (base.ControlMode == SPControlMode.Display) {
                        ctrl_Label.Text = _val;
                    } else {
                        ctrl_Texbox.Text = _val;
                    }
                }
            }
        }
    }
}
