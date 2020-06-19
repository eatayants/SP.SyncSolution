#region

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using Microsoft.SharePoint;
using SqlSync.Common;
using SqlSync.Model.DataContext;
using SqlSync.Model.Helpers;

#endregion

namespace SqlSync.BL.Providers
{
	public abstract class ProviderBase
	{
        #region Public variables

        // ------------------------------------------------------
        // Constants - SecGroups
        // "c:0!.s|windows" : "NT AUTHORITY\\authenticated users"
        // "c:0(.s|true"    : "Everyone"
        // ------------------------------------------------------
        public static int ACCOUNT_ID_NT_AUTHORITY = -1; // "NT AUTHORITY\\authenticated users"
        public static int ACCOUNT_ID_EVERYONE = -1;     // "Everyone"

        #endregion

		#region Service Members

        public List<int> GetTrusteeIds()
        {
            // get information about current user
            var currentUser = SPContext.Current.Web.CurrentUser;
            var trusteeIds = new List<int> { currentUser.ID };
            // add user ID
            trusteeIds.AddRange(currentUser.Groups.Cast<SPGroup>().Select(gr => gr.ID)); // add user Group IDs

            // add static Security Roles
            if (ACCOUNT_ID_EVERYONE == -1 || ACCOUNT_ID_NT_AUTHORITY == -1)
            {
                var siteId = SPContext.Current.Site.ID;
                var webId = SPContext.Current.Web.ID;
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    using (var eSite = new SPSite(siteId))
                    using (var eWeb = eSite.OpenWeb(webId))
                    {
                        bool oldValue = eWeb.AllowUnsafeUpdates;
                        try {
		                    eWeb.AllowUnsafeUpdates = true;
		                    ACCOUNT_ID_NT_AUTHORITY =   eWeb.EnsureUser("c:0!.s|windows").ID; // init "NT AUTHORITY\\authenticated users"
                            ACCOUNT_ID_EVERYONE =       eWeb.EnsureUser("c:0(.s|true").ID;    // init "Everyone"
	                    }
	                    catch (Exception) { }
	                    finally {
                            eWeb.AllowUnsafeUpdates = oldValue;
	                    }
                        
                    }
                });
            }
            trusteeIds.Add(ACCOUNT_ID_NT_AUTHORITY); // add "NT AUTHORITY\\authenticated users"
            trusteeIds.Add(ACCOUNT_ID_EVERYONE);     // add "Everyone"
            return trusteeIds;
        }

        private string _connectionString;

	    protected ProviderBase(string connectionString)
	    {
            _connectionString = connectionString;
	    }
        protected static void CopyColumns(object source, object dest)
		{
			EntityHelper.CopyColumns(source, dest);
		}

        public SqlSyncEntities CreateObjectContext()
		{
            return string.IsNullOrWhiteSpace(_connectionString) ?
                new SqlSyncEntities() : new SqlSyncEntities(_connectionString);
		}

		public void ReleaseObjectContextIfNotReused()
		{
			ReleaseObjectContext();
		}

		public void ReleaseObjectContext()
		{

		}
		#endregion
	}
}