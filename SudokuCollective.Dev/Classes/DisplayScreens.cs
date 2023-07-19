using System;
using System.Collections.Generic;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Models;

namespace SudokuCollective.Dev.Classes
{
    public static class DisplayScreens
    {
        public static void GameScreen(IGame game)
        {
            DisplayMatix(game.SudokuMatrix);

            Console.Write(string.Format("\n\n{0}, please make your selection\n\nEnter 1 to enter a value or 'ENTER'", game.User.NickName));
            Console.Write("\nEnter 2 to delete a value or 'DELETE' \nEnter 3 to check your answer or 'CHECK'");
            Console.Write("\nEnter 4 to reset the sudoku puzzle or 'RESET'\nEnter 5 to exit to Main Menu or 'EXIT'\n");
            Console.Write("\nYour Selection: ");
        }

        internal static void ProgramPrompt()
        {
            Console.WriteLine("\nWould you like to generate solutions, solve a solution, or play a game:\n");
            Console.WriteLine("Enter 1 to generate solutions");
            Console.WriteLine("Enter 2 to solve a solution");
            Console.WriteLine("Enter 3 to play a game");
            Console.WriteLine("Enter 4 to exit Sudoku Collective Dev App\n");
            Console.Write("Please make your selection: ");

        }

        internal static void DisplayMatix(ISudokuMatrix matrix)
        {
            Console.Write("\n  Sudoku Collective\n");
            Console.Write("\n   1 2 3 4 5 6 7 8 9\n");
            var i = 1;

            var m = (SudokuMatrix)matrix;

            foreach (var row in m.Rows)
            {
                Console.Write(string.Format("\n{0}  ", i));
                DisplayRow(row);
                i++;
            }
        }

        private static void DisplayRow(List<ISudokuCell> row)
        {
            foreach (var cell in row)
            {
                if (!cell.Hidden)
                {
                    var _previousColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(string.Format("{0} ", cell));
                    Console.ForegroundColor = _previousColor;
                }
                else
                {
                    Console.Write(string.Format("{0} ", cell));
                }
            }
        }

        internal static void InvalidCommand()
        {
            Console.WriteLine("\nInvalid Command.");
            Console.WriteLine("\tPlease try again.\n\n\t         (Press Enter to Continue)");
            Console.ReadLine();
            Console.Clear();
        }

        internal static void InvalidCoordinate()
        {
            Console.WriteLine("\nYour response must be an integer 1 through 9.");
            Console.WriteLine("\tPlease try again.\n\n\t         (Press Enter to Continue)");
            Console.ReadLine();
        }
    }
}
