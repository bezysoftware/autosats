using Microsoft.Data.Sqlite;
using System.IO;

namespace AutoSats.Data
{
    public static class DbInitializer
    {
        private const string QuartzScriptPath = "Data/Quartz.sql";

        public static void InitializeQuartzDatabase(string connectionString)
        {
            var builder = new SqliteConnectionStringBuilder(connectionString);
            var path = Path.GetDirectoryName(builder.DataSource);

            if (path != null && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            
            using var connection = new SqliteConnection(connectionString);
            
            connection.Open();

            if (new FileInfo(connection.DataSource).Length > 0)
            {
                return;
            }

            CreateQuartzTables(QuartzScriptPath, connection);
        }

        private static void CreateQuartzTables(string scriptPath, SqliteConnection connection)
        {
            var sql = File.ReadAllText(scriptPath);
            var command = new SqliteCommand(sql, connection);

            command.ExecuteNonQuery();
        }
    }
}
