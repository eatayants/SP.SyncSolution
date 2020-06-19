namespace SqlSync.Common
{
	public enum TrackEventActions
	{
		I, U, D
	}
	public enum DefStatus
	{
		Unantive = 0,
		Active = 1
	}

    public enum DefTypes
    {
        Unspecified = 0,
        SqlSyncStatus = 1
    }

	public enum SqlSyncEventType
	{
		[StringValue("Planned SqlSync Event")]
		PlannedSqlSyncEvent = 0,
		[StringValue("Working SqlSync Event")]
		WorkingSqlSyncEvent = 1,
		[StringValue("Template SqlSync Event")]
		TemplateSqlSyncEvent = 2,
        [StringValue("TimeSheet SqlSync Event")]
        TimeSheetSqlSyncEvent = 3
	}

    public enum CompareType
    {
        [StringValue("Equal")]
        Equal = 0,
        [StringValue("Not Equal")]
        NotEqual = 1,
        [StringValue("Less")]
        Less = 2,
        [StringValue("More")]
        More = 3,
        [StringValue("Less Or Equal")]
		LessOrEqual = 4,
        [StringValue("More Or Equal")]
		MoreOrEqual = 5,
		[StringValue("IN")]
		InValue = 6,
        [StringValue("Not IN")]
        NotInValue = 7,
        [StringValue("Contains")]
        Contains = 8,
        [StringValue("Not Contains")]
        NotContains = 9
    }

    public enum ConcateOperator
    {
        And = 0,
        Or  = 1
    }

	public enum SortDirection
	{
		Unspecified = 0,
		Ascending = 1,
		Descending = 2,
    }

	public enum LookupSourceType
	{
		[StringValue("")]
		None = 0,
		[StringValue("SharePoint List")]
		SpList = 1,
		[StringValue("Database Table")]
        Table = 2,
        [StringValue("Custom Query")]
        Query = 3
	}
}