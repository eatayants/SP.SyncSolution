using Roster.Common;
// ReSharper disable CheckNamespace
using System;
using System.Data;
using System.Linq;
using Roster.Model.Helpers;

namespace Roster.Model.DataContext
// ReSharper restore CheckNamespace
{
    public partial class ListMetadataField
	{
        public string UsedInContentTypes
        {
            get { return String.Join(",", ListMetadataFieldContentTypes.Select(item=>item.ListMetadataContentType.Name)); }
        }

        public SqlDbType SqlDbType
        {
            get 
			{
                var fieldType = Enum.GetNames(typeof (SqlDbType)).FirstOrDefault(enumValue => 
                    String.Equals(enumValue, FieldType, StringComparison.InvariantCultureIgnoreCase));
			    if (string.IsNullOrWhiteSpace(fieldType))
			    {
			        return SqlDbType.NVarChar;
			    }
                return (SqlDbType)Enum.Parse(typeof(SqlDbType), fieldType);
			}
        }

        public Type ClrType
        {
            get
            {
                return SqlDbType.GetClrType();
            }
        }

        public int FieldLenght 
        {
            get { return FieldType.ExtractNumber(); }
        }

        public string DefaultField
        {
            get
            {
                var result = InternalName;
                if (!DataSourceType.In((int) LookupSourceType.Query, (int) LookupSourceType.Table)) 
                    return result;
                var dataSourceFields = DataSourceField.Split('$').ToList();
                if (dataSourceFields.IsEmpty())
                {
                    result = dataSourceFields[0];
                }
                return result;
            }
        }
    }
}