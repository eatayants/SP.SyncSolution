using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roster.Model.DataContext.Extention
{
    public class EventStatusProperty : TableDynamic
    {
        public EventStatusProperty()
        {
            PrimaryField = "Id";
            Name = "vwEventStatusProperty";
            ParentField = "EventStatusId";

            ParentName = "vwEventStatus";
            ParentPrimariField = "Id";
        }
    }
}
