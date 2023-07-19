using SudokuCollective.Core.Interfaces.Cache;

namespace SudokuCollective.Cache
{
    public class CacheKeys : ICacheKeys
    {
        #region Fields
        private const string _getAppCacheKey = ":GetApp:{0}";
        private const string _getAppByLicenseCacheKey = ":GetAppByLicense:{0}";
        private const string _getAppsCacheKey = ":GetApps";
        private const string _getMyAppsCacheKey = ":GetMyApps:{0}";
        private const string _getMyRegisteredCacheKey = ":GetMyRegistered:{0}";
        private const string _getAppUsersCacheKey = ":GetAppUsers:{0}";
        private const string _getNonAppUsersCacheKey = ":GetNonAppUsers:{0}";
        private const string _getAppLicenseCacheKey = ":GetAppLicense:{0}";
        private const string _hasAppCacheKey = ":HasApp:{0}";
        private const string _isAppLicenseValidCacheKey = ":IsAppLicenseValid:{0}";
        private const string _getDifficultyCacheKey = ":GetDifficulty:{0}";
        private const string _getDifficultiesCacheKey = ":GetDifficutlies";
        private const string _getUserCacheKey = ":GetUser:{0}-{1}";
        private const string _getUserByUsernameCacheKey = ":GetUserByUsername:{0}-{1}";
        private const string _getUserByEmailCacheKey = ":GetUserByEmail:{0}-{1}";
        private const string _getUsersCacheKey = ":GetUsers";
        private const string _hasUserCacheKey = ":HasUser:{0}";
        private const string _isUserRegisteredCacheKey = ":IsUserRegistered:{0}-{1}";
        private const string _getSolutionsCacheKey = ":GetSolutions";
        private const string _getRoleCacheKey = ":GetRole:{0}";
        private const string _getRolesCacheKey = ":GetRoles";
        private const string _getValuesKey = ":GetValues";
        private const string _getGalleryAppsKey = ":GetGalleryApps";
        #endregion

        #region Properties
        public string GetAppCacheKey { get => _getAppCacheKey; }
        public string GetAppByLicenseCacheKey { get => _getAppByLicenseCacheKey; }
        public string GetAppsCacheKey { get => _getAppsCacheKey; }
        public string GetMyAppsCacheKey { get => _getMyAppsCacheKey; }
        public string GetMyRegisteredCacheKey { get => _getMyRegisteredCacheKey; }
        public string GetAppUsersCacheKey { get => _getAppUsersCacheKey; }
        public string GetNonAppUsersCacheKey { get => _getNonAppUsersCacheKey; }
        public string GetAppLicenseCacheKey { get => _getAppLicenseCacheKey; }
        public string HasAppCacheKey { get => _hasAppCacheKey; }
        public string IsAppLicenseValidCacheKey { get => _isAppLicenseValidCacheKey; }
        public string GetDifficultyCacheKey { get => _getDifficultyCacheKey; }
        public string GetDifficultiesCacheKey { get => _getDifficultiesCacheKey; }
        public string GetUserCacheKey { get => _getUserCacheKey; }
        public string GetUserByUsernameCacheKey { get => _getUserByUsernameCacheKey; }
        public string GetUserByEmailCacheKey { get => _getUserByEmailCacheKey; }
        public string GetUsersCacheKey { get => _getUsersCacheKey; }
        public string HasUserCacheKey { get => _hasUserCacheKey; }
        public string IsUserRegisteredCacheKey { get => _isUserRegisteredCacheKey; }
        public string GetSolutionsCacheKey { get => _getSolutionsCacheKey; }
        public string GetRoleCacheKey { get => _getRoleCacheKey; }
        public string GetRolesCacheKey { get => _getRolesCacheKey; }
        public string GetValuesKey  { get => _getValuesKey; }
        public string GetGalleryAppsKey { get => _getGalleryAppsKey; }
        #endregion
    }
}
