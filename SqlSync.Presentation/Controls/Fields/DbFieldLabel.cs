using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roster.Presentation.Controls.FieldControls;

namespace Roster.Presentation.Controls.Fields
{
    public class DbFieldLabel : DbField
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

        public DbFieldLabel(string fieldName) : base(fieldName)
        {}

        public DbFieldLabel(Guid fieldId)
            : base(fieldId)
        { }

        public override DbBaseFieldControl BaseControl
        {
            get
            {
                return new DbLabelControl { Field = this };
            }
        }

        public override object GetFieldValue(object dirtyValue)
        {
            return (dirtyValue == null) ? null : dirtyValue.ToString();
        }
    }
}
