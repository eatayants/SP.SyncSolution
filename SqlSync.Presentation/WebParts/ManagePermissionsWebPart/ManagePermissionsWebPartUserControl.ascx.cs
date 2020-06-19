using System;
using System.Linq;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using Roster.Common;
using Roster.Model.DataContext;
using Roster.BL;
using Roster.Presentation.Helpers;
using System.Web.UI;

namespace Roster.Presentation.WebParts.ManagePermissionsWebPart
{
    public partial class ManagePermissionsWebPartUserControl : UserControl
    {
        #region Private variables

        private readonly RosterConfigService _confService = new RosterConfigService();

        #endregion

        #region Public properites

        public Guid? ItemId
        {
            get
            {
                return Request.QueryString["ElemId"].ToNullableGuid();
            }
        }

        public Guid? ListId
        {
            get
            {
                return Request.QueryString["ListId"].ToNullableGuid();
            }
        }

        public List<AccessControlItem> ItemPermissions
        {
            get
            {
                return (this.ItemId.HasValue) ?
                    _confService.GetItemAccessControls(this.ItemId.Value) : null;
            }
        }

        public List<AccessControlList> ListPermissions
        {
            get
            {
                return (this.ListId.HasValue) ?
                    _confService.GetListAccessControls(this.ListId.Value) : null;
            }
        }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                try
                {
                    List<PermissionItm> pi = new List<PermissionItm>();
                    bool isInheritedPerms = false;

                    if (!this.ItemId.HasValue && !this.ListId.HasValue) {
                        throw new Exception("Cannot find one of the following url params: 'ListId', 'ElemId'");
                    }

                    // try get Item permissions
                    if (this.ItemId.HasValue)
                    {
                        if (this.ListId.HasValue && !_confService.HasRights(this.ItemId.Value, this.ListId.Value, AccessRight.Control))
                            throw new Exception("Only users with FullControl rights can view/edit data on this page!");

                        var itmPerms = this.ItemPermissions;
                        if (itmPerms != null && itmPerms.Any()) {
                            itmPerms.ForEach(ipItm => pi.Add(new PermissionItm(ipItm)));
                        } else {
                            isInheritedPerms = true;
                        }
                    }

                    // if still no permissions - try get from List
                    if (this.ListId.HasValue && !pi.Any()) {
                        this.ListPermissions.ForEach(lpItm => pi.Add(new PermissionItm(lpItm)));
                    }

                    JavaScriptSerializer jsSer = new JavaScriptSerializer();
                    hidDataInfo.Value = jsSer.Serialize(pi.OrderBy(x => x.rights));

                    // show/hide Inheritance panel
                    statusPanel.Visible = isInheritedPerms;
                }
                catch (Exception ex)
                {
                    pnlErrorPanel.Controls.Add(new System.Web.UI.WebControls.Literal { Text = ex.Message });
                    statusPanel.Visible = btnSave.Visible = false;
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                JavaScriptSerializer jsSer = new JavaScriptSerializer();
                List<PermissionItm> piList = jsSer.Deserialize<List<PermissionItm>>(hidDataInfo.Value);

                var nonEmptyPiList = piList.Where(x => !string.IsNullOrEmpty(x.key));
                if (!nonEmptyPiList.Any() && !this.ItemId.HasValue)
                    throw new Exception("Please select minimum one user/group!"); // if set permissions for Item when it's possible to remove all -> will mean inheritance from List

                // clear old permissions
                if (this.ItemId.HasValue)
                {
                    var itmPerms = this.ItemPermissions;
                    if (itmPerms != null && itmPerms.Any()) {
                        itmPerms.ForEach(ipItm => _confService.DeleteAccessControlItem(ipItm.Id));
                    }
                }
                else
                {
                    var lstPerms = this.ListPermissions;
                    if (lstPerms != null && lstPerms.Any()) {
                        lstPerms.ForEach(lpItm => _confService.DeleteAccessControlList(lpItm.Id));
                    }
                }

                if (nonEmptyPiList.Any())
                {
                    Guid siteId = SPContext.Current.Site.ID;
                    Guid webId = SPContext.Current.Web.ID;
                    SPSecurity.RunWithElevatedPrivileges(delegate()
                    {
                        using (SPSite site = new SPSite(siteId))
                        using (SPWeb web = site.OpenWeb(webId))
                        {
                            foreach (var pi in nonEmptyPiList.GroupBy(g => g.key).Select(g => g.LastOrDefault()))
                            {
                                if (this.ItemId.HasValue)
                                {
                                    _confService.SaveAccessControlItem(new AccessControlItem {
                                        AccessRight = pi.rights, Id = Guid.NewGuid(), ItemId = this.ItemId.Value,
                                        ListMetadataId = this.ListId.Value,
                                        TrusteeId = pi.id > 0 ? pi.id : web.EnsureUser(pi.key).ID // if (id > 0) - it is a Group
                                    });
                                }
                                else
                                {
                                    _confService.SaveAccessControlList(new AccessControlList {
                                        AccessRight = pi.rights, Id = Guid.NewGuid(), ListMetadataId = this.ListId.Value,
                                        TrusteeId = pi.id > 0 ? pi.id : web.EnsureUser(pi.key).ID
                                    });
                                }
                            }
                        }
                    });
                }

                Utils.GoBackOnSuccess(this.Page, this.Context);
            }
            catch (Exception ex)
            {
                pnlErrorPanel.Controls.Add(new System.Web.UI.WebControls.Literal {
                    Text = ex.ToReadbleSrting("Saving Error"),
                });
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Utils.GoBackOnSuccess(this.Page, this.Context);
        }


        private class PermissionItm
        {
            public string key { get; set; }
            public int id { get; set; }
            public int rights { get; set; }

            public PermissionItm()
            {
                this.rights = (int)AccessRight.Read;
            }

            public PermissionItm(int trusteeId, int accessLevelId)
            {
                SPUser userVal = new SPFieldUserValue(SPContext.Current.Web, trusteeId, string.Empty).User;

                try
                {
                    if (userVal == null) {
                        this.key = SPContext.Current.Web.SiteGroups.GetByID(trusteeId).Name;
                    } else if (userVal.IsDomainGroup) {
                        this.key = userVal.Name;
                    } else {
                        this.key = userVal.LoginName;
                    }
                } catch {
                    this.key = string.Empty;
                }
                this.rights = accessLevelId;
            }

            public PermissionItm(AccessControlItem acItem)
                : this(acItem.TrusteeId, acItem.AccessRight)
            {
            }

            public PermissionItm(AccessControlList acList)
                : this(acList.TrusteeId, acList.AccessRight)
            {
            }
        }
    }
}
