using Microsoft.SharePoint.Utilities;
using Roster.Model.DataContext;
using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace Roster.Presentation.Helpers
{
    public class RosterClientContext
    {
        public Guid viewId { get; set; }
        public Guid listId { get; set; }
        public string listDispForm { get; set; }
        public string listEditForm { get; set; }
        public string listNewForm { get; set; }

        public object ColorSettings { get; set; }
        public bool WithTooltips { get; set; }
        public string FilterOperators { get; set; }
        public bool QuickEditMode { get; set; }
        public List<QuickEditColumnProperties> QuickEditSettings { get; set; }
        public CalendarProperties CalendarProps { get; set; }
        public ViewFilter FilterFromConnectedWebPart { get; set; }
        public string ShortDatePattern { get; set; }
        public List<ContentTypeInfo> ContentTypes { get; set; }

        public RosterClientContext()
        {
            this.QuickEditSettings = new List<QuickEditColumnProperties>();
        }
    }

    public class CalendarProperties
    {
        [ScriptIgnore]
        public DateTime? PeriodStartDate { get; set; }
        public string PeriodStart
        {
            get
            {
                return this.PeriodStartDate.HasValue ? SPUtility.CreateISO8601DateTimeFromSystemDateTime(this.PeriodStartDate.Value) : string.Empty;
            }
        }
        [ScriptIgnore]
        public DateTime? PeriodEndDate { get; set; }
        public string PeriodEnd
        {
            get
            {
                return this.PeriodEndDate.HasValue ? SPUtility.CreateISO8601DateTimeFromSystemDateTime(this.PeriodEndDate.Value) : string.Empty;
            }
        }
    }

    public class QuickEditColumnProperties
    {
        [ScriptIgnore]
        public Constants.EditableType DataType { get; set; }
        public string type
        {
            get
            {
                return this.DataType.ToString().ToLower();
            }
        }
        [ScriptIgnore]
        public Constants.EditableType EditorType { get; set; }
        public string editor
        {
            get
            {
                return this.EditorType.ToString().ToLower();
            }
        }
        public string renderer { get; set; }
        public string name { get; set; }
        public int data { get; set; }
        public string[] selectOptions { get; set; }
        public string format { get; set; } // '$0,0.00'
        public string dateFormat { get; set; }
        public bool readOnly { get; set; }
        public Select2LookupOptions select2LookupOptions { get; set; }
        public UserLookupOptions userLookupOptions { get; set; }
        public DatePickerOptions datePickerConfig { get; set; }

        public string checkedTemplate { get; set; }
        public string uncheckedTemplate { get; set; }
        
        //[ScriptIgnore]
        //public string Validator { get; set; }
        //public string validator
        //{
        //    get
        //    {
        //        return string.IsNullOrEmpty(this.Validator) ? null : string.Format("%%%%{0}%%%%", this.Validator);
        //    }
        //}

        public QuickEditColumnProperties()
        {
            this.renderer = "text";
        }
    }

    public class SourceItem
    {
        public string value { get; set; }
        public string text { get; set; }

        public SourceItem()
        {
        }

        public SourceItem(string val)
        {
            this.value = val;
            this.text = val;
        }
    }

    public class Select2LookupOptions
    {
        public string Source { get; set; }
        public string Key { get; set; }
        public string Fields { get; set; }
        public int ListType { get; set; }
        public string MetadataId { get; set; }
        public string ParentKeyValue { get; set; }
        public int ParentData { get; set; }

        public Select2LookupOptions()
        { }

        public Select2LookupOptions(Roster.Presentation.Controls.Fields.DbFieldLookup field)
        {
            this.Source = field.ListId;
            this.Key = field.LookupKey;
            this.Fields = field.LookupField;
            this.ListType = field.ListSource;
            this.MetadataId = field.Id.ToString();
        }
    }

    public class UserLookupOptions
    {
        public bool MultiSelect { get; set; }
        public string CustomProperty { get; set; }
        public string DialogImage { get; set; }
        public string PickerDialogType { get; set; }
        public bool ForceClaims { get; set; }
        public bool DisableClaims { get; set; }
        public string EnabledClaimProviders { get; set; }
        public string EntitySeparator { get; set; }
        public string MetadataId { get; set; }

        public UserLookupOptions()
        {
            this.DialogImage = "%2F%5Flayouts%2F15%2Fimages%2Fppeople%2Egif";
            this.PickerDialogType = "Microsoft%2ESharePoint%2EWebControls%2EPeoplePickerDialog%2C%20Microsoft%2ESharePoint%2C%20Version%3D15%2E0%2E0%2E0%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3D71e9bce111e9429c";
            this.ForceClaims = false;
            this.DisableClaims = false;
            this.EnabledClaimProviders = "";
            this.EntitySeparator = "%3B%EF%BC%9B%EF%B9%94%EF%B8%94%E2%8D%AE%E2%81%8F%E1%8D%A4%D8%9B";
        }
        public UserLookupOptions(Roster.Presentation.Controls.Fields.DbFieldUser field) : this()
        {
            this.MetadataId = field.Id.ToString();
            this.MultiSelect = field.AllowMultipleValues;
            
            // {SelectionSet};{SharePointGroup};{PrincipalSource};{WebApplicationId};{UrlZone};{AllUrlZones}
            this.CustomProperty = string.Format("{0};{1};15;;;False",
                string.IsNullOrEmpty(field.ChooseFromGroup) ? "User%2CSecGroup%2CSPGroup" : "User%2CSecGroup",
                string.IsNullOrEmpty(field.ChooseFromGroup) ? "" : this.GetGroupNameById(field.ChooseFromGroup));
        }

        private string GetGroupNameById(string groupId)
        {
            string groupName = "";
            int grId = 0;

            if (Int32.TryParse(groupId, out grId)) {
                try {
                    groupName = Microsoft.SharePoint.SPContext.Current.Web.SiteGroups.GetByID(grId).Name;
                } catch { }
            }

            return SPHttpUtility.UrlKeyValueEncode(groupName);
        }
    }

    public class DatePickerOptions
    {
        public bool showTime { get; set; }
    }

    public class ViewFilter
    {
        public string Field { get; set; }
        public string Value { get; set; }
    }

    public class ContentTypeInfo
    {
        public int Id { get; set; }
        public string DispFormUrl { get; set; }
        public string EditFormUrl { get; set; }
    }
}