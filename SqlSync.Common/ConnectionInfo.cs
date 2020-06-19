using System.Data.SqlClient;

namespace Roster.Common
{
    public class ConnectionInfo
    {
        public void Init()
        {
#if Debug
            Server = @"localhost";
            Database = @"RosterDb";
            User = @"Roster";
            Password = @"Roster123456";
#endif

        }
        public string Server { get; set; }
        public string Database { get; set; }
        public string User { get; set; }
        public string Password { get; set; }

        public bool IsEmpty()
        {
            return string.IsNullOrWhiteSpace(string.Format("{0}{1},{2},{3}", Server, Database, User, Password));
        }

        public string GetConnectionString()
        {
            if (IsEmpty())
            {
                Init();
            }
            var builder = new SqlConnectionStringBuilder();
            builder["Data Source"] = Server;
            builder["integrated Security"] = false;
            builder["user"] = User;
            builder["password"] = Password;
            builder["Initial Catalog"] = Database;
            return builder.ConnectionString;
        }

        public override string ToString()
        {
            return string.Format("metadata=res://*/{0}.csdl|res://*/{0}.ssdl|res://*/{0}.msl;provider=System.Data.SqlClient;" +
                        "provider connection string=\"{1}\"", "DataContext.RosterModel", GetConnectionString());
        }
    }

    public class NavConnectionInfo
    {
        public string CreateTimesheetUrl { get; set; }
        public string ProcessTimesheetsUrl { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Mapping { get; set; }

        public bool IsEmpty()
        {
            return string.IsNullOrWhiteSpace(CreateTimesheetUrl) || string.IsNullOrWhiteSpace(ProcessTimesheetsUrl) ||
                   string.IsNullOrWhiteSpace(User) || string.IsNullOrWhiteSpace(Password);
        }
    }
}
