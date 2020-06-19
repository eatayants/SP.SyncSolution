using Microsoft.SharePoint.WebControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using Roster.Common;
using Roster.Presentation.Controls.Fields;

namespace Roster.Presentation.Controls.FieldControls
{
    public class DbTextControl : DbBaseFieldControl
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
            if (Field == null) return;
            if (Field.ReadOnly || ControlMode == SPControlMode.Display)
            {
                ctrl_Label = new Label {ID = Field.InternalName};
                Controls.Add(ctrl_Label);
            }
            else
            {
                ctrl_Texbox = new TextBox
                {
                    ID = Field.InternalName,
                    CssClass = "ms-long ms-spellcheck-true",
                    MaxLength = ((DbFieldText) Field).MaxLength
                };
                Controls.Add(ctrl_Texbox);
                if (Field.Required) 
                {
                    ctrl_TexboxValidator = new RequiredFieldValidator
                    {
                        ID = Field.InternalName + "Validator",
                        ControlToValidate = ctrl_Texbox.ID,
                        Text = @"You can't leave this blank",
                        Display = ValidatorDisplay.Dynamic,
                        CssClass = "ms-formvalidation"
                    };
                    Controls.Add(ctrl_TexboxValidator);
                }
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            AddAttributesToRender(writer);

            if (Field.ReadOnly || ControlMode == SPControlMode.Display)
            {
                ctrl_Label.RenderControl(writer); // <label/>
            }
            else
            {
                ctrl_Texbox.RenderControl(writer); // <input/>
                if (Field.Required)
                {
                    ctrl_TexboxValidator.RenderControl(writer); // <validator/>
                }
            }
        }

        public override object Value
        {
            get
            {
                EnsureChildControls();
                return (Field.ReadOnly || ControlMode == SPControlMode.Display) ? ctrl_Label.Text : ctrl_Texbox.Text;
            }
            set
            {
                EnsureChildControls();
                if (Field.ReadOnly || ControlMode == SPControlMode.Display)
                {
                    ctrl_Label.Text = value.ToSafeString();
                }
                else
                {
                    ctrl_Texbox.Text = value.ToSafeString();
                }
            }
        }
    }
}
