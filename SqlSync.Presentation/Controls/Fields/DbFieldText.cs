using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roster.Presentation.Controls.FieldControls;

namespace Roster.Presentation.Controls.Fields
{
    public class DbFieldText : DbField
    {
        public int MaxLength { get; set; }

        public override SPFieldType Type
        {
            get
            {
                return SPFieldType.Text;
            }
        }

        public override string SqlType
        {
            get
            {
                return String.Format("nvarchar({0})", MaxLength == 0 ? 255 : MaxLength);
            }
        }

        public override string TypeAsDisplayLabel
        {
            get
            {
                return "Single line of text";
            }
        }

        public DbFieldText(string fieldName) : base(fieldName)
        {}

        public DbFieldText(Guid fieldId) : base(fieldId)
        { }

        public override DbBaseFieldControl BaseControl
        {
            get
            {
                return new DbTextControl() { Field = this };
            }
        }

        public override void CopyFrom(DbField sourceField)
        {
            DbFieldText fldSource = sourceField as DbFieldText;

            if (fldSource != null)
            {
                this.Required = fldSource.Required;
                this.DefaultValue = fldSource.DefaultValue;
                this.MaxLength = fldSource.MaxLength;
            }
        }

        public override object GetFieldValue(object dirtyValue)
        {
            return (dirtyValue == null) ? null : dirtyValue.ToString();
        }
    }
}
