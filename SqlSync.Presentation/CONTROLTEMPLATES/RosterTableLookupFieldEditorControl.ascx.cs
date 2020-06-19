using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Roster.Presentation.Controls.CustomFields;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Roster.BL;
using Roster.Common;

namespace Roster.Presentation.CONTROLTEMPLATES
{
    public partial class RosterTableLookupFieldEditorControl : UserControl, IFieldEditor
    {
        RosterTableLookupField _field = null;

        public bool DisplayAsNewSection
        {
            get { return true; }
        }

        public void InitializeWithField(SPField field)
        {
            this._field = field as RosterTableLookupField;
        }

        public void OnSaveChange(SPField field, bool isNewField)
        {
            var myField = field as RosterTableLookupField;
            if (myField == null) return;
            myField.DataSoureType = ddlSource.SelectedValue ?? string.Empty;
            if (myField.DataSoureType == ((int)LookupSourceType.Query).ToString())
            {
                myField.TableName = hdnCustomQueryName.Value;
            }
            else
            {
                myField.TableName = ddlTable.SelectedValue ?? string.Empty;                    
            }
            myField.KeyName = ddlLookupKeyField.SelectedValue ?? string.Empty;
            myField.FieldName = ddlLookupDispField.SelectedValue ?? string.Empty;
        }
        protected void btnValidateQuery_Click(object sender, EventArgs e)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(hdnCustomQueryName.Value))
                {
                    hdnCustomQueryName.Value = Guid.NewGuid().ToString().ToNormalize();
                }
                new RosterConfigService().SaveDataSource(hdnCustomQueryName.Value, txtCustomQuery.Text);
            }
            catch (Exception ex)
            {
                pnlValidateResult.Controls.Add(new Label
                {
                    Text = string.Format("Unable to validate query: {0}", ex.Message),
                    ForeColor = Color.Red
                });
            }
            ddlLookupKeyField.Items.Clear();
            ddlLookupKeyField.Items.AddRange(SetKeyFieldItemsBySource());
            ddlLookupDispField.Items.Clear();
            ddlLookupDispField.Items.AddRange(SetFieldItemsBySource());
        }
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            try
            {
                if (!IsPostBack)
                {
                    ddlSource.Items.Clear();
                    var listSources = new[]
                    {
                        new ListItem { Text = LookupSourceType.Table.GetStringValue(), Value = ((int) LookupSourceType.Table).ToString() },
                        new ListItem { Text = LookupSourceType.Query.GetStringValue(), Value = ((int) LookupSourceType.Query).ToString() }
                    };
                    ddlSource.Items.AddRange(listSources);
                    if (_field != null && !String.IsNullOrEmpty(_field.DataSoureType))
                    {
                        ddlSource.SelectedValue = null;
                        var item = ddlSource.Items.FindByValue(_field.DataSoureType);
                        if (item != null)
                        {
                            ddlSource.SelectedValue = _field.DataSoureType;
                            item.Selected = true;
                        }
                    }
                    ddlTable.Items.Clear();
                    ddlTable.Items.AddRange(SetTableBySource());
                    if (_field != null && !String.IsNullOrEmpty(_field.TableName))
                    {
                        if (_field.DataSoureType == ((int)LookupSourceType.Query).ToString())
                        {
                            hdnCustomQueryName.Value = _field.TableName;
                            txtCustomQuery.Text = new RosterConfigService().ReadDataSource(hdnCustomQueryName.Value);
                        }
                        else
                        {
                            ddlTable.SelectedValue = null;
                            var item = ddlTable.Items.FindByValue(_field.TableName);
                            if (item != null)
                            {
                                ddlTable.SelectedValue = _field.TableName;
                                item.Selected = true;
                            }
                        }
                    }
                    ddlLookupKeyField.Items.Clear();
                    ddlLookupKeyField.Items.AddRange(SetKeyFieldItemsBySource());
                    if (_field != null && !String.IsNullOrEmpty(_field.KeyName))
                    {
                        ddlLookupKeyField.SelectedValue = null;
                        var item = ddlLookupKeyField.Items.FindByValue(_field.KeyName);
                        if (item != null)
                        {
                            ddlLookupKeyField.SelectedValue = _field.KeyName;
                            item.Selected = true;
                        }
                    }
                    ddlLookupDispField.Items.Clear();
                    ddlLookupDispField.Items.AddRange(SetFieldItemsBySource());
                    if (_field != null && !String.IsNullOrEmpty(_field.FieldName))
                    {
                        ddlLookupDispField.SelectedValue = null;
                        var item = ddlLookupDispField.Items.FindByValue(_field.FieldName);
                        if (item != null)
                        {
                            ddlLookupDispField.SelectedValue = _field.FieldName;
                            item.Selected = true;
                        }
                    }
                    SetVisibleBySelectedSource();
                }
            }
            catch (Exception ex)
            {
                pnlExeption.Controls.Add(new Label
                {
                    Text = string.Format("Execption: {0}", ex.Message),
                    ForeColor = Color.Red
                });           
            }
        }

        private void SetVisibleBySelectedSource()
        {
            fcQuery.Visible = ddlSource.SelectedValue == ((int)LookupSourceType.Query).ToString();
            fcTable.Visible = !fcQuery.Visible;
        }

        protected void ddlSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetVisibleBySelectedSource();
            ddlTable.Items.Clear();
            ddlTable.Items.AddRange(SetTableBySource());
            ddlLookupKeyField.Items.Clear();
            ddlLookupKeyField.Items.AddRange(SetKeyFieldItemsBySource());
            ddlLookupDispField.Items.Clear();
            ddlLookupDispField.Items.AddRange(SetFieldItemsBySource());
        }

        protected void ddlTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlLookupKeyField.Items.Clear();
            ddlLookupKeyField.Items.AddRange(SetKeyFieldItemsBySource());
            ddlLookupDispField.Items.Clear();
            ddlLookupDispField.Items.AddRange(SetFieldItemsBySource());
        }

        private ListItem[] SetTableBySource()
        {
            var result = new List<ListItem>();
            result.AddRange(new RosterConfigService().DatabaseTables().Select(x => new ListItem(x, x)).ToList());
            return result.ToArray();
        }

        private ListItem[] SetFieldItemsBySource()
        {
            var result = new List<ListItem>();

            var dataSourceName = ddlSource.SelectedValue.ToInt() == ((int)LookupSourceType.Query)
                    ? hdnCustomQueryName.Value : ddlTable.SelectedValue;
            result.AddRange(new RosterConfigService().TablesFields(dataSourceName).
                Select(x => new ListItem(x, x)).ToList());

            return result.ToArray();
        }

        private ListItem[] SetKeyFieldItemsBySource()
        {
            var result = new List<ListItem>();
            var dataSourceName = ddlSource.SelectedValue.ToInt() == ((int)LookupSourceType.Query)
                 ? hdnCustomQueryName.Value : ddlTable.SelectedValue;
            result.AddRange(new RosterConfigService().TablesKeyFields(dataSourceName).
                Select(x => new ListItem(x, x)).ToList());
            return result.ToArray();
        }

        protected T FindControlRecursive<T>(Control rootControl, String id) where T : Control
        {
            T retVal = null;
            if (rootControl.HasControls())
            {
                foreach (Control c in rootControl.Controls)
                {
                    if (c.ID == id) return (T)c;
                    retVal = FindControlRecursive<T>(c, id);
                    if (retVal != null) break;
                }
            }

            return retVal;
        }
    }
}
