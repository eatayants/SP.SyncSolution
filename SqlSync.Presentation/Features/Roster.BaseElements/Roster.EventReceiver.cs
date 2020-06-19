using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.SharePoint;
using System.Collections.Generic;
using Roster.Presentation.Helpers;

namespace Roster.Presentation.Features.Roster.BaseElements
{
    [Guid("1cc04b19-06dc-438c-906b-ee5313b9b3a1")]
    public class RosterEventReceiver : SPFeatureReceiver
    {
        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            SPWeb web = (SPWeb)properties.Feature.Parent;

            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                using (SPSite eSite = new SPSite(web.Site.ID))
                using (SPWeb eWeb = eSite.OpenWeb(web.ID))
                {
                    var roleSrv = new RoleService();
                    List<Constants.Role> groups = new List<Constants.Role> { Constants.Role.RosterAdmins };
                    groups.ForEach(gr => roleSrv.CreateGroupForRole(gr, eWeb));
                }
            });
        }
    }
}
