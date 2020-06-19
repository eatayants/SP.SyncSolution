using Microsoft.SharePoint.WebControls;
using Roster.BL;
using Roster.Presentation.Helpers;
using Roster.Presentation.WebParts.ExternalListViewWebPart;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Roster.Presentation.Controls.ToolParts
{
    public class DbListViewToolPart : Microsoft.SharePoint.WebPartPages.ToolPart
    {
        DropDownList ddlCatalogs;
        DropDownList ddlViews;
        HiddenField hidAllViewsInfo;
        TextBox txtRelationFld;
        TextBox txtFilterOperators;
        TextBox txtCalendarWidth;
        TextBox txtCalendarHeight;
        CheckBox chEditMode;
        CheckBox chDisplayPrepopulated;

        protected override void CreateChildControls()
        {
            RosterConfigService configProvider = new RosterConfigService();
            var lists = configProvider.GetLists();

            ddlCatalogs = new DropDownList();
            ddlCatalogs.ID = "catalogSelector";
            ddlCatalogs.CssClass = "lists_control";
            ddlCatalogs.Attributes.Add("onchange", "UpdateViewsList(this);");
            ddlCatalogs.Items.AddRange(lists.Select(x => new ListItem(x.Name, x.Id.ToString())).ToArray());

            ddlViews = new DropDownList();
            ddlViews.ID = "viewSelector";
            ddlViews.CssClass = "views_control";
            ddlViews.Items.AddRange(lists.SelectMany(x => x.ViewMetadatas).Select(v => new ListItem(v.Name, v.Id.ToString())).ToArray());

            hidAllViewsInfo = new HiddenField();
            hidAllViewsInfo.ID = "hidAllViewsHierarchy";
            hidAllViewsInfo.Value = new JavaScriptSerializer().Serialize(lists.Select(x => new { list = x.Id, views = x.ViewMetadatas.Select(v => new { viewId = v.Id, viewName = v.Name }) }));

            txtRelationFld = new TextBox();
            txtRelationFld.ID = "txtRelationField";

            txtFilterOperators = new TextBox();
            txtFilterOperators.ID = "txtFilterOperators";

            txtCalendarWidth = new TextBox();
            txtCalendarWidth.ID = "txtCalendarWidth";

            txtCalendarHeight = new TextBox();
            txtCalendarHeight.ID = "txtCalendarHeight";

            chEditMode = new CheckBox();
            chEditMode.ID = "chEditMode";
            chEditMode.Text = "Edit mode? (GridView only)";

            chDisplayPrepopulated = new CheckBox();
            chDisplayPrepopulated.ID = "chDisplayPrepopulated";
            chDisplayPrepopulated.Text = "Display pre-populated? (CalendarView only)";

            this.Controls.Add(ddlCatalogs);
            this.Controls.Add(ddlViews);
            this.Controls.Add(hidAllViewsInfo);
            this.Controls.Add(txtRelationFld);
            this.Controls.Add(txtFilterOperators);
            this.Controls.Add(txtCalendarWidth);
            this.Controls.Add(txtCalendarHeight);
            this.Controls.Add(chEditMode);
            this.Controls.Add(chDisplayPrepopulated);
            ScriptLink.Register(this.Page, "/_layouts/15/Roster.Presentation/js/cascadeSelection.view.js", true);

            //setting the last set values as current values on custom toolpart
            ExternalListViewWebPart wp = (ExternalListViewWebPart)this.ParentToolPane.SelectedWebPart;
            if (wp != null) {
                this.ddlCatalogs.SelectedValue = wp.CatalogId;
                this.ddlViews.SelectedValue = wp.ViewName;
                this.txtRelationFld.Text = wp.RelationField;
                this.txtFilterOperators.Text = wp.FilterOperators;
                this.txtCalendarWidth.Text = wp.CalendarWidth;
                this.txtCalendarHeight.Text = wp.CalendarHeight;
                this.chEditMode.Checked = wp.EditMode;
                this.chDisplayPrepopulated.Checked = wp.DisplayPrepopulated;
            }

            base.CreateChildControls();
        }

        protected override void RenderToolPart(HtmlTextWriter writer)
        {
            AddAttributesToRender(writer);

            writer.Write(string.Format("<div id=\"{0}\">", "list_and_view_selector_block"));
            writer.RenderBeginTag(HtmlTextWriterTag.Table); // <table>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            new LiteralControl("Catalog: ").RenderControl(writer); // <label/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            ddlCatalogs.RenderControl(writer); // <select/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            new LiteralControl("View: ").RenderControl(writer); // <label/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            ddlViews.RenderControl(writer); // <select/>
            hidAllViewsInfo.RenderControl(writer); // <hidden/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            new LiteralControl("Relation field: ").RenderControl(writer); // <label/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            txtRelationFld.RenderControl(writer); // <select/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            new LiteralControl("Filter operators (eg. 'Location;#AND'): ").RenderControl(writer); // <label/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            txtFilterOperators.RenderControl(writer); // <select/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            new LiteralControl("Calendar width (in px): ").RenderControl(writer); // <label/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            txtCalendarWidth.RenderControl(writer); // <select/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            new LiteralControl("Calendar height (in px): ").RenderControl(writer); // <label/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            txtCalendarHeight.RenderControl(writer); // <select/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            chEditMode.RenderControl(writer); // <checkbox/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // <tr>
            writer.RenderBeginTag(HtmlTextWriterTag.Td); // <td>
            chDisplayPrepopulated.RenderControl(writer); // <checkbox/>
            writer.RenderEndTag(); // </td>
            writer.RenderEndTag(); // </tr>

            writer.RenderEndTag(); // </table>
            writer.Write("</div>");
        }

        public override void ApplyChanges()
        {
            ExternalListViewWebPart wp = (ExternalListViewWebPart)this.ParentToolPane.SelectedWebPart;
            wp.CatalogId = this.ddlCatalogs.SelectedValue;
            wp.ViewName = this.ddlViews.SelectedValue;
            wp.RelationField = this.txtRelationFld.Text;
            wp.FilterOperators = this.txtFilterOperators.Text;
            wp.CalendarHeight = this.txtCalendarHeight.Text;
            wp.CalendarWidth = this.txtCalendarWidth.Text;
            wp.EditMode = this.chEditMode.Checked;
            wp.DisplayPrepopulated = this.chDisplayPrepopulated.Checked;

            base.ApplyChanges();
        }
    }
}
