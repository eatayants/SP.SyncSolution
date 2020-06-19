using System;
using System.Linq;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using SqlSync.Model.DataContext;
using System.Collections.Generic;
using SqlSync.BL;
using Microsoft.SharePoint.Utilities;
using SqlSync.SP.Extensions;
using System.Web.UI.WebControls;
using SqlSync.BL.Extentions;
using SqlSync.Common;
using SqlSync.SP.Helpers;

namespace SqlSync.SP.Layouts
{
    public partial class DatabaseSettingsPage : LayoutsPageBase
    {
        #region Private

        #endregion

        #region Public properties

        #endregion


        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Utils.GoBackOnSuccess(this, this.Context);
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var connectionInfo = new ConnectionInfo
                {
                    Server = txtServer.Text,
                    Database = txtDatabase.Text,
                    User = txtUser.Text,
                    Password = txtPassword.Text
                };
                connectionInfo.ValidateConnection();
                SPContext.Current.SetConnection(connectionInfo);
                Utils.GoBackOnSuccess(this, this.Context);
            }
            catch (Exception ex)
            {
                pannelError.Controls.Add(new Label { Text = ex.Message, ForeColor = System.Drawing.Color.Red });                
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                try
                {
                    var conInfo = SPContext.Current.GetConnection();
                    txtServer.Text = conInfo.Server;
                    txtDatabase.Text = conInfo.Database;
                    txtUser.Text = conInfo.User;
                    txtPassword.Text = conInfo.Password;
                }
                catch (Exception ex)
                {
                    pannelError.Controls.Add(new Label { Text = ex.Message, ForeColor = System.Drawing.Color.Red });
                }
            }
        }
    }
}
