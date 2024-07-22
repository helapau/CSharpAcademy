using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace HabitTracker.src
{
    enum Choices
    {
        Exit = 0,
        AddHabit = 1,
        ListAllHabits = 2,
        DeleteAHabitWithLogs = 3,

        ViewLogsOfHabit = 11,
        AddLogForHabit = 22,
        DeleteLogForHabit = 33,
        UpdateLogForHabit = 44
    }


    internal static class HabitTracker
    {


        public static void Run()
        {
            string ConnectionString = $"Data Source={Path.Combine(AppSettings.PROJECT_ROOT_DIR, "habitsdb.sqlite3")}";
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                _Run(connection);
            }
        }



        private static void _Run(SqliteConnection connection)
        {
            DataStore db = new(connection);

            bool isRunning = true;


            while (isRunning)
            {
                ShowMainMenu();
                Choices choice = GetValidMenuChoice();
                try
                {
                    switch (choice)
                    {
                        case Choices.Exit:
                            isRunning = false;
                            break;
                        case Choices.AddHabit:
                            AddHabit(db, connection);
                            break;
                        case Choices.ListAllHabits:
                            ListAllHabits(db, connection);
                            break;
                        case Choices.DeleteAHabitWithLogs:
                            DeleteHabitWithLogs(db, connection);
                            break;
                        case Choices.ViewLogsOfHabit:
                            ViewLogsOfHabit(db, connection);
                            break;
                        case Choices.AddLogForHabit:
                            AddLogForHabit();
                            break;
                        case Choices.DeleteLogForHabit:
                            DeleteLogForHabit();
                            break;
                        case Choices.UpdateLogForHabit:
                            UpdateLogForHabit();
                            break;
                        default:
                            Console.WriteLine("Invalid choice.");
                            break;
                    }
                }
                catch (Exception error)
                {
                    Console.WriteLine("Error! " + error.Message);
                    isRunning = false;
                }
            }

        }

        private static Choices GetValidMenuChoice()
        {
            Choices choice = 0;
            do
            {
                int maybeResult = UserInput.GetValidIntInput();
                if (Enum.IsDefined(typeof(Choices), maybeResult))
                {
                    choice = (Choices)maybeResult;
                    break;
                }
                else
                {
                    Console.WriteLine("That's not a valid choice, try again.");
                }
            } while (true);
            return choice;
        }



        private static void ShowMainMenu()
        {
            Console.WriteLine("\nMAIN MENU");
            Console.WriteLine("What would you like to do? Press the corresponding key to select an action:\n");

            Console.WriteLine("0 - Exit");
            Console.WriteLine("1 - Start tracking a new habit");
            Console.WriteLine("2 - List all habits");
            Console.WriteLine("3 - Delete a habit and all its logs\n");

            Console.WriteLine("11 - View all logs for a given habit");
            Console.WriteLine("22 - Add a log for a given habit");
            Console.WriteLine("33 - Delete a log for a given habit");
            Console.WriteLine("44 - Update a log for a given habit");
        }

        private static void AddHabit(DataStore db, SqliteConnection connection)
        {

            string habit = UserInput.GetValidHabitName();
            try
            {
                db.AddHabit(habit, connection);
                Console.WriteLine("Habit successfully added.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

        }

        private static void ListAllHabits(DataStore db, SqliteConnection connection)
        {
            List<string> habits = db.ListAllHabits(connection);
            if (habits.Count == 0)
            {
                Console.WriteLine("Currently you're not tracking any habits.");
                return;
            }
            Console.WriteLine("Listing all tracked habits:");
            foreach (string habit in habits)
            {
                Console.WriteLine($"{habit}");
            }
            Console.WriteLine(string.Empty);
        }

        private static void DeleteHabitWithLogs(DataStore db, SqliteConnection connection)
        {
            string habitName = UserInput.GetValidHabitName();
            try
            {
                db.DeleteHabit(habitName, connection);
                Console.WriteLine("Habit deleted.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error - {ex}");
            }
        }

        private static void ViewLogsOfHabit(DataStore db, SqliteConnection connection)
        {
            string habitName = UserInput.GetValidHabitName();
            try
            {
                List<HabitRecord> result = db.ViewLogsForHabit(habitName, connection);
                if (result.Count == 0)
                {
                    Console.WriteLine("No logs found.");
                }
                else
                {
                    Console.WriteLine($"Logs for habit {habitName}:");
                    foreach (HabitRecord habit in result)
                    {
                        Console.WriteLine($"Amount: {habit.Amount}, Last modified: {habit.Date}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error - {ex}");
            }


        }
    }

    
}
