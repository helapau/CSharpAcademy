using System.Globalization;


namespace HabitTracker.src
{
    internal static class UserInput
    {
        public static int GetValidIntInput()
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

        public static string GetValidHabitName()
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

        public static string GetValidDateInput()
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
    }
}
