using Microsoft.SharePoint;
using Microsoft.SharePoint.WebPartPages;
using System;
using System.Linq;
using System.Collections.Generic;
using Roster.Presentation.WebParts.AvailabilityButtonWebPart;
using System.Web.UI.WebControls.WebParts;

namespace Roster.Presentation.Helpers
{
    public enum FieldFilterOperator
    {
        NOT,
        OR,
        AND,
        None
    }

    public class FieldFilterOperatorsLayer
    {
        private string operatorsAsString = string.Empty;

        public FieldFilterOperatorsLayer(string operatorsAsString)
        {
            this.operatorsAsString = operatorsAsString;
        }

        public FieldFilterOperatorsLayer(SPWeb web)
        {
            this.Initialization(web);
        }

        private void Initialization(SPWeb web)
        {
            object cachedValue = System.Web.HttpContext.Current.Cache["FilterConditionsKey"];
            if (cachedValue == null)
            {
                SPLimitedWebPartManager webPartManager = null;

                try
                {
                    //string wpPageUrl = "workerfilterpageurl"; //web.Properties[Constants.BAG_KEY_FILTER_PAGE_URL];
                    SPFile page = web.GetFile(System.Web.HttpContext.Current.Request.UrlReferrer.AbsolutePath);
                    webPartManager = page.GetLimitedWebPartManager(PersonalizationScope.Shared);
                    foreach (System.Web.UI.WebControls.WebParts.WebPart wp in webPartManager.WebParts)
                    {
                        AvailabilityButtonWebPart abwp = wp as AvailabilityButtonWebPart;
                        if (abwp != null) {
                            this.operatorsAsString = abwp.FilterConditions;
                            break;
                        }
                    }
                }
                catch { }
                finally
                {
                    if (webPartManager != null)
                        webPartManager.Web.Dispose();
                }
            }
            else
            {
                // get Value from ASP.NET Cache
                this.operatorsAsString = cachedValue.ToString();
            }
        }

        public Dictionary<string, FieldFilterOperator> GetOperatorsDict()
        {
            Dictionary<string, FieldFilterOperator> result = new Dictionary<string, FieldFilterOperator>();

            if (!string.IsNullOrEmpty(this.operatorsAsString))
            {
                string[] ops = this.operatorsAsString.Split(new string[] { ";#" }, StringSplitOptions.None);
                for (int i = 0; i < ops.Length; i += 2) {
                    if ((i + 1) < ops.Length) {
                        result.Add(ops[i], GetOperatorFromString(ops[i + 1].ToLower()));
                    }
                }
            }

            return result;
        }

        public static string GetOperatorsAsString(Dictionary<string, FieldFilterOperator> opers)
        {
            return opers == null ? string.Empty :
                string.Join(";#", opers.Select(o => string.Format("{0};#{1}", o.Key, GetStringFromOperator(o.Value))));
        }

        private static FieldFilterOperator GetOperatorFromString(string oper)
        {
            switch (oper)
            {
                case "or":
                    return FieldFilterOperator.OR;
                case "and":
                    return FieldFilterOperator.AND;
                case "not":
                    return FieldFilterOperator.NOT;
                default:
                    return FieldFilterOperator.OR;  // default value
            }
        }
        private static string GetStringFromOperator(FieldFilterOperator oper)
        {
            switch (oper)
            {
                case FieldFilterOperator.OR:
                    return "or";
                case FieldFilterOperator.AND:
                    return "and";
                case FieldFilterOperator.NOT:
                    return "not";
                default:
                    return "or";  // default value
            }
        }
    }
}
