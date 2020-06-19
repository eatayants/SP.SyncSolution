using System;
using System.Linq;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Web.UI.WebControls;
using Roster.BL;
using Roster.Model.DataContext;
using Roster.Presentation.Helpers;
using Roster.Common;

namespace Roster.Presentation.Layouts
{
    public partial class DbListAddFieldToContentType : LayoutsPageBase
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

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!Page.IsPostBack)
                {
                    listBoxOfColumns.DataSource = this.ContentType.ListMetadata.ListMetadataFields.OrderBy(f => f.FieldName);
                    listBoxOfColumns.DataBind();
                }
            }
            catch (Exception ex)
            {
                errorHolder.Controls.Add(new Label() { Text = ex.Message, ForeColor = System.Drawing.Color.Red });
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                int pos = 100;
                var ct = this.ContentType;
                var listFields = ct.ListMetadata.ListMetadataFields;

                int[] indexes = listBoxOfColumns.GetSelectedIndices();
		        for (int index = 0; index < indexes.Length; index++)
                {
                    ct.ListMetadataFieldContentTypes.Add(new ListMetadataFieldContentType {
                        Id = Guid.NewGuid(),
                        ContentTypeId = ct.Id,
                        ListMetadataFieldId = listBoxOfColumns.Items[indexes[index]].Value.ToGuid(),
                        ItemPosition = pos++
                    });
                }

                // update data in DB
                new RosterConfigService().SaveContentType(ct);

                // close form
                Utils.GoBackOnSuccess(this, Context);
            }
            catch (Exception ex)
            {
                errorHolder.Controls.Add(new Label() { Text = ex.Message, ForeColor = System.Drawing.Color.Red });
            }
        }
    }
}
