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
    public class DbNoteControl : DbBaseFieldControl
    {
        private TextBox ctrl_Texbox;
        private RequiredFieldValidator ctrl_TexboxValidator;

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
                    ctrl_Texbox.CssClass = "ms-long";
                    ctrl_Texbox.TextMode = TextBoxMode.MultiLine;
                    ctrl_Texbox.Rows = ((DbFieldNote)Field).NumberOfLines;
                    ctrl_Texbox.Columns = 20;
                    this.Controls.Add(ctrl_Texbox);

                    if (Field.Required) {
                        ctrl_TexboxValidator = new RequiredFieldValidator();
                        ctrl_TexboxValidator.ID = Field.InternalName + "_Validator";
                        ctrl_TexboxValidator.ControlToValidate = ctrl_Texbox.ID;
                        ctrl_TexboxValidator.Text = "You can't leave this blank";
                        ctrl_TexboxValidator.Display = ValidatorDisplay.Dynamic;
                        ctrl_TexboxValidator.CssClass = "ms-formvalidation";
                        this.Controls.Add(ctrl_TexboxValidator);
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
                ctrl_Texbox.RenderControl(writer); // <textarea/>
                if (Field.Required)
                    ctrl_TexboxValidator.RenderControl(writer); // <validator/>
            }
        }

        public override object Value
        {
            get
            {
                EnsureChildControls();
                return ctrl_Texbox.Text;
            }
            set
            {
                EnsureChildControls();
                if (value != null)
                {
                    if (base.ControlMode == SPControlMode.Display)
                    {
                        ctrl_Label.Text = value.ToString();
                    }
                    else
                    {
                        ctrl_Texbox.Text = value.ToString();
                    }
                }
            }
        }
    }
}
