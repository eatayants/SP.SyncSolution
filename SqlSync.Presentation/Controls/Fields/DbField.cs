using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roster.Common;
using Roster.Presentation.Controls.FieldControls;

namespace Roster.Presentation.Controls.Fields
{
    public class DbField : IEquatable<DbField>
    {
        private string m_strInternalName;
        private Guid m_fieldId;

        public string InternalName
        {
            get
            {
                return SPEncode.UrlEncode(m_strInternalName);
            }
            set
            {
                m_strInternalName = value;
            }
        }
        public string InternalNameOriginal
        {
            get
            {
                return m_strInternalName;
            }
        }
        public Guid Id
        {
            get
            {
                return m_fieldId;
            }
        }
        public bool Required { get; set; }
        public bool Hidden { get; set; }
        public bool ReadOnly { get; set; }
        public virtual string DisplayName { get; set; }
        public string Description { get; set; }
        public virtual string DefaultValue { get; set; }
        public virtual SPFieldType Type
        {
            get
            {
                return SPFieldType.Invalid;
            }
        }
        public virtual string SqlType
        {
            get
            {
                return "";
            }
        }
        public string TypeAsString
        {
            get
            {
                return this.Type.ToString();
            }
        }
        public virtual string TypeAsDisplayLabel
        {
            get
            {
                return this.Type.ToString();
            }
        }

        #region .ctor

        public DbField(string fieldName)
        {
            m_strInternalName = fieldName;
            this.DisplayName = fieldName;
        }

        public DbField(Guid fieldId)
        {
            this.m_fieldId = fieldId;
        }

        #endregion

        public virtual DbBaseFieldControl BaseControl
        {
            get
            {
                return new DbBaseFieldControl() { Field = this };
            }
        }

        public virtual List<DbField> SubFields()
        {
            return new List<DbField>();
        }

        public virtual object GetFieldValue(object dirtyValue)
        {
            return null;
        }

        public virtual void CopyFrom(DbField sourceField)
        {
            this.Required = sourceField.Required;
            this.DisplayName = sourceField.DisplayName;
            this.Description = sourceField.Description;
            this.DefaultValue = sourceField.DefaultValue;
        }

        public virtual string GetFieldValueAsText(object dirtyValue)
        {
            return dirtyValue.ToSafeString();
        }

        public bool Equals(DbField other)
        {
            return (this.InternalName == other.InternalName);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return this.InternalName.GetHashCode();
        }
    }
}
