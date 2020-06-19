using Microsoft.SharePoint.WebControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using Roster.Common;
using Roster.Presentation.Controls.Fields;

namespace Roster.Presentation.Controls.FieldControls
{
    public class DbChoiceControl : DbBaseFieldControl
    {
        private DropDownList ctrl_DropDown;
        private RadioButtonList ctrl_RadioList;
        private CheckBoxList ctrl_CheckList;

        private RequiredFieldValidator ctrl_ChoiceValidator;
        private CustomValidator ctrl_ChoiceValidatorForListBox;

        private Label ctrl_Label;

        private ListControl ControlByType
        {
            get
            {
                if (ctrl_DropDown == null && ctrl_RadioList == null && ctrl_CheckList == null)
                {
                    string ctrlType = ((DbFieldChoice)Field).ControlType;
                    switch (ctrlType)
                    {
                        case "Dropdown":
                            ctrl_DropDown = new DropDownList();
                            return ctrl_DropDown;
                        case "RadioButtons":
                            ctrl_RadioList = new RadioButtonList();
                            return ctrl_RadioList;
                        case "Checkboxes":
                            ctrl_CheckList = new CheckBoxList();
                            return ctrl_CheckList;
                        default:
                            ctrl_DropDown = new DropDownList(); // default control
                            return ctrl_DropDown;
                    }
                }
                else if (ctrl_DropDown != null)
                {
                    return ctrl_DropDown;
                }
                else if (ctrl_RadioList != null)
                {
                    return ctrl_RadioList;
                }
                else if (ctrl_CheckList != null)
                {
                    return ctrl_CheckList;
                }
                else
                {
                    return null;
                }
            }
        }

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
                    ListControl listCtrl = this.ControlByType;
                    if (listCtrl != null)
                    {
                        listCtrl.ID = Field.InternalName;
                        listCtrl.Items.AddRange(((DbFieldChoice)Field).Choices.Select(x => new ListItem(x)).ToArray());
                        this.Controls.Add(listCtrl);

                        if (Field.Required) {
                            if (ctrl_CheckList == null)
                            {
                                ctrl_ChoiceValidator = new RequiredFieldValidator();
                                ctrl_ChoiceValidator.ID = Field.InternalName + "Validator";
                                ctrl_ChoiceValidator.ControlToValidate = listCtrl.ID;
                                ctrl_ChoiceValidator.Text = "You can't leave this blank";
                                ctrl_ChoiceValidator.Display = ValidatorDisplay.Dynamic;
                                ctrl_ChoiceValidator.CssClass = "ms-formvalidation";
                                this.Controls.Add(ctrl_ChoiceValidator);
                            }
                            else
                            {
                                ctrl_ChoiceValidatorForListBox = new CustomValidator();
                                ctrl_ChoiceValidatorForListBox.ID = Field.InternalName + "Validator";
                                ctrl_ChoiceValidatorForListBox.Text = "You can't leave this blank";
                                ctrl_ChoiceValidatorForListBox.Display = ValidatorDisplay.Dynamic;
                                ctrl_ChoiceValidatorForListBox.CssClass = "ms-formvalidation";
                                ctrl_ChoiceValidatorForListBox.ClientValidationFunction = string.Format("DoValidationFor_{0}", Field.InternalName);
                                this.Controls.Add(ctrl_ChoiceValidatorForListBox);
                            }
                        }
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
                ListControl listCtrl = this.ControlByType;
                if (listCtrl != null) {
                    listCtrl.RenderControl(writer); // <listInput/>
                    if (Field.Required)
                    {
                        if (ctrl_CheckList == null) {
                            ctrl_ChoiceValidator.RenderControl(writer); // <validator/>
                        } else {
                            ctrl_ChoiceValidatorForListBox.RenderControl(writer); // <validator/>
                        }
                    }
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (ctrl_CheckList != null)
            {
                // CUSTOM VALIDATOR for CheckBoxList

                Type cstype = this.GetType();
                ClientScriptManager csm = Page.ClientScript;
                string csname = "CheckBoxListValidatorFor" + Field.InternalName;
                if (!csm.IsClientScriptBlockRegistered(csname))
                {
                    //StringBuilder cstext = new StringBuilder();
                    //cstext.AppendFormat("<script type=\"text/javascript\"> function DoValidationFor_{0}(sender, args) {{", Field.InternalName);
                    //cstext.AppendFormat("var checkBoxList = document.getElementById('{0}');", ctrl_CheckList.ClientID.Substring(0, ctrl_CheckList.ClientID.LastIndexOf("_" + Field.InternalName)));
                    //cstext.Append("var checkboxes = checkBoxList.getElementsByTagName('input');");
                    //cstext.Append("var isValid = false;");
                    //cstext.Append("for (var i = 0; i < checkboxes.length; i++) {");
                    //cstext.Append(" if (checkboxes[i].checked) {");
                    //cstext.Append("     isValid = true;");
                    //cstext.Append("     break;");
                    //cstext.Append(" }");
                    //cstext.Append("}");
                    //cstext.Append("args.IsValid = isValid;} </");
                    //cstext.Append("script>");

                    //csm.RegisterClientScriptBlock(cstype, csname, cstext.ToString(), false);
                }
            }
        }

        public override object Value
        {
            get
            {
                EnsureChildControls();
                var listCtrl = ControlByType;
                var cbl = listCtrl as CheckBoxList;
                return cbl != null ? cbl.Items.Cast<ListItem>().Where(x => x.Selected).
                    Select(item => item.Text).ToList().ListToXml() : 
                    new List<string>{listCtrl.SelectedValue}.ListToXml();
            }
            set
            {
                EnsureChildControls();
                if (value != null)
                {
                    var listCtrl = ControlByType;
                    var cbl = listCtrl as CheckBoxList;
                    var values = value.IsXml() ? value.XmlToList() : 
                        new List<string>{value.ToSafeString()}.ListToXml().XmlToList();
                    if (ControlMode == SPControlMode.Display)
                    {
                        ctrl_Label.Text = values.IsEmpty()?string.Empty:String.Join(", ", values);
                    }
                    else
                    {
                        if (cbl != null)
                        {
                            if (values.IsEmpty()) return;
                            var selection = cbl.Items.Cast<ListItem>().Where(x => values.Contains(x.Value));
                            foreach (var li in selection)
                            {
                                li.Selected = true;
                            }
                        }
                        else
                        {
                            listCtrl.SelectedValue = values.IsEmpty() ? string.Empty : values.FirstOrDefault();
                        }
                    }
                }
            }
        }
    }
}
