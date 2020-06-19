using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.WebControls;
using SqlSync.Model.DataContext;
using SqlSync.SP.Helpers;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Xml.Linq;
using SqlSync.BL.Extentions;
using SqlSync.Common;
using SqlSync.BL;

namespace SqlSync.SP.Extensions
{
    public static class BLExtensions
    {
        public static void SyncList(Guid listId)
        {
            var configProvider = new AppConfigService();
            var mappingSetting = configProvider.GetMappingByList(listId.ToString());
            if (mappingSetting == null) return;
            var dataProvider = new AppDataService();
            {
                var list = SPContext.Current.SPList(listId);
                var listitems = list.ListItems(mappingSetting.Key, 
                    String.Join("$", mappingSetting.ListMappingFields.Select(item => item.ItemName).ToList()),true);

                dataProvider.SaveRows(mappingSetting, listitems);
            }
        }

        public static Tuple<int,ExpandoObject> ToSyncObject(this SPListItem listItem, ListMapping mapping)
        {
            var result = (IDictionary<string,object>)new ExpandoObject();
            mapping.ListMappingFields.ToList().ForEach(item =>
            {
                var spField = item.ItemName.IsGuid() ? listItem.FieldById(item.ItemName.ToGuid()) : listItem.FieldByName(item.ItemName);
                object spItemValue;
                try {
                    spItemValue = (spField != null) ? listItem.GetObjectValue(spField) : string.Empty;
                } catch {
                    spItemValue = string.Empty;
                }
                result.Add(item.ItemName, spItemValue);
            });
            return new Tuple<int, ExpandoObject>(listItem.ID, (ExpandoObject)result);
        }

        public static List<Tuple<int, ExpandoObject>> SourceContent(string source, string key, string fields, int type, List<Tuple<string, string>> whereCriteria = null)
        {
            List<Tuple<int, ExpandoObject>> itemCollection = null;
            switch (type)
            {
                case (int)LookupSourceType.SpList:
                    {
                        itemCollection = SPContext.Current.SPList(source.ToGuid()).ListItemsByCriteria(key, fields, false, whereCriteria);
                        break;
                    }
                case (int)LookupSourceType.Table:
                case (int)LookupSourceType.Query:
                    {
                        itemCollection = new AppDataService().TableContent(source, key, fields, whereCriteria);
                        break;
                    }
            }
            return itemCollection;
        }
    }
}
