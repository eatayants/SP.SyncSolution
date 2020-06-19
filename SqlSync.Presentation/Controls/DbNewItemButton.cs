using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Roster.BL;
using Roster.Model.DataContext;
using Roster.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Roster.Presentation.Controls
{
    public class DbNewItemButton : CompositeControl
    {
        private const string URL_FORMAT = "{0}/{1}&ListId={2}";

        #region Private variables

        private ListMetadata m_list;
        private Dictionary<string, string> m_urlParams;
        private Dictionary<string, string> m_allUrlParams = null;
        private Panel newItemPanel;
        private object m_parentId;

        #endregion

        #region Properties

        public ListMetadata List
        {
            get { return this.m_list; }
            set { this.m_list = value; }
        }

        public Dictionary<string, string> AdditionalUrlParams
        {
            get { return this.m_urlParams; }
            set { this.m_urlParams = value; }
        }

        public object ParentElemId
        {
            get { return this.m_parentId; }
            set { this.m_parentId = value; }
        }

        public Dictionary<string, string> UrlParamsFromParent
        {
            get
            {
                if (this.m_allUrlParams == null)
                {
                    m_allUrlParams = new Dictionary<string, string>(this.AdditionalUrlParams);
                    if (this.ParentElemId != null)
                    {
                        var parentInfo = new RosterDataService().GetParentEntity(this.ParentElemId);
                        if (parentInfo != null)
                        {
                            string[] excludeFieldNames = new string[] { FieldNames.ID, FieldNames.CONTENT_TYPE_ID, FieldNames.PARENT_ROSTER_ID, FieldNames.ROSTER_EVENT_ID,
                                StaticFields.Modified, StaticFields.ModifiedBy, StaticFields.Created, StaticFields.CreatedBy };
                            var parentInfoDict = parentInfo as IDictionary<string, object>;
                            foreach (string key in parentInfoDict.Keys)
                            {
                                if (!m_allUrlParams.ContainsKey(key))
                                {
                                    object _val = parentInfoDict[key];
                                    if (_val == null || string.IsNullOrEmpty(_val.ToSafeString().Trim()) || excludeFieldNames.Contains(key, StringComparer.InvariantCultureIgnoreCase))
                                        //key.Equals(FieldNames.ID) || key.Equals(FieldNames.CONTENT_TYPE_ID) || key.Equals(FieldNames.PARENT_ROSTER_ID) || key.Equals(FieldNames.ROSTER_EVENT_ID))
                                    {
                                        continue;
                                    }
                                    else if (_val is DateTime)
                                    {
                                        m_allUrlParams.Add(key, SPUtility.CreateISO8601DateTimeFromSystemDateTime((DateTime) _val));
                                    }
                                    else
                                    {
                                        m_allUrlParams.Add(key, SPHttpUtility.UrlKeyValueEncode(_val.ToSafeString()));
                                    }
                                }
                            }
                        }
                    }
                }

                return this.m_allUrlParams;
            }
        }

        public bool IsMultiContentTypeList
        {
            get { return this.VisibleContentTypes.Count() > 1; }
        }

        public IEnumerable<ListMetadataContentType> VisibleContentTypes
        {
            get { return this.List.ListMetadataContentTypes.Where(ct => ct.IsOnNewAction); }
        }

        #endregion

        protected override void RecreateChildControls()
        {
            EnsureChildControls();
        }

        protected override void CreateChildControls()
        {
            Controls.Clear();

            SPWeb web = SPContext.Current.Web;
            newItemPanel = new Panel() { ID = "panelNewItem" };

            string navigateUrl = "javascript:void(0);";
            string webUrl = SPContext.Current.Web.ServerRelativeUrl.TrimEnd('/');
            string listId = SPEncode.UrlEncode(this.List.Id.ToString("B").ToUpper());
            if (!this.IsMultiContentTypeList) {
                navigateUrl = string.Format("javascript:SP.UI.ModalDialog.ShowPopupDialog('{0}')",
                    BuidUri(string.Format(URL_FORMAT, webUrl, this.List.DefaultContent.NewItemUrl.TrimStart('/'), listId), this.UrlParamsFromParent));
            }

            HyperLink addNewLink = new HyperLink {
                ID = "linkAddNewItem", ToolTip = "new item",
                CssClass = "ms-heroCommandLink " + this.List.Id.ToString("N"),
                NavigateUrl = navigateUrl
            };
            Label addNewImgLabel = new Label() {CssClass = "ms-list-addnew-imgSpan20"};
            Image addNewImg = new Image {
                ImageUrl = "/_layouts/15/images/spcommon.png?rev=23",
                CssClass = "ms-list-addnew-img20"
            };
            Label addNewLabel = new Label() {Text = "new item"};

            addNewImgLabel.Controls.Add(addNewImg);
            addNewLink.Controls.Add(addNewImgLabel);
            addNewLink.Controls.Add(addNewLabel);

            // add extra hidden SELECT control to access all list Content types from Ribbon Button
            DropDownList ctList = new DropDownList {
                ID = "listOfContentTypes", CssClass = "ct-for-" + this.List.Id.ToString("N").ToUpper()
            };
            ctList.Style.Add("display", "none");
            ctList.Items.AddRange(this.VisibleContentTypes.Select(item => new ListItem(item.Name,
                BuidUri(string.Format(URL_FORMAT, webUrl, item.NewItemUrl.TrimStart('/'), listId), UrlParamsFromParent))).ToArray());

            newItemPanel.Controls.Add(addNewLink);
            newItemPanel.Controls.Add(ctList);
            this.Controls.Add(newItemPanel);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            AddAttributesToRender(writer);

            newItemPanel.RenderControl(writer);

            if (this.IsMultiContentTypeList)
            {
                writer.Write(
                    "<link href=\"/_layouts/15/Roster.Presentation/js/jquery.contextMenu.css\" type=\"text/css\" rel=\"stylesheet\" />");
                writer.Write(
                    "<script src=\"/_layouts/15/Roster.Presentation/js/jquery.contextMenu.js\" type=\"text/javascript\"></script>");
                writer.Write("<script type=\"text/javascript\">");
                writer.Write(
                    string.Format(
                        " jQuery.contextMenu({{selector: '.ms-heroCommandLink.{0}', trigger: 'left', items: {1}}})",
                        this.List.Id.ToString("N"), this.GetMenuJsScript()));
                writer.Write("</script>");
            }
        }

        private static string BuidUri(string baseUrl, IDictionary<string, string> query)
        {
            var queryString = string.Empty;
            if (baseUrl.Contains("?"))
            {
                var urlSplit = baseUrl.Split('?');
                baseUrl = urlSplit[0];
                queryString = urlSplit.Length > 1 ? urlSplit[1] : string.Empty;
            }
            var queryParams = HttpUtility.ParseQueryString(queryString);
            var newParams = query.Where(param => !queryParams.AllKeys.Contains(param.Key)).ToList();
            foreach (var param in newParams)
            {
                queryParams[param.Key] = param.Value;
            }
            return new Uri(string.Format("{0}?{1}", baseUrl, queryParams), UriKind.Relative).ToString();
        }

        private string GetMenuJsScript()
        {
            var webUrl = SPContext.Current.Web.ServerRelativeUrl.TrimEnd('/');
            var listId = SPEncode.UrlEncode(this.List.Id.ToString("B").ToUpper());
            
            const string menuItemFormat = "\"{0}\": {{name: \"{0}\", callback: function(key, opt){{ SP.UI.ModalDialog.ShowPopupDialog('{1}'); }}}}";
            var menuItems = VisibleContentTypes.Select(item => string.Format(menuItemFormat,
                item.Name, BuidUri(string.Format(URL_FORMAT, webUrl, item.NewItemUrl.TrimStart('/'), listId), UrlParamsFromParent))).ToList();
            return string.Format("{{{0}}}", string.Join(",", menuItems));
        }
    }
}
