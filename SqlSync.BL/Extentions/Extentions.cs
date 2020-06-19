using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;
using Roster.Common;
using Roster.Model.DataContext;

namespace Roster.BL.Extentions
{
    public static class Extentions
    {
        public static void ValidateConnection(this ConnectionInfo connectionInfo)
	    {
            using (var connection = new SqlConnection(connectionInfo.GetConnectionString()))
			{
				var command = new SqlCommand("SELECT 1", connection);
				connection.Open();
				command.ExecuteNonQuery();
			}
	    }

        public static List<ExpandoObject> MapList(this List<ExpandoObject> properties, ListMetadata list)
        {
            var spListDef = list.ListMetadataFields.Where(item =>
                item.DataSourceType == (int)LookupSourceType.SpList).ToList();
            spListDef.ForEach(item =>
            {
                var key = item.InternalName;
                var refKeys = properties.Select(refer => refer.FirstOrDefault(value => value.Key == key))
                    .Select(parentKey => parentKey.Value.ToInt()).ToList();
                var referContent = SPContext.Current.SPList(item.DataSource.ToGuid())
                            .ListItems(item.DataSourceKey, item.DataSourceField, false, refKeys);

                properties.ForEach(subItem =>
                {
                    var exProp = (subItem as IDictionary<string, object>);
                    var value = referContent.FirstOrDefault(content => content.Item1 == exProp[key].ToInt());
                    if (value != null)
                    {
                        var exValue = (value.Item2 as IDictionary<string, object>);
                        item.DataSourceField.Split('$').ToList().ForEach(itemField =>
                        {
                            var name = string.Format("{0}_{1}", item.DataSource, itemField);
                            exProp[name] = exValue != null ? exValue[itemField] : string.Empty;
                        });
                    }
                });
            });
            return properties;
        }

        public static List<ExpandoObject> MapList(this List<ExpandoObject> properties, ViewMetadata view)
        {
            var spListDef = view.ViewMetadataFields.Where(item =>
                item.ListMetadataField.DataSourceType == (int)LookupSourceType.SpList).ToList();
            spListDef.ForEach(item =>
            {
                var key = item.ListMetadataField.InternalName;
                var refKeys = properties.Select(refer => refer.FirstOrDefault(value => value.Key == key))
                    .Select(parentKey => parentKey.Value.ToInt()).ToList();
                var referContent = SPContext.Current.SPList(item.ListMetadataField.DataSource.ToGuid())
                            .ListItems(item.ListMetadataField.DataSourceKey, item.ListMetadataField.DataSourceField, false, refKeys);
                properties.ForEach(subItem =>
                {
                    var exProp = (subItem as IDictionary<string, object>);
                    var value = referContent.FirstOrDefault(content => content.Item1 == exProp[key].ToInt());
                    if (value != null)
                    {
                        var exValue = (value.Item2 as IDictionary<string, object>);
                        item.ListMetadataField.DataSourceField.Split('$').ToList().ForEach(itemField =>
                        {
                            var name = string.Format("{0}_{1}", item.ListMetadataField.DataSource, itemField);
                            exProp[name] = exValue != null ? exValue[itemField] : string.Empty;
                        });  
                    }
                });
            });
            return properties;
        }
    }
}
