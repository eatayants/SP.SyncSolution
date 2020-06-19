#region

using System;
using System.Collections.Generic;
using System.Dynamic;

#endregion

namespace Roster.Common
{
	public static class SystemSettingName
	{
		public const string DatabaseVersion = "database_version";
        public const string PlannedPublishingDaysAhead = "planned_publishing_daysahead";
	}

    public static class StaticFields
    {
        public const string StartDate = "StartDate";
        public const string EndDate = "EndDate";

        public const string Created = "Created";
        public const string CreatedBy = "CreatedBy";
        public const string Modified   = "Modified";
        public const string ModifiedBy = "ModifiedBy";
    }

	public static class EntityOperations
	{
		public const string Add = "Add";
		public const string Update = "Update";
		public const string Delete = "Delete";
		public const string Get = "Get";
		public const string List = "List";
	}

    public static class TableNames
    {
        public static readonly string RosterTable = "RosterEvent";
        public static readonly string StatusTable = "StatusEvent";
        public static readonly string UserInformationTable = "UserInformationTable";
    }

    public static class TableIDs
    {
        public readonly static Guid PLANNED_ROSTERS  = new Guid("{5B7156BB-84A5-4F8C-AE2B-BCDE4ED9C486}");
        public readonly static Guid WORKING_ROSTERS  = new Guid("{23E6F5B9-1843-4F9B-AAAB-20F45EBB946B}");
        public readonly static Guid TEMPLATE_ROSTERS = new Guid("{E297A6B7-D798-4A03-A41F-DA87DF122B07}");
        public readonly static Guid STATUS_HISTORY      = new Guid("{4F3DAA18-1BA5-4889-A277-4D3FB8A660E1}");
        public readonly static Guid TIMESHEET_ROSTERS   = new Guid("{45C78A04-4725-4B07-8AFA-A638670C080E}");
        public static readonly List<Tuple<Guid,string>> ReferenceItems = new List<Tuple<Guid,string>>
        {
            new Tuple<Guid,string>(PLANNED_ROSTERS, TableNames.RosterTable),
            new Tuple<Guid,string>(WORKING_ROSTERS, TableNames.RosterTable),
            new Tuple<Guid,string>(TIMESHEET_ROSTERS, TableNames.RosterTable),
            new Tuple<Guid,string>(STATUS_HISTORY, TableNames.StatusTable)
        };
    }

    [Flags]
    public enum AccessRight : long
    {
        Read = 1,
        Write = 2,
        Delete = 4,
        List = 8,
        Control = 16,
    }

    public static class FieldNames
    {
        public const string ID = "Id";
        public const string ROSTER_EVENT_TITLE = "Title";
        public const string ROSTER_EVENT_ID = "RosterEventId";
        public const string MASTER_ROSTER_ID = "MasterRosterId";
        public const string START_DATE = "StartDate";
        public const string END_DATE = "EndDate";
        public const string RECURRENCE = "Recurrence";
        public const string ALL_DAY_EVENT = "AllDayEvent";
        public const string PARENT_ROSTER_ID = "ParentRosterId";
        public const string CONTENT_TYPE_ID = "ContentTypeId";
        public const string WORKER_PERSON_ID = "WorkerPersonId";
        public const string USE_TIMESHEET = "UseTimesheet";
        public const string STATUS_ID = "StatusId";
        public const string WORKER_TEAMLEADER = "TeamLeader";
        public const string WORKER_MANAGER = "Manager";
        public const string WORKER_AD_ACCOUNT = "ADAccount";
        public const string MODIFIED_BY = "ModifiedBy";
        public const string MODIFIED = "Modified";

        // UserInformationTable
        public const string USER_LOGIN_NAME = "LoginName";
        public const string USER_DISPLAY_NAME = "DisplayName";
        public const string USER_EMAIL = "Email";
        public const string USER_MOBILE = "MobilePhone";
        public const string USER_SIP = "SIPAddress";
        public const string USER_DEPARTMENT = "Department";
        public const string USER_PRINCIPAL_TYPE = "PrincipalType";
        public const string USER_EXTERNAL_ID = "ExternalId";
    }

    public static class ViewNames
    {
        public const string WORKING_ROSTERS_FOR_TIMESHEETS = "Timesheets View";
    }

    public enum RosterStatus : int
    {
        Draft = 1,
        Active = 2,
        Terminated = 3,
        Unallocated = 4,
        Cancelled = 5,
        Unconfirmed = 6,
        Confirmed = 7
    }
}