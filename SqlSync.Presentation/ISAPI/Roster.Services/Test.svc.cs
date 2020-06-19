using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client.Services;

namespace Roster.Services
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class Test : ITest
    {
        public string CallTest()
        {
            return "CallTest";
        }
    }
}
