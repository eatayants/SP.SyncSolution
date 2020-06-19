using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roster.Model.DataContext.Extention
{
    public class MasterRoster : TableDynamic
    {
        public string IsTemplate = "IsTemplate";
        public string Description = "Description";
        public MasterRoster()
        {
            PrimaryField = "Id";
            Name = "MasterRoster";
        }
    }
}
