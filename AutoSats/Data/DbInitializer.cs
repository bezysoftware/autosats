using System.Data.SQLite;
using System.IO;

namespace AutoSats.Data
{
    public static class DbInitializer
    {
        private const string QuartzScriptPath = "Data/Quartz.sql";

        public static void InitializeQuartzDatabase(string connectionString)
        {
            using var connection = new SQLiteConnection(connectionString);
            
            connection.Open();

            if (new FileInfo(connection.FileName).Length > 0)
            {
                return;
            }

            CreateQuartzTables(QuartzScriptPath, connection);
        }

        private static void CreateQuartzTables(string scriptPath, SQLiteConnection connection)
        {
            var sql = File.ReadAllText(scriptPath);
            var command = new SQLiteCommand(connection)
            {
                CommandText = sql
            };

            command.ExecuteNonQuery();
        }
    }
}
