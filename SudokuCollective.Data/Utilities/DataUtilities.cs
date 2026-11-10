using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Values;
using SudokuCollective.Core.Interfaces.ServiceModels;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Messages;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Data.Models.Values;
using SudokuCollective.Logs;
using SudokuCollective.Logs.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("SudokuCollective.Test")]
namespace SudokuCollective.Data.Utilities
{
    internal static class DataUtilities
    {
        internal async static Task AttachSudokuCells(
            this ISudokuMatrix matrix,
            IDatabaseContext context)
        {

            var cells = await context.SudokuCells
                .Where(cell => cell.SudokuMatrixId == matrix.Id)
                .ToListAsync();

            ((SudokuMatrix)matrix).SudokuCells = cells.ConvertAll(cell => (SudokuCell)cell);
        }

        internal async static Task<bool> IsGameInActiveApp(this IGame game, IDatabaseContext context)
        {
            var app = await context.Apps.FirstOrDefaultAsync(a => a.Id == game.AppId);

            return app.IsActive;
        }

        internal static bool IsPageValid(IPaginator paginator, List<IDomainEntity> entities)
        {
            if (paginator.ItemsPerPage * paginator.Page > entities.Count && paginator.Page == 1)
            {
                return true;
            }
            else if (paginator.ItemsPerPage * paginator.Page > entities.Count && paginator.Page > 1)
            {
                return false;
            }
            else
            {
                return paginator.ItemsPerPage * paginator.Page <= entities.Count;
            }
        }

        internal static IResult ProcessException<T>(
            IRequestService requestService, 
            ILogger<T> logger, 
            IResult result, 
            Exception e,
            string message = null)
        {
            if (string.IsNullOrEmpty(message))
            {
                message = string.Format(LoggerMessages.ErrorThrownMessage, result.Message);
            }

            result.IsSuccess = false;
            result.Message = e.Message;

            SudokuCollectiveLogger.LogError<T>(
                logger,
                LogsUtilities.GetServiceErrorEventId(), 
                message,
                e,
                (SudokuCollective.Logs.Models.Request)requestService.Get());

            return result;
        }

        internal static IResult GetReleaseEnvironments()
        {
            var result = new Result();

            try
            {
                var releaseEnvironment = new List<string> { "releaseEnvironment" };

                var items = new List<IEnumListItem>
                {
                    new EnumListItem {
                        Label = "Local",
                        Value = (int)ReleaseEnvironment.LOCAL,
                        AppliesTo = releaseEnvironment },
                    new EnumListItem {
                        Label = "Test",
                        Value = (int)ReleaseEnvironment.TEST,
                        AppliesTo = releaseEnvironment },
                    new EnumListItem {
                        Label = "Staging",
                        Value = (int)ReleaseEnvironment.STAGING,
                        AppliesTo = releaseEnvironment },
                    new EnumListItem {
                        Label = "Production",
                        Value = (int)ReleaseEnvironment.PROD,
                        AppliesTo = releaseEnvironment },
                };

                result.IsSuccess = true;

                result.Message = ValuesMessages.ReleaseEnvironmentsRetrieved;

                result.Payload = [.. items
                    .ConvertAll(x => (object)x)
                    .OrderBy(x => ((EnumListItem)x).Value)];

                return result;
            }
            catch
            {
                result.Message = ValuesMessages.ReleaseEnvironmentsNotRetrieved;

                return result;
            }
        }

        internal static IResult GetSortValues()
        {
            var result = new Result();

            try
            {
                var all = new List<string> { "apps", "gallery", "users", "games" };
                var apps = new List<string> { "apps" };
                var gallery = new List<string> { "gallery" };
                var users = new List<string> { "users" };
                var games = new List<string> { "games" };
                var appsAndGallery = new List<string> { "apps", "gallery" };
                var usersAndGallery = new List<string> { "gallery", "users" };

                var items = new List<IEnumListItem>
                {
                    new EnumListItem {
                        Label = "Id",
                        Value = (int)SortValue.ID,
                        AppliesTo = all },
                    new EnumListItem {
                        Label = "Username",
                        Value = (int)SortValue.USERNAME,
                        AppliesTo = usersAndGallery },
                    new EnumListItem {
                        Label = "First Name",
                        Value = (int)SortValue.FIRSTNAME,
                        AppliesTo = users },
                    new EnumListItem {
                        Label = "Last Name",
                        Value = (int)SortValue.LASTNAME,
                        AppliesTo = users },
                    new EnumListItem {
                        Label = "Full Name",
                        Value = (int)SortValue.FULLNAME,
                        AppliesTo = users },
                    new EnumListItem {
                        Label = "Nick Name",
                        Value = (int)SortValue.NICKNAME,
                        AppliesTo = users },
                    new EnumListItem {
                        Label = "Game Count",
                        Value = (int)SortValue.GAMECOUNT,
                        AppliesTo = users },
                    new EnumListItem {
                        Label = "App Count",
                        Value = (int)SortValue.APPCOUNT,
                        AppliesTo = users },
                    new EnumListItem {
                        Label = "Name",
                        Value = (int)SortValue.NAME,
                        AppliesTo = appsAndGallery },
                    new EnumListItem {
                        Label = "Date Created",
                        Value = (int)SortValue.DATECREATED,
                        AppliesTo = all },
                    new EnumListItem {
                        Label = "Date Updated",
                        Value = (int)SortValue.DATEUPDATED,
                        AppliesTo = all },
                    new EnumListItem {
                        Label = "Difficulty Level",
                        Value = (int)SortValue.DIFFICULTYLEVEL,
                        AppliesTo = games },
                    new EnumListItem {
                        Label = "User Count",
                        Value = (int)SortValue.USERCOUNT,
                        AppliesTo = appsAndGallery },
                    new EnumListItem {
                        Label = "Score",
                        Value = (int)SortValue.SCORE,
                        AppliesTo = games },
                    new EnumListItem {
                        Label = "Url",
                        Value = (int)SortValue.URL,
                        AppliesTo = gallery }
                };

                result.IsSuccess = true;

                result.Message = ValuesMessages.SortValuesRetrieved;

                result.Payload = [.. items.ConvertAll(x => (object)x).OrderBy(x => ((EnumListItem)x).Value)]; ;

                return result;
            }
            catch
            {
                result.Message = ValuesMessages.SortValuesNotRetrieved;

                return result;
            }
        }

        internal static IResult GetTimeFrames()
        {
            var result = new Result();

            try
            {
                var authToken = new List<string> { "authToken" };

                var items = new List<IEnumListItem>
                {
                    new EnumListItem {
                        Label = "Seconds",
                        Value = (int)TimeFrame.SECONDS,
                        AppliesTo = authToken },
                    new EnumListItem {
                        Label = "Minutes",
                        Value = (int)TimeFrame.MINUTES,
                        AppliesTo = authToken },
                    new EnumListItem {
                        Label = "Hours",
                        Value = (int)TimeFrame.HOURS,
                        AppliesTo = authToken },
                    new EnumListItem {
                        Label = "Days",
                        Value = (int)TimeFrame.DAYS,
                        AppliesTo = authToken },
                    new EnumListItem {
                        Label = "Months",
                        Value = (int)TimeFrame.MONTHS,
                        AppliesTo = authToken },
                    new EnumListItem {
                        Label = "Years",
                        Value = (int)TimeFrame.YEARS,
                        AppliesTo = authToken },
                };

                result.IsSuccess = true;

                result.Message = ValuesMessages.TimeFramesRetrieved;

                result.Payload = [.. items.ConvertAll(x => (object)x).OrderBy(x => ((EnumListItem)x).Value)]; ;

                return result;
            }
            catch
            {
                result.Message = ValuesMessages.TimeFramesNotRetrieved;

                return result;
            }
        }
    }
}
