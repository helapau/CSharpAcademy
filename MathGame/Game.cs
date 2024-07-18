using System.Text;

namespace CSharpAcademy.MathGame
{
    internal record GameRecord(string questions, int score);

    enum Arithmetic { 
        Add,
        Subtract,
        Multiply,
        Divide
    }

    internal class Game
    {

        bool isOn = true;
        Random rand = new Random();
        List<GameRecord> previousGames = new List<GameRecord>();
        int totalRounds = 5;

        public Game() { }

        string GetSymbol(Arithmetic operation) => operation switch
        {
            Arithmetic.Add => "+",
            Arithmetic.Subtract => "-",
            Arithmetic.Multiply => "*",
            Arithmetic.Divide => "/",
            _ => throw new NotImplementedException()
        };

        private int CalculateResult(Arithmetic operation, int a, int b) => operation switch
        {
            Arithmetic.Add => a + b,
            Arithmetic.Subtract => a - b,
            Arithmetic.Multiply => a * b,
            Arithmetic.Divide => a / b,
            _ => throw new NotImplementedException()
        };

        /// <summary>
        /// Return a pseudo random int between 0 and 10
        /// </summary>
        /// <returns></returns>
        private int GetRandomInt()
        {
            return rand.Next(11);
        }

        private string JoinQuestions(List<string> questions)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < questions.Count; i++)
            {
                result.Append(questions[i]);
                if (i != questions.Count - 1)
                {
                    result.Append(", ");
                }
            }            
            return result.ToString();
        }

        private int GetUserInputAsInt()
        {
            int guess = -1;
            bool isParsed = false;
            do {
                string? input = Console.ReadLine();
                if (input is string && int.TryParse(input, out guess)) {
                    isParsed = true;
                }
                else {
                    Console.WriteLine("Please enter a valid integer.");
                }
            } while (isParsed is false);
            return guess;
        }

        private void ArithmeticOperation(Arithmetic operation, List<string> questions)
        {
            int score = 0;
            string symbol = GetSymbol(operation);
            for (; score < totalRounds; score++)
            {
                var (a, b) = GenerateNumbersForOperation(operation);
                string question = $"{a} {symbol} {b}";
                Console.WriteLine(question);
                questions.Add(question);                
                int guess = GetUserInputAsInt();
                int result = CalculateResult(operation, a, b);
                if (guess != result)
                {
                    Console.WriteLine($"Game over! Your total score is: {score}");
                    break;                    
                }               
            }
            previousGames.Add(new GameRecord(JoinQuestions(questions), score));
            if (score == totalRounds)
            {
                Console.WriteLine($"Congrats you won this game! Your score is: {score}");
            }
        }


        
        private (int, int) GenerateNumbersForOperation(Arithmetic operation)
        {
            int b = GetRandomInt();
            int a;

            if (operation == Arithmetic.Divide)
            {
                b++;
                a = b * 1 * GetRandomInt();                
            }
            else
            {
                a = GetRandomInt();
            }
            return (a, b);            
        }       


        void ViewPrevious()
        {
            if (previousGames.Count == 0)
            {
                Console.WriteLine("You haven't played any games yet.");
                return;
            }

            int i = previousGames.Count - 1;

            while (true)
            {
                Console.WriteLine("Your last game was: ");
                Console.WriteLine(previousGames[i]);
                i--;
                if (i < 0)
                {
                    Console.WriteLine("And that's all the games you played in this session.\n");
                    break;
                }
                else
                {
                    Console.WriteLine("Press any key to continue or Q to return to main menu.");
                    string? feedback = Console.ReadLine();
                    if (feedback is string && feedback.ToUpper() == "Q")
                        break;
                }
            }            
        }

        private void PickNumberOfRounds()
        {
            Console.WriteLine("Enter an integer:");           
            int userDefinedRounds = GetUserInputAsInt();
            totalRounds = userDefinedRounds;            
        }

        private void WriteMainMenu()
        {
            Console.WriteLine("What game would you like to play? Choose one from the options below:");
            Console.WriteLine("A - Addition");
            Console.WriteLine("S - Subtraction");
            Console.WriteLine("M - Multiplication");
            Console.WriteLine("D - Division");
            Console.WriteLine("\nQ - Quit the program");
            Console.WriteLine("V - View previous games");
            Console.WriteLine($"R - Pick the number of rounds for a game. The default is 5. Your current choice: {totalRounds}");
        }
        

        public void Play()
        {       

            while (isOn)
            {
                WriteMainMenu();
                string choice = Console.ReadLine();                
                switch (choice.ToUpper())
                {
                    case "A":
                        ArithmeticOperation(Arithmetic.Add, new List<string>());
                        break;
                    case "S":
                        ArithmeticOperation(Arithmetic.Subtract, new List<string>());
                        break;
                    case "M":
                        ArithmeticOperation(Arithmetic.Multiply, new List<string>());
                        break;
                    case "D":
                        ArithmeticOperation(Arithmetic.Divide, new List<string>());
                        break;
                    case "Q":
                        isOn = false;
                        break;
                    case "V":
                        ViewPrevious();
                        break;
                    case "R":
                        PickNumberOfRounds();
                        break;
                    default:
                        Console.WriteLine($"Unknown command: {choice}");
                        break;
                }
            }
        }

        
    }
}
