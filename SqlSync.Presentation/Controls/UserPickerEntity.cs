using Roster.Common;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Roster.Presentation.Controls
{
    public class UserPickerEntity
    {
        #region Private variables

        private IDictionary<string, object> _data;

        #endregion

        #region Properties
        public string Key { get; set; }
        public string Description { get; set; }
        public string DisplayText { get; set; }
        public bool IsResolved { get; set; }
        public string AutoFillKey { get; set; }
        public string AutoFillDisplayText { get; set; }
        public string AutoFillSubDisplayText { get; set; }
        public string AutoFillTitleText { get; set; }
        public bool Resolved { get; set; }
        public string EntityType { get; set; }

        public int RosterLookupId { get; set; }

        [ScriptIgnore]
        public string SharePointEntityType
        {
            get
            {
                if (!string.IsNullOrEmpty(this.EntityType)) {
                    return this.EntityType;
                } else if (this.EntityData != null) {
                    return this.EntityData.PrincipalType.ToSafeString();
                } else {
                    return string.Empty;
                }
            }
        }
        [ScriptIgnore]
        public string SharePointEntityId
        {
            get
            {
                if (this.EntityData != null) {
                    if (!string.IsNullOrEmpty(this.EntityData.SPGroupID)) {
                        return this.EntityData.SPGroupID;
                    } else {
                        return this.EntityData.SPUserID.ToSafeString();
                    }
                } else {
                    return string.Empty;
                }
            }
        }

        //public string EntityGroupName { get; set; }
        //public string HierarchyIdentifier { get; set; }
        //public string ProviderDisplayName { get; set; }
        //public string ProviderName { get; set; }
        //public string[] MultipleMatches { get; set; }
        //public string DomainText { get; set; }

        public UserPickerEntityData EntityData { get; set; }

        #endregion

        #region Constructors

        public UserPickerEntity() { }

        public UserPickerEntity(string accountName)
        {
            this.Key = accountName;
            this.IsResolved = false;
        }

        public UserPickerEntity(int lookupId)
        {
            this.RosterLookupId = lookupId;
            this.IsResolved = false;
        }

        public UserPickerEntity(int lookupId, IDictionary<string, object> data)
        {
            this.RosterLookupId = lookupId;
            this.IsResolved = true;
            this.Resolved = true;
            this.EntityData = new UserPickerEntityData();
            this._data = data;

            if (data.ContainsKey(FieldNames.USER_LOGIN_NAME)) {
                this.Key = data[FieldNames.USER_LOGIN_NAME].ToSafeString();
                this.Description = this.Key;
                this.AutoFillKey = this.Key;
                this.AutoFillTitleText = string.Concat("\n", this.Key.Replace("i:0#.w|", ""));
            }
            if (data.ContainsKey(FieldNames.USER_DISPLAY_NAME)) {
                this.DisplayText = data[FieldNames.USER_DISPLAY_NAME].ToSafeString();
                this.AutoFillDisplayText = this.DisplayText;
                this.AutoFillSubDisplayText = "";
            }
            if (data.ContainsKey(FieldNames.USER_EMAIL)) {
                this.EntityData.Email = data[FieldNames.USER_EMAIL].ToSafeString();
            }
            if (data.ContainsKey(FieldNames.USER_SIP)) {
                this.EntityData.SIPAddress = data[FieldNames.USER_SIP].ToSafeString();
            }
            if (data.ContainsKey(FieldNames.USER_DEPARTMENT)) {
                this.EntityData.Department = data[FieldNames.USER_DEPARTMENT].ToSafeString();
            }
            if (data.ContainsKey(FieldNames.USER_PRINCIPAL_TYPE)) {
                this.EntityData.PrincipalType = data[FieldNames.USER_PRINCIPAL_TYPE].ToSafeString();
            }
        }

        #endregion

        public ExpandoObject ToSyncObject()
        {
            var result = (IDictionary<string, object>)new ExpandoObject();

            result.Add(FieldNames.USER_LOGIN_NAME, this.Key);
            result.Add(FieldNames.USER_DISPLAY_NAME, this.DisplayText.ToSafeString());
            result.Add(FieldNames.USER_EMAIL, this.EntityData == null ? "" : this.EntityData.Email.ToSafeString());
            result.Add(FieldNames.USER_SIP, this.EntityData == null ? "" : this.EntityData.SIPAddress.ToSafeString());
            result.Add(FieldNames.USER_DEPARTMENT, this.EntityData == null ? "" : this.EntityData.Department.ToSafeString());
            result.Add(FieldNames.USER_MOBILE, this.EntityData == null ? "" : this.EntityData.MobilePhone.ToSafeString());

            result.Add(FieldNames.USER_EXTERNAL_ID, this.SharePointEntityId);
            result.Add(FieldNames.USER_PRINCIPAL_TYPE, this.SharePointEntityType);

            return (ExpandoObject)result;
        }

        public string GetTitleByLookupField(string lookupField)
        {
            return (this._data != null && this._data.ContainsKey(lookupField)) ?
                this._data[lookupField].ToSafeString() :
                this.Key;
        }
    }

    public class UserPickerEntityData
    {
        public string Title { get; set; }
        public string MobilePhone { get; set; }
        public string SIPAddress { get; set; }
        public string Department { get; set; }
        public string Email { get; set; }
        public string SPGroupID { get; set; }
        public string SPUserID { get; set; }
        public string AccountName { get; set; }
        public string PrincipalType { get; set; }
    }
}
