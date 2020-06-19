using System;
using System.Linq;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Roster.Presentation.Helpers;
using System.Web.UI.WebControls;
using Roster.BL;
using Roster.Model.DataContext;

namespace Roster.Presentation.Layouts
{
    public partial class DbContentTypeNew : LayoutsPageBase
    {
        #region Private

        private RosterConfigService configProvider = new RosterConfigService();
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
                    Guid _lid;
                    string lstId = Request.QueryString["List"];
                    if (!string.IsNullOrEmpty(lstId) && Guid.TryParse(lstId, out _lid))
                        this.m_listId = _lid;
                }

                return this.m_listId;
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
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string newContentTypeName = txtContentTypeName.Text;
                if (this.List.ListMetadataContentTypes.Any(ct => ct.Name.ToLower().Equals(newContentTypeName.ToLower())))
                    throw new Exception("Content type with name '" + newContentTypeName + "' already exists!");

                ListMetadataContentType newContentType = configProvider.CreateContentType(this.List.Id);
                newContentType.Name = newContentTypeName;
                newContentType.IsDefault = false;
                newContentType.IsOnNewAction = true;
                newContentType.DispItemUrl += "&ContentTypeId=" + newContentType.Id;
                newContentType.EditItemUrl += "&ContentTypeId=" + newContentType.Id;
                newContentType.NewItemUrl += "&ContentTypeId=" + newContentType.Id;
                newContentType.ListMetadataFieldContentTypes.Add(new ListMetadataFieldContentType {
                        Id = Guid.NewGuid(),
                        ContentTypeId = newContentType.Id,
                        ListMetadataFieldId = this.List.ListMetadataFields.FirstOrDefault(f => f.InternalName == Common.FieldNames.CONTENT_TYPE_ID).Id
                    });

                // save
                configProvider.SaveContentType(newContentType);

                // close form
                Utils.GoBackOnSuccess(this, this.Context);
            }
            catch (Exception ex)
            {
                errorHolder.Controls.Add(new Label() { Text = ex.Message, ForeColor = System.Drawing.Color.Red });
            }
        }
    }
}
