#region

using System;
using System.Collections.Generic;
using System.Dynamic;

#endregion

namespace SqlSync.Common
{
	public static class SystemSettingName
	{
		public const string DatabaseVersion = "database_version";
	}

	public static class EntityOperations
	{
		public const string Add = "Add";
		public const string Update = "Update";
		public const string Delete = "Delete";
		public const string Get = "Get";
		public const string List = "List";
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

    public enum SqlSyncStatus : int
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