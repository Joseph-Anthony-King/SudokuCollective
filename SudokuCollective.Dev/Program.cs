﻿using System;
using SudokuCollective.ConsoleDev.Classes;

namespace SudokuCollective.ConsoleDev {

    class Program {
        static void Main(string[] args) {

            Console.WriteLine("\nWelcome to the Sudoku Collecive Dev App!");
            DisplayScreens.ProgramPrompt();

        begin:

            var response = Console.ReadLine();

            do {

                if (Int32.TryParse(response, out var number)) {

                    if (number == 1 || number == 2 || number == 3 || number == 4) {

                        if (number == 1) {

                            Routines.GenerateSolutions.Run();
                            DisplayScreens.ProgramPrompt();

                            goto begin;

                        } else if (number == 2) {

                            Routines.SolveSolutions.Run();
                            DisplayScreens.ProgramPrompt();

                            goto begin;

                        } else if (number == 3) {

                            Routines.PlayGames.Run();
                            DisplayScreens.ProgramPrompt();

                            goto begin;

                        } else if (number == 4) {

                            break;

                        } else {

                            DisplayScreens.ProgramPrompt();
                        }

                        goto begin;

                    } else {

                        Console.WriteLine("\nInvalid response.");
                        Console.Write("\nPlease make your selection: ");
                        goto begin;
                    }

                } else {

                    Console.WriteLine("\nInvalid response.");
                    Console.Write("\nPlease make your selection: ");
                    goto begin;
                }

            } while (true);

            Console.WriteLine("\nPress enter to exit the app...");
            Console.ReadLine();
        }
    }
}
