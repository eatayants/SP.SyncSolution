using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Roster.Common;
using Roster.Presentation.Controls.FieldControls;

namespace Roster.Presentation.Controls.Fields
{
    public class DbFieldChoice : DbField
    {
        public string[] Choices { get; set; }
        public string ControlType { get; set; }

        public override SPFieldType Type
        {
            get
            {
                return SPFieldType.Choice;
            }
        }

        public override string SqlType
        {
            get
            {
                return "xml";
            }
        }

        public override string TypeAsDisplayLabel
        {
            get
            {
                return "Choice (menu to choose from)";
            }
        }

        public DbFieldChoice(string fieldName)
            : base(fieldName)
        {}
        public DbFieldChoice(Guid fieldId)
            : base(fieldId)
        { }

        public override DbBaseFieldControl BaseControl
        {
            get
            {
                return new DbChoiceControl() { Field = this };
            }
        }

        public override void CopyFrom(DbField sourceField)
        {
            DbFieldChoice fldSource = sourceField as DbFieldChoice;

            if (fldSource != null)
            {
                this.DefaultValue = fldSource.DefaultValue;
                this.Required = fldSource.Required;
                this.Choices = fldSource.Choices;
                this.ControlType = fldSource.ControlType;
            }
        }

        public override object GetFieldValue(object dirtyValue)
        {
            return String.Join(", ", dirtyValue.XmlToList());
        }

        public override string GetFieldValueAsText(object dirtyValue)
        {
            return (dirtyValue == null || !dirtyValue.IsXml()) ? dirtyValue.ToSafeString() : String.Join(", ", dirtyValue.XmlToList());
        }
    }
}
