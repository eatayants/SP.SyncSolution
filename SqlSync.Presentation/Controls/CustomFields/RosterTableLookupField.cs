using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Roster.Presentation.Controls.CustomFields
{
    public class RosterTableLookupField : SPFieldText
    {
        #region Private properties

        private string _tblName = string.Empty;
        private string _keyName = string.Empty;
        private string _fldName = string.Empty;
        private string _dataSoureType = string.Empty;

        #endregion

        #region Constructors

        public RosterTableLookupField(SPFieldCollection fields, string fieldName) : base(fields, fieldName)
        {
            Init();
        }

        public RosterTableLookupField(SPFieldCollection fields, string typeName, string displayName)
            : base(fields, typeName, displayName)
        {
            Init();
        }

        #endregion

        public string DataSoureType
        {
            get
            {
                return _dataSoureType;
            }
            set
            {
                SetFieldAttribute("DataSoureType", value);
                _dataSoureType = value;
            }
        }
        public string TableName
        {
            get
            {
                return _tblName;
            }
            set
            {
                SetFieldAttribute("TableName", value);
                _tblName = value;
            }
        }
        public string KeyName
        {
            get
            {
                return _keyName;
            }
            set
            {
                SetFieldAttribute("KeyName", value);
                _keyName = value;
            }
        }
        public string FieldName
        {
            get
            {
                return _fldName;
            }
            set
            {
                SetFieldAttribute("FieldName", value);
                _fldName = value;
            }
        }

        private void Init()
        {
            DataSoureType = GetFieldAttribute("DataSoureType");
            TableName = GetFieldAttribute("TableName");
            KeyName = GetFieldAttribute("KeyName");
            FieldName = GetFieldAttribute("FieldName");
        }

        public override void Update()
        {
            SetFieldAttribute("DataSoure", DataSoureType);
            SetFieldAttribute("TableName", TableName);
            SetFieldAttribute("KeyName", KeyName);
            SetFieldAttribute("FieldName", FieldName);
            base.Update();
        }

        public override string JSLink
        {
            get
            {
                return "/_layouts/15/Roster.Presentation/js/jquery-2.1.1.min.js|/_layouts/15/Roster.Presentation/js/select2.min.js|/_layouts/15/Roster.Presentation/js/RosterTableLookupField.js";
            }
            set
            {
                base.JSLink = value;
            }
        }


        private void SetFieldAttribute(string attribute, string value)
        {
            var baseType = typeof(RosterTableLookupField);
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var mi = baseType.GetMethod("SetFieldAttributeValue", flags);
            mi.Invoke(this, new object[] { attribute, value });
        }

        private string GetFieldAttribute(string attribute)
        {
            var baseType = typeof(RosterTableLookupField);
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var mi = baseType.GetMethod("GetFieldAttributeValue",flags,null,new[] { typeof(String) },null);
            var obj = mi.Invoke(this, new object[] { attribute });
            return obj == null ? "" : obj.ToString();
        }
    }
}
