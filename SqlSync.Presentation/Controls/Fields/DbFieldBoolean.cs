using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roster.Presentation.Controls.FieldControls;
using System.Globalization;
using Microsoft.SharePoint.Utilities;

namespace Roster.Presentation.Controls.Fields
{
    public class DbFieldBoolean : DbField
    {
        public override SPFieldType Type
        {
            get
            {
                return SPFieldType.Boolean;
            }
        }

        public override string SqlType
        {
            get
            {
                return "bit";
            }
        }

        public override string TypeAsDisplayLabel
        {
            get
            {
                return "Yes/No (check box)";
            }
        }

        public DbFieldBoolean(string fieldName)
            : base(fieldName)
        {}
        public DbFieldBoolean(Guid fieldId)
            : base(fieldId)
        { }

        public override DbBaseFieldControl BaseControl
        {
            get
            {
                return new DbBooleanControl() { Field = this };
            }
        }

        public override void CopyFrom(DbField sourceField)
        {
            DbFieldBoolean fldSource = sourceField as DbFieldBoolean;

            if (fldSource != null)
            {
                this.DefaultValue = fldSource.DefaultValue;
            }
        }

        public override object GetFieldValue(object dirtyValue)
        {
            if (dirtyValue == null)
                return false;

            string val = dirtyValue.ToString();
            if (string.IsNullOrEmpty(val) || (!string.Equals(val, "TRUE", StringComparison.OrdinalIgnoreCase) && (!(val == "-1") && !(val == "1"))))
                return false;

            return true;
        }

        public override string GetFieldValueAsText(object dirtyValue)
        {
            if (dirtyValue == null)
                return "No";
            else if (dirtyValue is bool)
                return ((bool)dirtyValue) ? "Yes" : "No";
            else
                return (dirtyValue.ToString() == "1") ? "Yes" : "No";
        }
    }
}
