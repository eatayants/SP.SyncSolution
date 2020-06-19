using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Roster.BL;
using Roster.Common;
using Roster.Model.DataContext;

namespace Roster.Presentation.Layouts
{
    public partial class DbListForm : LayoutsPageBase
    {
        private SPControlMode _mode = SPControlMode.Invalid;
        private readonly RosterConfigService _configService = new RosterConfigService();
        private ListMetadata _list;

        public SPControlMode ControlMode
        {
            get
            {
                if (_mode != SPControlMode.Invalid) return _mode;
                _mode = (SPControlMode)Request.QueryString["Mode"].ToInt();
                return _mode;
            }
            set
            {
                _mode = value;
            }
        }

        public Guid? ListId
        {
            get
            {
                return Request.QueryString["List"].ToGuid();
            }
        }

        public ListMetadata List
        {
            get { return _list ?? (_list = ListId.HasValue ? _configService.GetList(ListId.Value): null); }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            var pageName = @"List";
            if (List != null)
            {
                pageName = List.Name;
            }
            var itemName = @"Item";
            switch (ControlMode)
            {
                case SPControlMode.New:
                {
                    itemName = @"New Item";//SPResource.GetString("NewItem");
                    break;
                }
                case SPControlMode.Display:
                {
                    itemName = @"Display Item"; //SPResource.GetString("DisplayItem");
                    break;
                }
                case SPControlMode.Edit:
                {
                    itemName = @"Edit Item"; //SPResource.GetString("EditItem");
                    break;
                }
            }
            lbPageName.Text = string.Format("{0} - {1}", pageName, itemName);
            ltPageName.Text = lbPageName.Text;
        }
    }
}
