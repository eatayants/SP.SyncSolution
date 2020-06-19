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

namespace Roster.Presentation.Controls.FieldControls
{
    public class DbLookupControl : DbBaseFieldControl
    {
        private const string RestService = "DataService.svc/Lookup";
        private SPDropDownList m_dropList;
        private RequiredFieldValidator ctrl_TexboxValidator;
        private HiddenField fld_Value;
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
                m_dropList = new SPDropDownList
                {
                    ID = Field.InternalName,
                    DataValueField = "ValueField",
                    DataTextField = "TextField",
                    EnableViewState = false, 
                    ViewStateMode = ViewStateMode.Disabled,
                    ToolTip = SPHttpUtility.NoEncode(Field.DisplayName),
                };
                var field = (DbFieldLookup)Field;
                if (field != null)
                {
                    m_dropList.Attributes.Add("data-name", field.InternalName);
                    m_dropList.Attributes.Add("data-dependent", field.DependentParent);
                    m_dropList.Attributes.Add("data-metadataId", field.Id.ToString());
                }
                Controls.Add(m_dropList);
                if (Field.Required)
                {
                    ctrl_TexboxValidator = new RequiredFieldValidator
                    {
                        ID = Field.InternalName + "Validator",
                        ControlToValidate = this.m_dropList.ID,
                        Text = @"You can't leave this blank",
                        Display = ValidatorDisplay.Dynamic,
                        CssClass = "ms-formvalidation"
                    };
                    Controls.Add(ctrl_TexboxValidator);
                }
            }
            fld_Value = new HiddenField
            {
                ID = Field.InternalName + "_ddlValue",
                ClientIDMode = ClientIDMode.Static
            };
            Controls.Add(fld_Value);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            AddAttributesToRender(writer);
            if (Field == null) return;
            if (Field.ReadOnly || (ControlMode == SPControlMode.Display))
            {
                ctrl_Label.RenderControl(writer); // <label/>
            }
            else
            {
                m_dropList.RenderControl(writer); // <select/>
                if (Field != null && Field.Required)
                {
                    ctrl_TexboxValidator.RenderControl(writer); // <validator/>
                }
                var clientName = m_dropList.ClientID.Replace("_" + m_dropList.ID, "") + "_" + m_dropList.ID;
                writer.Write("<script type=\"text/javascript\">");
                writer.Write(GetJsScript(clientName, fld_Value.ID, RestService));
                writer.Write("</script>");
            }
            fld_Value.RenderControl(writer);
        }

        internal string GetJsScript(string controlName, string valueName, string serviceName)
		{
            var field = (DbFieldLookup)Field;
            if (field == null)
            {
                return string.Empty;
            }
            var subcontolsList = new List<string>();
            Subcontrols.ForEach(item =>
            {
                subcontolsList.Add(string.Format("{{fieldName:'{0}',controlId:'#{1}'}}",
                    item.Field.InternalNameOriginal, item.ClientID.Replace("_" + item.ID, "") + "_" + item.ID));            
            });
            var subcontrols = string.Format("[{0}]",String.Join(",", subcontolsList));
            var result = string.Format(@"Roster.Common.LookupInit('#{0}','#{1}','{2}','{3}','{4}','{5}','{6}','{7}',{8})",
                controlName, valueName, serviceName, field.ListId, field.LookupKey,
                field.LookupField, field.ListSource, field.DependentParent, subcontrols);
			return result;
		}

        public override object Value
        {
            get
            {
                EnsureChildControls();
                return fld_Value.Value;
            }
            set
            {
                EnsureChildControls();
                fld_Value.Value = value.ToSafeString();
                if ((Field != null && Field.ReadOnly)||(ControlMode == SPControlMode.Display))
                {
                    var selectedItem = SelectedItem(value);
                    ctrl_Label.Text = selectedItem != null ? selectedItem.Item2.FirstValue() : value.ToSafeString();
                }
                else
                {
                    if (m_dropList != null)
                    {
                        m_dropList.Items.Clear();
                        m_dropList.Items.AddRange(SetListItem(value));
                        m_dropList.SelectedValue = value.ToSafeString();  
                    }
                }
            }
        }

        internal Tuple<int, ExpandoObject> SelectedItem(object value)
        {
            var id = value.ToNullableInt();
            if (!id.HasValue)
            {
                return null;    
            }
            var itemCollection = GetItemCollection((Field as DbFieldLookup));
            return itemCollection.IsEmpty() ? null : 
                itemCollection.FirstOrDefault(item => item.Item1 == id.Value);
        }

        internal List<Tuple<int, ExpandoObject>> GetItemCollection(DbFieldLookup field)
        {
            return field == null ? null : BLExtensions.SourceContent(field.ListId, field.LookupKey, field.LookupField, field.ListSource);
        }

        internal ListItem[] SetListItem(object value)
        {
            var result = new List<ListItem>();
            if (value == null)
            {
                return result.ToArray();
            }
            var item = SelectedItem(value);
            if (item == null) return result.ToArray();
            var listLitem = new ListItem(item.Item2.FirstValue(), item.Item1.ToString());
            result.Add(listLitem);
            return result.ToArray();
        }
    }
}
