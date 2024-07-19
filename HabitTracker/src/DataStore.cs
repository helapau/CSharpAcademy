using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace HabitTracker.src
{
    public record struct HabitRecord(string Date, int Amount);
    
    internal class DataStore
    {
        // the driver creates the Database if it doesn't exist
        // it uses pooling by deafult so the open connection is reused
        private static readonly string ConnectionString = $"Data Source={Path.Combine(AppSettings.PROJECT_ROOT_DIR, "habitsdb.sqlite3")}";

        private static readonly string HabitsTable = "Habits";
        private static readonly string HabitLogsTable = "HabitLogs";        
        

        private static void CreateHabitsTableIfNotExists()
        {
            var commandBuilder = new StringBuilder();
            commandBuilder.Append("CREATE TABLE IF NOT EXISTS " + HabitsTable + " ");
            commandBuilder.Append(@"
                    (                        
                        Name TEXT NOT NULL PRIMARY KEY
                    )
            ");

            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                var command = connection.CreateCommand();                
                command.CommandText = commandBuilder.ToString();
                command.ExecuteNonQuery();
            }
        } 

        private static void CreateHabitLogsTableIfNotExists()
        {
            var commandBuilder = new StringBuilder();
            commandBuilder.Append("CREATE TABLE IF NOT EXISTS " + HabitLogsTable + " ");
            commandBuilder.Append(@"
                    (
                        HabitName INTEGER NOT NULL,
                        Amount INTEGER NOT NULL,
                        Date TEXT NOT NULL PRIMARY KEY,
	                ");
            commandBuilder.Append($"FOREIGN KEY(HabitName) REFERENCES {HabitsTable}(Name)");
            commandBuilder.Append(");");

            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = commandBuilder.ToString();
                command.ExecuteNonQuery();
            }
        }

        public DataStore()
        {
            CreateHabitsTableIfNotExists();
            CreateHabitLogsTableIfNotExists();            
        }

        /// <summary>
        /// Insert a new row into the HabitsTable
        /// </summary>
        public void AddHabit(string name)
        {
            
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = $"INSERT INTO {HabitsTable} (Name)" + @"
                    VALUES (@name)
                ";
                command.Parameters.AddWithValue("name", name);
                command.ExecuteNonQuery();
            }
        }

        public List<string> ListAllHabits()
        {
            List<string> result = new();
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = $"SELECT Name FROM {HabitsTable}";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(reader.GetString(0));
                    }
                }

            }
                return result;
        }

        public void DeleteHabit(string name)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var deleteLogs = connection.CreateCommand();
                deleteLogs.CommandText = $@"
                    DELETE FROM {HabitLogsTable} WHERE HabitName = @name;
                ";
                deleteLogs.Parameters.AddWithValue("name", name);
                deleteLogs.ExecuteNonQuery();

                var command = connection.CreateCommand();
                command.CommandText = @$"DELETE FROM {HabitsTable} WHERE Name = @name";
                command.Parameters.AddWithValue("name", name);
                command.ExecuteNonQuery();
            }
        }

        public List<HabitRecord> ViewLogsForHabit(string habit)
        {
            List<HabitRecord> result = new();
            using (var connextion = new SqliteConnection(ConnectionString))
            { 
                connextion.Open();
                var command = connextion.CreateCommand();                
                command.CommandText = @$"
                    SELECT Amount, Date
                    FROM {HabitLogsTable}                    
                    WHERE HabitName = @habit
                ";
                command.Parameters.AddWithValue("habit", habit);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int amount = reader.GetInt32(0);
                        string date = reader.GetString(1);
                        result.Add(new HabitRecord(date, amount));                       
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Returns today's date in the local format
        /// </summary>
        /// <returns>string</returns>
        public static string GetToday() => DateTime.Today.ToString().Split(" ")[0];

        public HabitRecord? GetLogForDate(string habit, string date)
        {             
            HabitRecord? record = null;
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = $@"
                    SELECT Amount, Date FROM {HabitLogsTable}
                    WHERE HabitName = @habit
                    AND Date = @today
                ";
                command.Parameters.AddWithValue("habit", habit);
                command.Parameters.AddWithValue("today", date);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        record = new(reader.GetString(1), reader.GetInt32(0));
                    }
                }                
            }
            return record;
        }

        public void UpsertLogForHabit(string habit, int amount)
        {
            
            using (var conn = new SqliteConnection(ConnectionString))
            {
                conn.Open();
                string today = GetToday();
                HabitRecord? todaysRecordMaybe = GetLogForDate(habit, today);                
                if (todaysRecordMaybe is not null)
                {

                    UpdateLogForHabit(habit, amount, today);
                }
                else
                {
                    List<string> habits = ListAllHabits();
                    if (!habits.Contains(habit))
                    {
                        AddHabit(habit);
                    }
                    string date = GetToday();
                    var command = conn.CreateCommand();
                    command.CommandText = $@"
                    INSERT INTO {HabitLogsTable} (HabitName, Amount, Date)
                    VALUES (@name, @amount, @date)                    
                    ";
                    command.Parameters.AddWithValue("amount", amount);
                    command.Parameters.AddWithValue("date", date);
                    command.Parameters.AddWithValue("name", habit);
                    command.ExecuteNonQuery();
                }                
            }
        }

        public void DeleteLogForHabit(string habit, string date)
        {
            using (var conn = new SqliteConnection(ConnectionString))
            {
                conn.Open();

                var deleteHabit = conn.CreateCommand();
                deleteHabit.CommandText = $@"
                    DELETE FROM {HabitLogsTable}
                    WHERE HabitName = @name                    
                    AND Date = @date
                ";
                deleteHabit.Parameters.AddWithValue("name", habit);
                deleteHabit.Parameters.AddWithValue("date", date);
                deleteHabit.ExecuteNonQuery();
            }
        }

        public void UpdateLogForHabit(string habit, int amount, string date)
        {
            HabitRecord? recordMaybe = GetLogForDate(habit, date);
            if (recordMaybe is null)
            {
                return;
            }
            using (var conn = new SqliteConnection(ConnectionString))
            { 
                conn.Open();
                var command = conn.CreateCommand();
                command.CommandText = $@"
                    UPDATE {HabitLogsTable}
                    SET Amount = @amount                        
                    WHERE HabitName = @habit
                    AND Date = @date;                    
                ";
                command.Parameters.AddWithValue("amount", amount);
                command.Parameters.AddWithValue("date", date);
                command.Parameters.AddWithValue("habit", habit);
                command.ExecuteNonQuery();
            }
        }
    }
}
