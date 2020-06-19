using System;
using System.Web.UI;
using Roster.Common;
using Roster.BL;

namespace Roster.Presentation.WebParts.TemplateSelectorWebPart
{
    public partial class TemplateSelectorWebPartUserControl : UserControl
    {
        private int _sourceMasterId;

        public int MasterId_Target
        {
            get
            {
                if (this._sourceMasterId == 0) {
                    Int32.TryParse(Request.QueryString["ID"], out _sourceMasterId);
                }

                return this._sourceMasterId;
            }
        }
        public int MasterId_Source
        {
            get
            {
                int _id = 0;
                Int32.TryParse(this.hidValueHolder.Value, out _id);
                return _id;
            }
        }

        public TemplateSelectorWebPart WebPartInst { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnApplyTemplate_Click(object sender, EventArgs e)
        {
            try
            {
                //this.WebPartInst.TemplateId = this.hidValueHolder.Value.ToGuid();

                //new RosterDataService().ClonePlannedRosters(this.MasterId_Source, this.MasterId_Target);

            } catch (Exception ex) {
                pnlErrorInfo.Controls.Add(new System.Web.UI.WebControls.Label { Text = ex.Message, ForeColor = System.Drawing.Color.Red });
            }
        }
    }
}
