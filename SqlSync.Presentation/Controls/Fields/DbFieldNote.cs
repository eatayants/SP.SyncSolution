using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roster.Presentation.Controls.FieldControls;

namespace Roster.Presentation.Controls.Fields
{
    public class DbFieldNote : DbField
    {
        public int NumberOfLines { get; set; }

        public override SPFieldType Type
        {
            get
            {
                return SPFieldType.Note;
            }
        }

        public override string SqlType
        {
            get
            {
                return "nvarchar(MAX)";
            }
        }

        public override string TypeAsDisplayLabel
        {
            get
            {
                return "Multiple lines of text";
            }
        }

        public DbFieldNote(string fieldName)
            : base(fieldName)
        {}
        public DbFieldNote(Guid fieldId)
            : base(fieldId)
        { }

        public override DbBaseFieldControl BaseControl
        {
            get
            {
                return new DbNoteControl() { Field = this };
            }
        }

        public override void CopyFrom(DbField sourceField)
        {
            DbFieldNote fldSource = sourceField as DbFieldNote;

            if (fldSource != null)
            {
                this.Required = fldSource.Required;
                this.DefaultValue = fldSource.DefaultValue;
                this.NumberOfLines = fldSource.NumberOfLines;
            }
        }

        public override object GetFieldValue(object dirtyValue)
        {
            return (dirtyValue == null) ? null : dirtyValue.ToString();
        }
    }
}
