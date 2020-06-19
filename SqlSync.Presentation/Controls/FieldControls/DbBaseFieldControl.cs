using Microsoft.SharePoint.WebControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using Roster.Presentation.Controls.Fields;

namespace Roster.Presentation.Controls.FieldControls
{
    public class DbBaseFieldControl : CompositeControl
    {
        public DbBaseFieldControl()
        {
            Subcontrols = new List<DbBaseFieldControl>();
        }
        public List<DbBaseFieldControl> Subcontrols { get; set; }
        public DbField Field { get; set; }
        public SPControlMode ControlMode { get; set; }

        public virtual object Value
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        public virtual void Validate()
        {
        }
    }
}
