using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SudokuCollective.Core.Interfaces.Cache;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;
using SudokuCollective.Core.Interfaces.Repositories;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Data.Models.Results;
using SudokuCollective.Data.Utilities;

namespace SudokuCollective.Data.Services
{
    public class AdminService(
        IAppsRepository<App> appsRepository,
        IDifficultiesRepository<Difficulty> difficultiesRepository,
        IEmailConfirmationsRepository<EmailConfirmation> emailConfirmationsRepository,
        IGamesRepository<Game> gamesRepository,
        IPasswordResetsRepository<PasswordReset> passwordResetsRepository,
        IRolesRepository<Role> rolesRepository,
        ISolutionsRepository<SudokuSolution> solutionsRepository,
        IUsersRepository<User> usersRepository,
        IRequestService requestService,
        IDistributedCache distributedCache,
        ICacheService cacheService,
        ICacheKeys cacheKeys,
        ICachingStrategy cachingStrategy,
        ILogger<AdminService> logger) : IAdminService
    {
        #region Fields
        private readonly IAppsRepository<App> _appsRepository = appsRepository;
        private readonly IDifficultiesRepository<Difficulty> _difficultiesRepository = difficultiesRepository;
        private readonly IEmailConfirmationsRepository<EmailConfirmation> _emailConfirmationsRepository = emailConfirmationsRepository;
        private readonly IGamesRepository<Game> _gamesRepository = gamesRepository;
        private readonly IPasswordResetsRepository<PasswordReset> _passwordResetsRepository = passwordResetsRepository;
        private readonly IRolesRepository<Role> _rolesRepository = rolesRepository;
        private readonly ISolutionsRepository<SudokuSolution> _solutionsRepository = solutionsRepository;
        private readonly IUsersRepository<User> _usersRepository = usersRepository;
        private readonly IRequestService _requestService = requestService;
        private readonly IDistributedCache _distributedCache = distributedCache;
        private readonly ICacheService _cacheService = cacheService;
        private readonly ICacheKeys _cacheKeys = cacheKeys;
        private readonly ICachingStrategy _cachingStrategy = cachingStrategy;
        private readonly ILogger<AdminService> _logger = logger;
        #endregion


        public async Task<IResult> GetSystemStatsAndValues()
        {
            var result = new Result();

            try
            {
                var response = await _cacheService.GetSystemStatsAndValues(
                    _appsRepository,
                    _difficultiesRepository,
                    _emailConfirmationsRepository,
                    _gamesRepository,
                    _passwordResetsRepository,
                    _rolesRepository,
                    _solutionsRepository,
                    _usersRepository,
                    _distributedCache,
                    _cacheKeys.GetAdminSummaryKey,
                    _cachingStrategy.Medium,
                    result);

                var adminSummaryResult = (AdminSummaryResult)response.Item1.Object;

                result.Payload.Add(adminSummaryResult);

                return result;
            }
            catch (Exception e)
            {
                return DataUtilities.ProcessException<AdminService>(
                    _requestService,
                    _logger,
                    result,
                    e);
            }
        }
    }
}
