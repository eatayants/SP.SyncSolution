using System;
using System.Linq;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Microsoft.SharePoint.ApplicationPages;
using System.Web;
using System.Globalization;
using Microsoft.SharePoint.Utilities;
using System.IO;
using System.Text;
using Roster.Presentation.Helpers;

namespace Roster.Presentation.Layouts
{
    public partial class InplViewAdv : InplaceViewEditor
    {
        #region Private properties

        private bool? m_bIsCSR;
        private bool? m_bIsRibbon;
        private SPList m_list;
        private string m_strCmd;
        private string m_strPage;
        private string m_strViewCount;
        private SPView m_view;
        private string m_viewId;

        #endregion

        protected override void OnLoad(EventArgs e)
        {
            if (this.IsCSR && !this.IsRibbon)
            {
                this.Context.Response.ContentType = "application/json";
                HttpContext current = HttpContext.Current;
                if (((current != null) && (current.Request != null)) && !current.Request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    throw new UnauthorizedAccessException(SPResource.GetString(CultureInfo.CurrentCulture, "JsonOnGetNotAllowed", new object[0]));
                }
            }

            bool flag = true;
            try
            {
                flag = this.Execute(this.Cmd);
            }
            catch (Exception ex)
            {
                //string str = string.Format(CultureInfo.InvariantCulture, "<div class=\"{0}\">{1}</div>", new string[] { "ms-vb", SPResource.GetString("CallbackDataBadJSONText", new object[0]) }) + "<br/><br/>" + SPHttpUtility.HtmlEncode(SPResource.GetString("ErrorPageRequestGuid", new object[0])) + "<br/>";
                string str = string.Format(CultureInfo.InvariantCulture, "<div class=\"{0}\">{1}</div>", new string[] { "ms-vb", ex.Message }) + "<br/><br/>StackTrace: " + ex.StackTrace + "<br/>";
                base.Response.StatusCode = 0x259;
                base.Response.Write(str);
            }

            if (flag)
            {
                base.Response.Flush();
                base.Response.End();
            }
        }

        protected override void OnInitComplete(EventArgs e)
        {
            //base.OnInitComplete(e);
            if (!string.IsNullOrEmpty(this.Context.Request.QueryString["List"]))
            {
                base.PageTargetSecurableObject = this.List;
            }
            base.CheckRights();
        }

        #region Private methods

        private bool RenderListData(SPView view, TextWriter output)
        {
            if (view == null) { return false; }

            FieldFilterOperatorsLayer fol = new FieldFilterOperatorsLayer(base.Web);
            CamlFiltersLayer fl = new CamlFiltersLayer(this.List, HttpContext.Current.Request.QueryString, fol);
            SPListItemCollection navigator = this.List.GetItems(new SPQuery() {
                Query = fl.GetQueryByFilters(),
                ViewFields = CamlFiltersLayer.GetViewFieldsFromView(this.View),
                RowLimit = this.View.RowLimit
            });

            StringBuilder newFilterLink = new StringBuilder();
            foreach (string fKey in HttpContext.Current.Request.QueryString.AllKeys.Where(x => x.StartsWith("Filter") || x.StartsWith("Sort")))
                newFilterLink.AppendFormat("{0}={1}&", fKey, HttpContext.Current.Request.QueryString[fKey]);

            output.Write("{ \"Row\" : \n");
            if ((navigator == null) || (navigator != null && navigator.Count == 0))
            {
                output.Write("[]");
                if (!string.IsNullOrEmpty(newFilterLink.ToString()))
                {
                    output.Write(",\"FilterLink\" : \"");
                    output.Write(JsonTextWriter.EscapeJScriptString("?" + newFilterLink.ToString()));
                    output.Write("\"");
                }
            }
            else
            {
                int num8;
                int num9;
                output.Write("[");

                bool isFirstElem = true;
                string lastField = this.View.ViewFields.Cast<string>().Last();
                SPFieldCollection allFields = this.List.Fields;
                foreach (SPListItem itm in navigator)
                {
                    output.WriteLine(isFirstElem ? "{" : ",{");

                    // general fields
                    output.WriteLine("\"{0}\": \"{1}\",", "ID", JsonTextWriter.EscapeJScriptString(itm.ID.ToString()));
                    try {
                        output.WriteLine("\"{0}\": \"{1}\",", "Title", JsonTextWriter.EscapeJScriptString(itm.Title ?? string.Empty));
                    } catch (Exception ex) {
                        output.WriteLine("\"{0}\": \"{1}\",", "Title", JsonTextWriter.EscapeJScriptString(ex.Message));
                    }
                    try {
                        output.WriteLine("\"{0}\": \"{1}\",", "ContentTypeId", JsonTextWriter.EscapeJScriptString(itm["ContentTypeId"].ToString()));
                    } catch (Exception ex) {
                        output.WriteLine("\"{0}\": \"{1}\",", "ContentTypeId", JsonTextWriter.EscapeJScriptString(ex.Message));
                    }
                    try {
                        output.WriteLine("\"{0}\": \"{1}\",", "PermMask", JsonTextWriter.EscapeJScriptString((string)itm["PermMask"] ?? string.Empty));
                    } catch (Exception ex) {
                        output.WriteLine("\"{0}\": \"{1}\",", "PermMask", JsonTextWriter.EscapeJScriptString(ex.Message));
                    }
                    try {
                        output.WriteLine("\"{0}\": \"{1}\",", "FSObjType", JsonTextWriter.EscapeJScriptString((string)itm["FSObjType"] ?? string.Empty));
                    } catch (Exception ex) {
                        output.WriteLine("\"{0}\": \"{1}\",", "FSObjType", JsonTextWriter.EscapeJScriptString(ex.Message));
                    }
                    try {
                        output.WriteLine("\"{0}\": \"{1}\",", "FileLeafRef", JsonTextWriter.EscapeJScriptString((string)itm["FileLeafRef"] ?? string.Empty));
                    } catch (Exception ex) {
                        output.WriteLine("\"{0}\": \"{1}\",", "FileLeafRef", JsonTextWriter.EscapeJScriptString(ex.Message));
                    }
                    try {
                        output.WriteLine("\"{0}\": \"{1}\",", "FileRef", JsonTextWriter.EscapeJScriptString((string)itm["FileRef"] ?? string.Empty));
                    } catch (Exception ex) {
                        output.WriteLine("\"{0}\": \"{1}\",", "FileRef", JsonTextWriter.EscapeJScriptString(ex.Message));
                    }
                    try {
                        output.WriteLine("\"{0}\": \"{1}\",", "File_x0020_Type", JsonTextWriter.EscapeJScriptString((string)itm["File_x0020_Type"] ?? string.Empty));
                    } catch (Exception ex) {
                        output.WriteLine("\"{0}\": \"{1}\",", "File_x0020_Type", JsonTextWriter.EscapeJScriptString(ex.Message));
                    }

                    // view fields
                    string valAsJson = string.Empty;
                    foreach (string fld in this.View.ViewFields)
                    {
                        valAsJson = "";
                        try
                        {
                            valAsJson = allFields.GetFieldByInternalName(fld).RenderFieldValueAsJson(itm[fld]);

                            // !!! not all SPFields override RenderFieldValueAsJson method
                            if (valAsJson == null)
                                valAsJson = (itm[fld] == null) ? "\"\"" : "\"" + JsonTextWriter.EscapeJScriptString(itm[fld].ToString()) + "\"";
                        }
                        catch (Exception ex)
                        {
                            valAsJson = "Field: " + fld + ". Error: " + ex.Message;
                        }

                        output.WriteLine("\"{0}\": {1}{2}", JsonTextWriter.EscapeJScriptString(fld), valAsJson, fld != lastField ? "," : string.Empty);
                    }

                    output.WriteLine("}");
                    isFirstElem = false;
                }

                output.Write("],");

                // FirstRow
                num8 = Convert.ToInt32(this.Context.Request.QueryString["FirstRow"], CultureInfo.InvariantCulture);
                if (num8 < 1)
                {
                    num8 = 1;
                }
                output.WriteLine("\"FirstRow\" : {0},", num8);

                // LastRow
                num9 = Math.Min((num8 + ((int)view.RowLimit)) - 1, navigator.Count); // xslWebPart.GridDataSerializer.ExplicitRowFilterCount.Value
                output.WriteLine("\"LastRow\" : {0}", num9);

                StringBuilder builder = new StringBuilder();
                StringBuilder sb = new StringBuilder();

                // add FilterLink
                if (HttpContext.Current.Request.QueryString.HasKeys())
                {
                    output.Write(",\"FilterLink\" : \"");
                    output.Write(JsonTextWriter.EscapeJScriptString("?" + newFilterLink.ToString()));
                    output.Write("\"\n");
                }
                // add SortField
                if (HttpContext.Current.Request.QueryString.AllKeys.Contains("SortField"))
                {
                    output.Write(",\"SortField\" : \"");
                    output.Write(JsonTextWriter.EscapeJScriptString(HttpContext.Current.Request.QueryString["SortField"]));
                    output.Write("\"\n");
                }

                // add FilterFields
                var filterFieldsKeys = HttpContext.Current.Request.QueryString.AllKeys.Where(x => x.StartsWith("FilterField"));
                var fValAr = filterFieldsKeys.Select(x => HttpContext.Current.Request.QueryString[x]);
                var fVal = String.Join(";", fValAr);
                if (!string.IsNullOrEmpty(fVal))
                {
                    output.Write(",\"FilterFields\" : \"");
                    output.Write(JsonTextWriter.EscapeJScriptString(";" + fVal + ";"));
                    output.Write("\"\n");
                }

                // add SortDir
                if (HttpContext.Current.Request.QueryString.AllKeys.Contains("SortDir"))
                {
                    output.Write(",\"SortDir\" : \"");
                    output.Write(JsonTextWriter.EscapeJScriptString(HttpContext.Current.Request.QueryString["SortDir"] == "Asc" ? "ascending" : "descending"));
                    output.Write("\"\n");
                }

                // Hierarchy
                output.Write(",\"ForceNoHierarchy\" : \"1\"\n");
                output.Write(",\"HierarchyHasIndention\" : \"\"\n");
            }
            output.Write("\n}");

            return true;
        }

        private bool Execute(string strCmd)
        {
            if (!string.IsNullOrEmpty(this.ListViewPageUrl))
            {
                //base.Web.Request.SetVar(base.Web.Url, "ListViewPageUrl", this.ListViewPageUrl);
            }
            if (!string.IsNullOrEmpty(this.ViewCount))
            {
                //base.Web.Request.SetVar(base.Web.Url, "ViewCount", this.ViewCount);
            }

            //base.Web.Request.SetVar(base.Web.Url, "MasterVersion", base.Web.UIVersion.ToString(CultureInfo.InvariantCulture));
            base.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            string str = this.Context.Request.Form["InplRootFolder"];
            if (str != null)
            {
                //base.Web.Request.SetVar(base.Web.Url, "RootFolder", str);
            }

            this.RenderListData(this.View, base.Response.Output);

            return true;
        }

        protected override SPBasePermissions RightsRequired
        {
            get
            {
                return Microsoft.SharePoint.SPBasePermissions.ViewListItems;
            }
        }

        protected override bool AllowAnonymousAccess
        {
            get
            {
                return true;
            }
        }

        private SPView View
        {
            get
            {
                if (this.m_view == null)
                {
                    string[] values = this.Context.Request.QueryString.GetValues("View");
                    if ((values != null) && (values.Length > 0))
                    {
                        Guid guid = new Guid(values[0]);
                        if (guid != Guid.Empty)
                        {
                            this.m_view = this.List.Views[guid];
                        }
                        else
                        {
                            this.m_viewId = Guid.Empty.ToString("B");
                        }
                    }
                    if (this.m_view == null)
                    {
                        string str = this.Context.Request.QueryString["ListViewPageUrl"];
                        if (string.IsNullOrEmpty(str))
                        {
                            this.m_view = this.List.DefaultView;
                        }
                        else
                        {
                            str = SPHttpUtility.UrlKeyValueDecode(str);
                            if (str.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
                            {
                                str = str.Substring(7);
                            }
                            else if (str.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
                            {
                                str = str.Substring(8);
                            }
                            str = str.Substring(str.IndexOf('/'));
                            foreach (SPView view in this.List.Views)
                            {
                                if (view.ServerRelativeUrl == str)
                                {
                                    this.m_view = view;
                                    break;
                                }
                            }
                            if (this.m_view == null)
                            {
                                this.m_view = this.List.DefaultView;
                            }
                        }
                    }
                    string str2 = this.Context.Request.QueryString["IsXslView"];
                    //this.m_view.set_IsXslView(str2 == "TRUE");
                    string str3 = this.Context.Request.QueryString["IsGroupRender"];
                    //this.m_view.set_IsGroupRender(str3 == "TRUE");
                    string str4 = this.Context.Request.QueryString["HasOverrideSelectCommand"];
                    string str5 = this.Context.Request.Form["OverrideSelectCommand"];
                    if ((str4 == "TRUE") && !string.IsNullOrEmpty(str5))
                    {
                        //this.m_view.set_OverrideSelectCommand(str5);
                    }
                    string str6 = this.Context.Request.QueryString["OverrideScope"];
                    if (!string.IsNullOrEmpty(str6))
                    {
                        SPViewScope scope = ConvertStringToViewScope(str6);
                        this.m_view.Scope = scope;
                    }
                }
                return this.m_view;
            }
        }

        private static SPViewScope ConvertStringToViewScope(string str)
        {
            switch (str)
            {
                case "Recursive":
                    return SPViewScope.Recursive;

                case "RecursiveAll":
                    return SPViewScope.RecursiveAll;

                case "FilesOnly":
                    return SPViewScope.FilesOnly;
            }
            return SPViewScope.Default;
        }

        private bool IsCSR
        {
            get
            {
                if (!this.m_bIsCSR.HasValue)
                {
                    if (this.Context.Request.QueryString["IsCSR"] == "TRUE")
                    {
                        this.m_bIsCSR = true;
                    }
                    else
                    {
                        this.m_bIsCSR = false;
                    }
                }
                return this.m_bIsCSR.Value;
            }
        }

        private bool IsRibbon
        {
            get
            {
                if (!this.m_bIsRibbon.HasValue)
                {
                    if (this.Context.Request.QueryString["IsRibbon"] == "TRUE")
                    {
                        this.m_bIsRibbon = true;
                    }
                    else
                    {
                        this.m_bIsRibbon = false;
                    }
                }
                return this.m_bIsRibbon.Value;
            }
        }

        private SPList List
        {
            get
            {
                if (this.m_list == null)
                {
                    string g = this.Context.Request.QueryString["List"];
                    Guid guid = new Guid(g);
                    this.m_list = base.Web.Lists[guid]; //.GetListById(guid, true);
                }
                return this.m_list;
            }
        }

        private string Cmd
        {
            get
            {
                if (string.IsNullOrEmpty(this.m_strCmd))
                {
                    this.m_strCmd = this.Context.Request.QueryString["Cmd"];
                }
                return this.m_strCmd;
            }
        }

        private string ListViewPageUrl
        {
            get
            {
                if (string.IsNullOrEmpty(this.m_strPage))
                {
                    this.m_strPage = this.Context.Request.QueryString["ListViewPageUrl"];
                }
                return this.m_strPage;
            }
        }

        private string ViewCount
        {
            get
            {
                if (string.IsNullOrEmpty(this.m_strViewCount))
                {
                    this.m_strViewCount = this.Context.Request.QueryString["ViewCount"];
                }
                return this.m_strViewCount;
            }
        }

        #endregion
    }
}
