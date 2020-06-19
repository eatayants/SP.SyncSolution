using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Roster.Presentation.Controls.Fields;

namespace Roster.Presentation.Controls.FieldEditors
{
    public class DbFieldRecurrenceEditor : CompositeControl, IDbFieldEditor
    {
        #region Private variables

        #endregion

        #region Properties


        #endregion

        protected override void RecreateChildControls()
        {
            EnsureChildControls();
        }

        protected override void CreateChildControls()
        {
            Controls.Clear();
        }

        protected override void Render(HtmlTextWriter writer)
        {
            AddAttributesToRender(writer);
        }

        #region Interface methods

        public DbField GetField(string fieldName)
        {
            return new DbFieldRecurrence(fieldName);
        }

        #endregion
    }
}
