using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Roster.BL;
using Roster.Common;
using Roster.Presentation.Controls.Fields;
using System.Drawing;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;

namespace Roster.Presentation.Controls.FieldEditors
{
    public class DbFieldUserEditor : CompositeControl, IDbFieldEditor
    {
        private readonly Guid _metadataId;

        public DbFieldUserEditor(Guid metadataId)
        {
            _metadataId = metadataId;
        }

        #region Private variables

        private RadioButtonList radioList_Required;
        private Label label_Required;

        private RadioButtonList radioList_AllowSelection;
        private Label label_AllowSelection;

        private RadioButtonList radioList_ChooseFrom;
        private Label label_ChooseFrom;

        private DropDownList ddl_Groups;

        private DropDownList ddl_LookupMainDispField;
        private Label label_LookupMainDispField;

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

        public string AllowSelection
        {
            get
            {
                EnsureChildControls();
                return (radioList_AllowSelection.SelectedValue);
            }
            set
            {
                EnsureChildControls();
                foreach (ListItem li in radioList_AllowSelection.Items)
                    li.Selected = (li.Value.Equals(value));
            }
        }

        public string ChooseFrom
        {
            get
            {
                EnsureChildControls();
                return (radioList_ChooseFrom.SelectedValue);
            }
            set
            {
                EnsureChildControls();
                foreach (ListItem li in radioList_ChooseFrom.Items)
                    li.Selected = (li.Value.Equals(value));
            }
        }

        public int ListSource
        {
            get
            {
                return (int)LookupSourceType.Table;
            }
        }

        public string ListId
        {
            get
            {
                return TableNames.UserInformationTable;
            }
        }

        public string LookupKey
        {
            get
            {
                return "Id";
            }
        }

        public string LookupField
        {
            get
            {
                EnsureChildControls();
                var fields = new List<string> {ddl_LookupMainDispField.SelectedValue};
                return string.Join("$", fields.Distinct());
            }
            set
            {
                EnsureChildControls();

                var fields = value.Split('$').ToArray();
                for (int i = 0; i < fields.Length; i++) {
                    var li = ddl_LookupMainDispField.Items.FindByValue(fields[i]) ??
                                 ddl_LookupMainDispField.Items[0];
                    ddl_LookupMainDispField.SelectedValue = null;
                    li.Selected = true;
                }
            }
        }

        public string ChooseFromGroup
        {
            get
            {
                EnsureChildControls();
                return radioList_ChooseFrom.SelectedValue == "RadChooseFromGroup" ? ddl_Groups.SelectedValue : string.Empty;
            }
            set
            {
                EnsureChildControls();

                if (!string.IsNullOrEmpty(value))
                {
                    ddl_Groups.Enabled = true;
                    var li = ddl_Groups.Items.FindByValue(value);
                    if (li != null) {
                        ddl_Groups.SelectedValue = null;
                        li.Selected = true;
                    }
                }
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

            // REQUIRED
            label_Required = new Label {ID = "lblRequired", Text = @"Require that this column contains information:"};

            radioList_Required = new RadioButtonList {
                ID = "rListRequired",
                RepeatDirection = RepeatDirection.Horizontal
            };
            radioList_Required.Items.AddRange(new[] {new ListItem("Yes", "1"), new ListItem("No", "0")});
            radioList_Required.SelectedValue = "0";

            // ALLOW SELECTION
            label_AllowSelection = new Label { ID = "lblAllowSelection", Text = @"Allow selection of:" };

            radioList_AllowSelection = new RadioButtonList {
                ID = "rListAllowSelection",
                RepeatDirection = RepeatDirection.Horizontal
            };
            radioList_AllowSelection.Items.AddRange(new[] { new ListItem("People Only", "RadSelectionPeopleOnly"), new ListItem("People and Groups", "RadSelectionPeopleAndGroups") });
            radioList_AllowSelection.SelectedValue = "RadSelectionPeopleOnly";

            // CHOOSE FROM
            label_ChooseFrom = new Label { ID = "lblChooseFrom", Text = @"Choose from:" };

            radioList_ChooseFrom = new RadioButtonList {
                ID = "rListChooseFrom",
                RepeatDirection = RepeatDirection.Vertical
            };
            radioList_ChooseFrom.Items.AddRange(new[] { new ListItem("All Users", "RadChooseFromAllPeopleGroups"), new ListItem("SharePoint Group", "RadChooseFromGroup") });
            foreach (ListItem li in radioList_ChooseFrom.Items) {
                li.Attributes.Add(HtmlTextWriterAttribute.Onchange.ToString(), "ChooseFromChanged();");
            }
            radioList_ChooseFrom.SelectedValue = "RadChooseFromAllPeopleGroups";

            // GROUPS
            ddl_Groups = new DropDownList { ID = "ddlLookupGroups", Width = 283, Enabled = false };
            ddl_Groups.Attributes.CssStyle.Add(HtmlTextWriterStyle.MarginLeft, "22px");
            ddl_Groups.Items.AddRange(SPContext.Current.Web.SiteGroups.OfType<SPGroup>().Select(g => new ListItem(g.Name, g.ID.ToString())).ToArray());

            // Main Display field
            label_LookupMainDispField = new Label { ID = "lblLookupMainDispField", Text = @"Show field:" };
            ddl_LookupMainDispField = new DropDownList { ID = "ddlLookupMainDispField", Width = 200 };
            var tblColumns = new RosterConfigService().TablesFields(this.ListId);
            foreach(string colName in tblColumns) {
                var li = new ListItem(colName);
                li.Selected = colName.Equals("DisplayName");
                ddl_LookupMainDispField.Items.Add(li);
            }

            Controls.Add(label_Required);
            Controls.Add(radioList_Required);
            Controls.Add(label_AllowSelection);
            Controls.Add(radioList_AllowSelection);
            Controls.Add(label_ChooseFrom);
            Controls.Add(radioList_ChooseFrom);
            Controls.Add(ddl_Groups);

            Controls.Add(label_LookupMainDispField);
            Controls.Add(ddl_LookupMainDispField);
        }

        protected override void OnPreRender(EventArgs e)
        {
            string func_ChooseFromChanged = string.Format(@"
                function ChooseFromChanged() {{
                    var chooseFrom = (document.querySelector('input[name=""{0}""]:checked'));
                    var ddlGroup = (document.getElementById('{1}'));
                    ddlGroup.disabled = (chooseFrom.value === 'RadChooseFromAllPeopleGroups');
                }}
            ", radioList_ChooseFrom.UniqueID, ddl_Groups.ClientID);
            SPPageContentManager.RegisterClientScriptBlock(this.Page, base.GetType(), "ChooseFromChanged", func_ChooseFromChanged);
            //SPPageContentManager.RegisterArrayDeclaration(this.Page, strArray[i], builder.ToString());

            base.OnPreRender(e);
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
            label_AllowSelection.RenderControl(writer); // <label/>
			writer.RenderEndTag(); // </td>
			writer.RenderEndTag(); // </tr>

			writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
			writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            radioList_AllowSelection.RenderControl(writer); // <radioList/>
			writer.RenderEndTag(); // </td>
			writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            label_ChooseFrom.RenderControl(writer); // <label/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            radioList_ChooseFrom.RenderControl(writer); // <radioList/>
            ddl_Groups.RenderControl(writer); // <select/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            label_LookupMainDispField.RenderControl(writer); // <label/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            ddl_LookupMainDispField.RenderControl(writer); // <select/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderEndTag(); // </table>
        }

        #region Interface methods

        public DbField GetField(string fieldName)
        {
            if (string.IsNullOrWhiteSpace(this.LookupField)) {
                throw new Exception("Show field is required field.");
            }

            return new DbFieldUser(fieldName) {
                Required = this.Required,
				ListSource = this.ListSource,
				ListId = this.ListId,
                LookupKey = this.LookupKey,
                LookupField = this.LookupField,
                AllowSelection = this.AllowSelection,
                ChooseFrom = this.ChooseFrom,
                ChooseFromGroup = this.ChooseFromGroup
            };
        }

        #endregion
    }
}
