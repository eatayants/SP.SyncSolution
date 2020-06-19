using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roster.Presentation.ColourCoding
{
    public class DynamicColourSettings
    {
        private List<DynamicCondition> _conditions = new List<DynamicCondition>();

        public List<DynamicCondition> conditions
        {
            get { return _conditions; }
            set { _conditions = value; }
        }

        public string matchingField { get; set; }

        public string filterFieldsColl { get; set; }

        public string[] GetFiltersArray()
        {
            return this.filterFieldsColl.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
        }

        public bool IsEmpty
        {
            get
            {
                return (_conditions == null || !_conditions.Any());
            }
        }
    }
}
