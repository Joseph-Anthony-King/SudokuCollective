using SudokuCollective.Dev.Classes;
using System;

namespace SudokuCollective.Dev
{
    public class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("\nWelcome to the Sudoku Collecive Dev App!");
            DisplayScreens.ProgramPrompt();

            begin:

            var response = Console.ReadLine();

            do
            {

                if (int.TryParse(response, out var number))
                {

                    if (number == 1 || number == 2 || number == 3 || number == 4)
                    {

                        if (number == 1)
                        {

                            Routines.GenerateSolutions.Run();
                            DisplayScreens.ProgramPrompt();

                            goto begin;

                        }
                        else if (number == 2)
                        {

                            Routines.SolveSolutions.Run();
                            DisplayScreens.ProgramPrompt();

                            goto begin;

                        }
                        else if (number == 3)
                        {

                            Routines.PlayGames.Run();
                            DisplayScreens.ProgramPrompt();

                            goto begin;

                        }
                        else if (number == 4)
                        {
                            Console.Write("\nAre you sure you want to exit Sudoku Collective Dev App (yes/no): ");

                            var exitCommand = Console.ReadLine();

                            if (exitCommand.ToLower().Equals("yes") || exitCommand.ToLower().Equals("y"))
                            {
                                break;
                            }
                            else
                            {
                                DisplayScreens.ProgramPrompt();
                                goto begin;
                            }
                        }
                        else
                        {

                            DisplayScreens.ProgramPrompt();
                        }

                        goto begin;

                    }
                    else
                    {

                        Console.WriteLine("\nInvalid response.");
                        Console.Write("\nPlease make your selection: ");
                        goto begin;
                    }

                }
                else
                {

                    Console.WriteLine("\nInvalid response.");
                    Console.Write("\nPlease make your selection: ");
                    goto begin;
                }

            } while (true);

            Console.WriteLine("\nThanks for playing! (Press Enter to Exit)");
            Console.ReadLine();
        }
    }
}
