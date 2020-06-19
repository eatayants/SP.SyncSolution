using Microsoft.SharePoint;
using System;
using System.Linq;

namespace Roster.Presentation.Helpers
{
    public class RoleService
    {
        // ------------------------------------------------------
        // Constants - SecGroups
        // "c:0!.s|windows" : "NT AUTHORITY\\authenticated users"
        // "c:0(.s|true"    : "Everyone"
        // ------------------------------------------------------
        public static int ACCOUNT_ID_NT_AUTHORITY = -1; // "NT AUTHORITY\\authenticated users"
        public static int ACCOUNT_ID_EVERYONE = -1;     // "Everyone"

        static RoleService()
        {
            if (ACCOUNT_ID_EVERYONE == -1 || ACCOUNT_ID_NT_AUTHORITY == -1)
            {
                var siteId = SPContext.Current.Site.ID;
                var webId = SPContext.Current.Web.ID;
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    using (var site = new SPSite(siteId))
                    using (var web = site.OpenWeb(webId))
                    {
                        ACCOUNT_ID_NT_AUTHORITY = web.EnsureUser("c:0!.s|windows").ID; // init "NT AUTHORITY\\authenticated users"
                        ACCOUNT_ID_EVERYONE = web.EnsureUser("c:0(.s|true").ID;        // init "Everyone"
                    }
                });
            }
        }

        public int GetGroupIdForRole(Constants.Role role, SPWeb web)
        {
            string groupName = EnumHelper.GetEnumDescription(role);
            SPGroup gr = FindGroupByName(groupName, web);

            if (gr == null)
                throw new ArgumentException("There is no sharepoint group for role '" + groupName + "'.");

            return gr.ID;
        }

        public void CreateGroupForRole(Constants.Role role, SPWeb web)
        {
            string groupName = EnumHelper.GetEnumDescription(role);
            SPGroup gr = FindGroupByName(groupName, web);

            if (gr == null)
                web.SiteGroups.Add(groupName, web.Site.Owner, web.CurrentUser, "");
        }

        private SPGroup FindGroupByName(string groupName, SPWeb web)
        {
            SPGroup gr = web.Groups.Cast<SPGroup>().FirstOrDefault(g => g.Name.ToLower() == groupName.ToLower());

            if (gr == null)
                gr = web.SiteGroups.Cast<SPGroup>().FirstOrDefault(g => g.Name.ToLower() == groupName.ToLower());

            return gr;
        }
    }
}
