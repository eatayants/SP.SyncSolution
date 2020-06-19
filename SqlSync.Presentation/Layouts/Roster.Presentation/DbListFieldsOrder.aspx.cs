using System;
using System.Linq;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Roster.Presentation.Helpers;
using System.Web.UI.WebControls;
using Roster.Model.DataContext;
using System.Collections.Generic;
using Roster.BL;
using System.Web.UI;

namespace Roster.Presentation.Layouts
{
    public partial class DbListFieldsOrder : LayoutsPageBase
    {
        #region Private

        private RosterConfigService configProvider = new RosterConfigService();
        private int m_contentTypeId;
        private ListMetadataContentType m_contentType = null;
        private int m_fieldsCount;

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
            get { return this.m_contentType ?? (this.m_contentType = this.configProvider.GetContentType(this.ContentTypeId)); }
        }
        public int FieldsCount
        {
            get
            {
                return (m_fieldsCount != 0) ? m_fieldsCount : (m_fieldsCount = this.ContentType.ListMetadataFieldContentTypes.Count);
            }
        }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                listColumnsGrid.DataSource = this.ContentType.ListMetadataFieldContentTypes.Select(fld => new {
                    Id = fld.Id,
                    DisplayName = fld.ListMetadataField.FieldName,
                    Position = fld.ItemPosition
                }).OrderBy(fld => fld.Position);
                listColumnsGrid.DataBind();
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                ListMetadataContentType contentType = this.ContentType;

                foreach (GridViewRow gvRow in listColumnsGrid.Rows)
                {
                    Guid fldId = new Guid(listColumnsGrid.DataKeys[gvRow.RowIndex].Value.ToString());
                    var fld = contentType.ListMetadataFieldContentTypes.FirstOrDefault(f => f.Id == fldId);
                    if (fld != null) {
                        fld.ItemPosition = Int32.Parse(((System.Web.UI.WebControls.DropDownList)gvRow.FindControl("ddlColumnPosition")).SelectedValue);
                    }
                }
                new RosterConfigService().SaveContentType(contentType);

                // close form
                Utils.GoBackOnSuccess(this, Context);
            }
            catch (Exception ex)
            {
                panelError.Controls.Add(new Label { Text = ex.Message, ForeColor = System.Drawing.Color.Red });
            }
        }

        protected void ListColumnsGrid_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //Finding the Dropdown control
                Control ctrl = e.Row.FindControl("ddlColumnPosition");
                if (ctrl != null)
                {
                    int total = this.FieldsCount;
                    DropDownList dd = ctrl as DropDownList;
                    dd.Attributes.Add("onchange", string.Format("Reorder(this,{0},{1})", e.Row.RowIndex, total));
                    dd.CssClass = "ViewOrder" + e.Row.RowIndex;
                    dd.DataSource = Enumerable.Range(1, total);
                    dd.DataBind();
                    dd.SelectedIndex = e.Row.RowIndex;
                }
            }
        }
    }
}
