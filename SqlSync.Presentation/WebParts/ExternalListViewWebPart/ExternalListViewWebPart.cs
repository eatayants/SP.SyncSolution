using System;
using System.Linq;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Roster.Presentation.Controls.ToolParts;
using Microsoft.SharePoint.WebPartPages;
using System.Collections.Generic;
using Roster.Presentation.Helpers;
using Roster.Presentation.Controls.Interfaces;
using Roster.Common;

namespace Roster.Presentation.WebParts.ExternalListViewWebPart
{
    [ToolboxItemAttribute(false)]
    public class ExternalListViewWebPart : Microsoft.SharePoint.WebPartPages.WebPart
    {
        // Visual Studio might automatically update this path when you change the Visual Web Part project item.
        private const string _ascxPath = @"~/_CONTROLTEMPLATES/15/Roster.Presentation.WebParts/ExternalListViewWebPart/ExternalListViewWebPartUserControl.ascx";
        private string _viewName = string.Empty;
        private string _catalogId = string.Empty;

        public string ViewName
        {
            get
            {
                return _viewName;
            }
            set
            {
                _viewName = value;
            }
        }
        public string CatalogId
        {
            get
            {
                return _catalogId;
            }
            set
            {
                _catalogId = value;
            }
        }
        public string RelationField { get; set; }
        public string FilterOperators { get; set; }
        public string CalendarHeight { get; set; }
        public string CalendarWidth { get; set; }
        public bool EditMode { get; set; }
        public bool DisplayPrepopulated { get; set; }

        protected override void CreateChildControls()
        {
            ExternalListViewWebPartUserControl control = Page.LoadControl(_ascxPath) as ExternalListViewWebPartUserControl;
            control.ViewId = this.ViewName;
            control.RelationField = this.RelationField;
            control.FilterOperators = new FieldFilterOperatorsLayer(this.FilterOperators).GetOperatorsDict();
            int _width = 0;
            if (Int32.TryParse(this.CalendarWidth, out _width)) {
                control.CalendarWidth = _width;
            }
            int _height = 0;
            if (Int32.TryParse(this.CalendarHeight, out _height)) {
                control.CalendarHeight = _height;
            }
            control.ConnectionFilterExpression = (string)ViewState["FilterFromConnectedWebPart"] ?? string.Empty;
            control.StatusIDsForFiltering = (ViewState["FilterFromStatusWebPart"] == null) ?
                new int[0] : ((string)ViewState["FilterFromStatusWebPart"]).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.ToInt()).ToArray();
            control.ParentWebPartId = this.ID;
            control.EditMode = this.EditMode;
            control.DisplayPrepopulatedRosters = this.DisplayPrepopulated;

            Controls.Add(control);
        }

        public override Microsoft.SharePoint.WebPartPages.ToolPart[] GetToolParts()
        {
            Microsoft.SharePoint.WebPartPages.ToolPart[] allToolParts = new Microsoft.SharePoint.WebPartPages.ToolPart[3];

            //Toolpart for Default properties for every webpart
            Microsoft.SharePoint.WebPartPages.WebPartToolPart standardToolPart = new Microsoft.SharePoint.WebPartPages.WebPartToolPart();
            //Toolpart for Custom properties for every webpart
            Microsoft.SharePoint.WebPartPages.CustomPropertyToolPart customToolParts = new Microsoft.SharePoint.WebPartPages.CustomPropertyToolPart();
            allToolParts[0] = standardToolPart;
            allToolParts[1] = customToolParts;
            //our custom toolpart
            allToolParts[2] = new DbListViewToolPart();

            return allToolParts;
        }

        [ConnectionConsumer("Master-Detail Consumer", "ParentConsumer")]
        public void QueryStringConsumer(ITransformableFilterValues Provider)
        {
            if (Provider != null && Provider.ParameterValues != null && Provider.ParameterValues.Count > 0 && !Provider.ParameterValues[0].Equals("0"))
                ViewState["FilterFromConnectedWebPart"] = string.Format("{1} = '{0}'", Provider.ParameterValues[0], Provider.ParameterName);
        }

        [ConnectionConsumer("Status Consumer", "StatusConsumer")]
        public void StatusIDsConsumer(IStatusFilter Provider)
        {
            if (Provider != null && !string.IsNullOrEmpty(Provider.StatusIDs)) {
                ViewState["FilterFromStatusWebPart"] = Provider.StatusIDs;
            }
        }
    }
}
