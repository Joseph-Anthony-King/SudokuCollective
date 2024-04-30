using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SudokuCollective.Core.Extensions;
using SudokuCollective.Core.Interfaces.Jobs;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models;
using SudokuCollective.Logs.Utilities;

namespace SudokuCollective.Data.Jobs
{
    public class DataJobs : IDataJobs
    {
        private readonly DatabaseContext _context;
        private readonly ILogger<DataJobs> _logger;

        public DataJobs(DatabaseContext context, ILogger<DataJobs> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddSolutionJobAsync(List<int> intList)
        {
            try
            {
                var sudokuSolution = new SudokuSolution(intList);

                // Add solution to the database
                var solutions = await _context.SudokuSolutions.ToListAsync();

                var solutionInDB = false;

                if (solutions.Count > 0)
                {
                    foreach (var solution in solutions.Where(s => s.DateSolved > DateTime.MinValue))
                    {
                        if (solution.SolutionList.IsThisListEqual(sudokuSolution.SolutionList))
                        {
                            solutionInDB = true;
                        }
                    }
                }

                if (!solutionInDB)
                {
                    _context.Add(sudokuSolution);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                string message = e.Message;
                _logger.LogWarning(LogsUtilities.GetControllerWarningEventId(), message);
            }
        }

        public async Task GenerateSolutionsJobAsync(int limit)
        {
            var solutionsInDB = new List<List<int>>();

            try
            {
                var solutions = await _context.SudokuSolutions.Where(s => s.DateCreated > DateTime.MinValue).ToListAsync();

                foreach (var solution in solutions)
                {
                    solutionsInDB.Add(solution.SolutionList);
                }
            }
            catch (Exception e)
            {
                string message = e.Message;
                _logger.LogWarning(LogsUtilities.GetControllerWarningEventId(), message);
            }

            var matrix = new SudokuMatrix();

            try
            {
                List<List<int>> solutionsList = new();
                List<SudokuSolution> newSolutions = new();

                for (var i = 0; i < limit; i++)
                {
                    var continueLoop = true;

                    do
                    {
                        await matrix.GenerateSolutionAsync();

                        if (!solutionsInDB.Contains(matrix.ToIntList()))
                        {
                            solutionsList.Add(matrix.ToIntList());
                            continueLoop = false;
                        }

                    } while (continueLoop);
                }

                foreach (var solutionList in solutionsList)
                {
                    newSolutions.Add(new SudokuSolution(solutionList));
                }
                
                await _context.SudokuSolutions.AddRangeAsync(newSolutions);
                
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                string message = e.Message;
                _logger.LogWarning(LogsUtilities.GetControllerWarningEventId(), message);
            }
        }
    }
}
