using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.SharePoint;
using System.Collections.Generic;
using SqlSync.SP.Helpers;
using System.Web.ApplicationServices;

namespace SqlSync.SP.Features.SqlSync.BaseElements
{
    [Guid("b03e1ae1-040a-473d-873f-b094390ec11f")]
    public class SqlSyncEventReceiver : SPFeatureReceiver
    {
    }
}
