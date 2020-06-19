using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roster.Presentation.Controls.FieldControls;
using System.Globalization;

namespace Roster.Presentation.Controls.Fields
{
    public class DbFieldNumber : DbField
    {
        public double MaxValue { get; set; }
        public double MinValue { get; set; }
        public int DecimalPlaces { get; set; }

        public override SPFieldType Type
        {
            get
            {
                return SPFieldType.Number;
            }
        }

        public override string SqlType
        {
            get
            {
                return "float";
            }
        }

        public override string TypeAsDisplayLabel
        {
            get
            {
                return "Number (1, 1.0, 100)";
            }
        }

        public DbFieldNumber(string fieldName)
            : base(fieldName)
        {}
        public DbFieldNumber(Guid fieldId)
            : base(fieldId)
        { }

        public override DbBaseFieldControl BaseControl
        {
            get
            {
                return new DbNumberControl() { Field = this };
            }
        }

        public override void CopyFrom(DbField sourceField)
        {
            DbFieldNumber fldSource = sourceField as DbFieldNumber;

            if (fldSource != null)
            {
                this.Required = fldSource.Required;
                this.DefaultValue = fldSource.DefaultValue;
                this.MaxValue = fldSource.MaxValue;
                this.MinValue = fldSource.MinValue;
                this.DecimalPlaces = fldSource.DecimalPlaces;
            }
        }

        public override object GetFieldValue(object dirtyValue)
        {
            return (dirtyValue == null) ? 0 : Convert.ToDouble(dirtyValue.ToString(), CultureInfo.InvariantCulture);
        }
    }
}
