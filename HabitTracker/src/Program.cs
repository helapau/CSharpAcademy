using System.Globalization;
using HabitTracker.src;







string? EnsureInputHabitExists()
{
    string habitName = UserInput.GetValidHabitName();
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
        int amount = UserInput.GetValidIntInput();
        db.UpsertLogForHabit(habit, amount);
        Console.WriteLine("Habit update success.");
    }
}



void DeleteLogForHabit()
{
    if (EnsureInputHabitExists() is string habit)
    {
        Console.WriteLine("Input the date of the log you want to delete: ");
        string date = UserInput.GetValidDateInput();
        db.DeleteLogForHabit(habit, date);
        Console.WriteLine("Record deleted");
    }
}

void UpdateLogForHabit()
{
    if (EnsureInputHabitExists() is string habit)
    {
        Console.WriteLine("Input the date of the log you want to update: ");
        string date = UserInput.GetValidDateInput();
        HabitRecord? recordMaybe = db.GetLogForDate(habit, date);
        if (recordMaybe is null)
        {
            Console.WriteLine($"No record found for the given habit - {habit} and date - {date}.");
            return;
        }
        Console.WriteLine("Input the amount update: ");
        int amount = UserInput.GetValidIntInput();
        db.UpdateLogForHabit(habit, amount, date);
        Console.WriteLine("Record updated.");
    }
}








