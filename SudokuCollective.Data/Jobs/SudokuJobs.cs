using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Jobs;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Data.Messages;
using SudokuCollective.Data.Models;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Logs.Utilities;

namespace SudokuCollective.Data.Jobs
{
    public class SudokuJobs(
        IGamesService gamesService,
        DatabaseContext context, 
        ILogger<SudokuJobs> logger
        ) : ISudokuJobs
    {
        private readonly IGamesService _gamesService = gamesService;
        private readonly DatabaseContext _context = context;
        private readonly ILogger<SudokuJobs> _logger = logger;

        async public Task<IResult> CreateGameJobAsync(DifficultyLevel difficultyLevel)
        {
            try
            {
                var difficulty = _context.Difficulties
                    .Where(difficulty => difficulty.DifficultyLevel == difficultyLevel) 
                    ?? throw new ArgumentNullException(nameof(difficultyLevel));

                return await _gamesService.CreateAnnonymousAsync(difficultyLevel);
            }
            catch (Exception e)
            {
                string message = e.Message;

                _logger.LogWarning(LogsUtilities.GetControllerWarningEventId(), message);

                var result = new Result
                {
                    IsSuccess = false,
                    Message = ControllerMessages.StatusCode500(e.Message)
                };

                return result;
            }
        }
    }
}
