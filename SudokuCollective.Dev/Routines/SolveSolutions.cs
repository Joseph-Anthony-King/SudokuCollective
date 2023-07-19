using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Validation.Attributes;
using SudokuCollective.Dev.Classes;

namespace SudokuCollective.Dev.Routines
{
    internal static class SolveSolutions
    {
        private static readonly List<string> _order = new()
        { 
            "first", 
            "second", 
            "third", 
            "fourth", 
            "fifth", 
            "sixth", 
            "seventh", 
            "eighth", 
            "ninth"
        };
        private static readonly RowValidatedAttribute _rowValidatedAttribute = new();

        internal static void Run()
        {
            Console.WriteLine("\nPlease enter the sudoku puzzle you wish to solve.\n");
            Console.WriteLine("You will be entering 9 characters for each row. You");
            Console.WriteLine("cannot enter more than 9 characters per row.  Valid");
            Console.WriteLine("values are numeric characters of 0 through 9, for");
            Console.WriteLine("unknown values enter 0.  Only the 0 character can");
            Console.WriteLine("be duplicate per row, characters 1 through 9 can only");
            Console.WriteLine("be used once per row.  Once you're done the solver will");
            Console.WriteLine("produce an answer.  The solver will notify you if");
            Console.WriteLine("the sodoku puzzle cannot be solved.\n");
            Console.WriteLine("Press enter to continue!");

            Console.ReadLine();

            var continueLoop = true;

            do
            {
                var intList = new List<int>();

                for (var i = 0; i < _order.Count; i++)
                {
                rowEntry:

                    var prompt = string.Format("Enter the {0} row:", _order[i]);

                    if (_order[i].Equals("first") || _order[i].Equals("third") || _order[i].Equals("fifth") || _order[i].Equals("sixth") || _order[i].Equals("ninth"))
                    {
                        for (var j = 0; j < 3; j++)
                        {
                            prompt = string.Format("{0} ", prompt);
                        }
                    }
                    else if (_order[i].Equals("second") || _order[i].Equals("fourth") || _order[i].Equals("eighth"))
                    {
                        for (var j = 0; j < 2; j++)
                        {
                            prompt = string.Format("{0} ", prompt);
                        }
                    }
                    else
                    {
                        for (var j = 0; j < 1; j++)
                        {
                            prompt = string.Format("{0} ", prompt);
                        }
                    }

                    Console.Write(prompt);

                    var response = Console.ReadLine();

                    var charArray = response.ToCharArray();
                    var rowList = new List<int>();

                    for (var j = 0; j < charArray.Length; j++)
                    {
                        var success = int.TryParse(charArray[j].ToString(), out int outInt);

                        if (success)
                        {
                            rowList.Add(outInt);
                        }
                    }

                    if (rowList.Count == 9 && _rowValidatedAttribute.IsValid(rowList))
                    {
                        intList.AddRange(rowList);
                    }
                    else
                    {
                        Console.WriteLine("\nEntry was invalid, please try again...\n");
                        goto rowEntry;
                    }
                }

                Console.Write("\nPress enter to continue... ");

                Console.ReadLine();

                Console.WriteLine();

                var matrix = new SudokuMatrix(intList);

                Task solver = matrix.Solve();

                ConsoleSpiner spin = new ConsoleSpiner();

                while (!solver.IsCompleted)
                {
                    spin.Turn();
                }

                Console.WriteLine();

                Console.Beep();

                if (matrix.IsValid())
                {
                    var displayMatrix = new SudokuMatrix(matrix.ToIntList());

                    displayMatrix.SetDifficulty(
                        new Difficulty
                        {
                            Name = "Test",
                            DifficultyLevel = DifficultyLevel.TEST
                        });

                    DisplayScreens.DisplayMatix(displayMatrix);

                    // Format and display the TimeSpan value.
                    string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                        matrix.Stopwatch.Elapsed.Hours,
                        matrix.Stopwatch.Elapsed.Minutes,
                        matrix.Stopwatch.Elapsed.Seconds,
                        matrix.Stopwatch.Elapsed.Milliseconds / 10);

                    Console.Write("\n\nTime to generate solution: " + elapsedTime + "\n\n");

                }
                else
                {
                    Console.WriteLine("\nNeed more values in order to deduce a solution.\n");
                }

                Console.Write("Would you like to solve another solution (yes/no): ");

                var result = Console.ReadLine();

                if (result.ToLower().Equals("no") || result.ToLower().Equals("n"))
                {
                    continueLoop = false;
                }
                else
                {
                    Console.WriteLine();
                }

            } while (continueLoop);
        }
    }
}
