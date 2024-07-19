using System.Globalization;
using HabitTracker.src;

DataStore db = new();
bool isRunning = true;
int[] CHOICES = { 0, 1, 2, 3, 11, 22, 33, 44};

while (isRunning)
{
    ShowMainMenu();
    int choice = GetValidMenuChoice();
    try
    {
        switch (choice)
        {
            case 0:
                isRunning = false;
                break;
            case 1:
                AddHabit();
                break;
            case 2:
                ListAllHabits();
                break;
            case 3:
                DeleteAHabitWithLogs();
                break;
            case 11:
                ViewLogsOfHabit();
                break;
            case 22:
                AddLogForHabit();
                break;
            case 33:
                DeleteLogForHabit();
                break;
            case 44:
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

void ShowMainMenu()
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
};

int GetValidMenuChoice()
{
    int choice = 0;
    do
    {
        int maybeResult = GetValidIntInput();
        if (CHOICES.Contains(maybeResult))
        {
            choice = maybeResult;
            break;
        }
        else
        {
            Console.WriteLine("That's not a valid choice, try again.");
        }
    } while (true);
    return choice;
}

int GetValidIntInput()
{    
    string? input;
    do
    {
        input = Console.ReadLine();
        if (input is string && int.TryParse(input, out int result))
        {
            return result;
        }
        else
        {
            Console.WriteLine("That's not a valid integer, try again.");
        }
    } while (true);
    
}

string GetValidHabitName()
{
    string result = string.Empty;
    do
    {
        Console.WriteLine("Please type the name of the habit: ");
        string? input = Console.ReadLine();
        if (input is string && !string.IsNullOrEmpty(input) && input.Length <= 100)
        {
            result = input;
        }
        else
        {
            Console.WriteLine("Please enter a non-empty string that's less that 100 characters.");
        }

    } while (result == string.Empty);
    return result;
}

string GetValidDateInput()
{
    
    Console.WriteLine("Enter the year as integer: ");
    int year = GetValidIntInput();
    Console.WriteLine("Enter the month as integer: ");
    int month = GetValidIntInput();
    Console.WriteLine("Enter the day as integer: ");
    int day = GetValidIntInput();
    DateTime dt = DateTime.ParseExact($"{month}/{day}/{year}", "M/d/yyyy", CultureInfo.InvariantCulture);
    return dt.ToString().Split(" ")[0];
}

void AddHabit()
{
    
    string habit = GetValidHabitName();
    List<string> habits = db.ListAllHabits();
    if (habits.Contains(habit))
    {
        Console.WriteLine("You are already tracking this habit.");
        return;
    }
    db.AddHabit(habit);
    Console.WriteLine("Habit successfully added.\n");  
}

void ListAllHabits()
{
    List<string> habits = db.ListAllHabits();
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

string? EnsureInputHabitExists()
{
    string habitName = GetValidHabitName();
    List<string> habits = db.ListAllHabits();
    if (!habits.Contains(habitName))
    {
        Console.WriteLine("Error - No such habit: " + habitName);
        return null;
    }
    return habitName;
}

void DeleteAHabitWithLogs()
{
    if (EnsureInputHabitExists() is string habitName)
    {
        db.DeleteHabit(habitName);
        Console.WriteLine($"Habit {habitName} deleted");
    }
    
}

void ViewLogsOfHabit()
{
    if (EnsureInputHabitExists() is string habitName)
    {
        List<HabitRecord> logs = db.ViewLogsForHabit(habitName);
        if (logs.Count == 0)
        {
            Console.WriteLine("No records found.");
            return;
        }
        Console.WriteLine($"View logs for habit: {habitName}");
        foreach (HabitRecord log in logs)
        {
            Console.WriteLine($"Amount: {log.Amount}, Last modified: {log.Date}");
        }
    }
    Console.WriteLine();
}

void AddLogForHabit()
{
    if (EnsureInputHabitExists() is string habit)
    {
        
        Console.WriteLine("Input amount completed:");
        int amount = GetValidIntInput();
        db.UpsertLogForHabit(habit, amount);
        Console.WriteLine("Habit update success.");
    }
}



void DeleteLogForHabit()
{
    if (EnsureInputHabitExists() is string habit)
    {
        Console.WriteLine("Input the date of the log you want to delete: ");
        string date = GetValidDateInput();
        db.DeleteLogForHabit(habit, date);
        Console.WriteLine("Record deleted");
    }
}

void UpdateLogForHabit()
{
    if (EnsureInputHabitExists() is string habit)
    {
        Console.WriteLine("Input the date of the log you want to update: ");
        string date = GetValidDateInput();
        HabitRecord? recordMaybe = db.GetLogForDate(habit, date);
        if (recordMaybe is null)
        {
            Console.WriteLine($"No record found for the given habit - {habit} and date - {date}.");
            return;
        }
        Console.WriteLine("Input the amount update: ");
        int amount = GetValidIntInput();
        db.UpdateLogForHabit(habit, amount, date);
        Console.WriteLine("Record updated.");
    }
}








