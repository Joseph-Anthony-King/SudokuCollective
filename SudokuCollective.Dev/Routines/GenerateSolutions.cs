using System;
using System.Threading.Tasks;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Models;
using SudokuCollective.Dev.Classes;

namespace SudokuCollective.Dev.Routines
{
    internal static class GenerateSolutions
    {
        internal static async Task Run()
        {
            string result;
            var continueLoop = true;

            do
            {
                var matrix = new SudokuMatrix();

                matrix.SetDifficulty(new Difficulty()
                {
                    Name = "Test",
                    DifficultyLevel = DifficultyLevel.TEST
                });

                await matrix.GenerateSolutionAsync();

                DisplayScreens.DisplayMatix(matrix);

                Console.Write("\n\nWould you like to generate another solution (yes/no): ");

                result = Console.ReadLine();

                if (result.ToLower().Equals("no") || result.ToLower().Equals("n"))
                {
                    continueLoop = false;
                }

            } while (continueLoop);
        }
    }
}
