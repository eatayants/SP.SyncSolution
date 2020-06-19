using Microsoft.SharePoint.WebControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using Roster.Common;
using Roster.Presentation.Controls.Fields;

namespace Roster.Presentation.Controls.FieldControls
{
    public class DbLabelControl : DbBaseFieldControl
    {
        private Label BaseControl { get; set; }
        protected override void RecreateChildControls()
        {
            EnsureChildControls();
        }

        protected override void CreateChildControls()
        {
            Controls.Clear();
            if (Field != null)
            {
                BaseControl = new Label { ID = Field.InternalNameOriginal };
                Controls.Add(BaseControl);
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            AddAttributesToRender(writer);
            BaseControl.RenderControl(writer); // <label/>
        }

        public override object Value
        {
            get
            {
                EnsureChildControls();
                return BaseControl.Text;
            }
            set
            {
                EnsureChildControls();
                BaseControl.Text = value.ToSafeString();
            }
        }
    }
}
