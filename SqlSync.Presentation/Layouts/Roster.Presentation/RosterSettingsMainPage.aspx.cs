using System;
using System.Linq;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Roster.Presentation.Controls;
using Roster.Presentation.Controls.Fields;
using Roster.Model.DataContext;
using System.Collections.Generic;
using Roster.BL;
using Microsoft.SharePoint.Utilities;
using Roster.Presentation.Extensions;

namespace Roster.Presentation.Layouts
{
    public partial class RosterSettingsMainPage : LayoutsPageBase
    {
        #region Private

        private RosterConfigService configProvider = new RosterConfigService();
        private Guid m_listId = Guid.Empty;
        private List<ViewMetadata> m_views = null;
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
        public List<ViewMetadata> ListViews
        {
            get
            {
                if (m_views == null) {
                    this.m_views = new List<ViewMetadata>();
                    this.m_views.AddRange(this.configProvider.GetViews(this.ListGuid));
                }

                return this.m_views;
            }
        }
        public ListMetadata List
        {
            get
            {
                if (m_list == null) {
                    this.m_list = this.configProvider.GetList(this.ListGuid);
                }

                return this.m_list;
            }
        }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                try
                {
                    if (this.List == null)
                        throw new Exception("List not found");

                    // init List name
                    lblListName.Text = this.List.Name;

                    // Add fields
                    var allFields = this.List.ListMetadataContentTypes.SelectMany(ct => ct.ListMetadataFieldContentTypes.Select(ctf => new {
                        ContentType = ct.Name,
                        FieldId = ctf.ListMetadataFieldId,
                        Field = ctf.ListMetadataField
                    }));
                    ColumnsRepeater.DataSource = allFields.GroupBy(g => g.FieldId).Select(g => new {
                        Id = g.Key,
                        FieldName = g.First().Field.FieldName,
                        FieldType = g.First().Field.FieldType,
                        UsedIn = string.Join(", ", g.Select(x => x.ContentType))
                    }).OrderBy(f => f.FieldName);
                    ColumnsRepeater.DataBind();

                    // Add Content Types
                    ContTypesRepeater.DataSource = this.List.ListMetadataContentTypes.OrderBy(ct => ct.IsDefault);
                    ContTypesRepeater.DataBind();

                    // Add Views
                    ViewsRepeater.DataSource = this.ListViews.OrderBy(f => f.Name);
                    ViewsRepeater.DataBind();

                    // add 'List' url param to links
                    string encodedListId = SPEncode.UrlEncode(this.ListGuid.ToString("B").ToUpper());
                    linkNewField.NavigateUrl += "?List=" + encodedListId;
                    linkNewView.NavigateUrl += "?List=" + encodedListId;
                    linkNewContentType.NavigateUrl += "?List=" + encodedListId;
                    linkListPermissions.NavigateUrl += "?ListId=" + encodedListId;
                }
                catch (Exception ex) {
                    panelErrror.Controls.Add(new Microsoft.SharePoint.Mobile.Controls.Label() { Text = ex.Message });
                }
            }
        }
    }
}
