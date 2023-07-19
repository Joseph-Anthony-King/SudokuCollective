using NUnit.Framework;
using SudokuCollective.Cache;
using SudokuCollective.Core.Interfaces.Cache;

namespace SudokuCollective.Test.TestCases.Cache
{
    public class CacheKeysShould
    {
        public ICacheKeys sut;

        [SetUp]
        public void SetUp()
        {
            sut = new CacheKeys();
        }

        [Test, Category("Cache")]
        public void HaveAGetAppKey()
        {
            // Arrange

            // Act

            // Assert
            Assert.That(sut.GetAppCacheKey, Is.EqualTo(":GetApp:{0}"));
        }

        [Test, Category("Cache")]
        public void HaveAGetAppByLicenseKey()
        {
            // Arrange

            // Act

            // Assert
            Assert.That(sut.GetAppByLicenseCacheKey, Is.EqualTo(":GetAppByLicense:{0}"));
        }

        [Test, Category("Cache")]
        public void HaveAGetAppsKey()
        {
            // Arrange

            // Act

            // Assert
            Assert.That(sut.GetAppsCacheKey, Is.EqualTo(":GetApps"));
        }

        [Test, Category("Cache")]
        public void HaveGetMyAppsKey()
        {
            // Arrange

            // Act

            // Assert
            Assert.That(sut.GetMyAppsCacheKey, Is.EqualTo(":GetMyApps:{0}"));
        }

        [Test, Category("Cache")]
        public void HaveGetMyRegisteredKey()
        {
            // Arrange

            // Act

            // Assert
            Assert.That(sut.GetMyRegisteredCacheKey, Is.EqualTo(":GetMyRegistered:{0}"));
        }

        [Test, Category("Cache")]
        public void HaveGetAppUsersKey()
        {
            // Arrange

            // Act

            // Assert
            Assert.That(sut.GetAppUsersCacheKey, Is.EqualTo(":GetAppUsers:{0}"));
        }

        [Test, Category("Cache")]
        public void HaveGetNonAppUsersKey()
        {
            // Arrange

            // Act

            // Assert
            Assert.That(sut.GetNonAppUsersCacheKey, Is.EqualTo(":GetNonAppUsers:{0}"));
        }

        [Test, Category("Cache")]
        public void HaveGetAppLicenseKey()
        {
            // Arrange

            // Act

            // Assert
            Assert.That(sut.GetAppLicenseCacheKey, Is.EqualTo(":GetAppLicense:{0}"));
        }

        [Test, Category("Cache")]
        public void HaveHasAppKey()
        {
            // Arrange

            // Act

            // Assert
            Assert.That(sut.HasAppCacheKey, Is.EqualTo(":HasApp:{0}"));
        }

        [Test, Category("Cache")]
        public void HaveIsAppLicenseValidKey()
        {
            // Arrange

            // Act

            // Assert
            Assert.That(sut.IsAppLicenseValidCacheKey, Is.EqualTo(":IsAppLicenseValid:{0}"));
        }

        [Test, Category("Cache")]
        public void HaveGetDifficultyKey()
        {
            // Arrange

            // Act

            // Assert
            Assert.That(sut.GetDifficultyCacheKey, Is.EqualTo(":GetDifficulty:{0}"));
        }

        [Test, Category("Cache")]
        public void HaveGetDifficultiesKey()
        {
            // Arrange

            // Act

            // Assert
            Assert.That(sut.GetDifficultiesCacheKey, Is.EqualTo(":GetDifficutlies"));
        }

        [Test, Category("Cache")]
        public void HaveGetUserKey()
        {
            // Arrange

            // Act

            // Assert
            Assert.That(sut.GetUserCacheKey, Is.EqualTo(":GetUser:{0}-{1}"));
        }

        [Test, Category("Cache")]
        public void HaveGetUserByUsernameKey()
        {
            // Arrange

            // Act

            // Assert
            Assert.That(sut.GetUserByUsernameCacheKey, Is.EqualTo(":GetUserByUsername:{0}-{1}"));
        }

        [Test, Category("Cache")]
        public void HaveGetUserByEmailKey()
        {
            // Arrange

            // Act

            // Assert
            Assert.That(sut.GetUserByEmailCacheKey, Is.EqualTo(":GetUserByEmail:{0}-{1}"));
        }

        [Test, Category("Cache")]
        public void HaveGetUsersKey()
        {
            // Arrange

            // Act

            // Assert
            Assert.That(sut.GetUsersCacheKey, Is.EqualTo(":GetUsers"));
        }

        [Test, Category("Cache")]
        public void HaveHasUserKey()
        {
            // Arrange

            // Act

            // Assert
            Assert.That(sut.HasUserCacheKey, Is.EqualTo(":HasUser:{0}"));
        }

        [Test, Category("Cache")]
        public void HaveIsUserRegisteredKey()
        {
            // Arrange

            // Act

            // Assert
            Assert.That(sut.IsUserRegisteredCacheKey, Is.EqualTo(":IsUserRegistered:{0}-{1}"));
        }

        [Test, Category("Cache")]
        public void HaveGetSolutionsKey()
        {
            // Arrange

            // Act

            // Assert
            Assert.That(sut.GetSolutionsCacheKey, Is.EqualTo(":GetSolutions"));
        }

        [Test, Category("Cache")]
        public void HaveGetRoleKey()
        {
            // Arrange

            // Act

            // Assert
            Assert.That(sut.GetRoleCacheKey, Is.EqualTo(":GetRole:{0}"));
        }

        [Test, Category("Cache")]
        public void HaveGetRolesKey()
        {
            // Arrange

            // Act

            // Assert
            Assert.That(sut.GetRolesCacheKey, Is.EqualTo(":GetRoles"));
        }

        [Test, Category("Cache")]
        public void HaveGetValuesKey()
        {
            // Arrange

            // Act

            // Assert
            Assert.That(sut.GetValuesKey, Is.EqualTo(":GetValues"));
        }

        [Test, Category("Cache")]
        public void HaveGetGalleryAppsKey()
        {
            // Arrange

            // Act

            // Assert
            Assert.That(sut.GetGalleryAppsKey, Is.EqualTo(":GetGalleryApps"));
        }
    }
}
