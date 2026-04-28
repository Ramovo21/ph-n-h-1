using Oracle.ManagedDataAccess.Client;

namespace HospitalApp.Services
{
    public class DBConnection
    {
        public OracleConnection GetConnection(string user, string pass)
        {
            OracleConnectionStringBuilder builder = new OracleConnectionStringBuilder();

            builder.UserID = user;
            builder.Password = pass;
            builder.DataSource = "localhost:1521/orclpdb";

            // 👉 FIX SYS LOGIN
            if (user.ToUpper() == "SYS")
            {
                builder.ConnectionString += ";DBA Privilege=SYSDBA";
            }

            return new OracleConnection(builder.ToString());
        }
    }
}
