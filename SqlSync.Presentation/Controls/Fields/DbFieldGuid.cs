using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roster.Presentation.Controls.FieldControls;
using System.Globalization;
using Microsoft.SharePoint.Utilities;
using Roster.Common;

namespace Roster.Presentation.Controls.Fields
{
    public class DbFieldGuid : DbField
    {
        public override SPFieldType Type
        {
            get
            {
                return SPFieldType.Guid;
            }
        }

        public override string SqlType
        {
            get
            {
                return "uniqueidentifier";
            }
        }

        public override string TypeAsDisplayLabel
        {
            get
            {
                return "Guid";
            }
        }

        public DbFieldGuid(string fieldName)
            : base(fieldName)
        {}
        public DbFieldGuid(Guid fieldId)
            : base(fieldId)
        { }

        public override DbBaseFieldControl BaseControl
        {
            get
            {
                return new DbGuidControl() { Field = this };
            }
        }

        public override void CopyFrom(DbField sourceField)
        {
            var fldSource = sourceField as DbFieldGuid;
            if (fldSource != null)
            {
                this.DefaultValue = fldSource.DefaultValue;
            }
        }

        public override object GetFieldValue(object dirtyValue)
        {
            return dirtyValue.ToNullableGuid();
        }
    }
}
