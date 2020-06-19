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
    public class DbGuidControl : DbBaseFieldControl
    {
        private HiddenField _hiddenField;

        protected override void RecreateChildControls()
        {
            EnsureChildControls();
        }

        protected override void CreateChildControls()
        {
            Controls.Clear();

            if (Field != null)
            {
                _hiddenField = new HiddenField {ID = Field.InternalName };
                this.Controls.Add(_hiddenField);
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            AddAttributesToRender(writer);
            _hiddenField.RenderControl(writer);
        }

        public override object Value
        {
            get
            {
                EnsureChildControls();
                return _hiddenField.Value.ToNullableGuid();
            }
            set
            {
                EnsureChildControls();
                _hiddenField.Value = value.ToSafeString();
            }
        }
    }
}
