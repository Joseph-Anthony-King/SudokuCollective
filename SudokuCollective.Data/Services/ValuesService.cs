using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
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
    public class ValuesService(
        IDifficultiesRepository<Difficulty> difficultiesRepository,
        IAppsRepository<App> appsRepository,
        IDistributedCache distributedCache,
        ICacheService cacheService,
        ICacheKeys cacheKeys,
        ICachingStrategy cachingStrategy) : IValuesService
    {
        private readonly IDifficultiesRepository<Difficulty> _difficultiesRepository = difficultiesRepository;
        private readonly IAppsRepository<App> _appsRepository = appsRepository;
        private readonly IDistributedCache _distributedCache = distributedCache;
        private readonly ICacheService _cacheService = cacheService;
        private readonly ICacheKeys _cacheKeys = cacheKeys;
        private readonly ICachingStrategy _cachingStrategy = cachingStrategy;

        public async Task<IResult> GetAsync(IPaginator paginator = null)
        {
            paginator ??= new Paginator();

            var result = new Result();

            try
            {
                var releaseEnvironments = (DataUtilities.GetReleaseEnvironments()).Payload.ConvertAll(x => (IEnumListItem)x);
                var sortValues = (DataUtilities.GetSortValues()).Payload.ConvertAll(x => (IEnumListItem)x);
                var timeFrames = (DataUtilities.GetTimeFrames()).Payload.ConvertAll(x => (IEnumListItem)x);

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
                        ((Values)result.Payload[0]).Gallery = [];
                        return result;
                    }
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = ServicesMesages.PageNotFoundMessage;
                    ((Values)result.Payload[0]).Gallery = [];

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

        public static IResult GetReleaseEnvironments()
        {
            return DataUtilities.GetReleaseEnvironments();
        }

        IResult IValuesService.GetReleaseEnvironments()
        {
            return GetReleaseEnvironments();
        }

        public static IResult GetSortValues()
        {
            return DataUtilities.GetSortValues();
        }

        IResult IValuesService.GetSortValues()
        {
            return GetSortValues();
        }

        public static IResult GetTimeFrames()
        {
            return DataUtilities.GetTimeFrames();
        }

        IResult IValuesService.GetTimeFrames()
        {
            return GetTimeFrames();
        }
    }
}
