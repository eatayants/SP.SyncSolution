using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;
using Roster.Model.DataContext;
using Roster.Presentation.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using Roster.Presentation.Extensions;
using Roster.Presentation.Controls.Fields;
using Roster.BL.Extentions;
using System.Web.Script.Serialization;
using Roster.Common;

namespace Roster.Presentation.Controls
{
    public class SPGridView2 : SPGridView
    {
        public SPGridView2()
        {
            this.FilteredDataSourcePropertyName = "FilterExpression";
            this.FilteredDataSourcePropertyFormat = "{1} = '{0}'";
            this.AutoGenerateColumns = false;
            this.EmptyDataText = "There are no items to show in this view";
            this.HeaderStyle.HorizontalAlign = HorizontalAlign.Left;
        }

        private ViewMetadata view;
        private ObjectDataSource _gridDS;
        private string[] _filterSeparator = { "AND" };
        private string _eventArgument;
        public ObjectDataSource GridDataSource
        {
            get { return _gridDS; }
            private set
            {
                _gridDS = value;
                this.DataSourceID = _gridDS.ID;
            }
        }
        public QueryParams GridFilterExpression { get; set; }
 
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            // set CUSTOM callback function
            this.Attributes["callbackEventReference"] = this.Attributes["callbackEventReference"].Replace("SPGridView_FilterCallbackHandler", "SPGridView_FilterCallbackHandler_Custom"); 
        }

        protected override void RaiseCallbackEvent(string eventArgument)
        {
            this._eventArgument = eventArgument;
        }
        protected override string GetCallbackResult()
        {
            return this.InvokeCallbackEvent2(this._eventArgument);
        }
        protected string InvokeCallbackEvent2(string eventArgument)
        {
            string valueAfterPrefix = Utils.GetValueAfterPrefix(eventArgument, "__SPGridView__;");
            if (valueAfterPrefix == null) {
                base.RaiseCallbackEvent(eventArgument);
                return base.GetCallbackResult();
            }

            string callbackFilterDataFieldName = valueAfterPrefix;
            if (string.IsNullOrEmpty(callbackFilterDataFieldName)) {
                return "SPGridView's RaiseCallbackEvent method was passed an empty data field name.";
            }

            StringBuilder builder = new StringBuilder();
            var listField = this.view.ViewMetadataFields.Select(f => f.ListMetadataField).First(fld => fld.InternalName == callbackFilterDataFieldName || fld.Id == callbackFilterDataFieldName.Split('$')[0].ToGuid());
            var listDbField = listField.GetDbField();

            if (listDbField is DbFieldDateTime)
            {
                object[] valsFromFilter = Utils.GetFilterValuesFromQueryString(this.Page.Request.QueryString, listField.InternalName);
                builder.Append((valsFromFilter != null && valsFromFilter.Length > 0) ?
                    valsFromFilter[0] :                             // DISPLAY only selected filter value
                    Constants.Filters.FILTER_BY_SINGLE_DATE_KEY);   // DISPLAY special key to convert it to DatePicker clientside
            }
            else if (listDbField is DbFieldBoolean)
            {
                builder.Append("Yes;No");
            }
            else if (listDbField is DbFieldChoice)
            {
                builder.Append(string.Join(";", listField.FieldValues().ToArray()));
            }
            else
            {
                // check if Lookup/User field
                string realFieldName = callbackFilterDataFieldName;
                if (listDbField is DbFieldLookup) {
                    realFieldName = string.Format("{0}_{1}", (listDbField as DbFieldLookup).ListId, callbackFilterDataFieldName.Split('$')[1]);
                } else if (listDbField is DbFieldUser) {
                    realFieldName = string.Format("{0}_{1}", (listDbField as DbFieldUser).ListId, callbackFilterDataFieldName.Split('$')[1]);
                }

                object[] valsFromFilter = Utils.GetFilterValuesFromQueryString(this.Page.Request.QueryString, callbackFilterDataFieldName);
                if (valsFromFilter != null && valsFromFilter.Length > 0) {
                    builder.Append(string.Join(";", valsFromFilter.Select(v => v.ToString().Replace("%", "%25").Replace(";", "%3b")).ToArray()));
                }

                var filterOptions = new {
                    ServiceName = "DataService.svc/List",
                    ViewId = this.view.Id,
                    FieldId = listField.Id,
                    DisplayField = realFieldName
                };

                if (builder.Length > 0) { builder.Append(";"); }
                builder.Append(Constants.Filters.VIEW_ALL_FILTERS_KEY + ":" + new JavaScriptSerializer().Serialize(filterOptions));
            }

            return builder.ToString();
        }
        
        public ObjectDataSource SetObjectDataSource(string dataSourceId, string selectMethod, string selectCountMethod, SPGridView2DataSource dataSource, ViewMetadata view)
        {
            this.view = view;

            ObjectDataSource gridDS = new ObjectDataSource();
            gridDS.ID = dataSourceId;
            gridDS.SelectMethod = selectMethod;
            gridDS.TypeName = dataSource.GetType().AssemblyQualifiedName;
            gridDS.EnableViewState = false;
            gridDS.SelectCountMethod = selectCountMethod;
            gridDS.MaximumRowsParameterName = "iMaximumRows";
            gridDS.StartRowIndexParameterName = "iBeginRowIndex";
            gridDS.SortParameterName = "SortExpression";
            gridDS.EnablePaging = true;
            gridDS.ObjectCreating += gridDS_ObjectCreating;
            this.GridDataSource = gridDS;
 
            return gridDS;
        }

        void gridDS_ObjectCreating(object sender, ObjectDataSourceEventArgs e)
        {
            //SPGridView2DataSource viewDS = new SPGridView2DataSource(this.view, this.GridDataSource.FilterExpression);
            SPGridView2DataSource viewDS = new SPGridView2DataSource(this.view, this.GridFilterExpression);
            e.ObjectInstance = viewDS;
        }
    }
}
