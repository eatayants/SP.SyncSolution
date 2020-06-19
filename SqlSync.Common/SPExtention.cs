using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using CamlexNET;
using CamlexNET.Interfaces;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using System.Globalization;

namespace Roster.Common
{
    // ReSharper disable once InconsistentNaming
	public static class SPExtention
	{
        public static ConnectionInfo GetConnection(this SPContext context)
        {
            var web = context.Web.IsRootWeb ? context.Web : context.Web.Site.RootWeb;
            return new ConnectionInfo
            {
                Server = web.Properties["Server"].ToSafeString(),
                Database = web.Properties["Database"].ToSafeString(),
                User = web.Properties["User"].ToSafeString(),
                Password = web.Properties["Password"].ToSafeString()
            };
        }
        public static ConnectionInfo GetConnection(this SPWeb web)
        {
            var rootWeb = web.IsRootWeb ? web : web.Site.RootWeb;
            return new ConnectionInfo {
                Server = rootWeb.Properties["Server"].ToSafeString(),
                Database = rootWeb.Properties["Database"].ToSafeString(),
                User = rootWeb.Properties["User"].ToSafeString(),
                Password = rootWeb.Properties["Password"].ToSafeString()
            };
        }

        public static void SetConnection(this SPContext context, ConnectionInfo connectionInfo)
        {
            var web = context.Web.IsRootWeb ? context.Web : context.Web.Site.RootWeb;
            web.Properties["Server"] = connectionInfo.Server;
            web.Properties["Database"] = connectionInfo.Database;
            web.Properties["User"] = connectionInfo.User;
            web.Properties["Password"] = connectionInfo.Password;
            web.Properties.Update();
        }

        public static NavConnectionInfo GetNavWebServiceConnection(this SPContext context)
        {
            var web = context.Web.IsRootWeb ? context.Web : context.Web.Site.RootWeb;
            return new NavConnectionInfo
            {
                CreateTimesheetUrl = web.Properties["NavCreateTimesheetUrl"].ToSafeString(),
                ProcessTimesheetsUrl = web.Properties["NavProcessTimesheetsUrl"].ToSafeString(),
                User = web.Properties["NavUser"].ToSafeString(),
                Password = web.Properties["NavPassword"].ToSafeString(),
                Mapping = web.Properties["webservicemapping"].ToSafeString()
            };
        }
        public static void SetNavWebServiceConnection(this SPContext context, NavConnectionInfo connectionInfo)
        {
            var web = context.Web.IsRootWeb ? context.Web : context.Web.Site.RootWeb;
            web.Properties["NavCreateTimesheetUrl"] = connectionInfo.CreateTimesheetUrl;
            web.Properties["NavProcessTimesheetsUrl"] = connectionInfo.ProcessTimesheetsUrl;
            web.Properties["NavUser"] = connectionInfo.User;
            web.Properties["NavPassword"] = connectionInfo.Password;
            web.Properties["webservicemapping"] = connectionInfo.Mapping;
            web.Properties.Update();
        }

        public static DateOptions GetDateOptions(this SPWeb web, System.Web.HttpRequest request)
        {
            SPRegionalSettings regSet = web.RegionalSettings;
            return new DateOptions(regSet.LocaleId.ToString(CultureInfo.InvariantCulture), (SPCalendarType)regSet.CalendarType,
                BitsToString(regSet.WorkDays, 7), regSet.FirstDayOfWeek.ToString(), regSet.AdjustHijriDays.ToString(),
                SPUtility.GetTimeSpanFromTimeZone(regSet.TimeZone).ToString(), SPUtility.GetSelectedDate(request, web));
        }
        public static DateOptions GetDateOptions(this SPWeb web, DateTime selectedDate)
        {
            SPRegionalSettings regSet = web.RegionalSettings;
            return new DateOptions(regSet.LocaleId.ToString(CultureInfo.InvariantCulture), (SPCalendarType)regSet.CalendarType,
                BitsToString(regSet.WorkDays, 7), regSet.FirstDayOfWeek.ToString(), regSet.AdjustHijriDays.ToString(),
                SPUtility.GetTimeSpanFromTimeZone(regSet.TimeZone).ToString(), selectedDate.ToString(CultureInfo.InvariantCulture.DateTimeFormat));
        }

        #region GetFieldValue
        public static object GetObjectValue(this SPListItem item, SPField field)
        {
            object result = null;
            switch (field.Type)
            {
                case SPFieldType.Text:
                {
                    result = item.GetStringValue(field);
                    break;
                }
                case SPFieldType.Number:
                case SPFieldType.Currency:
                {
                    result = item.GetNumberValue(field);
                    break;
                }
                case SPFieldType.DateTime:
                {
                    result = item.GetDateTimeValue(field);
                    break;
                }
                case SPFieldType.MultiChoice:
                {
                    var multiChoice = item.GetMultipleChoiceValueCollection(field);
                    if (multiChoice != null)
                    {
                        result = multiChoice.ToString();
                    }
                    break;
                }
                case SPFieldType.Choice:
                {
                    result = item.GetChoiceValue(field);
                    break;
                }
                case SPFieldType.Lookup:
                {
                    var lookupField = (field as SPFieldLookup);
                    if (lookupField != null)
                    {
                        if (lookupField.AllowMultipleValues)
                        {
                            var lookupValue = item.GetLookupValueCollection(field);
                            if (lookupValue != null)
                            {
                                result = lookupField.IsRelationship ? 
                                    String.Join(",", lookupValue.Select(value => value.LookupId)) : 
                                    String.Join(",", lookupValue.Select(value => value.LookupValue));
                             }
                        }
                        else
                        {
                            var lookupValue = item.GetLookupValue(field);
                            if (lookupValue != null)
                            {
                                if (lookupField.IsRelationship)
                                {
                                    result = lookupValue.LookupId;
                                }
                                else
                                {
                                    result = lookupValue.LookupValue;
                                }
                            }
                        }
                    }
                    break;
                }
                case SPFieldType.Boolean:
                {
                    result = item.GetTrueFalseValue(field);
                    break;
                }
                case SPFieldType.User:
                {
                    var userValue = item.GetUserValue(field);
                    if (userValue != null) {
                        result = (userValue.User == null) ? userValue.LookupValue : userValue.User.LoginName;
                    }
                    break;
                }
                case SPFieldType.URL:
                {
                    var url = item.GetUrlValue(field);
                    if (url != null)
                    {
                        result = url.Url;
                    }
                    break;
                }
                default:
                {
                    var defaultValue = item[field.Id].ToString();
                    if (!string.IsNullOrWhiteSpace(defaultValue))
                    {
                        result = defaultValue.Contains(";#") ? String.Join(",", 
                            new SPFieldLookupValueCollection(defaultValue).Select(value => value.LookupId)) : defaultValue;
                    }
                    break;
                }
            }
            return result;
        }

        public static string GetStringValue(this SPListItem item, SPField field)
        {
            return item[field.Id] == null ? null : item[field.Id] as String;
        }

        public static Double? GetNumberValue(this SPListItem item, SPField field)
        {
            return item[field.Id] == null ? null : item[field.Id] as Double?;
        }

        public static Double? GetCurrencyValue(this SPListItem item, SPField field)
        {
            return item[field.Id] == null ? null : item[field.Id] as Double?;
        }
 
        public static DateTime? GetDateTimeValue(this SPListItem item, SPField field)
        {
            return item[field.Id] == null ? null : item[field.Id] as DateTime?;
        }

        public static string GetChoiceValue(this SPListItem item, SPField field)
        {
            return item[field.Id] == null ? null : item[field.Id] as String;
        }

        public static SPFieldMultiChoiceValue GetMultipleChoiceValueCollection(this SPListItem item, SPField field)
        {
            return item[field.Id] == null ? null : (item[field.Id] as SPFieldMultiChoiceValue);
        }

        public static SPFieldLookupValue GetLookupValue(this SPListItem item, SPField field)
        {
            return item[field.Id] == null ? null : new SPFieldLookupValue(item[field.Id].ToString());
        }

        public static SPFieldLookupValueCollection GetLookupValueCollection(this SPListItem item, SPField field)
        {
            return item[field.Id] == null ? null : (item[field.Id] as SPFieldLookupValueCollection);
        }

        public static Boolean? GetTrueFalseValue(this SPListItem item, SPField field)
        {
            return item[field.Id] == null ? null : item[field.Id] as Boolean?;
        }

        public static SPFieldUserValue GetUserValue(this SPListItem item, SPField field)
        {
            if (item[field.Id] == null) return null;
            var userField = item.ParentList.Fields.GetFieldByInternalName(field.InternalName);
            if (userField == null) return null;
            return userField.GetFieldValue(item[field.Id] as string) as SPFieldUserValue;
        }

        public static SPFieldUserValueCollection GetUserValueCollection(this SPListItem item, SPField field)
        {
            return item[field.Id] == null ? null : item[field.Id] as SPFieldUserValueCollection;
        }

        public static SPFieldUrlValue GetUrlValue(this SPListItem item, SPField field)
        {
            return item[field.Id] == null ? null : new SPFieldUrlValue(item[field.Id] as string);
        }

        #endregion GetFieldValue

		public static SPList SPList(this SPContext context, Guid list)
		{
			return context.Web.Lists[list];
		}

        public static SPList FindListByName(this SPContext context, string listName)
        {
            return SPContext.Current.Web.Lists.Cast<SPList>().FirstOrDefault(item => 
                Equals(string.Compare(item.Title, listName, StringComparison.InvariantCultureIgnoreCase), 0));
        }

        public static SPField FieldById(this SPListItem listItem, Guid fieldId)
        {
            return listItem.Fields.Cast<SPField>().FirstOrDefault(field => field.Id == fieldId);
        }

        public static SPField FieldByName(this SPListItem listItem, string fieldName)
        {
            return listItem.Fields.Cast<SPField>().FirstOrDefault(field =>
                String.Equals(field.InternalName, fieldName, StringComparison.InvariantCultureIgnoreCase));
        }

        public static SPField FieldById(this SPList list, Guid fieldId)
        {
            return list.Fields.Cast<SPField>().FirstOrDefault(field => field.Id==fieldId);
        }

        public static string FieldTitle(this SPList list, Guid fieldId)
        {
            if (list == null) return string.Empty;
            var spField = list.FieldById(fieldId);
            return spField == null ? string.Empty : spField.Title;
        }

        public static SPField FieldByName(this SPList list, string fieldName)
        {
            return list.Fields.Cast<SPField>().FirstOrDefault(field => 
                String.Equals(field.InternalName, fieldName, StringComparison.InvariantCultureIgnoreCase));
        }

	    public static List<Tuple<int, ExpandoObject>> ListItemsByCriteria(this SPList list, string key, string fields,
            bool valueAsObject, List<Tuple<string, string>> whereCriteria)
	    {
            var result = new List<Tuple<int, ExpandoObject>>();
            var fldId = key.IsGuid() ? list.FieldById(key.ToGuid()) : list.FieldByName(key);
            IQuery camlQuery = Camlex.Query();
            var conditions = new List<Expression<Func<SPListItem, bool>>>();
	        if (!whereCriteria.IsEmpty())
	        {
                whereCriteria.ForEach(item =>
                {
                    var fieldCriteria = item.Item1.IsGuid() ? list.FieldById(item.Item1.ToGuid()) : list.FieldByName(item.Item1);
                    if (fieldCriteria != null)
                    {
                        conditions.Add(CamlQueryHealper.GetOperation_Eq(fieldCriteria, item.Item2));
                    }
                });
                camlQuery.WhereAll(conditions);
	        }
	        var viewFields = new List<Guid>();
            fields.Split('$').ToList().ForEach(item =>
            {
                var fldTitle = item.IsGuid() ? list.FieldById(item.ToGuid()) : list.FieldByName(item);
                if (fldTitle != null)
                {
                    viewFields.Add(fldTitle.Id);
                }
            });
            
            
            var query = new SPQuery
			{
				ExpandRecurrence = true, DatesInUtc = true,
                ViewFields = Camlex.Query().ViewFields(viewFields),
                Query = camlQuery.ToString()
			};
            var itemCollection = list.GetItems(query);
            for (var i = 0; i < itemCollection.Count; i++)
            {
                var id = itemCollection[i][fldId.InternalName].ToInt();
                var value = (IDictionary<string, object>)new ExpandoObject();
                fields.Split('$').ToList().ForEach(item =>
                {
                    var fldTitle = item.IsGuid() ? list.FieldById(item.ToGuid()) : list.FieldByName(item);
                    var spListItem = itemCollection[i];
                    if (fldTitle != null && spListItem != null)
                    {
                        object spItemValue;
                        try
                        {
                            spItemValue = valueAsObject ? spListItem.GetObjectValue(fldTitle) : fldTitle.GetFieldValueAsText(spListItem[fldTitle.Id]);
                        }
                        catch
                        {
                            spItemValue = string.Empty;
                        }
                        value.Add(item, spItemValue);
                    }
                });
                result.Add(new Tuple<int, ExpandoObject>(id, (ExpandoObject)value));
            }
            return result;
	    }

        public static List<Tuple<int, ExpandoObject>> ListItems(this SPList list, string key, string fields, 
            bool valueAsObject = false, List<int> idList = null)
		{
            var result = new List<Tuple<int, ExpandoObject>>();
            var fldId = key.IsGuid() ? list.FieldById(key.ToGuid()) : list.FieldByName(key);
            var viewFields = new StringBuilder();
            fields.Split('$').ToList().ForEach(item =>
            {
                var fldTitle = item.IsGuid() ? list.FieldById(item.ToGuid()) : list.FieldByName(item);
                if (fldTitle != null)
                {
                    viewFields.AppendFormat("<FieldRef ID='{0}' />", fldTitle.Id.ToString());
                }
            });
            viewFields.AppendFormat("<FieldRef ID='{0}' />", fldId.Id);
			var query = new SPQuery
			{
				ExpandRecurrence = true,
				DatesInUtc = true,
                ViewFields = viewFields.ToString(),
                Query = string.Format("<OrderBy>{0}</OrderBy>", viewFields)
			};
            if (!idList.IsEmpty())
            {
                var whereBuilder = new StringBuilder();
                whereBuilder.AppendLine("<Where>");
                whereBuilder.AppendLine("<In>");
                whereBuilder.Append(string.Format(@"<FieldRef ID='{0}' />", fldId.Title));
                whereBuilder.AppendLine("<Values>");
                idList.ForEach(item =>
                {
                    whereBuilder.AppendFormat("<Value Type='Number'>{0}</Value>", item);
                });
                whereBuilder.AppendLine("</Values>");
                whereBuilder.AppendLine("</In>");
                whereBuilder.AppendLine("</Where>");
                query.Query = string.Format("{0} {1}", whereBuilder, query.Query);

            }
			var itemCollection = list.GetItems(query);
			for (var i = 0; i < itemCollection.Count; i++)
		    {
				var id = itemCollection[i][fldId.InternalName].ToInt();
                var value = (IDictionary<string, object>)new ExpandoObject();
                fields.Split('$').ToList().ForEach(item =>
                {
                    var fldTitle = item.IsGuid() ? list.FieldById(item.ToGuid()) : list.FieldByName(item);
                    var spListItem = itemCollection[i];
                    if (fldTitle != null && spListItem != null)
                    {
                        object spItemValue;
                        try
                        {
                            spItemValue = valueAsObject ? spListItem.GetObjectValue(fldTitle) : fldTitle.GetFieldValueAsText(spListItem[fldTitle.Id]);
                        }
                        catch
                        {
                            spItemValue = string.Empty;
                        }
                        value.Add(item, spItemValue);
                    }
                });
                result.Add(new Tuple<int, ExpandoObject>(id, (ExpandoObject)value));
		    }
			return result;
		}

        public static string BitsToString(short bits, int len)
        {
            string str = string.Empty;
            for (int i = 0; i < len; i++) {
                str = ((((bits >> i) & 1) > 0) ? "1" : "0") + str;
            }
            return str;
        }
	}
}
