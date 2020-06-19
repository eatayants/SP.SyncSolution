using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roster.Presentation.ColourCoding
{
    public class StaticColourSettings
    {
        private List<StaticCondition> _conditions = new List<StaticCondition>();

        public List<StaticCondition> Conditions
        {
            get { return _conditions; }
            set { _conditions = value; }
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
