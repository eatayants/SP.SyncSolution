using System;
using System.Web.UI.HtmlControls;

namespace Roster.Presentation.Controls
{
    public class WebPartConfigException : Exception
    {
        public WebPartConfigException()
        {
        }

        public WebPartConfigException(string message)
            : base(message)
        {
        }

        public WebPartConfigException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public HtmlAnchor GetHtmlControl(string wpId)
        {
            var __link = new HtmlAnchor() { InnerText = this.Message };
            __link.Attributes.Add("onclick", string.Format("javascript:EnsureScriptFunc('browserScript', 'MSOTlPn_ShowToolPane2', function() {{ MSOTlPn_ShowToolPane2('Edit','{0}'); }}); return false;", wpId));

            return __link;
        }
    }
}