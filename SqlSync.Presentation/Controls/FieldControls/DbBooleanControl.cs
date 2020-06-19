using Microsoft.SharePoint.WebControls;
using System.Web.UI;
using System.Web.UI.WebControls;
using Roster.Common;

namespace Roster.Presentation.Controls.FieldControls
{
    public class DbBooleanControl : DbBaseFieldControl
    {
        private CheckBox ctrl_Checkbox;
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
                    ctrl_Checkbox = new CheckBox();
                    ctrl_Checkbox.ID = Field.InternalName;
                    if (Field.InternalName == FieldNames.ALL_DAY_EVENT) {
                        ctrl_Checkbox.Attributes.Add("onclick",
                            string.Format("Roster.Common.ShowHideTime(this.checked, ['{0}','{1}'])", FieldNames.START_DATE, FieldNames.END_DATE));
                    }

                    this.Controls.Add(ctrl_Checkbox);
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
                ctrl_Checkbox.RenderControl(writer); // <checkbox/>

                if (Field.InternalName == FieldNames.ALL_DAY_EVENT)
                {
                    writer.Write("<script type=\"text/javascript\">");
                    writer.Write("SP.SOD.executeOrDelayUntilEventNotified(function() {{ Roster.Common.ShowHideTime({0}, ['{1}','{2}']); }}, \"sp.bodyloaded\");",
                        ctrl_Checkbox.Checked.ToString().ToLower(), FieldNames.START_DATE, FieldNames.END_DATE);
                    writer.Write("</script>");
                }
            }
        }

        public override object Value
        {
            get
            {
                EnsureChildControls();
                if (base.ControlMode == SPControlMode.Display)
                {
                    return (ctrl_Label.Text == "Yes");
                }
                else
                {
                    return ctrl_Checkbox.Checked;
                }
            }
            set
            {
                EnsureChildControls();
                if (value != null)
                {
                    bool valAsBool;
                    if (value is bool)
                        valAsBool = (bool)value;
                    else
                        valAsBool = value.ToString() == "1";

                    if (base.ControlMode == SPControlMode.Display)
                    {
                        ctrl_Label.Text = valAsBool ? "Yes" : "No";
                    }
                    else
                    {
                        ctrl_Checkbox.Checked = valAsBool;
                    }
                }
            }
        }
    }
}
