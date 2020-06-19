using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roster.Presentation.ColourCoding
{
    public class StaticCondition
    {
        // FieldId
        public string Id { get; set; }
        public string camlOperator { get; set; }
        public string filterValue { get; set; }
        public string color { get; set; }
        public string fontColor { get; set; }
    }
}
