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

namespace Roster.Presentation.Controls.FieldEditors
{
    public class DbFieldLookupEditor : CompositeControl, IDbFieldEditor
    {
        private readonly Guid _metadataId;

        public DbFieldLookupEditor(Guid metadataId)
        {
            _metadataId = metadataId;
        }

        #region Private variables

        private RadioButtonList radioList_Required;
        private Label label_Required;

        private DropDownList ddl_LookupSource;
        private Label label_LookupSource;

        private DropDownList ddl_LookupList;
        private Label label_LookupList;

        private HiddenField hdn_CustomQueryName;
        private TextBox txt_CustomQuery;
        private Label label_CustomQuery;
        private Button btn_ValidateQuery;
        private PlaceHolder pnl_ValidateResult;

        private DropDownList ddl_LookupKeyField;
        private Label label_LookupKeyField;

        private DropDownList ddl_LookupMainDispField;
        private Label label_LookupMainDispField;

        private CheckBoxList cbl_LookupField;
        private Label label_LookupField;

        private Label label_DependentColumn;
        private CheckBox chk_DependentColumn;

        private DropDownList ddl_DependentParent;
        private DropDownList ddl_DependentParentField;
        private Label label_DependentParentField;
        private Label label_DependentParent;

        private DropDownList ddl_FilterByField;
        private Label label_FilterByField;

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

        public int ListSource
        {
            get
            {
                EnsureChildControls();
                return ddl_LookupSource.SelectedValue.ToInt();
            }
            set
            {
                EnsureChildControls();
                ddl_LookupSource.SelectedValue = null;
                var li = ddl_LookupSource.Items.FindByValue(value.ToString());
                if (li != null)
                {
                    ddl_LookupSource.SelectedValue = null;
                    li.Selected = true;
                }
                SetVisibleBySelectedSource();
            }
        }

        public string ListId
        {
            get
            {
                EnsureChildControls();
                return ListSource == (int) LookupSourceType.Query
                    ? hdn_CustomQueryName.Value
                    : ddl_LookupList.SelectedValue;
            }
            set
            {
                EnsureChildControls();
                if (ListSource == (int) LookupSourceType.Query)
                {
                    hdn_CustomQueryName.Value = value;
                    txt_CustomQuery.Text = new RosterConfigService().ReadDataSource(hdn_CustomQueryName.Value);
                }
                else
                {
                    ddl_LookupList.Items.Clear();
                    ddl_LookupList.Items.AddRange(SetListItemsBySource());
                    var li = ddl_LookupList.Items.FindByValue(value);
                    if (li != null)
                    {
                        ddl_LookupList.SelectedValue = null;
                        li.Selected = true;
                    }
                }
            }
        }

        public string LookupKey
        {
            get
            {
                EnsureChildControls();
                return ddl_LookupKeyField.SelectedValue;
            }
            set
            {
                EnsureChildControls();
                ddl_LookupKeyField.Items.Clear();
                ddl_LookupKeyField.Items.AddRange(SetKeyFieldItemsBySource());
                var li = ddl_LookupKeyField.Items.FindByValue(value);
                if (li != null)
                {
                    ddl_LookupKeyField.SelectedValue = null;
                    li.Selected = true;
                }
            }
        }

        public string LookupField
        {
            get
            {
                EnsureChildControls();
                var fields = new List<string> {ddl_LookupMainDispField.SelectedValue};
                fields.AddRange(
                    cbl_LookupField.Items.Cast<ListItem>().Where(item => item.Selected).Select(item => item.Value));
                return string.Join("$", fields.Distinct());
            }
            set
            {
                EnsureChildControls();
                ddl_LookupMainDispField.Items.Clear();
                ddl_LookupMainDispField.Items.AddRange(SetFieldItemsBySource());
                cbl_LookupField.Items.Clear();
                cbl_LookupField.Items.AddRange(SetFieldItemsBySource());

                var fields = value.Split('$').ToArray();
                for (int i = 0; i < fields.Length; i++)
                {
                    if (i == 0)
                    {
                        // MAIN display field always goes FIRST
                        var li = ddl_LookupMainDispField.Items.FindByValue(fields[i]) ??
                                 ddl_LookupMainDispField.Items[0];
                        ddl_LookupMainDispField.SelectedValue = null;
                        li.Selected = true;
                    }
                    else
                    {
                        // additional field
                        var li = cbl_LookupField.Items.FindByValue(fields[i]);
                        if (li != null)
                        {
                            li.Selected = true;
                        }
                    }
                }
            }
        }

        public string DependentParent 
        {
            get 
            {
                EnsureChildControls();
                return chk_DependentColumn.Checked  ? ddl_DependentParent.SelectedValue : string.Empty;
            }
            set 
            {
                EnsureChildControls();
                ddl_DependentParent.Items.Clear();
                ddl_DependentParent.Items.AddRange(SetFieldItemsByDependence());
                var li = ddl_DependentParent.Items.FindByValue(value);
                if (li != null)
                {
                    ddl_DependentParent.SelectedValue = null;
                    li.Selected = true;
                    chk_DependentColumn.Checked = true;
                }
                else
                {
                    chk_DependentColumn.Checked = false;
                }
                SetVisibleByDependent();
            }
        }

        public string DependentParentField
        {
            get
            {
                EnsureChildControls();
                return chk_DependentColumn.Checked ? ddl_DependentParentField.SelectedValue : string.Empty;
            }
            set
            {
                EnsureChildControls();
                ddl_DependentParentField.Items.Clear();
                ddl_DependentParentField.Items.AddRange(SetFieldItemsByDependenceField());
                var li = ddl_DependentParentField.Items.FindByValue(value);
                if (li != null)
                {
                    ddl_DependentParentField.SelectedValue = null;
                    li.Selected = true;
                }
            }
        }

        public string FilterByField
        {
            get
            {
                EnsureChildControls();
                return chk_DependentColumn.Checked ? ddl_FilterByField.SelectedValue : string.Empty;
            }
            set
            {
                EnsureChildControls();
                ddl_FilterByField.Items.Clear();
                ddl_FilterByField.Items.AddRange(SetFieldItemsBySource());
                var li = ddl_FilterByField.Items.FindByValue(value);
                if (li != null)
                {
                    ddl_FilterByField.SelectedValue = null;
                    li.Selected = true;
                }
            }
        }

        #endregion

        protected override void RecreateChildControls()
        {
            EnsureChildControls();
        }

	    private ListItem[] SetListItemsBySource()
	    {
		    var result = new List<ListItem>();
		    if (ddl_LookupSource == null)
		    {
				return result.ToArray();
		    }            
            result.AddRange(ddl_LookupSource.SelectedValue == ((int) LookupSourceType.SpList).ToString()
            ? SPContext.Current.Web.Lists.Cast<SPList>().Where(l => !l.Hidden)
                .Select(x => new ListItem(x.Title, x.ID.ToString("B").ToUpper())).ToList()
            : new RosterConfigService().DatabaseTables().Select(x => new ListItem(x, x)).ToList());
	        return result.ToArray();
	    }

        private ListItem[] SetFieldItemsByDependence()
        {
            var result = new List<ListItem>();
            var list = new RosterConfigService().GetList(_metadataId);
            if (list != null)
            {
                result.AddRange(list.ListMetadataFields.Where(item => item.DataSourceType != 0).Select(x =>
                        new ListItem(x.FieldName,x.InternalName)).ToArray());
            }

            return result.ToArray();
        }

        private ListItem[] SetFieldItemsByDependenceField()
        {
            var result = new List<ListItem>();
            if (ddl_DependentParent == null)
            {
                return result.ToArray();
            }
            var listParent = new RosterConfigService().GetList(_metadataId);
            if (listParent == null)
            {
                return result.ToArray();
            }
            var item = listParent.ListMetadataFields.FirstOrDefault(i => i.InternalName == ddl_DependentParent.SelectedValue);
            if (item == null)
            {
                return result.ToArray();
            }
            if (item.DataSourceType == (int)LookupSourceType.SpList)
			{
				var list = SPContext.Current.Web.Lists[item.DataSource.ToGuid()];
				if (list != null)
				{
				    var idField = list.Fields.Cast<SPField>().FirstOrDefault(f => item.DataSourceKey.ToGuid() == f.Id);
				    if (idField != null)
				    {
                        result.Add(new ListItem(idField.Title, idField.Id.ToString("B").ToUpper()));
				    }
                    result.AddRange(list.Fields.Cast<SPField>().Where(f => item.DataSourceField.Split('$')
                        .Contains(f.Id.ToString("B").ToUpper())).Select(x =>new ListItem(x.Title, x.Id.ToString("B").ToUpper())));
				}
			}
			else
			{
                result.Add(new ListItem(item.DataSourceKey, item.DataSourceKey));
                result.AddRange(item.DataSourceField.Split('$').Select(x => new ListItem(x, x)));
			}
            return result.ToArray();
        }

		private ListItem[] SetFieldItemsBySource()
		{
			var result = new List<ListItem>();
			if (ddl_LookupSource == null)
			{
				return result.ToArray();
			}
			if (ddl_LookupSource.SelectedValue == ((int)LookupSourceType.SpList).ToString())
			{
				var selected = ddl_LookupList.SelectedValue.ToGuid();
				var list = SPContext.Current.Web.Lists[selected];
				if (list != null)
				{
                    result.AddRange(list.Fields.Cast<SPField>().Where(f => !f.Hidden 
                        && f.Type != SPFieldType.Note).Select(x =>new ListItem(x.Title, x.Id.ToString("B").ToUpper())));
				}
			}
			else
			{
                var dataSourceName = ddl_LookupSource.SelectedValue.ToInt() == ((int)LookupSourceType.Query)
                        ? hdn_CustomQueryName.Value : ddl_LookupList.SelectedValue;
                result.AddRange(new RosterConfigService().TablesFields(dataSourceName).
                    Select(x => new ListItem(x, x)).ToList());
			}
			return result.ToArray();
		}

        private ListItem[] SetKeyFieldItemsBySource()
		{
			var result = new List<ListItem>();
			if (ddl_LookupSource == null)
			{
				return result.ToArray();
			}
			if (ddl_LookupSource.SelectedValue == ((int)LookupSourceType.SpList).ToString())
			{
				var selected = ddl_LookupList.SelectedValue.ToGuid();
				var list = SPContext.Current.Web.Lists[selected];
				if (list != null)
				{
                    result.AddRange(list.Fields.Cast<SPField>().Where(f =>
                        !f.Hidden && f.Type == SPFieldType.Counter || f.Type == SPFieldType.Integer || f.Type == SPFieldType.Number).Select(x =>
                            new ListItem(x.Title, x.Id.ToString("B").ToUpper())));
				}
			}
			else
			{
			    var dataSourceName = ddl_LookupSource.SelectedValue.ToInt() == ((int) LookupSourceType.Query)
			        ? hdn_CustomQueryName.Value : ddl_LookupList.SelectedValue;
                result.AddRange(new RosterConfigService().TablesKeyFields(dataSourceName).
                    Select(x => new ListItem(x, x)).ToList());
			}
			return result.ToArray();
		}

        private void SetVisibleBySelectedSource()
        {
            txt_CustomQuery.Visible = label_CustomQuery.Visible = btn_ValidateQuery.Visible =
                ddl_LookupSource.SelectedValue == ((int)LookupSourceType.Query).ToString();
            ddl_LookupList.Visible = label_LookupList.Visible = 
                ddl_LookupSource.SelectedValue != ((int) LookupSourceType.Query).ToString();
        }

        private void SetVisibleByDependent()
        {
            label_DependentParent.Visible = ddl_DependentParent.Visible = ddl_DependentParentField.Visible 
            = label_DependentParentField.Visible = label_FilterByField.Visible = ddl_FilterByField.Visible = chk_DependentColumn.Checked;
        }

        protected void ddl_LookupSource_SelectedIndexChanged(object sender, EventArgs e)
		{
		    SetVisibleBySelectedSource();
			ddl_LookupList.Items.Clear();
			ddl_LookupList.Items.AddRange(SetListItemsBySource());
            ddl_LookupKeyField.Items.Clear();
            ddl_LookupKeyField.Items.AddRange(SetKeyFieldItemsBySource());
            ddl_LookupMainDispField.Items.Clear();
            ddl_LookupMainDispField.Items.AddRange(SetFieldItemsBySource());
			cbl_LookupField.Items.Clear();
            cbl_LookupField.Items.AddRange(SetFieldItemsBySource());
            ddl_FilterByField.Items.Clear();
            ddl_FilterByField.Items.AddRange(SetKeyFieldItemsBySource());
		}

        protected void btn_ValidateQuery_Click(object sender, EventArgs e)
        {
            RecreateChildControls();
            try
            {
                if (String.IsNullOrWhiteSpace(hdn_CustomQueryName.Value))
                {
                    hdn_CustomQueryName.Value = Guid.NewGuid().ToString().ToNormalize();                  
                }
                new RosterConfigService().SaveDataSource(hdn_CustomQueryName.Value, txt_CustomQuery.Text);
            }
            catch (Exception ex)
            {
                pnl_ValidateResult.Controls.Add(new Label
                {
                    Text = string.Format("Unable to validate query: {0}", ex.Message), ForeColor = Color.Red 
                });
            }
            ddl_LookupKeyField.Items.Clear();
            ddl_LookupKeyField.Items.AddRange(SetKeyFieldItemsBySource());
            ddl_LookupMainDispField.Items.Clear();
            ddl_LookupMainDispField.Items.AddRange(SetFieldItemsBySource());
            cbl_LookupField.Items.Clear();
            cbl_LookupField.Items.AddRange(SetFieldItemsBySource());
            ddl_FilterByField.Items.Clear();
            ddl_FilterByField.Items.AddRange(SetKeyFieldItemsBySource());
        }

		protected void ddl_LookupList_SelectedIndexChanged(object sender, EventArgs e)
		{
			RecreateChildControls();
            ddl_LookupKeyField.Items.Clear();
            ddl_LookupKeyField.Items.AddRange(SetKeyFieldItemsBySource());
            ddl_LookupMainDispField.Items.Clear();
            ddl_LookupMainDispField.Items.AddRange(SetFieldItemsBySource());
            cbl_LookupField.Items.Clear();
            cbl_LookupField.Items.AddRange(SetFieldItemsBySource());
            ddl_FilterByField.Items.Clear();
            ddl_FilterByField.Items.AddRange(SetKeyFieldItemsBySource());
        }

        protected void ddl_DependentParent_SelectedIndexChanged(object sender, EventArgs e)
        {
            RecreateChildControls();
            ddl_DependentParentField.Items.Clear();
            ddl_DependentParentField.Items.AddRange(SetFieldItemsByDependenceField());
        }

        protected override void CreateChildControls()
        {
            Controls.Clear();

            label_Required = new Label {ID = "lblRequired", Text = @"Require that this column contains information:"};

            radioList_Required = new RadioButtonList
            {
                ID = "rListRequired",
                RepeatDirection = RepeatDirection.Horizontal
            };
            radioList_Required.Items.AddRange(new[] {new ListItem("Yes", "1"), new ListItem("No", "0")});
            radioList_Required.SelectedValue = "0";

            label_LookupSource = new Label {ID = "lblLookupListLookupSource", Text = @"Lookup information source:"};

            var listSources = new[]
            {
                new ListItem { Text = LookupSourceType.SpList.GetStringValue(),Value = ((int) LookupSourceType.SpList).ToString()},
                new ListItem { Text = LookupSourceType.Table.GetStringValue(),Value = ((int) LookupSourceType.Table).ToString()},
                new ListItem { Text = LookupSourceType.Query.GetStringValue(),Value = ((int) LookupSourceType.Query).ToString() }
            };

            ddl_LookupSource = new DropDownList { ID = "ddlLookupSource", Width = 200};
            ddl_LookupSource.Items.AddRange(listSources);
            ddl_LookupSource.AutoPostBack = true;
            ddl_LookupSource.SelectedIndexChanged += ddl_LookupSource_SelectedIndexChanged;

            hdn_CustomQueryName = new HiddenField { ID = "hdnCustomQueryName" };
            label_CustomQuery = new Label {ID = "lblCustomQuery", Text = @"Write SQL query:"};
            txt_CustomQuery = new TextBox {ID = "txtCustomQuery", 
                TextMode = TextBoxMode.MultiLine, Rows = 10, Width = 300};
            btn_ValidateQuery = new Button {ID = "btnValidateQuery", Text = @"Validate Custom Query"};
            btn_ValidateQuery.Click += btn_ValidateQuery_Click;
            pnl_ValidateResult = new PlaceHolder { ID = "ValidateResult" };

            label_LookupList = new Label {ID = "lblLookupList", Text = @"Get information from:"};

            ddl_LookupList = new DropDownList { ID = "ddlLookupList", Width = 200 };
            ddl_LookupList.Items.AddRange(SetListItemsBySource());
            ddl_LookupList.AutoPostBack = true;
            ddl_LookupList.SelectedIndexChanged += ddl_LookupList_SelectedIndexChanged;

            // KEY field
            label_LookupKeyField = new Label {ID = "lblLookupKeyField", Text = @"Select key field:"};
            ddl_LookupKeyField = new DropDownList { ID = "ddlLookupKeyField", Width = 200 };
            ddl_LookupKeyField.Items.AddRange(SetKeyFieldItemsBySource());

            // Main Display field
            label_LookupMainDispField = new Label { ID = "lblLookupMainDispField", Text = @"Select display field:" };
            ddl_LookupMainDispField = new DropDownList { ID = "ddlLookupMainDispField", Width = 200 };
            ddl_LookupMainDispField.Items.AddRange(SetFieldItemsBySource());

            // Additional fields
            label_LookupField = new Label {ID = "lblLookupField", Text = @"Add additional display columns:"};
            cbl_LookupField = new CheckBoxList {ID = "ddlLookupField", Width = 300};
            cbl_LookupField.Items.AddRange(SetFieldItemsBySource());

            label_DependentColumn = new Label { ID = "lblIsDependentColumn", Text = @"Cascading column:" };
            label_DependentColumn.Font.Bold = true; 

            chk_DependentColumn = new CheckBox {ID = "chkIsDependentColumn", AutoPostBack = true};
            chk_DependentColumn.CheckedChanged +=chk_IsDependentColumn_SelectedIndexChanged;

            // Filter By field
            label_FilterByField = new Label { ID = "lblFilterByField", Text = @"This column is filtered by field: " };
            ddl_FilterByField = new DropDownList { ID = "ddlFilterByField", Width = 200 };
            ddl_FilterByField.Items.AddRange(SetFieldItemsBySource());

            // Get information from list
            label_DependentParent = new Label { ID = "lblDependentParent", Text = @"This column is dependent on column: " };
            ddl_DependentParent = new DropDownList { ID = "ddlDependentParent", Width = 200 };
            ddl_DependentParent.Items.AddRange(SetFieldItemsByDependence());
            ddl_DependentParent.AutoPostBack = true;
            ddl_DependentParent.SelectedIndexChanged += ddl_DependentParent_SelectedIndexChanged;

            label_DependentParentField = new Label { ID = "lblDependentParentField", Text = @" and value from field " };
            ddl_DependentParentField = new DropDownList { ID = "ddlDependentParentField", Width = 200 };
            ddl_DependentParentField.Items.AddRange(SetFieldItemsByDependenceField());

            // show/hide some blocks
            SetVisibleBySelectedSource();
            SetVisibleByDependent();

            Controls.Add(label_Required);
            Controls.Add(radioList_Required);
            Controls.Add(label_LookupSource);
            Controls.Add(ddl_LookupSource);

            Controls.Add(hdn_CustomQueryName);
            Controls.Add(txt_CustomQuery);
            Controls.Add(label_CustomQuery);
            Controls.Add(btn_ValidateQuery);
            Controls.Add(pnl_ValidateResult);

            Controls.Add(label_LookupList);
            Controls.Add(ddl_LookupList);
            Controls.Add(label_LookupKeyField);
            Controls.Add(ddl_LookupKeyField);
            Controls.Add(label_LookupMainDispField);
            Controls.Add(ddl_LookupMainDispField);
            Controls.Add(label_LookupField);
            Controls.Add(cbl_LookupField);

            Controls.Add(label_DependentColumn);
            Controls.Add(chk_DependentColumn);
            Controls.Add(label_DependentParent);
            Controls.Add(ddl_DependentParent);
            Controls.Add(label_DependentParentField);
            Controls.Add(ddl_DependentParentField);
            Controls.Add(label_FilterByField);
            Controls.Add(ddl_FilterByField);
        }

        private void chk_IsDependentColumn_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetVisibleByDependent();
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
			label_LookupSource.RenderControl(writer); // <label/>
			writer.RenderEndTag(); // </td>
			writer.RenderEndTag(); // </tr>

			writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
			writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
			ddl_LookupSource.RenderControl(writer); // <select/>
			writer.RenderEndTag(); // </td>
			writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            hdn_CustomQueryName.RenderControl(writer); // <input/>
            label_CustomQuery.RenderControl(writer); // <label/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            txt_CustomQuery.RenderControl(writer); // <textbox/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            btn_ValidateQuery.RenderControl(writer); // <label/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            pnl_ValidateResult.RenderControl(writer); // <label/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            label_LookupList.RenderControl(writer); // <label/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            ddl_LookupList.RenderControl(writer); // <select/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            label_LookupKeyField.RenderControl(writer); // <label/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            ddl_LookupKeyField.RenderControl(writer); // <select/>
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

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            label_LookupField.RenderControl(writer); // <label/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            cbl_LookupField.RenderControl(writer); // <LookupField/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            label_DependentColumn.RenderControl(writer); // <label/>
            chk_DependentColumn.RenderControl(writer); // <LookupField/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            label_FilterByField.RenderControl(writer); // <label/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            ddl_FilterByField.RenderControl(writer); // <LookupField/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            label_DependentParent.RenderControl(writer); // <label/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            ddl_DependentParent.RenderControl(writer); // <LookupField/>
            label_DependentParentField.RenderControl(writer); // <label/>
            ddl_DependentParentField.RenderControl(writer); // <LookupField/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>    

            writer.RenderEndTag(); // </table>
        }

        #region Interface methods

        public DbField GetField(string fieldName)
        {
            if (string.IsNullOrWhiteSpace(this.LookupKey))
            {
                throw new Exception("Key is required field.");
            }
            if (string.IsNullOrWhiteSpace(this.ListId))
            {
                throw new Exception("Key is required field.");
            }
            if (string.IsNullOrWhiteSpace(this.LookupField))
            {
                throw new Exception("Display Fields are required fields.");
            }
            return new DbFieldLookup(fieldName)
            {
                Required = this.Required,
				ListSource = this.ListSource,
				ListId = this.ListId,
                LookupKey = this.LookupKey,
                LookupField = this.LookupField,
                FilterByField = this.FilterByField,
                DependentParent = this.DependentParent,
                DependentParentField = this.DependentParentField,
            };
        }

        #endregion
    }
}
