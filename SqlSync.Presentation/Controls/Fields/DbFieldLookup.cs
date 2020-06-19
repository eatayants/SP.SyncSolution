using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint.WebControls;
using Roster.Common;
using Roster.Presentation.Controls.FieldControls;
using Roster.Presentation.Extensions;

namespace Roster.Presentation.Controls.Fields
{
    public class DbFieldLookup : DbField
    {
        private const char FieldSeparator = '$';
        public int ListSource { get; set; }
		public string ListId { get; set; }
        public string LookupKey { get; set; }
		public string LookupField { get; set; }

        public string DependentParent { get; set; }
        public string DependentParentField { get; set; }
        public string FilterByField { get; set; }

        public override SPFieldType Type
        {
            get
            {
                return SPFieldType.Lookup;
            }
        }

        public List<string> LookupFields
        {
            get 
            { 
               return string.IsNullOrWhiteSpace(LookupField) ? 
                    new List<string>() : LookupField.Split(FieldSeparator).ToList();
            }
        }

        public override string SqlType
        {
            get
            {
                return "int";
            }
        }

        public override string TypeAsDisplayLabel
        {
            get
            {
                return "Lookup (information already exists)";
            }
        }

        public DbFieldLookup(string fieldName)
            : base(fieldName)
        {}
        public DbFieldLookup(Guid fieldId)
            : base(fieldId)
        { }

        public override DbBaseFieldControl BaseControl
        {
            get
            {
                return new DbLookupControl { Field = this };
            }
        }

        public override List<DbField> SubFields()
        {
            var result = new List<DbField>();
            var list = (ListSource == (int) LookupSourceType.SpList) ? SPContext.Current.SPList(ListId.ToGuid()): null;
            LookupFields.ForEach(item =>
            {
                if (LookupFields.FindIndex(i => i == item) != 0)
                {
                    var displayName = DisplayName;
                    if (ListSource == (int) LookupSourceType.SpList)
                    {
                        if (list != null)
                        {
                            if (LookupFields.Count() > 1)
                            {
                                displayName = string.Format("{0} : {1}", DisplayName, list.FieldTitle(item.ToGuid()));
                            }
                        }
                    }
                    else
                    {
                        displayName = string.Format("{0} : {1}", DisplayName, item);
                    }

                    result.Add(new DbFieldLabel(string.Format("{0}_{1}", ListId, item))
                    {
                        DisplayName = displayName,
                        ReadOnly = true
                    });
                }
            });
            return result;
        }

        public override void CopyFrom(DbField sourceField)
        {
            DbFieldLookup fldSource = sourceField as DbFieldLookup;

            if (fldSource != null)
            {
                this.Required = fldSource.Required;
                this.DefaultValue = fldSource.DefaultValue;
				this.ListSource = fldSource.ListSource;
				this.ListId = fldSource.ListId;
                this.LookupKey = fldSource.LookupKey;
                this.LookupField = fldSource.LookupField;
                this.DependentParent = fldSource.DependentParent;
                this.DependentParentField = fldSource.DependentParentField;
                this.FilterByField = fldSource.FilterByField;
            }
        }

        public override object GetFieldValue(object dityValue)
        {
            if (dityValue == null)
            {
                return null;
            }
            // TO-DO :: handle lookup values CORRECT
            return dityValue.ToString();
        }

        public override string GetFieldValueAsText(object dirtyValue)
        {
            if (dirtyValue == null) {
                return string.Empty;
            }

            var lookupVals = BLExtensions.SourceContent(this.ListId, this.LookupKey, this.LookupField, this.ListSource, new List<Tuple<string, string>> {
                new Tuple<string, string>(this.LookupKey, dirtyValue.ToString())
            });
            if (lookupVals == null || !lookupVals.Any()) {
                return "=deleted=";
            } else {
                var props = lookupVals.First().Item2 as IDictionary<string, object>;
                return (props == null || (this.LookupFields.Any() && !props.ContainsKey(this.LookupFields[0]))) ? string.Empty : props[this.LookupFields[0]].ToSafeString();
            }
        }
    }
}
