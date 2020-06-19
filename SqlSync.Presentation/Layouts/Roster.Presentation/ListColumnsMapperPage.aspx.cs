using System;
using System.Linq;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Roster.Common;
using Roster.BL;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Reflection;
using Microsoft.SharePoint.Utilities;
using System.Web;
using Roster.Model.DataContext;
using Roster.Presentation.Extensions;

namespace Roster.Presentation.Layouts
{
    public partial class ListColumnsMapperPage : LayoutsPageBase
    {
        #region Private variables

        private readonly RosterConfigService _configService = new RosterConfigService();
        private Dictionary<string, string> _listFields;
        private ListMapping _mapping;

        #endregion

        #region Public props

        public Guid ListId
        {
            get
            {
                if (Request.QueryString["List"] == null) {
                    throw new Exception(@"List is requered paramenter");
                }
                return Request.QueryString["List"].ToGuid();
            }
        }
        public Dictionary<string, string> ListFields
        {
            get
            {
                return _listFields ?? (_listFields = SPContext.Current.Web.Lists[ListId].
                    Fields.Cast<SPField>().Where(f => !f.Hidden && f.Type != SPFieldType.Attachments 
                    && f.Type != SPFieldType.Computed).OrderBy(f => f.Title).ToDictionary(d => d.Id.ToString(), d => d.Title));
            }
        }
        public ListMapping Mapping
        {
            get
            {
                return _mapping ?? (_mapping = _configService.GetMapping().FirstOrDefault(m => m.ListName == this.ListId.ToString()));
            }
        }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                try
                {
                    // init SharePoint List Title
                    SPList currentList = SPContext.Current.Web.Lists[this.ListId];
                    lblCurrentListName.Text = string.Format("'{0}' list", currentList.Title);
                    // init list of all DB tables
                    var listDs = _configService.DatabaseTables().Select(item => new { Id = item, Title = item });
                    ddlDbTables.DataSource = listDs;
                    ddlDbTables.DataBind();
                    // try find existing mapping
                    if (this.Mapping != null) 
                    {
                        // pre-load existing Mapping
                        ddlDbTables.Visible = false;
                        lblTableName.Text = string.Format("'{0}'", listDs.Where(t => t.Id.ToString() == Mapping.TableName).Select(t => t.Title).FirstOrDefault());

                        this.UpdateMapperDataSource(Mapping.TableName, Mapping.ListMappingFields, Mapping.Key);
                    } 
                    else 
                    {
                        // init grid DataSource with NO mapping
                        this.UpdateMapperDataSource(listDs.First().Id, null, null);
                    }

                    btnRemove.Visible = (this.Mapping != null);
                }
                catch (Exception ex)
                {
                    pnlError.Controls.Add(new Label{ Text = ex.Message, ForeColor = System.Drawing.Color.Red });
                }
            }
        }

        protected void ddlDbTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateMapperDataSource(((DropDownList)sender).SelectedValue, null, null);
        }

        protected void ddlTableKey_SelectedIndexChanged(object sender, EventArgs e)
        {
            var ddl = (DropDownList)sender;
            if (Mapping != null)
            {
                UpdateMapperDataSource(Mapping.TableName, Mapping.ListMappingFields, ddl.SelectedValue);
            }
            else
            {
                UpdateMapperDataSource(ddlDbTables.SelectedValue, null, ddl.SelectedValue);
            }
        }
        
        protected void ColumnsMapperRep_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var ddlCtrl = e.Item.FindControl("ddlListFields");
                var dd = ddlCtrl as DropDownList;
                if (dd == null) return;
                dd.DataSource = ListFields;
                dd.DataBind();
                dd.SelectedValue = (string)DataBinder.Eval(e.Item.DataItem, "ShPColumnIntName");
            }
        }

        /// <summary>
        /// Add mapping to current SharePoint list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                SaveMapping();
                // go back to List settings page
                GotoBackToSettingsPage();
            }
            catch (Exception ex)
            {
                pnlError.Controls.Add(new Label{ Text = ex.Message, ForeColor = System.Drawing.Color.Red });
            }
        }

        protected void btnSaveSync_Click(object sender, EventArgs e)
        {
            try
            {
                this.SaveMapping();
                BLExtensions.SyncList(this.ListId);
                // go back to List settings page
                this.GotoBackToSettingsPage();
            }
            catch (Exception ex)
            {
                pnlError.Controls.Add(new Label { Text = ex.Message, ForeColor = System.Drawing.Color.Red });
            }
        }

        /// <summary>
        /// Remove mapping from current list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnRemove_Click(object sender, EventArgs e)
        {
            try
            {
                SPList list = SPContext.Current.Web.Lists[this.ListId];
                Guid listId = list.ID;

                // remove mapping
                _configService.RemoveMapping(this.Mapping.Id);

                // remove EventReceiver
                RemoveReceiverFromList(list, Constants.ReceiversNames.SYNC_DATA_ADDED);
                RemoveReceiverFromList(list, Constants.ReceiversNames.SYNC_DATA_UPDATED);
                RemoveReceiverFromList(list, Constants.ReceiversNames.SYNC_DATA_DELETING);
                list.Update();

                // update form to allow add new mapping
                ddlDbTables.Visible = true;
                lblTableName.Text = string.Empty;
                this.UpdateMapperDataSource(ddlDbTables.Items[0].Value, null, null);
            }
            catch (Exception ex)
            {
                pnlError.Controls.Add(new Label{ Text = ex.Message, ForeColor = System.Drawing.Color.Red });
            }
        }

        #region Private methods

        private void SaveMapping()
        {
            var list = SPContext.Current.Web.Lists[ListId];
            var mapping = Mapping ?? new ListMapping 
            { 
                Id = Guid.NewGuid(), 
                ListName = list.ID.ToString(), 
                TableName = ddlDbTables.SelectedValue,
                Key = ddlTableKey.SelectedValue
            };
                
            // collect mapping info
            mapping.ListMappingFields.Clear();
            foreach (RepeaterItem rItm in ColumnsMapperRep.Items)
            {
                var dbColumnId = ((HiddenField)rItm.FindControl("hidDbColumnId")).Value;
                var shpFieldName = ((DropDownList)rItm.FindControl("ddlListFields")).SelectedValue;

                if (!string.IsNullOrEmpty(shpFieldName)) 
                {
                    mapping.ListMappingFields.Add(new ListMappingField {
                        Id = Guid.NewGuid(), ListMappingId = mapping.Id,
                        FieldName = dbColumnId,
                        ItemName = shpFieldName });
                }
            }
            if (mapping.ListMappingFields.IsEmpty())
            {
                throw new Exception("Select at least one mapping pare.");
            }
            // save mapping
            _configService.SaveMapping(mapping);

            // add EventReceiver
            AddReceiverToList(list, Constants.ReceiversNames.SYNC_DATA_ADDED, SPEventReceiverType.ItemAdded, 10010);
            AddReceiverToList(list, Constants.ReceiversNames.SYNC_DATA_UPDATED, SPEventReceiverType.ItemUpdated, 10011);
            AddReceiverToList(list, Constants.ReceiversNames.SYNC_DATA_DELETING, SPEventReceiverType.ItemDeleting, 10012);
        }

        private static void AddReceiverToList(SPList list, string rName, SPEventReceiverType rType, int seq)
        {
            if (!list.EventReceivers.Cast<SPEventReceiverDefinition>().Any(r => r.Name == rName)) {
                SPEventReceiverDefinition _def = list.EventReceivers.Add();
                _def.Assembly = Assembly.GetExecutingAssembly().FullName;
                _def.Class = "Roster.Presentation.EventReceivers.SyncDataReceiver";
                _def.Name = rName;
                _def.Type = rType;
                _def.SequenceNumber = seq;
                _def.Synchronization = SPEventReceiverSynchronization.Default;
                _def.Update();
            }
        }

        private static void RemoveReceiverFromList(SPList list, string rName)
        {
            var rec = list.EventReceivers.Cast<SPEventReceiverDefinition>().FirstOrDefault(r => r.Name == rName);
            if (rec != null)
            {
                list.EventReceivers[rec.Id].Delete();
            }
        }

        private void GotoBackToSettingsPage()
        {
            string queryStr = string.Format("List={0}", this.ListId);
            SPUtility.Redirect("listedit.aspx", SPRedirectFlags.Static | SPRedirectFlags.RelativeToLayoutsPage, HttpContext.Current, queryStr);
        }

        private void UpdateMapperDataSource(string tableName, ICollection<ListMappingField> mapFields, string key)
        {
            UpdateMapperDataSourceKeys(tableName);
            if (string.IsNullOrWhiteSpace(key))
            {
                if (ddlTableKey.Items.Count > 0)
                {
                    key = ddlTableKey.Items[0].Value;
                }
            }
            else
            {
                ddlTableKey.SelectedValue = key;
            }
            UpdateMapperDataSourceFields(tableName, mapFields, key);
        }

        private void UpdateMapperDataSourceKeys(string tableName)
        {
            var fieldDs = _configService.TablesKeyFields(tableName).Select(item => new
            {
                Id = item, Title = item
            });
            ddlTableKey.DataSource = fieldDs;
            ddlTableKey.DataBind();
        }

        private void UpdateMapperDataSourceFields(string tableName, ICollection<ListMappingField> mapFields, string key)
        {
            var fields = _configService.TablesFields(tableName);
            ColumnsMapperRep.DataSource = fields.Where(item => 
                !String.Equals(item, key, StringComparison.InvariantCultureIgnoreCase)).Select(item => new
            {
                DbColumnName = item,
                DbColumnId = item,
                ShPColumnIntName = mapFields == null ? "" :
                    mapFields.Where(f => f.FieldName == item).Select(f => f.ItemName).FirstOrDefault()
            });
            ColumnsMapperRep.DataBind();
        }
        #endregion
    }
}
