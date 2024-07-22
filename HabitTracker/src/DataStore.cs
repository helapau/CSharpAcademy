using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Data.Sqlite;

namespace HabitTracker.src
{
    public record struct HabitRecord(string Date, int Amount);

    internal class DataStore
    {
        // the driver creates the Database if it doesn't exist
        // it uses pooling by deafult so the open connection is reused
        private static readonly string ConnectionString = $"Data Source={Path.Combine(AppSettings.PROJECT_ROOT_DIR, "habitsdb.sqlite3")}";



        public DataStore(SqliteConnection connection)
        {
            string createHabitsTable = @"
                CREATE TABLE IF NOT EXISTS Habits
                (Name TEXT NOT NULL PRIMARY KEY);
            ";
            string createHabitLogsTable = @"
                CREATE TABLE IF NOT EXISTS HabitLogs
                (HabitName TEXT NOT NULL,
                Amount INTEGER NOT NULL,
                Date TEXT NOT NULL)
                
                FOREIGN KEY(HabitName) REFERENCES Habits.Name
                PRIMARY KEY(HabitName, Date);                
            ";

            var command = connection.CreateCommand();
            command.CommandText = createHabitsTable + createHabitLogsTable;
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Insert a new row into the HabitsTable
        /// </summary>
        public void AddHabit(string name, SqliteConnection connection)
        {
            using (var transaction = connection.BeginTransaction())
            {

                var select = connection.CreateCommand();
                select.CommandText = @"SELECT Name from Habits;";
                bool nameCollision = false;
                using (var reader = select.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.GetString(0) == name)
                        {
                            nameCollision = true;
                            break;
                        }
                    }
                }

                if (nameCollision)
                {
                    throw new Exception("You are already tracking this habit.");
                }

                var insert = connection.CreateCommand();
                insert.CommandText = @"INSERT INTO Habits (Name) VALUES ($name)";
                insert.Parameters.AddWithValue("name", name);
                insert.ExecuteNonQuery();
                transaction.Commit();
            }
        }

        public List<string> ListAllHabits(SqliteConnection connection)
        {
            List<string> result = new();
            var select = connection.CreateCommand();
            select.CommandText = @"SELECT Name FROM Habits;";
            using (var reader = select.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add(reader.GetString(0));
                }
            }

            return result;
        }

        public bool HabitExists(SqliteConnection connection, string habit)
        {
            var select = connection.CreateCommand();
            select.CommandText = @"SELECT Name FROM Habits WHERE Name = $habit;";
            select.Parameters.AddWithValue("habit", habit);
            bool result = false;
            using (var reader = select.ExecuteReader())
            {
                while (reader.Read())
                {
                    result = true;
                    break;
                }
            }
            return result;
        }


        public void DeleteHabit(string name, SqliteConnection connection)
        {
            using (var transaction = connection.BeginTransaction())
            {
                bool habitExists = HabitExists(connection, name);
                if (!habitExists)
                {
                    throw new Exception("No such habit - " + name);
                }

                var deleteLogs = connection.CreateCommand();
                deleteLogs.CommandText = @"DELETE FROM HabitLogs WHERE HabitName = $name";
                deleteLogs.Parameters.AddWithValue("name", name);
                deleteLogs.ExecuteNonQuery();

                var delete = connection.CreateCommand();
                delete.CommandText = "DELETE FROM Habits WHERE Name = $name";
                delete.Parameters.AddWithValue("name", name);
                delete.ExecuteNonQuery();

                transaction.Commit();
            }
        }

        public List<HabitRecord> ViewLogsForHabit(string habit, SqliteConnection connection)
        {
            List<HabitRecord> result = new();
            var select = connection.CreateCommand();
            select.CommandText = @"SELECT Amount, Date FROM HabitLogs WHERE HabitName = $habit";
            select.Parameters.AddWithValue("habit", habit);
            using (var reader = select.ExecuteReader())
            {
                while (reader.Read())
                {
                    int amount = reader.GetInt32(0);
                    string date = reader.GetString(1);
                    result.Add(new HabitRecord(date, amount));                   
                }
            }
            return result;           
        }

        public void AddLog(string habit, int amount, SqliteConnection connection)
        { 
            
        }

        

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
