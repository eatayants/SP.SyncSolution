using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint;
using SqlSync.Common;
using SqlSync.Model.DataContext;

namespace SqlSync.BL.Extentions
{
    public static class Extentions
    {
        public static void ValidateConnection(this ConnectionInfo connectionInfo)
	    {
            using (var connection = new SqlConnection(connectionInfo.GetConnectionString()))
			{
				var command = new SqlCommand("SELECT 1", connection);
				connection.Open();
				command.ExecuteNonQuery();
			}
	    }
    }
}
