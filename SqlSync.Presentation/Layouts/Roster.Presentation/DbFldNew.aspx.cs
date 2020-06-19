using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Roster.Presentation.Controls.FieldEditors;
using Roster.Presentation.Controls.Fields;
using System.Web.UI.WebControls;
using Roster.Presentation.Helpers;
using Roster.Presentation.Controls;
using Microsoft.SharePoint.Utilities;
using Roster.Model.DataContext;
using Roster.BL;
using Roster.Common;
using Roster.Presentation.Extensions;

namespace Roster.Presentation.Layouts
{
    public partial class DbFldNew : LayoutsPageBase
    {
        #region Private

        private const string EDITOR_CONTROL_ID = "EditorControlId";

        private RosterConfigService configProvider = new RosterConfigService();
        private Guid m_listId = Guid.Empty;
        private List<Guid> _referenceListsGuids = new List<Guid>(); 
        private ListMetadata m_list = null;
        private int m_contentTypeId;
        private ListMetadataContentType m_contentType = null;

        #endregion

        #region Public properties

        public Guid ListGuid
        {
            get
            {
                if (this.m_listId == Guid.Empty)
                {
                    Guid _lid;
                    string lstId = Request.QueryString["List"];
                    if (!string.IsNullOrEmpty(lstId) && Guid.TryParse(lstId, out _lid))
                        this.m_listId = _lid;
                }

                return this.m_listId;
            }
        }

        public List<Guid> ReferenceListsGuids
        {
            get
            {
                _referenceListsGuids.Clear();
                if (ListGuid != Guid.Empty)
                {
                    var refItem = TableIDs.ReferenceItems.FirstOrDefault(item => item.Item1 == ListGuid);
                    if (refItem != null)
                    {
                        _referenceListsGuids.AddRange(
                            TableIDs.ReferenceItems.Where(item => item.Item2 == refItem.Item2 
                                && item.Item1 != refItem.Item1).Select(item=>item.Item1));
                    }
                }
                return _referenceListsGuids;
            }
        }

        public ListMetadata List
        {
            get
            {
                if (m_list == null)
                {
                    this.m_list = this.configProvider.GetList(this.ListGuid);
                }

                return this.m_list;
            }
        }

        public int ContentTypeId
        {
            get
            {
                if (this.m_contentTypeId == 0) {
                    Int32.TryParse(Request.QueryString["ContentType"], out this.m_contentTypeId);
                }

                return this.m_contentTypeId;
            }
        }

        public ListMetadataContentType ContentType
        {
            get
            {
                return this.m_contentType ?? this.configProvider.GetContentType(this.ContentTypeId);
            }
        }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                cbCreateNewColunm.Enabled = !ReferenceListsGuids.IsEmpty();
                cbCreateNewColunm.Checked = true;

                chListContentTypes.DataSource = this.List.ListMetadataContentTypes;
                chListContentTypes.DataBind();
                chListContentTypes.Enabled = (this.ContentTypeId == 0);

                if (this.ContentTypeId != 0) {
                    ListItem itm = chListContentTypes.Items.FindByValue(this.ContentTypeId.ToString());
                    if (itm != null) {
                        itm.Selected = true;
                    }
                }
            }

            if (string.IsNullOrEmpty(radioListFieldType.SelectedValue)) {
                radioListFieldType.SelectedValue = SPFieldType.Text.ToString();
            }

            this.UpdateFieldEditor();
        }

        protected void cbCreateNewColunm_OnCheckedChanged(object sender, EventArgs e)
        {
            UpdateCreateNewColunm();
        }

        private void UpdateCreateNewColunm()
        {
            lbNoFields.Visible = ddlTableFieldName.Visible = 
                ddlTableFieldNameColumn.Visible = !cbCreateNewColunm.Checked;
            if (cbCreateNewColunm.Checked) return;
            ddlTableFieldName.Items.Clear();
            ddlTableFieldName.Items.AddRange(GetDbFields());
        }

        private ListItem[] GetDbFields()
        {
            var result = new List<ListItem>();
            var editor = holderExtraSettings.FindControl(EDITOR_CONTROL_ID) as IDbFieldEditor;
            if (editor == null) return result.ToArray();
            var fld = editor.GetField(txtColName.Text);
            if (fld == null) return result.ToArray();
            lbNoFields.Visible = true;
            ddlTableFieldName.Visible = false;
            lbNoFields.Text = string.Format("There are no existing fields of selected type ({0}). The new field will be created.", fld.SqlType);

            var rosterConfigService = new RosterConfigService();

            var lists = new List<ListMetadataField>();
            var dependentLists = ReferenceListsGuids;
            if (dependentLists.IsEmpty()) return result.ToArray();

            var existingFields = List.ListMetadataFields.Select(item => item.InternalName).ToList();
            dependentLists.ForEach(listId =>
            {
                var list = rosterConfigService.GetList(listId);
                if (list != null)
                {
                    lists.AddRange(list.ListMetadataFields.Where(item => 
                        item.FieldType == fld.SqlType && !existingFields.Contains(item.InternalName)));
                }
                
            });
            if (!lists.Any()) return result.ToArray();
            var grouped = lists.GroupBy(item => item.InternalName).ToList();
            if (!grouped.IsEmpty())
            {
                grouped.ForEach(group =>
                {
                    var itemName = string.Format("{0} ({1})", @group.Key,
                        string.Join(",",
                            @group.Select(x => string.Format("{0} - {1}", x.FieldName, x.ListMetadata.Name))));
                    result.Add(new ListItem(itemName, @group.Key));
                });
                lbNoFields.Visible = false;
                ddlTableFieldName.Visible = true;
            }
            return result.ToArray();
        }

        /// <summary>
        /// Raised on field Type changing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void radioListFieldType_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.UpdateFieldEditor();
        }

        /// <summary>
        /// Dinamically set Field Editor
        /// </summary>
        private void UpdateFieldEditor()
        {
            holderExtraSettings.Controls.Clear();
            SPFieldType fldTypeAsString = EnumHelper.GetEnumByDescription<SPFieldType>(radioListFieldType.SelectedValue);

            switch (fldTypeAsString)
            {
                case SPFieldType.Text:
                    holderExtraSettings.Controls.Add(new DbFieldTextEditor() { ID = EDITOR_CONTROL_ID });
                    break;
                case SPFieldType.Note:
                    holderExtraSettings.Controls.Add(new DbFieldNoteEditor() { ID = EDITOR_CONTROL_ID });
                    break;
                case SPFieldType.Boolean:
                    holderExtraSettings.Controls.Add(new DbFieldBooleanEditor() { ID = EDITOR_CONTROL_ID });
                    break;
                case SPFieldType.DateTime:
                    holderExtraSettings.Controls.Add(new DbFieldDateTimeEditor() { ID = EDITOR_CONTROL_ID });
                    break;
                case SPFieldType.Number:
                    holderExtraSettings.Controls.Add(new DbFieldNumberEditor() { ID = EDITOR_CONTROL_ID });
                    break;
                case SPFieldType.Recurrence:
                    holderExtraSettings.Controls.Add(new DbFieldRecurrenceEditor() { ID = EDITOR_CONTROL_ID });
                    break;
                case SPFieldType.Choice:
                    holderExtraSettings.Controls.Add(new DbFieldChoiceEditor() { ID = EDITOR_CONTROL_ID });
                    break;
                case SPFieldType.Lookup:
                    holderExtraSettings.Controls.Add(new DbFieldLookupEditor(List.Id) { ID = EDITOR_CONTROL_ID });
                    break;
                case SPFieldType.User:
                    holderExtraSettings.Controls.Add(new DbFieldUserEditor(List.Id) { ID = EDITOR_CONTROL_ID });
                    break;
                default:
                    break;
            }
            UpdateCreateNewColunm();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var editor = holderExtraSettings.FindControl(EDITOR_CONTROL_ID) as IDbFieldEditor;
	            if (editor == null) {
					throw new Exception("Cannot find Field Editor on page!");
	            }

                var fld = editor.GetField(txtColName.Text);
                if (List.ListMetadataFields.Any(f => f.FieldName.ToNormalize().ToLower().Equals(fld.DisplayName.ToNormalize().ToLower()))) {
                    throw new Exception("Column with a name " + txtColName.Text + " already exists in a list!");
                }

                if (chListContentTypes.SelectedIndex < 0)
                    throw new Exception("Please select minimum one Content Type!");
                var internalName = cbCreateNewColunm.Checked || string.IsNullOrWhiteSpace(ddlTableFieldName.SelectedValue)
                    ? string.Format("{0}{1}", fld.DisplayName.ToNormalize(), Guid.NewGuid().ToString("N"))
                    : ddlTableFieldName.SelectedValue;

	            var metaField = new ListMetadataField
	            {
		            Id = Guid.NewGuid(),
                    InternalName = internalName,
		            FieldName = fld.DisplayName,
		            Description = txtDescription.Text,
		            ListMetadataId = List.Id,
		            Required = fld.Required,
		            FieldType = fld.SqlType,
		            DefaultValue = fld.DefaultValue
	            };
	            metaField.SetControlProperties(fld);
	            if (fld.Type == SPFieldType.Lookup)
	            {
                    var loFld = fld as DbFieldLookup;
		            if (loFld != null) {
			            metaField.DataSourceType = loFld.ListSource;
						metaField.DataSource = loFld.ListId;
                        metaField.DataSourceKey = loFld.LookupKey;
						metaField.DataSourceField = loFld.LookupField;
		            }
                } else if (fld.Type == SPFieldType.User)
                {
                    var usFld = fld as DbFieldUser;
                    if (usFld != null) {
                        metaField.DataSourceType = usFld.ListSource;
                        metaField.DataSource = usFld.ListId;
                        metaField.DataSourceKey = usFld.LookupKey;
                        metaField.DataSourceField = usFld.LookupField;
                    }
                }
                configProvider.SaveField(metaField);

                // update Content types
                var listContentTypes = this.List.ListMetadataContentTypes;
                foreach (ListItem ctItem in chListContentTypes.Items)
                {
                    if (!ctItem.Selected) { continue; }

                    var ct = listContentTypes.FirstOrDefault(cot => cot.Id == Int32.Parse(ctItem.Value));
                    if (ct != null)
                    {
                        ct.ListMetadataFieldContentTypes.Add(new ListMetadataFieldContentType 
                        {
                            Id = Guid.NewGuid(), ContentTypeId = ct.Id,
                            ListMetadataFieldId = metaField.Id
                        });
                    }

                    // update ContentType in DB
                    configProvider.SaveContentType(ct);
                }

                // close form
                Utils.GoBackOnSuccess(this, this.Context);
            }
            catch (Exception ex)
            {
                holderExtraSettings.Controls.Add(new Label
                {
                    Text = ex.Message, ForeColor = System.Drawing.Color.Red
                });
            }
        }
    }
}
