using System;
using System.Linq;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Models;
using SudokuCollective.Dev.Classes;

namespace SudokuCollective.Dev.Routines
{
    internal static class PlayGames
    {
        internal static void Run()
        {
            User user;
            begin:

            try
            {
                Console.Write("\nPlease enter your nickname: ");

                var nickName = new string(Console.ReadLine());

                user = new User() { NickName = nickName };
            } 
            catch (Exception e)
            {
                Console.WriteLine("\nError creating user: " + e.Message);
                Console.WriteLine("\nPlease try again...");

                goto begin;
            }

            Console.WriteLine("\nSet a difficulty level:\n");
            Console.WriteLine("Enter 1 for Steady Sloth (EASY)");
            Console.WriteLine("Enter 2 for Leaping Lemur (MEDIUM)");
            Console.WriteLine("Enter 3 for Mighty Mountain Lion (HARD)");
            Console.WriteLine("Enter 4 for Sneaky Shark (EVIL)\n");
            Console.Write(string.Format("{0}, please make your selection: ", user.NickName));

            var difficultyResponse = Console.ReadLine();
            IDifficulty difficulty = new Difficulty();

            if (int.TryParse(difficultyResponse, out var difficultyNumber))
            {
                if (difficultyNumber == 1 || difficultyNumber == 2
                    || difficultyNumber == 3 || difficultyNumber == 4)
                {
                    if (difficultyNumber == 1)
                    {
                        difficulty = new Difficulty()
                        {
                            Name = "Easy",
                            DifficultyLevel = DifficultyLevel.EASY
                        };
                    }
                    else if (difficultyNumber == 2)
                    {
                        difficulty = new Difficulty()
                        {
                            Name = "Medium",
                            DifficultyLevel = DifficultyLevel.MEDIUM
                        };
                    }
                    else if (difficultyNumber == 3)
                    {
                        difficulty = new Difficulty()
                        {
                            Name = "Hard",
                            DifficultyLevel = DifficultyLevel.HARD
                        };
                    }
                    else if (difficultyNumber == 4)
                    {
                        difficulty = new Difficulty()
                        {
                            Name = "Evil",
                            DifficultyLevel = DifficultyLevel.EVIL
                        };
                    }
                }
            }

            ISudokuMatrix matrix = new SudokuMatrix();

            matrix.GenerateSolution();

            matrix.SetDifficulty(difficulty);

            IGame game = new Game(
                user,
                (SudokuMatrix)matrix,
                (Difficulty)difficulty);

            game.KeepScore = true;

            game.SudokuMatrix.Stopwatch.Start();

            do
            {
                DisplayScreens.GameScreen(game);

                var command = Console.ReadLine();
                command = command.ToUpper().Trim();

                if (command.Equals("1") || command.Equals("ENTER") || command.Equals("2") || command.Equals("DELETE"))
                {
                    var continueX = true;

                    do
                    {
                        Console.Write("\nEnter the column: ");
                        var xValue = Console.ReadLine();

                        if (Int32.TryParse(xValue, out var xNumber))
                        {
                            if (xNumber > 0 && xNumber < 10)
                            {
                                var continueY = true;

                                do
                                {
                                    Console.Write("\nEnter the row: ");
                                    var yValue = Console.ReadLine();

                                    if (Int32.TryParse(yValue, out var yNumber))
                                    {
                                        if (yNumber > 0 && yNumber < 10)
                                        {
                                            var cell = game.SudokuMatrix.SudokuCells
                                                .Where(c => c.Column == xNumber && c.Row == yNumber).FirstOrDefault();

                                            if (cell.Hidden)
                                            {
                                                bool userEntryInvalid = true;

                                                do
                                                {
                                                    if (command.Equals("1") || command.Equals("ENTER"))
                                                    {

                                                        Console.Write("\nEnter a number from 1 through 9: ");
                                                        string userEntry = Console.ReadLine();

                                                        if (Int32.TryParse(userEntry, out var userNumber))
                                                        {
                                                            if (userNumber > 0 && userNumber < 10)
                                                            {
                                                                cell.DisplayedValue = userNumber;
                                                                continueX = false;
                                                                continueY = false;
                                                                userEntryInvalid = false;
                                                            }
                                                            else
                                                            {
                                                                DisplayScreens.InvalidCoordinate();
                                                            }

                                                        }
                                                        else
                                                        {
                                                            DisplayScreens.InvalidCoordinate();
                                                        }

                                                    }
                                                    else
                                                    {
                                                        cell.DisplayedValue = 0;
                                                        continueX = false;
                                                        continueY = false;
                                                        userEntryInvalid = false;
                                                    }

                                                } while (userEntryInvalid);
                                            }
                                            else
                                            {
                                                Console.WriteLine("\nThis value is a hint provided by the system and cannot be changed.");
                                                Console.WriteLine("Please try again.\n\n\t         (Press Enter to Continue)");
                                                Console.ReadLine();
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            DisplayScreens.InvalidCoordinate();
                                        }
                                    }

                                } while (continueY);

                            }
                            else
                            {
                                DisplayScreens.InvalidCoordinate();
                            }
                        }
                        else
                        {
                            DisplayScreens.InvalidCommand();
                        }

                    } while (continueX);

                }
                else if (command.Equals("3") || command.Equals("CHECK"))
                {
                    if (game.IsSolved())
                    {
                        Console.WriteLine(string.Format("\n{0}, you win!\n", user.NickName));

                        game.ContinueGame = false;

                        // Format and display the TimeSpan value.
                        string elapsedTime = String.Format("{0:00}{1:00}:{2:00}:{3:00}.{4:00}\n",
                            game.TimeToSolve.Days,
                            game.TimeToSolve.Hours,
                            game.TimeToSolve.Minutes,
                            game.TimeToSolve.Seconds,
                            game.TimeToSolve.Milliseconds / 10);

                        Console.Write("Time to solve: " + elapsedTime + "\n");
                        Console.Write("Score: " + game.Score + "\n");
                    }
                    else
                    {
                        Console.WriteLine("\nNOPE... TRY AGAIN!");
                    }

                }
                else if (command.Equals("4") || command.Equals("RESET"))
                {

                    Console.Write("\n{0}, are you sure you want to reset this sudoku puzzle (yes/no): ", user.NickName);

                    var exitCommand = Console.ReadLine();

                    if (exitCommand.ToLower().Equals("yes") || exitCommand.ToLower().Equals("y"))
                    {
                        foreach (var cell in game.SudokuMatrix.SudokuCells)
                        {
                            if (cell.Hidden)
                            {
                                cell.DisplayedValue = 0;
                            }
                        }
                    }
                }
                else if (command.Equals("5") || command.Equals("EXIT"))
                {

                    Console.Write("\n{0}, are you sure you want to exit to the main menu (yes/no): ", user.NickName);

                    var exitCommand = Console.ReadLine();

                    if (exitCommand.ToLower().Equals("yes") || exitCommand.ToLower().Equals("y"))
                    {
                        game.ContinueGame = false;
                    }
                }

            } while (game.ContinueGame);
        }
    }
}
