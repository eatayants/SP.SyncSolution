using System;
using System.Linq;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Roster.Presentation.Helpers;
using Roster.Model.DataContext;
using Roster.BL;
using System.Collections.Generic;
using System.Drawing;
using Roster.Presentation.WebParts.ExternalListFormWebPart;
using System.Web.Script.Serialization;
using Roster.Common;
using Microsoft.SharePoint.Mobile.Controls;
using System.Collections.ObjectModel;

namespace Roster.Presentation.Layouts
{
    public partial class DbListFromSettings : LayoutsPageBase
    {
        #region Private

        private RosterConfigService configProvider = new RosterConfigService();
        private int m_contentTypeId;
        private ListMetadataContentType m_contentType = null;

        #endregion

        #region Public properties

        public int ContentTypeId
        {
            get
            {
                if (this.m_contentTypeId == 0)
                {
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

        public IEnumerable<ListMetadataAction> CustomActions
        {
            get
            {
                int i = 1;
                var ca = new JavaScriptSerializer().Deserialize<List<ListMetadataAction>>(hidCustomActions.Value);
                foreach (var act in ca) {
                    act.Id = Guid.NewGuid();
                    act.Sequence = i++;
                }
                return ca;
            }
            set
            {
                hidCustomActions.Value = new JavaScriptSerializer().Serialize(value.OrderBy(a => a.Sequence).Select(a => new {
                    LabelText = a.LabelText,
                    Description = a.Description,
                    ImageUrl = a.ImageUrl,
                    Command = a.Command,
                    AccessGroup = a.AccessGroup
                }));
            }
        }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                try
                {
                    if (this.ContentType == null) {
                        throw new Exception("Content Type not found");
                    }

                    txtCtName.Text = this.ContentType.Name;
                    txtDisplayFormUrl.Text = this.ContentType.DispItemUrl;
                    txtEditFormUrl.Text = this.ContentType.EditItemUrl;
                    txtNewFormUrl.Text = this.ContentType.NewItemUrl;
                    chIsVisibleOnNew.Checked = this.ContentType.IsOnNewAction;

                    chIsDefault.Visible = !this.ContentType.IsDefault;
                    lblIsDefaultCT.Visible = this.ContentType.IsDefault;

                    this.CustomActions = this.ContentType.ListMetadata.ListMetadataActions.Where(act => act.ContentTypeId == this.ContentTypeId);
                }
                catch (Exception ex)
                {
                    panelError.Controls.Add(new Label() { Text = ex.Message, ForeColor = Color.Red });
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var ct = this.ContentType;

                string ctName = txtCtName.Text;
                if (configProvider.GetContentTypes(ct.ListMetadataId).Any(cot => cot.Id != ct.Id && cot.Name.ToLower().Equals(ctName.ToLower())))
                    throw new Exception("Content type with name '" + ctName + "' already exists!");

                ct.Name = ctName;
                ct.DispItemUrl = txtDisplayFormUrl.Text;
                ct.EditItemUrl = txtEditFormUrl.Text;
                ct.NewItemUrl = txtNewFormUrl.Text;
                ct.IsOnNewAction = chIsVisibleOnNew.Checked;

                if (!ct.IsDefault)
                    ct.IsDefault = chIsDefault.Checked; // user cannot remove 'IsDefault' flag

                // custom actions
                var otherActions = ct.ListMetadataActions.Where(act => act.ContentTypeId != this.ContentTypeId);
                ct.ListMetadataActions.Clear();
                var ca = CustomActions;
                ca.ToList().ForEach(a => { a.ListMetadataId = ct.ListMetadataId; a.ContentTypeId = ct.Id; });
                ca.ToList().AddRange(otherActions);
                ct.ListMetadataActions = new Collection<ListMetadataAction>(ca.ToList());

                // update data in DB
                configProvider.SaveContentType(ct);

                // REMOVE IsDefault flag from other views
                if (chIsDefault.Visible && chIsDefault.Checked) {
                    // chIsDefault.Visible == true   means that on PageLoad view was NOT Default
                    this.RemoveDefaultFlagsFromOtherCTs(ct.Id);
                }

                // close form
                Utils.GoBackOnSuccess(this, Context);
            }
            catch (Exception ex)
            {
                panelError.Controls.Add(new Label { Text = ex.Message, ForeColor = Color.Red });
            }
        }

        #region Private methods

        private void RemoveDefaultFlagsFromOtherCTs(int newDefaultContentTypeId)
        {
            var allCTs = configProvider.GetContentTypes(this.ContentType.ListMetadataId);
            foreach (ListMetadataContentType ct in allCTs.Where(x => x.Id != newDefaultContentTypeId))
            {
                ct.IsDefault = false;
                new RosterConfigService().SaveContentType(ct);
            }
        }

        #endregion
    }
}
