using Oracle.ManagedDataAccess.Client;
using System;

namespace HospitalApp.Services
{
    public class DBConnection
    {
        public OracleConnection GetConnection(string user, string pass)
        {
            OracleConnectionStringBuilder builder = new OracleConnectionStringBuilder();

            builder.UserID = user;
            builder.Password = pass;
            builder.DataSource =
                Environment.GetEnvironmentVariable("HOSPITALAPP_ORACLE_DATASOURCE")
                ?? "localhost:1521/XEPDB1";

            // 👉 FIX SYS LOGIN
            if (user.ToUpper() == "SYS")
            {
                builder.ConnectionString += ";DBA Privilege=SYSDBA";
            }

            return new OracleConnection(builder.ToString());
        }
    }
}
