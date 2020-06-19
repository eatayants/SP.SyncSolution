using System;
using System.Linq;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Roster.BL;
using Roster.Model.DataContext;
using System.Collections.Generic;
using Microsoft.SharePoint.Utilities;
using System.Web.UI.WebControls;
using Roster.Presentation.Helpers;

namespace Roster.Presentation.Layouts
{
    public partial class DbContentTypeEdit : LayoutsPageBase
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
                try
                {
                    if (this.ContentType == null)
                        throw new Exception("Content Type not found");

                    // init Content Type name
                    lblContentTypeName.Text = this.ContentType.Name;

                    // Add fields
                    ColumnsRepeater.DataSource = this.ContentType.ListMetadataFieldContentTypes.OrderBy(f => f.ItemPosition).Select(f => f.ListMetadataField);
                    ColumnsRepeater.DataBind();

                    // add 'ContentType' url param to links
                    string encodedContentTypeId = SPEncode.UrlEncode(this.ContentTypeId.ToString());
                    linkFormSettings.NavigateUrl += "?ContentType=" + encodedContentTypeId;
                    linkNewField.NavigateUrl += "?ContentType=" + encodedContentTypeId;
                    linkFieldsOrdering.NavigateUrl += "?ContentType=" + encodedContentTypeId;
                    linkAddField.NavigateUrl += "?ContentType=" + encodedContentTypeId;

                    // add 'List' url param to links
                    string encodedListId = SPEncode.UrlEncode(this.ContentType.ListMetadataId.ToString("B").ToUpper());
                    linkNewField.NavigateUrl += "&List=" + encodedListId;

                    btnDelete.Visible = !this.ContentType.IsDefault;
                }
                catch (Exception ex) {
                    panelErrror.Controls.Add(new Microsoft.SharePoint.Mobile.Controls.Label() { Text = ex.Message });
                }
            }
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                configProvider.RemoveContentType(this.ContentTypeId);

                // close form
                Utils.GoBackOnSuccess(this, Context);
            }
            catch (Exception ex)
            {
                panelErrror.Controls.Add(new Label { Text = ex.Message, ForeColor = System.Drawing.Color.Red });
            }
        }
    }
}
