using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint.WebControls;
using Roster.Common;
using Roster.Presentation.Controls.FieldControls;
using Roster.Presentation.Extensions;
using Roster.BL;
using System.Xml.Linq;

namespace Roster.Presentation.Controls.Fields
{
    public class DbFieldUser : DbField
    {
        private const char FieldSeparator = '$';

        #region Properties

        public int ListSource { get; set; }
		public string ListId { get; set; }
        public string LookupKey { get; set; }
		public string LookupField { get; set; }

        public string AllowSelection { get; set; }
        public string ChooseFrom { get; set; }
        public string ChooseFromGroup { get; set; }
        public bool AllowMultipleValues
        {
            get { return false; }
        }

        public override SPFieldType Type
        {
            get
            {
                return SPFieldType.User;
            }
        }

        public List<string> LookupFields
        {
            get 
            { 
               return string.IsNullOrWhiteSpace(LookupField) ? 
                    new List<string>() : LookupField.Split(FieldSeparator).ToList();
            }
        }

        public override string SqlType
        {
            get
            {
                return "int";
            }
        }

        public override string TypeAsDisplayLabel
        {
            get
            {
                return "Person or Group";
            }
        }

        #endregion

        #region Constructors

        public DbFieldUser(string fieldName) : base(fieldName)
        {}
        public DbFieldUser(Guid fieldId) : base(fieldId)
        { }

        #endregion

        #region Override methods

        public override DbBaseFieldControl BaseControl
        {
            get
            {
                return new DbUserControl { Field = this };
            }
        }

        public override List<DbField> SubFields()
        {
            var result = new List<DbField>();
            LookupFields.ForEach(item =>
            {
                if (LookupFields.FindIndex(i => i == item) != 0)
                {
                    var displayName = DisplayName;
                    displayName = string.Format("{0} : {1}", DisplayName, item);

                    result.Add(new DbFieldLabel(string.Format("{0}_{1}", ListId, item)) {
                        DisplayName = displayName,
                        ReadOnly = true
                    });
                }
            });
            return result;
        }

        public override void CopyFrom(DbField sourceField)
        {
            DbFieldUser fldSource = sourceField as DbFieldUser;

            if (fldSource != null)
            {
                this.Required = fldSource.Required;
                this.DefaultValue = fldSource.DefaultValue;
				this.ListSource = fldSource.ListSource;
				this.ListId = fldSource.ListId;
                this.LookupKey = fldSource.LookupKey;
                this.LookupField = fldSource.LookupField;
                this.AllowSelection = fldSource.AllowSelection;
                this.ChooseFrom = fldSource.ChooseFrom;
                this.ChooseFromGroup = fldSource.ChooseFromGroup;
            }
        }


        public override object GetFieldValue(object dityValue)
        {
            if (dityValue == null) {
                return null;
            }
            // TO-DO :: handle lookup values CORRECT
            return dityValue.ToString();
        }

        public override string GetFieldValueAsText(object dirtyValue)
        {
            return (dirtyValue == null) ?
                string.Empty :
                this.EnsureUser(new UserPickerEntity(dirtyValue.ToInt())).DisplayText;
        }

        #endregion

        #region Public methods

        public UserPickerEntity EnsureUser(string accountName)
        {
            return this.EnsureUser(new UserPickerEntity(accountName));
        }

        public UserPickerEntity EnsureUser(int rosterLookupId)
        {
            return this.EnsureUser(new UserPickerEntity(rosterLookupId));
        }

        public UserPickerEntity EnsureUser(SPFieldUserValue spUserVal)
        {
            var upe = new UserPickerEntity(spUserVal.User == null ? spUserVal.LookupValue : spUserVal.User.LoginName);
            upe.DisplayText = spUserVal.User == null ? spUserVal.LookupValue : spUserVal.User.Name;
            if (spUserVal.User != null) {
                upe.EntityData = new UserPickerEntityData();
                upe.EntityData.Email = spUserVal.User.Email;
                upe.EntityData.PrincipalType = "User";
            }
            return this.EnsureUser(upe);
        }

        public UserPickerEntity EnsureUser(SPUser spUser)
        {
            var upe = new UserPickerEntity(spUser.LoginName);
            upe.DisplayText = spUser.Name;
            return this.EnsureUser(upe);
        }

        public UserPickerEntity EnsureUser(XDocument doc)
        {
            var entity = doc.Root.Element("Entity"); // take ONLY first one !!!
            string key = entity.Attribute("Key").Value;

            UserPickerEntity pickerEntity = new UserPickerEntity(key);
            pickerEntity.DisplayText = entity.Attribute("DisplayText").Value;
            pickerEntity.IsResolved = true;
            pickerEntity.Resolved = true;

            // EntityData
            //UserPickerEntityData entityData = new UserPickerEntityData();
            //pickerEntity.EntityData = entityData;

            return this.EnsureUser(pickerEntity);
        }

        public UserPickerEntity EnsureUser(UserPickerEntity entity)
        {
            return EnsureUser(entity, this.ListId, this.ListSource, this.LookupKey);
        }

        public static int GetUserRosterId(SPUser spUser)
        {
            var upe = new UserPickerEntity(spUser.LoginName);
            upe.DisplayText = spUser.Name;
            
            return EnsureUser(upe, TableNames.UserInformationTable, (int)LookupSourceType.Table, "Id").RosterLookupId;
        }

        #endregion

        #region Private methods

        private static UserPickerEntity EnsureUser(UserPickerEntity entity, string listId, int listSource, string lookupKey)
        {
            if (entity.RosterLookupId != 0 && entity.IsResolved) { return entity; }
            if (string.IsNullOrEmpty(entity.Key) && entity.RosterLookupId == 0) {
                throw new ArgumentException("To ensureUser please specify accountName OR rosterLookupId!");
            }

            UserPickerEntity dbUser = null;
            List<Tuple<string, string>> whereCriteria = new List<Tuple<string, string>>();
            if (!string.IsNullOrEmpty(entity.Key)) {
                whereCriteria.Add(new Tuple<string, string>(FieldNames.USER_LOGIN_NAME, entity.Key));
            } else {
                whereCriteria.Add(new Tuple<string, string>(lookupKey, entity.RosterLookupId.ToString()));
            }
            var userFields = new[] { FieldNames.USER_LOGIN_NAME, FieldNames.USER_DISPLAY_NAME, FieldNames.USER_EMAIL, FieldNames.USER_SIP, FieldNames.USER_DEPARTMENT, FieldNames.USER_PRINCIPAL_TYPE };
            var users = BLExtensions.SourceContent(listId, lookupKey, String.Join("$", userFields), listSource, whereCriteria);
            if (users == null || !users.Any())
            {
                // add new user
                int newLookupId = new RosterDataService().SaveRow(lookupKey, listId, entity.ToSyncObject());
                if (newLookupId == 0) {
                    throw new Exception("Cannot add user to system table");
                } else {
                    dbUser = entity;
                    dbUser.RosterLookupId = newLookupId;
                }
            }
            else
            {
                var user = users.First();
                if (entity.IsResolved) {
                    dbUser = entity;
                    dbUser.RosterLookupId = user.Item1;
                } else {
                    dbUser = new UserPickerEntity(user.Item1, user.Item2 as IDictionary<string, object>);
                }
            }

            return dbUser;
        }

        #endregion
    }
}
