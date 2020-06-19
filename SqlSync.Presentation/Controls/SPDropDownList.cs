using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace Roster.Presentation.Controls
{   public class SPDropDownList:DropDownList
    {
        protected override void LoadViewState(Object savedState)
        {
            if (savedState != null)
            {
                base.LoadViewState(savedState);
            }
        }

        protected override object SaveViewState()
        {
            return null;
        }
    }
}
