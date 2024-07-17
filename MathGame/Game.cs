using System;
using System.Linq;
using System.Text;

namespace CSharpAcademy.MathGame
{
    internal record GameRecord(string questions, int score);

    internal class Game
    {

        bool isOn = true;
        Random rand = new Random();
        List<GameRecord> previousGames = new List<GameRecord>();
        int totalRounds = 5;

        public Game() { }

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


        private void ArithmeticOperation(int score, int round, string symbol, Func<int, int, int> operationCallback, List<string> questions)
        {
            
            if (round == totalRounds + 1)
            {
                Console.WriteLine($"Congrats you won this addition game! Your score is: {score}");
                previousGames.Add(new GameRecord(JoinQuestions(questions), score));
                return;
            }

            var (a, b) = GenerateNumbersForOperation(symbol);
            string question = $"{a} {symbol} {b}";            
            Console.WriteLine(question);
            questions.Add(question);       
            
            string? response = Console.ReadLine();
            int guess;
            int result = operationCallback(a, b);
            if (response is string && int.TryParse(response, out guess))
            {
                if (guess == result)
                {
                    ArithmeticOperation(score + 1, round + 1, symbol, operationCallback, questions);
                }
                else
                {
                    Console.WriteLine($"Game over! Your total score is: {score}");
                    previousGames.Add(new GameRecord(JoinQuestions(questions), score));
                    return;
                }
            }
        }

        private (int, int) GenerateNumbersForOperation(string symbol)
        {
            int b = GetRandomInt();
            int a;

            if (symbol == "/")
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


        private int Add(int a, int b) => a + b;
        private int Subtract(int a, int b) => a - b;
        private int Multiply(int a, int b) => a * b;
        private int Divide(int a, int b) => a / b;

        

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
            string? input = Console.ReadLine();
            int userDefinedRounds;
            if (input is string && int.TryParse(input, out userDefinedRounds))
            {
                totalRounds = userDefinedRounds;
                return;
            }
            else
            {
                Console.WriteLine("Number of rounds must be an integer. Try again.");
                PickNumberOfRounds();
            }
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
                        ArithmeticOperation(0, 1, "+", Add, new List<string>());
                        break;
                    case "S":
                        ArithmeticOperation(0, 1, "-", Subtract, new List<string>());
                        break;
                    case "M":
                        ArithmeticOperation(0, 1, "*", Multiply, new List<string>());
                        break;
                    case "D":
                        ArithmeticOperation(0, 1, "/", Divide, new List<string>());
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
                        Console.WriteLine("Unknown command: ", choice);
                        break;
                }
            }
        }

        
    }
}
