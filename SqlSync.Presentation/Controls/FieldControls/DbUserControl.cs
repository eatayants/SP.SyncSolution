using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using Roster.BL;
using Roster.Common;
using Roster.Presentation.Controls.Fields;
using Roster.Presentation.Extensions;
using System.Web.UI.HtmlControls;
using System.Web.Script.Serialization;

namespace Roster.Presentation.Controls.FieldControls
{
    public class DbUserControl : DbBaseFieldControl
    {
        private HiddenField fld_Value;
        private HtmlGenericControl div;
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
                ctrl_Label = new Label { ID = Field.InternalName };
                Controls.Add(ctrl_Label);
            }
            else
            {
                ClientScriptManager cs = this.Page.ClientScript;
                Type csType = this.GetType();
                //if (!cs.IsClientScriptBlockRegistered(csType, "clienttemplates.js")) {
                //    cs.RegisterClientScriptBlock(csType, "clienttemplates.js", "<script type=\"text/javascript\" src=\"/_layouts/15/clienttemplates.js\"></script>", false);
                //}
                if (!cs.IsClientScriptBlockRegistered(csType, "clientforms.js")) {
                    cs.RegisterClientScriptBlock(csType, "clientforms.js", "<script type=\"text/javascript\" src=\"/_layouts/15/clientforms.js\"></script>", false);
                }
                if (!cs.IsClientScriptBlockRegistered(csType, "clientpeoplepicker.js")) {
                    cs.RegisterClientScriptBlock(csType, "clientpeoplepicker.js", "<script type=\"text/javascript\" src=\"/_layouts/15/clientpeoplepicker.js\"></script>", false);
                }
                if (!cs.IsClientScriptBlockRegistered(csType, "autofill.js")) {
                    cs.RegisterClientScriptBlock(csType, "autofill.js", "<script type=\"text/javascript\" src=\"/_layouts/15/autofill.js\"></script>", false);
                }
                //if (!cs.IsClientScriptBlockRegistered(csType, "sp.js")) {
                //    cs.RegisterClientScriptBlock(csType, "sp.js", "<script type=\"text/javascript\" src=\"/_layouts/15/sp.js\"></script>", false);
                //}
                //if (!cs.IsClientScriptBlockRegistered(csType, "sp.runtime.js")) {
                //    cs.RegisterClientScriptBlock(csType, "sp.runtime.js", "<script type=\"text/javascript\" src=\"/_layouts/15/sp.runtime.js\"></script>", false);
                //}
                //if (!cs.IsClientScriptBlockRegistered(csType, "sp.core.js")) {
                //    cs.RegisterClientScriptBlock(csType, "sp.core.js", "<script type=\"text/javascript\" src=\"/_layouts/15/sp.core.js\"></script>", false);
                //}

                div = new HtmlGenericControl("div");
                div.ID = Field.InternalName + "_userField";
                Controls.Add(div);
            }

            fld_Value = new HiddenField {
                ID = Field.InternalName + "_userFieldValue",
                ClientIDMode = ClientIDMode.Static
            };
            Controls.Add(fld_Value);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            AddAttributesToRender(writer);
            if (Field == null) return;

            fld_Value.RenderControl(writer);
            if (Field.ReadOnly || (ControlMode == SPControlMode.Display))
            {
                ctrl_Label.RenderControl(writer); // <label/>
            }
            else
            {
                div.RenderControl(writer);
                writer.Write("<script type=\"text/javascript\">");
                writer.Write(GetJsScript(div.ClientID, fld_Value.UniqueID));
                writer.Write("</script>");
            }
        }

        internal string GetJsScript(string controlName, string valueName)
		{
            var field = (DbFieldUser)Field;
            if (field == null) {
                return string.Empty;
            }

            return string.Format(@"Roster.Common.InitStandalonePicker('{0}', '{1}', '{2}', '{3}', {4});",
                controlName, valueName, field.AllowSelection, field.ChooseFromGroup, (field.Required ? "true" : "false")
            );
		}

        public override object Value
        {
            get
            {
                EnsureChildControls();
                
                JavaScriptSerializer jsSer = new JavaScriptSerializer();
                if (string.IsNullOrEmpty(fld_Value.Value)) {
                    return null;
                }
                UserPickerEntity firstValue = jsSer.Deserialize<List<UserPickerEntity>>(fld_Value.Value).FirstOrDefault();
                if (firstValue == null)
                    return null;
                else
                    return (Field as DbFieldUser).EnsureUser(firstValue).RosterLookupId;
            }
            set
            {
                EnsureChildControls();

                if (Field != null && !string.IsNullOrEmpty(value.ToSafeString()))
                {
                    var selectedItem = (value.ToSafeString() == "[me]") ?
                        (Field as DbFieldUser).EnsureUser(SPContext.Current.Web.CurrentUser) :
                        (Field as DbFieldUser).EnsureUser(value.ToInt());
                    if (Field.ReadOnly || ControlMode == SPControlMode.Display) {
                        if (selectedItem == null) {
                            ctrl_Label.Text = value.ToSafeString();
                        } else {
                            ctrl_Label.Text = selectedItem.GetTitleByLookupField((Field as DbFieldUser).LookupField);
                        }
                        //http://sp13dev-10/sites/evraz/_layouts/15/listform.aspx?PageType=4&ListId={920a444d-4773-49ec-b894-65983d6a8e8d}&ID=2
                    } else {
                        fld_Value.Value = new JavaScriptSerializer().Serialize(new[] { selectedItem });
                    }
                }
            }
        }
    }
}
