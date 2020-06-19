using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Roster.Presentation.Controls.Fields;
using Roster.Presentation.Controls;
using System.Web.UI.WebControls;
using Microsoft.SharePoint.Client;
using Roster.Presentation.Controls.FieldEditors;
using Roster.Presentation.Helpers;
using Roster.BL;
using Roster.Common;
using Roster.Model.DataContext;
using Roster.Presentation.Extensions;
using ListItem = System.Web.UI.WebControls.ListItem;

namespace Roster.Presentation.Layouts
{
    public partial class DbFldEdit : LayoutsPageBase
    {
        #region Private

        private const string EDITOR_CONTROL_ID = "EditorControlId";

        private RosterConfigService configProvider = new RosterConfigService();
        private Guid m_fieldId = Guid.Empty;
        private readonly List<Guid> _referenceListsGuids = new List<Guid>();
        private ListMetadataField m_field = null;
        private Guid m_listId = Guid.Empty;
        private ListMetadata m_list = null;

        #endregion

        #region Public properties

        public Guid ListGuid
        {
            get
            {
                if (this.m_listId == Guid.Empty)
                {
                    this.m_listId = Field.ListMetadataId;
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
                                && item.Item1 != refItem.Item1).Select(item => item.Item1));
                    }
                }
                return _referenceListsGuids;
            }
        }

        public Guid FieldGuid
        {
            get
            {
                if (this.m_fieldId == Guid.Empty)
                {
                    Guid _fid;
                    string fldId = Request.QueryString["Field"];
                    if (!string.IsNullOrEmpty(fldId) && Guid.TryParse(fldId, out _fid))
                        this.m_fieldId = _fid;
                }

                return this.m_fieldId;
            }
        }

        public ListMetadataField Field
        {
            get
            {
                if (m_field == null) {
                    this.m_field = this.configProvider.GetField(this.FieldGuid);
                }

                return this.m_field;
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

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (this.Field == null)
                    throw new Exception("Cannot find Field!");

                // get typed Field
                DbField fld = this.Field.GetDbField();

                // pre-load common existing values
                if (!Page.IsPostBack) {
                    txtColName.Text = this.Field.FieldName;
                    txtDescription.Text = this.Field.Description;
                    radioFieldType.Text = fld.TypeAsDisplayLabel;

                    // hide 'Remove' button on Static fields
                    btnRemove.Visible = !this.Field.IsStatic;

                    // where current column is Used?
                    lblUsedInContentTypes.Text = String.Join(", ", this.Field.ListMetadata.ListMetadataContentTypes
                        .Where(ct => ct.ListMetadataFieldContentTypes.Any(f => f.ListMetadataFieldId == this.FieldGuid)).Select(ct => ct.Name));
                }

                this.UpdateFieldEditor(fld);
            }
            catch (Exception ex)
            {
                holderExtraSettings.Controls.Add(new Label() { Text = ex.Message, ForeColor = System.Drawing.Color.Red });
            }
        }

        private void UpdateDbColumn()
        {
            if (!Page.IsPostBack)
            {
                ddlTableFieldName.SelectedIndex = -1;
                ddlTableFieldName.Items.Clear();
                var listItem = new ListItem(this.Field.InternalName, this.Field.InternalName)
                {
                    Selected = true
                };
                ddlTableFieldName.Items.Add(listItem);
                ddlTableFieldName.Items.AddRange(GetDbFields());
            }
        }

        private ListItem[] GetDbFields()
        {
            var result = new List<ListItem>();
            var editor = holderExtraSettings.FindControl(EDITOR_CONTROL_ID) as IDbFieldEditor;
            if (editor == null) return result.ToArray();
            var fld = editor.GetField(txtColName.Text);
            if (fld == null) return result.ToArray();

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
            }
            return result.ToArray();
        }

        private void UpdateFieldEditor(DbField field)
        {
            holderExtraSettings.Controls.Clear();

            switch (field.Type)
            {
                case SPFieldType.Text:
                    DbFieldTextEditor txtEd = new DbFieldTextEditor() { ID = EDITOR_CONTROL_ID };
                    txtEd.DefaultValue = field.DefaultValue;
                    txtEd.Required = field.Required;
                    txtEd.MaxLength = (field as DbFieldText).MaxLength;
                    holderExtraSettings.Controls.Add(txtEd);
                    break;
                case SPFieldType.Note:
                    DbFieldNoteEditor noteEd = new DbFieldNoteEditor() { ID = EDITOR_CONTROL_ID };
                    noteEd.Required = field.Required;
                    noteEd.NumberOfLines = (field as DbFieldNote).NumberOfLines;
                    holderExtraSettings.Controls.Add(noteEd);
                    break;
                case SPFieldType.Boolean:
                    DbFieldBooleanEditor fbe = new DbFieldBooleanEditor() { ID = EDITOR_CONTROL_ID };
                    fbe.DefaultValue = string.IsNullOrEmpty(field.DefaultValue) ? false : (field.DefaultValue == "1");
                    holderExtraSettings.Controls.Add(fbe);
                    break;
                case SPFieldType.DateTime:
                    DbFieldDateTimeEditor dtEd = new DbFieldDateTimeEditor() { ID = EDITOR_CONTROL_ID };
                    dtEd.DefaultValue = field.DefaultValue;
                    dtEd.Required = field.Required;
                    dtEd.Format = (field as DbFieldDateTime).Format;
                    holderExtraSettings.Controls.Add(dtEd);
                    break;
                case SPFieldType.Number:
                    DbFieldNumberEditor numEd = new DbFieldNumberEditor() { ID = EDITOR_CONTROL_ID };
                    double defV = 0;
                    if (Double.TryParse(field.DefaultValue, out defV))
                        numEd.DefaultValue = defV;
                    numEd.Required = field.Required;
                    numEd.DecimalPlaces = (field as DbFieldNumber).DecimalPlaces;
                    numEd.MinValue = (field as DbFieldNumber).MinValue;
                    numEd.MaxValue = (field as DbFieldNumber).MaxValue;
                    holderExtraSettings.Controls.Add(numEd);
                    break;
                case SPFieldType.Recurrence:
                    DbFieldRecurrenceEditor recEd = new DbFieldRecurrenceEditor() { ID = EDITOR_CONTROL_ID };
                    holderExtraSettings.Controls.Add(recEd);
                    break;
                case SPFieldType.Choice:
                    DbFieldChoiceEditor chEd = new DbFieldChoiceEditor() { ID = EDITOR_CONTROL_ID };
                    chEd.DefaultValue = field.DefaultValue;
                    chEd.Required = field.Required;
                    chEd.ControlType = (field as DbFieldChoice).ControlType;
                    chEd.Choices = (field as DbFieldChoice).Choices;
                    holderExtraSettings.Controls.Add(chEd);
                    break;
                case SPFieldType.Lookup:
                    var lookEd = new DbFieldLookupEditor(this.Field.ListMetadataId)
                    {
                        ID = EDITOR_CONTROL_ID,
                        Required = field.Required,
                        ListSource = (field as DbFieldLookup).ListSource,
                        ListId = (field as DbFieldLookup).ListId,
                        LookupKey = (field as DbFieldLookup).LookupKey,
                        LookupField = (field as DbFieldLookup).LookupField,
                        DependentParent = (field as DbFieldLookup).DependentParent,
                        DependentParentField = (field as DbFieldLookup).DependentParentField,
                        FilterByField = (field as DbFieldLookup).FilterByField
                    };
                    holderExtraSettings.Controls.Add(lookEd);
                    break;
                case SPFieldType.User:
                    var userEd = new DbFieldUserEditor(this.Field.ListMetadataId)
                    {
                        ID = EDITOR_CONTROL_ID,
                        Required = field.Required,
                        LookupField = (field as DbFieldUser).LookupField,
                        AllowSelection = (field as DbFieldUser).AllowSelection,
                        ChooseFrom = (field as DbFieldUser).ChooseFrom,
                        ChooseFromGroup = (field as DbFieldUser).ChooseFromGroup
                    };
                    holderExtraSettings.Controls.Add(userEd);
                    break;
                default:
                    break;
            }
            UpdateDbColumn();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                IDbFieldEditor editor = holderExtraSettings.FindControl(EDITOR_CONTROL_ID) as IDbFieldEditor;
                if (editor == null)
                    throw new Exception("Cannot find Field Editor on page!");
                DbField fld = editor.GetField(txtColName.Text);

                ListMetadataField metaField = this.Field;
                metaField.FieldName = fld.DisplayName;
                metaField.InternalName = ddlTableFieldName.SelectedValue;
                metaField.Description = txtDescription.Text;
                metaField.Required = fld.Required;
                metaField.DefaultValue = fld.DefaultValue;
                metaField.SetControlProperties(fld);
	            if (fld.Type == SPFieldType.Lookup)
	            {
		            var loFld = fld as DbFieldLookup;
		            if (loFld != null)
		            {
			            metaField.DataSourceType = loFld.ListSource;
			            metaField.DataSource = loFld.ListId;
			            metaField.DataSourceKey = loFld.LookupKey;
                        metaField.DataSourceField = loFld.LookupField;
		            }
	            }
                else if (fld.Type == SPFieldType.User)
                {
                    var usFld = fld as DbFieldUser;
                    if (usFld != null) {
                        metaField.DataSourceField = usFld.LookupField;
                    }
                }
                else
	            {
					metaField.DataSourceType = (int)LookupSourceType.None;
					metaField.DataSource = null;
                    metaField.DataSourceKey = null;
					metaField.DataSourceField = null;
	            }
                new RosterConfigService().SaveField(metaField);

                // close form
                Utils.GoBackOnSuccess(this, this.Context);
            }
            catch (Exception ex)
            {
                holderExtraSettings.Controls.Add(new Label() { Text = ex.Message, ForeColor = System.Drawing.Color.Red });
            }
        }

        protected void btnRemove_Click(object sender, EventArgs e)
        {
            try
            {
                this.configProvider.RemoveField(this.FieldGuid);

                // close form
                Utils.GoBackOnSuccess(this, this.Context);
            }
            catch (Exception ex)
            {
                holderExtraSettings.Controls.Add(new Label() { Text = ex.Message, ForeColor = System.Drawing.Color.Red });
            }
        }
    }
}
