using CamlexNET;
using CamlexNET.Interfaces;
using Microsoft.SharePoint;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Roster.Common;

namespace Roster.Presentation.Helpers
{
    public class CamlFiltersLayer
    {
        private NameValueCollection queryString = null;
        private FieldFilterOperatorsLayer operatorsLayer = null;
        private SPList list = null;

        public CamlFiltersLayer(SPList list, NameValueCollection queryString, FieldFilterOperatorsLayer operatorsLayer)
        {
            this.queryString = queryString;
            this.operatorsLayer = operatorsLayer;
            this.list = list;
        }

        public string GetQueryByFilters()
        {
            Dictionary<string, FieldFilterOperator> operators = operatorsLayer.GetOperatorsDict();

            var conditions = new List<Expression<Func<SPListItem, bool>>>();
            var allKeys = this.queryString.Keys.Cast<string>();
            foreach (string fKey in allKeys.Where(x => x.StartsWith("FilterField")))
            {
                // get field info
                string fldIntName = queryString[fKey];
                SPField fld = this.list.Fields.GetFieldByInternalName(fldIntName);
                string fldFilterVal = System.Web.HttpUtility.UrlDecode(queryString[fKey.Replace("Field", "Value")]);
                bool isLookupIdFilter = false;
                if (allKeys.Contains(fKey.Replace("Field", "LookupId"))) {
                    isLookupIdFilter = (System.Web.HttpUtility.UrlDecode(queryString[fKey.Replace("Field", "LookupId")]) == "1"); // FilterLookupId{0}=1
                }
                bool isMultiValueFilter = fKey.Contains("s");
                bool isFieldOperExists = operators.ContainsKey(fldIntName) && (operators[fldIntName] != FieldFilterOperator.None);
                bool isFieldHasNOToperator = isFieldOperExists && (operators[fldIntName] == FieldFilterOperator.NOT);

                if (string.IsNullOrEmpty(fldFilterVal)) {
                    // empty filter value
                    if (isFieldHasNOToperator)
                        conditions.Add(x => x[fldIntName] != null); // IsNotNull
                    else
                        conditions.Add(x => x[fldIntName] == null); // IsNull
                } else if (isLookupIdFilter) {
                    // LookupId Filter
                    string[] values = fldFilterVal.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    if (!isFieldOperExists || (operators[fldIntName] == FieldFilterOperator.OR)) {
                        // standart IN operator: a OR b OR c OR d
                        conditions.Add(CamlQueryHealper.GetOperation_OrEqLookupId(fld, values)); // In operator
                    } else if (isFieldHasNOToperator) {
                        // sample: (NOT a) AND (NOT b)
                        conditions.Add(CamlQueryHealper.GetOperation_AndNeqLookupId(fld, values));
                    } else {
                        // sample: a AND b AND c
                        conditions.Add(CamlQueryHealper.GetOperation_AndEqLookupId(fld, values));
                    }
                } else if (!isMultiValueFilter) {
                    // single-value Filter
                    if (isFieldHasNOToperator)
                        conditions.Add(CamlQueryHealper.GetOperation_Neq(fld, fldFilterVal)); // Neq
                    else
                        conditions.Add(CamlQueryHealper.GetOperation_Eq(fld, fldFilterVal)); // Eq
                } else {
                    // multi-value filter
                    string[] values = fldFilterVal.Split(new string[] { ";#" }, StringSplitOptions.RemoveEmptyEntries);
                    if (!isFieldOperExists || (operators[fldIntName] == FieldFilterOperator.OR)) {
                        // standart IN operator: a OR b OR c OR d
                        conditions.Add(CamlQueryHealper.GetOperation_In(fld, values)); // In operator
                    } else if (isFieldHasNOToperator) {
                        // sample: (NOT a) AND (NOT b)
                        conditions.Add(CamlQueryHealper.GetOperation_AndNeq(fld, values));
                    } else {
                        // sample: a AND b AND c
                        conditions.Add(CamlQueryHealper.GetOperation_AndEq(fld, values));
                    }
                }
            }

            IQuery query = Camlex.Query();
            if (this.queryString.Keys.Cast<string>().Contains("SortField")) {
                if (this.queryString["SortDir"] == "Desc")
                    query.OrderBy(x => x[this.queryString["SortField"]] as Camlex.Desc);
                else
                    query.OrderBy(x => x[this.queryString["SortField"]] as Camlex.Asc);
            }

            if (conditions.Count > 0)
                query.WhereAll(conditions);

            return query.ToString();
        }

        public static string GetViewFieldsFromView(SPView view)
        {
            // common fields
            string[] commonFields = new string[] { "ContentTypeId", "PermMask", "FSObjType", "FileLeafRef", "FileRef", "File_x0020_Type" };

            if (view != null)
            {
                var meargedData = view.ViewFields.Cast<string>().Concat(commonFields).Distinct();
                return Camlex.Query().ViewFields(meargedData);
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
