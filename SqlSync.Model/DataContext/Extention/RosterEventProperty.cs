using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roster.Model.DataContext.Extention
{
    public class RosterEventProperty : TableDynamic
    {
        public RosterEventProperty()
        {
            PrimaryField = "Id";
            Name = "RosterEventProperty";
            ParentField = "RosterEventId";

            ParentName = "RosterEvent";
            ParentPrimariField = "Id";

            TemplateEventSet = "TemplateEventSet";
            TemplateEventKey = "TemplateEventId";
        }

        public string TemplateEventSet { get; set; }
        public string TemplateEventKey { get; set; }
    }
}
