using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Roster.Presentation.Controls
{
    public class PlainLookupFieldTemplate :ITemplate
    {
        private string columnName_ID;
        private string columnName_VALUE;

        public PlainLookupFieldTemplate(string colId, string colValue)
        {
            this.columnName_ID = colId;
            this.columnName_VALUE = colValue;
        }

        public void InstantiateIn(System.Web.UI.Control container)
        {
            Literal lc = new Literal();
            lc.DataBinding += lc_DataBinding;
            container.Controls.Add(lc);
        }

        void lc_DataBinding(object sender, EventArgs e)
        {
            Literal literal = (Literal)sender;
            GridViewRow container = (GridViewRow)literal.NamingContainer;
            object _id = DataBinder.Eval(container.DataItem, columnName_ID);
            object _value = DataBinder.Eval(container.DataItem, columnName_VALUE);

            if (_id != DBNull.Value && _value != DBNull.Value && !string.IsNullOrEmpty(_id.ToString())) {
                literal.Text = string.Format("{0};#{1}", _id, _value);
            }
        }
    }
}
