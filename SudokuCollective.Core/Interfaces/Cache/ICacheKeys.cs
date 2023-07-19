namespace SudokuCollective.Core.Interfaces.Cache
{
    public interface ICacheKeys
    {
        string GetAppCacheKey { get; }
        string GetAppByLicenseCacheKey { get; }
        string GetAppsCacheKey { get; }
        string GetMyAppsCacheKey { get; }
        string GetMyRegisteredCacheKey { get; }
        string GetAppUsersCacheKey { get; }
        string GetNonAppUsersCacheKey { get; }
        string GetAppLicenseCacheKey { get; }
        string HasAppCacheKey { get; }
        string IsAppLicenseValidCacheKey { get; }
        string GetDifficultyCacheKey { get; }
        string GetDifficultiesCacheKey { get; }
        string GetUserCacheKey { get; }
        string GetUserByUsernameCacheKey { get; }
        string GetUserByEmailCacheKey { get; }
        string GetUsersCacheKey { get; }
        string HasUserCacheKey { get; }
        string IsUserRegisteredCacheKey { get; }
        string GetSolutionsCacheKey { get; }
        string GetRoleCacheKey { get; }
        string GetRolesCacheKey { get; }
        string GetValuesKey { get; }
        string GetGalleryAppsKey { get; }
    }
}
