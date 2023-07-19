using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Cache;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Values;
using SudokuCollective.Core.Interfaces.Repositories;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Messages;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Data.Models.Values;
using SudokuCollective.Data.Utilities;

namespace SudokuCollective.Data.Services
{
    public class ValuesService : IValuesService
    {
        private IDifficultiesRepository<Difficulty> _difficultiesRepository;
        private IAppsRepository<App> _appsRepository;
        private readonly IDistributedCache _distributedCache;
        private readonly ICacheService _cacheService;
        private readonly ICacheKeys _cacheKeys;
        private readonly ICachingStrategy _cachingStrategy;

        public ValuesService(
            IDifficultiesRepository<Difficulty> difficultiesRepository,
            IAppsRepository<App> appsRepository,
            IDistributedCache distributedCache,
            ICacheService cacheService,
            ICacheKeys cacheKeys,
            ICachingStrategy cachingStrategy)
        {
            _difficultiesRepository = difficultiesRepository;
            _appsRepository = appsRepository;
            _distributedCache = distributedCache;
            _cacheService = cacheService;
            _cacheKeys = cacheKeys;
            _cachingStrategy = cachingStrategy;
        }

        public async Task<IResult> GetAsync(IPaginator paginator = null)
        {
            if (paginator == null)
            {
                paginator = new Paginator();
            }

            var result = new Result();

            try
            {
                var releaseEnvironments = (GetReleaseEnvironments()).Payload.ConvertAll(x => (IEnumListItem)x);
                var sortValues = (GetSortValues()).Payload.ConvertAll(x => (IEnumListItem)x);
                var timeFrames = (GetTimeFrames()).Payload.ConvertAll(x => (IEnumListItem)x);

                var response = await _cacheService.GetValuesAsync(
                    _difficultiesRepository, 
                    _appsRepository,
                    _distributedCache, 
                    _cacheKeys.GetValuesKey, 
                    _cachingStrategy.Heavy,
                    releaseEnvironments,
                    sortValues,
                    timeFrames, 
                    result);

                result = (Result)response.Item2;

                result.Payload.Add(response.Item1);

                if (DataUtilities.IsPageValid(paginator, ((Values)result.Payload[0]).Gallery.ConvertAll(a => (IDomainEntity)a)))
                {
                    result = PaginatorUtilities.PaginateGallery(paginator, result);

                    if (result.Message.Equals(
                        ServicesMesages.SortValueNotImplementedMessage))
                    {
                        ((Values)result.Payload[0]).Gallery = new List<GalleryApp>();
                        return result;
                    }
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = ServicesMesages.PageNotFoundMessage;
                    ((Values)result.Payload[0]).Gallery = new List<GalleryApp>();

                    return result;
                }

                result.IsSuccess = true;

                result.Message = ValuesMessages.ValuesRetrieved;
            }
            catch (Exception e)
            {
                result.Message = e.Message;
            }

            return result;
        }

        public IResult GetReleaseEnvironments()
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
                        Label = "Staging", 
                        Value = (int)ReleaseEnvironment.STAGING,
                        AppliesTo = releaseEnvironment },
                    new EnumListItem { 
                        Label = "Quality Assurance", 
                        Value = (int)ReleaseEnvironment.QA,
                        AppliesTo = releaseEnvironment },
                    new EnumListItem { 
                        Label = "Production", 
                        Value = (int)ReleaseEnvironment.PROD,
                        AppliesTo = releaseEnvironment },
                };

                result.IsSuccess = true;

                result.Message = ValuesMessages.ReleaseEnvironmentsRetrieved;

                result.Payload = items.ConvertAll(x => (object)x);

                return result;
            }
            catch
            {
                result.Message = ValuesMessages.ReleaseEnvironmentsNotRetrieved;

                return result;
            }
        }

        public IResult GetSortValues()
        {
            var result = new Result();

            try
            {
                var all = new List<string> { "apps", "gallery", "users", "games" };
                var apps = new List<string> { "apps" };
                var gallery = new List<string> { "gallery"};
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

                result.Payload = items.ConvertAll(x => (object)x);

                return result;
            }
            catch
            {
                result.Message = ValuesMessages.SortValuesNotRetrieved;

                return result;
            }
        }

        public IResult GetTimeFrames()
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

                result.Payload = items.ConvertAll(x => (object)x);

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
