using Oracle.ManagedDataAccess.Client;
using System;
using System.IO;

namespace HospitalApp.Services
{
    public class DBConnection
    {
        private const string DataSourceEnvVarName = "HOSPITALAPP_ORACLE_DATASOURCE";
        private const string DefaultDataSource = "localhost:1521/orclpdb";

        public static string GetEffectiveDataSource()
        {
            string? env = Environment.GetEnvironmentVariable(DataSourceEnvVarName);
            if (!string.IsNullOrWhiteSpace(env))
            {
                return env.Trim();
            }

            string? saved = TryReadUserDataSource();
            if (!string.IsNullOrWhiteSpace(saved))
            {
                return saved.Trim();
            }

            return DefaultDataSource;
        }

        public static void SaveUserDataSource(string dataSource)
        {
            string ds = dataSource?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(ds))
            {
                return;
            }

            string dir = GetConfigDirectory();
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path.Combine(dir, "datasource.txt"), ds);
        }

        private static string? TryReadUserDataSource()
        {
            try
            {
                string path = Path.Combine(GetConfigDirectory(), "datasource.txt");
                if (!File.Exists(path))
                {
                    return null;
                }

                string text = File.ReadAllText(path).Trim();
                return string.IsNullOrWhiteSpace(text) ? null : text;
            }
            catch
            {
                return null;
            }
        }

        private static string GetConfigDirectory()
        {
            // %AppData%\HospitalApp
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HospitalApp");
        }

        public OracleConnection GetConnection(string user, string pass)
        {
            return GetConnection(user, pass, GetEffectiveDataSource());
        }

        public OracleConnection GetConnection(string user, string pass, string dataSource)
        {
            OracleConnectionStringBuilder builder = new OracleConnectionStringBuilder();

            builder.UserID = user;
            builder.Password = pass;
            builder.DataSource = string.IsNullOrWhiteSpace(dataSource) ? DefaultDataSource : dataSource.Trim();

            // 👉 FIX SYS LOGIN
            if (user.ToUpper() == "SYS")
            {
                builder.ConnectionString += ";DBA Privilege=SYSDBA";
            }

            return new OracleConnection(builder.ToString());
        }
    }
}
