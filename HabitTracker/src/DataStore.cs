using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace HabitTracker.src
{
    public record HabitRecord(string HabitName, long Timestamp, int Amount, string AccomplishmentMessage);
    internal static class DataStore
    {
        public static readonly string DBName = "habitsdb.sqlite3";
        public static readonly string DbFilePath = Path.Combine(AppSettings.PROJECT_ROOT_DIR, DBName);
        public static readonly string ConnectionString = $"Data Source={DBName}";
        public static readonly string TableName = "HabitTracker";
        
        public static bool DBFileExists()
        {
            if (!Path.Exists(AppSettings.PROJECT_ROOT_DIR))
            {
                throw new Exception("Project root directory does not exist!");
            }            
            return Path.Exists(DbFilePath);            
        }       

        public static void EnsureDBCreated()
        {
            if (!DBFileExists())
            {
                File.WriteAllText(DbFilePath, string.Empty);
            }
        }

        public static bool TableExists(string tableName)
        { 
            EnsureDBCreated();

            bool tableExists = true;
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"SELECT HabitName FROM HabitTracker";               
                try
                {
                    command.ExecuteNonQuery();
                }
                catch (SqliteException e)
                {
                    tableExists = !e.Message.Contains("no such table", StringComparison.CurrentCultureIgnoreCase); // will be something like "no such table 'tableName' or 'tableName' already exists                     
                }
            }
            return tableExists;
        }

        public static void CreateHabitsTable()
        {
            if (TableExists(TableName))
                return;

            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                    @"CREATE TABLE HabitTracker(
                        HabitName TEXT NOT NULL,
                        Timestamp INTEGER NOT NULL,
                        Amount INTEGER NOT NULL,
                        AccomplishmentMessage TEXT NOT NULL
                    )";

                command.ExecuteNonQuery();
            }
        }

        public static void Insert()
        {
            var testRecord = new HabitRecord("Sit-ups", DateTimeOffset.Parse("2024-07-18").ToUnixTimeSeconds(), 0, "I haven't done any sit-ups today :(");
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO HabitTracker
                    VALUES (@HabitName, @TimeStamp, @Amount, @Msg);
";
                command.Parameters.AddWithValue("@HabitName", testRecord.HabitName);
                command.Parameters.AddWithValue("@TimeStamp", testRecord.Timestamp);
                command.Parameters.AddWithValue("@Amount", testRecord.Amount);
                command.Parameters.AddWithValue("@Msg", testRecord.AccomplishmentMessage);

                command.ExecuteNonQuery();
            }
        }

        public static void Select()
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"SELECT HabitName FROM " + TableName + ";";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine(reader.GetString(0));
                    }
                }
            }
        }

        


    }
}
