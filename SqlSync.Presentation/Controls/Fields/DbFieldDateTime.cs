using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roster.Presentation.Controls.FieldControls;
using Microsoft.SharePoint.Utilities;
using System.Globalization;

namespace Roster.Presentation.Controls.Fields
{
    public class DbFieldDateTime : DbField
    {
        public string Format { get; set; }

        public override SPFieldType Type
        {
            get
            {
                return SPFieldType.DateTime;
            }
        }

        public override string SqlType
        {
            get
            {
                return "datetime";
            }
        }

        public override string TypeAsDisplayLabel
        {
            get
            {
                return "Date and Time";
            }
        }

        public DbFieldDateTime(string fieldName)
            : base(fieldName)
        {}
        public DbFieldDateTime(Guid fieldId)
            : base(fieldId)
        { }

        public override DbBaseFieldControl BaseControl
        {
            get
            {
                return new DbDateTimeControl() { Field = this };
            }
        }

        public override void CopyFrom(DbField sourceField)
        {
            DbFieldDateTime fldSource = sourceField as DbFieldDateTime;

            if (fldSource != null)
            {
                this.Required = fldSource.Required;
                this.DefaultValue = fldSource.DefaultValue;
                this.Format = fldSource.Format;
            }
        }

        public override object GetFieldValue(object dirtyValue)
        {
            if (dirtyValue == null)
                return null;

            if (dirtyValue is DateTime)
                return (DateTime)dirtyValue;

            string val = dirtyValue.ToString();
            DateTime minValue;
            try
            {
                minValue = SPUtility.ParseDate(SPContext.Current.Web, val, SPDateFormat.DateTime, false);
                if ((minValue == DateTime.MaxValue) || (minValue == DateTime.MinValue)) {
                    throw new SPException();
                }
                return minValue;
            }
            catch (Exception)
            {
                try
                {
                    minValue = SPUtility.CreateSystemDateTimeFromXmlDataDateTimeFormat(val);
                }
                catch (Exception)
                {
                    if (!DateTime.TryParse(val, CultureInfo.InvariantCulture, DateTimeStyles.None, out minValue)) {
                        minValue = DateTime.MinValue;
                    }
                }
            }

            if ((minValue != DateTime.MaxValue) && (minValue != DateTime.MinValue))
                return minValue;
            else
                return null;
        }

        public override string GetFieldValueAsText(object dirtyValue)
        {
            if (dirtyValue == null) {
                return "";
            } else if (dirtyValue is DateTime) {
                return ((DateTime)dirtyValue).ToString("dd/MM/yyyy HH:mm");
            } else {
                return SPUtility.CreateSystemDateTimeFromXmlDataDateTimeFormat(dirtyValue.ToString()).ToString("dd/MM/yyyy HH:mm");
            }
        }
    }
}
