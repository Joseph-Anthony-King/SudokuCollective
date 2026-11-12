using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Moq;
using Moq.Protected;
using SudokuCollective.Cache;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Cache;
using SudokuCollective.Core.Interfaces.Repositories;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Test.Repositories;
using SudokuCollective.Test.TestData;
using IConnectionMultiplexer = StackExchange.Redis.IConnectionMultiplexer;
using IServer = StackExchange.Redis.IServer;
using IDatabase = StackExchange.Redis.IDatabase;
using RedisValue = StackExchange.Redis.RedisValue;
using RedisKey = StackExchange.Redis.RedisKey;
using CommandFlags = StackExchange.Redis.CommandFlags;

namespace SudokuCollective.Test.TestCases.Cache
{
    public class CacheServiceShould
    {
        private DatabaseContext context;
        private CacheService sut;
        private MockedAppsRepository MockedAppsRepository;
        private MockedDifficultiesRepository MockedDifficultiesRepository;
        private MockedRolesRepository MockedRolesRepository;
        private MockedUsersRepository MockedUsersRepository;
        private MemoryDistributedCache memoryCache;
        private Mock<IDistributedCache> MockedCache;
        private Mock<HttpMessageHandler> MockedHttpMessageHandler;
        private Mock<IUsersService> MockedUsersService;
        private Mock<IConnectionMultiplexer> MockedRedis;
        private Mock<IServer> MockedServer;
        private Mock<IDatabase> MockedRedisDatabase;
        private ICacheKeys cacheKeys;
        private ICachingStrategy cachingStrategy;

        [SetUp]
        public async Task SetUp()
        {
            context = await TestDatabase.GetDatabaseContext();

            MockedRedis = new Mock<IConnectionMultiplexer>();
            MockedServer = new Mock<IServer>();
            MockedRedisDatabase = new Mock<IDatabase>();

            MockedRedis
                .Setup(redis => redis.GetEndPoints(It.IsAny<bool>()))
                .Returns([new DnsEndPoint("localhost", 6379)]);
            MockedRedis
                .Setup(redis => redis.GetServer(It.IsAny<EndPoint>(), It.IsAny<object>()))
                .Returns(MockedServer.Object);
            MockedRedis
                .Setup(redis => redis.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                .Returns(MockedRedisDatabase.Object);
            MockedServer.SetupGet(server => server.IsConnected).Returns(true);
            MockedServer
                .Setup(server => server.KeysAsync(
                    It.IsAny<int>(),
                    It.IsAny<RedisValue>(),
                    It.IsAny<int>(),
                    It.IsAny<long>(),
                    It.IsAny<int>(),
                    It.IsAny<CommandFlags>()))
                .Returns(GetRedisKeysAsync());
            MockedRedisDatabase
                .Setup(database => database.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(true);

            sut = new CacheService(MockedRedis.Object);
            MockedAppsRepository = new MockedAppsRepository(context);
            MockedDifficultiesRepository = new MockedDifficultiesRepository(context);
            MockedRolesRepository = new MockedRolesRepository(context);
            MockedUsersRepository = new MockedUsersRepository(context);
            memoryCache = new MemoryDistributedCache(
                Options.Create(new MemoryDistributedCacheOptions()));

            MockedCache = new Mock<IDistributedCache>();
            MockedCache
                .Setup(cache => cache.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception("It was not possible to connect to the redis server(s), the password was incorrect."));
            MockedCache
                .Setup(cache => cache.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception("It was not possible to connect to the redis server(s), the password was incorrect."));

            MockedHttpMessageHandler = new Mock<HttpMessageHandler>();
            MockedUsersService = new Mock<IUsersService>();
            MockedUsersService
                .Setup(service => service.GetAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<SudokuCollective.Core.Interfaces.Models.DomainObjects.Params.IRequest>()))
                .ReturnsAsync(new Result
                {
                    IsSuccess = true,
                    Payload = [CreateUser(true)]
                });

            cacheKeys = new CacheKeys();
            cachingStrategy = new CachingStrategy();

            Environment.SetEnvironmentVariable("HEROKUCREDENTIALS:TOKEN", "1265753e-d175-4204-8a79-7396101d096a");
            Environment.SetEnvironmentVariable("HEROKU:HEROKUAPP", "sudokucollective-prod");
            Environment.SetEnvironmentVariable("HEROKU:HEROKUREDIS", "sudokucollective-prod-cache");
        }

        #region Add New Apps
        [Test, Category("Cache")]
        public async Task AddNewApps()
        {
            // Arrange
            var app = new App(
                "New Test App", 
                Guid.NewGuid().ToString(), 
                2, 
                "TestUser",
                "http://localhost:5173",
                "",
                "http://example-dev.com", 
                "http://example.com",
                string.Empty);

            // Act
            var result = await sut.AddWithCacheAsync<App>(
                MockedAppsRepository.SuccessfulRequest.Object, 
                memoryCache,
                cacheKeys.GetAppCacheKey, 
                cachingStrategy.Medium, 
                cacheKeys, 
                app);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That((App)result.DomainObject, Is.InstanceOf<App>());
        }

        [Test, Category("Cache")]
        public async Task ReturnsFalseIfAddNewAppsFails()
        {
            // Arrange
            var app = new App(
                "New Test App", 
                Guid.NewGuid().ToString(), 
                2,
                "TestUser",
                "http://localhost:5173",
                "",
                "http://example-dev.com", 
                "http://example.com",
                string.Empty);

            // Act
            var result = await sut.AddWithCacheAsync<App>(
                MockedAppsRepository.FailedRequest.Object,
                memoryCache,
                cacheKeys.GetAppCacheKey,
                cachingStrategy.Medium,
                cacheKeys,
                app);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfAddNewAppsRedisConnectionFails()
        {
            try
            {
                // Arrange
                var app = new App(
                    "New Test App",
                    Guid.NewGuid().ToString(),
                    2,
                    "TestUser",
                    "http://localhost:5173",
                    "",
                    "http://example-dev.com",
                    "http://example.com",
                    string.Empty);

                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                // Act
                var result = await sut.AddWithCacheAsync<App>(
                    MockedAppsRepository.SuccessfulRequest.Object,
                    MockedCache.Object,
                    string.Format(cacheKeys.GetAppCacheKey, 1),
                    cachingStrategy.Medium,
                    cacheKeys,
                    app,
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Add New Difficulties
        [Test, Category("Cache")]
        public async Task AddNewDifficulties()
        {
            // Arrange
            var difficulty = new Difficulty(7, "Test Difficulty", "Test Difficulty", DifficultyLevel.NULL);

            // Act
            var result = await sut.AddWithCacheAsync<Difficulty>(
                MockedDifficultiesRepository.CreateDifficultyRequest.Object,
                memoryCache,
                cacheKeys.GetDifficultyCacheKey,
                cachingStrategy.Heavy,
                cacheKeys,
                difficulty);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That((Difficulty)result.DomainObject, Is.InstanceOf<Difficulty>());
        }

        [Test, Category("Cache")]
        public async Task ReturnsFalseIfAddNewDifficultiesFails()
        {
            // Arrange
            var difficulty = new Difficulty(7, "Test Difficulty", "Test Difficulty", DifficultyLevel.NULL);

            // Act
            var result = await sut.AddWithCacheAsync<Difficulty>(
                MockedDifficultiesRepository.FailedRequest.Object,
                memoryCache,
                cacheKeys.GetDifficultyCacheKey,
                cachingStrategy.Heavy,
                cacheKeys,
                difficulty);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfAddNewDifficutliesRedisConnectionFails()
        {
            try
            {
                // Arrange
                var difficulty = new Difficulty(7, "Test Difficulty", "Test Difficulty", DifficultyLevel.NULL);

                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                // Act
                var result = await sut.AddWithCacheAsync<Difficulty>(
                    MockedDifficultiesRepository.CreateDifficultyRequest.Object,
                    memoryCache,
                    cacheKeys.GetDifficultyCacheKey,
                    cachingStrategy.Heavy,
                    cacheKeys,
                    difficulty,
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Add New Roles
        [Test, Category("Cache")]
        public async Task AddNewRoles()
        {
            // Arrange
            var Role = new Role(5, "Test Role", RoleLevel.NULL);

            // Act
            var result = await sut.AddWithCacheAsync<Role>(
                MockedRolesRepository.SuccessfulRequest.Object,
                memoryCache,
                cacheKeys.GetRoleCacheKey,
                cachingStrategy.Heavy,
                cacheKeys,
                Role);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That((Role)result.DomainObject, Is.InstanceOf<Role>());
        }

        [Test, Category("Cache")]
        public async Task ReturnFalseIfAddNewRolesFails()
        {
            // Arrange
            var Role = new Role(5, "Test Role", RoleLevel.NULL);

            // Act
            var result = await sut.AddWithCacheAsync<Role>(
                MockedRolesRepository.FailedRequest.Object,
                memoryCache,
                cacheKeys.GetRoleCacheKey,
                cachingStrategy.Heavy,
                cacheKeys,
                Role);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfAddNewRolesRedisConnectionFails()
        {
            try
            {
                // Arrange
                var Role = new Role(5, "Test Role", RoleLevel.NULL);

                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                // Act
                var result = await sut.AddWithCacheAsync<Role>(
                    MockedRolesRepository.SuccessfulRequest.Object,
                    memoryCache,
                    cacheKeys.GetRoleCacheKey,
                    cachingStrategy.Heavy,
                    cacheKeys,
                    Role,
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Add New Users
        [Test, Category("Cache")]
        public async Task AddNewUsers()
        {
            // Arrange
            var user = new User("John", "Doe", "T3stPass0rd?4");

            // Act
            var result = await sut.AddWithCacheAsync<User>(
                MockedUsersRepository.SuccessfulRequest.Object,
                memoryCache,
                cacheKeys.GetUserCacheKey,
                cachingStrategy.Medium,
                cacheKeys,
                user);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That((User)result.DomainObject, Is.InstanceOf<User>());
        }

        [Test, Category("Cache")]
        public async Task ReturnFallseIfAddNewUsersFails()
        {
            // Arrange
            var user = new User("John", "Doe", "T3stPass0rd?4");

            // Act
            var result = await sut.AddWithCacheAsync<User>(
                MockedUsersRepository.FailedRequest.Object,
                memoryCache,
                cacheKeys.GetUserCacheKey,
                cachingStrategy.Medium,
                cacheKeys,
                user);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfAddNewUsersRedisConnectionFails()
        {
            try
            {
                // Arrange
                var user = new User("John", "Doe", "T3stPass0rd?4");

                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                // Act
                var result = await sut.AddWithCacheAsync<User>(
                    MockedUsersRepository.SuccessfulRequest.Object,
                    memoryCache,
                    cacheKeys.GetUserCacheKey,
                    cachingStrategy.Medium,
                    cacheKeys,
                    user,
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Get Apps
        [Test, Category("Cache")]
        public async Task GetApp()
        {
            // Arrange

            // Act
            var result = await sut.GetWithCacheAsync<App>(
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetAppCacheKey, 1),
                cachingStrategy.Medium,
                1);

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.True);
            Assert.That(((RepositoryResponse)result.Item1).DomainObject, Is.InstanceOf<App>());
        }

        [Test, Category("Cache")]
        public async Task IndicateIfAppIsFromCache()
        {
            // Arrange
            _ = await sut.GetWithCacheAsync<App>(
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetAppCacheKey, 1),
                cachingStrategy.Medium,
                1);

            // Act
            var result = await sut.GetWithCacheAsync<App>(
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetAppCacheKey, 1),
                cachingStrategy.Medium,
                1,
                new Result());

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.True);
            Assert.That(((RepositoryResponse)result.Item1).DomainObject, Is.InstanceOf<App>());
            Assert.That(((Result)result.Item2).IsFromCache, Is.True);
        }

        [Test, Category("Cache")]
        public async Task ReturnFalseIfGetAppFails()
        {
            // Arrange

            // Act
            var result = await sut.GetWithCacheAsync<App>(
                MockedAppsRepository.FailedRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetAppCacheKey, 1),
                cachingStrategy.Medium,
                1);

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfGetAppRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                // Act
                var result = await sut.GetWithCacheAsync<App>(
                    MockedAppsRepository.SuccessfulRequest.Object,
                    MockedCache.Object,
                    string.Format(cacheKeys.GetAppCacheKey, 1),
                    cachingStrategy.Medium,
                    1,
                    null,
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Get Difficulties
        [Test, Category("Cache")]
        public async Task GetDifficulty()
        {
            // Arrange

            // Act
            var result = await sut.GetWithCacheAsync<Difficulty>(
                MockedDifficultiesRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetDifficultyCacheKey, 1),
                cachingStrategy.Medium,
                1);

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.True);
            Assert.That(((RepositoryResponse)result.Item1).DomainObject, Is.InstanceOf<Difficulty>());
        }

        [Test, Category("Cache")]
        public async Task IndicateIfDifficultyIsFromCache()
        {
            // Arrange
            _ = await sut.GetWithCacheAsync<Difficulty>(
                MockedDifficultiesRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetDifficultyCacheKey, 1),
                cachingStrategy.Medium,
                1);

            // Act
            var result = await sut.GetWithCacheAsync<Difficulty>(
                MockedDifficultiesRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetDifficultyCacheKey, 1),
                cachingStrategy.Medium,
                1,
                new Result());

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.True);
            Assert.That(((RepositoryResponse)result.Item1).DomainObject, Is.InstanceOf<Difficulty>());
            Assert.That(((Result)result.Item2).IsFromCache, Is.True);
        }

        [Test, Category("Cache")]
        public async Task ReturnFalseIfGetDifficultyFails()
        {
            // Arrange

            // Act
            var result = await sut.GetWithCacheAsync<Difficulty>(
                MockedDifficultiesRepository.FailedRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetDifficultyCacheKey, 1),
                cachingStrategy.Medium,
                1);

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfGetDifficultyRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                // Act
                var result = await sut.GetWithCacheAsync<Difficulty>(
                    MockedDifficultiesRepository.SuccessfulRequest.Object,
                    memoryCache,
                    string.Format(cacheKeys.GetDifficultyCacheKey, 1),
                    cachingStrategy.Medium,
                    1,
                    null,
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Get Roles
        [Test, Category("Cache")]
        public async Task GetRole()
        {
            // Arrange

            // Act
            var result = await sut.GetWithCacheAsync<Role>(
                MockedRolesRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetRoleCacheKey, 1),
                cachingStrategy.Medium,
                1);

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.True);
            Assert.That(((RepositoryResponse)result.Item1).DomainObject, Is.InstanceOf<Role>());
        }

        [Test, Category("Cache")]
        public async Task IndicateIfRoleIsFromCache()
        {
            // Arrange
            _ = await sut.GetWithCacheAsync<Role>(
                MockedRolesRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetRoleCacheKey, 1),
                cachingStrategy.Medium,
                1);

            // Act
            var result = await sut.GetWithCacheAsync<Role>(
                MockedRolesRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetRoleCacheKey, 1),
                cachingStrategy.Medium,
                1,
                new Result());

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.True);
            Assert.That(((RepositoryResponse)result.Item1).DomainObject, Is.InstanceOf<Role>());
            Assert.That(((Result)result.Item2).IsFromCache, Is.True);
        }

        [Test, Category("Cache")]
        public async Task ReturnFalseIfGetRoleFails()
        {
            // Arrange

            // Act
            var result = await sut.GetWithCacheAsync<Role>(
                MockedRolesRepository.FailedRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetRoleCacheKey, 1),
                cachingStrategy.Medium,
                1);

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfGetRolesRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                // Act
                var result = await sut.GetWithCacheAsync<Role>(
                    MockedRolesRepository.SuccessfulRequest.Object,
                    MockedCache.Object,
                    string.Format(cacheKeys.GetRoleCacheKey, 1),
                    cachingStrategy.Medium,
                    1,
                    null,
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Get Users
        [Test, Category("Cache")]
        public async Task GetUser()
        {
            // Arrange

            // Act
            var result = await sut.GetWithCacheAsync<User>(
                MockedUsersRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetUserCacheKey, 1, TestObjects.GetLicense()),
                cachingStrategy.Medium,
                1);

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.True);
            Assert.That(((RepositoryResponse)result.Item1).DomainObject, Is.InstanceOf<User>());
        }

        [Test, Category("Cache")]
        public async Task IndicateIfUserIsFromCache()
        {
            // Arrange
            _ = await sut.GetWithCacheAsync<User>(
                MockedUsersRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetUserCacheKey,1, TestObjects.GetLicense()),
                cachingStrategy.Medium,
                1);

            // Act
            var result = await sut.GetWithCacheAsync<User>(
                MockedUsersRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetUserCacheKey, 1, TestObjects.GetLicense()),
                cachingStrategy.Medium,
                1,
                new Result());

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.True);
            Assert.That(((RepositoryResponse)result.Item1).DomainObject, Is.InstanceOf<User>());
            Assert.That(((Result)result.Item2).IsFromCache, Is.True);
        }

        [Test, Category("Cache")]
        public async Task ReturnFalseIfGetUserFails()
        {
            // Arrange

            // Act
            var result = await sut.GetWithCacheAsync<User>(
                MockedUsersRepository.FailedRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetUserCacheKey, 1, TestObjects.GetLicense()),
                cachingStrategy.Medium,
                1);

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfGetUsersRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                // Act
                var result = await sut.GetWithCacheAsync<User>(
                    MockedUsersRepository.SuccessfulRequest.Object,
                    memoryCache,
                    string.Format(cacheKeys.GetUserCacheKey, 1, TestObjects.GetLicense()),
                    cachingStrategy.Medium,
                    1,
                    null,
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Get All Apps
        [Test, Category("Cache")]
        public async Task GetAllApps()
        {
            // Arrange

            // Act
            var result = await sut.GetAllWithCacheAsync<App>(
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                cacheKeys.GetAppsCacheKey,
                cachingStrategy.Medium);

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.True);
            Assert.That(((RepositoryResponse)result.Item1)
                .DomainObjects
                .ConvertAll(a => (App)a), Is.InstanceOf<List<App>>());
        }

        [Test, Category("Cache")]
        public async Task IndicateIfAllAppsAreFromCache()
        {
            // Arrange
            _ = await sut.GetAllWithCacheAsync<App>(
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                cacheKeys.GetAppsCacheKey,
                cachingStrategy.Medium);

            // Act
            var result = await sut.GetAllWithCacheAsync<App>(
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                cacheKeys.GetAppsCacheKey,
                cachingStrategy.Medium,
                new Result());

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.True);
            Assert.That(((RepositoryResponse)result.Item1)
                .DomainObjects
                .ConvertAll(a => (App)a), Is.InstanceOf<List<App>>());
            Assert.That(((Result)result.Item2).IsFromCache, Is.True);
        }

        [Test, Category("Cache")]
        public async Task ReturnFalseIfGetAllAppsFails()
        {
            // Arrange

            // Act
            var result = await sut.GetAllWithCacheAsync<App>(
                MockedAppsRepository.FailedRequest.Object,
                memoryCache,
                cacheKeys.GetAppsCacheKey,
                cachingStrategy.Medium);

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfGetAllAppsRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                // Act
                var result = await sut.GetAllWithCacheAsync<App>(
                    MockedAppsRepository.SuccessfulRequest.Object,
                    memoryCache,
                    cacheKeys.GetAppsCacheKey,
                    cachingStrategy.Medium,
                    null,
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Get All Difficulties
        [Test, Category("Cache")]
        public async Task GetAllDifficulties()
        {
            // Arrange

            // Act
            var result = await sut.GetAllWithCacheAsync<Difficulty>(
                MockedDifficultiesRepository.SuccessfulRequest.Object,
                memoryCache,
                cacheKeys.GetDifficultiesCacheKey,
                cachingStrategy.Heavy);

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.True);
            Assert.That(((RepositoryResponse)result.Item1)
                .DomainObjects
                .ConvertAll(a => (Difficulty)a), Is.InstanceOf<List<Difficulty>>());
        }

        [Test, Category("Cache")]
        public async Task IndicateIfAllDifficultiesAreFromCache()
        {
            // Arrange
            _ = await sut.GetAllWithCacheAsync<Difficulty>(
                MockedDifficultiesRepository.SuccessfulRequest.Object,
                memoryCache,
                cacheKeys.GetDifficultiesCacheKey,
                cachingStrategy.Heavy);

            // Act
            var result = await sut.GetAllWithCacheAsync<Difficulty>(
                MockedDifficultiesRepository.SuccessfulRequest.Object,
                memoryCache,
                cacheKeys.GetDifficultiesCacheKey,
                cachingStrategy.Heavy,
                new Result());

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.True);
            Assert.That(((RepositoryResponse)result.Item1)
                .DomainObjects
                .ConvertAll(a => (Difficulty)a), Is.InstanceOf<List<Difficulty>>());
            Assert.That(((Result)result.Item2).IsFromCache, Is.True);
        }

        [Test, Category("Cache")]
        public async Task ReturnFalseIfGetAllDifficultiesFails()
        {
            // Arrange

            // Act
            var result = await sut.GetAllWithCacheAsync<Difficulty>(
                MockedDifficultiesRepository.FailedRequest.Object,
                memoryCache,
                cacheKeys.GetDifficultiesCacheKey,
                cachingStrategy.Heavy);

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfGetAllDifficultiesRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                // Act
                var result = await sut.GetAllWithCacheAsync<Difficulty>(
                    MockedDifficultiesRepository.SuccessfulRequest.Object,
                    memoryCache,
                    cacheKeys.GetDifficultiesCacheKey,
                    cachingStrategy.Heavy,
                    null,
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Get All Roles
        [Test, Category("Cache")]
        public async Task GetAllRoles()
        {
            // Arrange

            // Act
            var result = await sut.GetAllWithCacheAsync<Role>(
                MockedRolesRepository.SuccessfulRequest.Object,
                memoryCache,
                cacheKeys.GetRolesCacheKey,
                cachingStrategy.Heavy);

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.True);
            Assert.That(((RepositoryResponse)result.Item1)
                .DomainObjects
                .ConvertAll(a => (Role)a), Is.InstanceOf<List<Role>>());
        }

        [Test, Category("Cache")]
        public async Task IndicateIfAllRolesAreFromCache()
        {
            // Arrange
            _ = await sut.GetAllWithCacheAsync<Role>(
                MockedRolesRepository.SuccessfulRequest.Object,
                memoryCache,
                cacheKeys.GetRolesCacheKey,
                cachingStrategy.Heavy);

            // Act
            var result = await sut.GetAllWithCacheAsync<Role>(
                MockedRolesRepository.SuccessfulRequest.Object,
                memoryCache,
                cacheKeys.GetRolesCacheKey,
                cachingStrategy.Heavy,
                new Result());

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.True);
            Assert.That(((RepositoryResponse)result.Item1)
                .DomainObjects
                .ConvertAll(a => (Role)a), Is.InstanceOf<List<Role>>());
            Assert.That(((Result)result.Item2).IsFromCache, Is.True);
        }

        [Test, Category("Cache")]
        public async Task ReturnFalseIfGetAllRolesFails()
        {
            // Arrange

            // Act
            var result = await sut.GetAllWithCacheAsync<Role>(
                MockedRolesRepository.FailedRequest.Object,
                memoryCache,
                cacheKeys.GetRolesCacheKey,
                cachingStrategy.Heavy);

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfGetAllRolesRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                // Act
                var result = await sut.GetAllWithCacheAsync<Role>(
                    MockedRolesRepository.SuccessfulRequest.Object,
                    memoryCache,
                    cacheKeys.GetRolesCacheKey,
                    cachingStrategy.Heavy,
                    null,
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Get All Users
        [Test, Category("Cache")]
        public async Task GetAllUsers()
        {
            // Arrange

            // Act
            var result = await sut.GetAllWithCacheAsync<User>(
                MockedUsersRepository.SuccessfulRequest.Object,
                memoryCache,
                cacheKeys.GetUsersCacheKey,
                cachingStrategy.Medium);

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.True);
            Assert.That(((RepositoryResponse)result.Item1)
                .DomainObjects
                .ConvertAll(a => (User)a), Is.InstanceOf<List<User>>());
        }

        [Test, Category("Cache")]
        public async Task IndicateIfAllUsersAreFromCache()
        {
            // Arrange
            _ = await sut.GetAllWithCacheAsync<User>(
                MockedUsersRepository.SuccessfulRequest.Object,
                memoryCache,
                cacheKeys.GetUsersCacheKey,
                cachingStrategy.Medium);

            // Act
            var result = await sut.GetAllWithCacheAsync<User>(
                MockedUsersRepository.SuccessfulRequest.Object,
                memoryCache,
                cacheKeys.GetUsersCacheKey,
                cachingStrategy.Medium,
                new Result());

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.True);
            Assert.That(((RepositoryResponse)result.Item1)
                .DomainObjects
                .ConvertAll(a => (User)a), Is.InstanceOf<List<User>>());
            Assert.That(((Result)result.Item2).IsFromCache, Is.True);
        }

        [Test, Category("Cache")]
        public async Task ReturnFalseIfGetAllUsersFails()
        {
            // Arrange

            // Act
            var result = await sut.GetAllWithCacheAsync<User>(
                MockedUsersRepository.FailedRequest.Object,
                memoryCache,
                cacheKeys.GetUsersCacheKey,
                cachingStrategy.Medium,
                new Result());

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfGetAllUsersRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                // Act
                var result = await sut.GetAllWithCacheAsync<User>(
                    MockedUsersRepository.SuccessfulRequest.Object,
                    memoryCache,
                    cacheKeys.GetUsersCacheKey,
                    cachingStrategy.Medium,
                    null,
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Update Apps
        [Test, Category("Cache")]
        public async Task UpdateApps()
        {
            // Arrange
            var app = (App)MockedAppsRepository
                .SuccessfulRequest
                .Object
                .GetAsync(1)
                .Result
                .DomainObject;
            app.Name = string.Format(app.Name + " {0}", "UPDATED!");

            // Act
            var result = await sut.UpdateWithCacheAsync<App>(
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                cacheKeys,
                app,
                TestObjects.GetLicense());

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That((App)result.DomainObject, Is.InstanceOf<App>());
            Assert.That(((App)result.DomainObject).Name, Is.EqualTo(app.Name));
        }

        [Test, Category("Cache")]
        public async Task ReturnsFalseIfUpdateAppsFails()
        {
            // Arrange
            var app = (App)MockedAppsRepository
                .SuccessfulRequest
                .Object
                .GetAsync(1)
                .Result
                .DomainObject;
            app.Name = string.Format(app.Name + " {0}", "UPDATED!");

            // Act
            var result = await sut.UpdateWithCacheAsync<App>(
                MockedAppsRepository.FailedRequest.Object,
                memoryCache,
                cacheKeys,
                app,
                TestObjects.GetLicense());

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfUpdateAppsRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                var app = (App)MockedAppsRepository
                    .SuccessfulRequest
                    .Object
                    .GetAsync(1)
                    .Result
                    .DomainObject;
                app.Name = string.Format(app.Name + " {0}", "UPDATED!");

                // Act
                var result = await sut.UpdateWithCacheAsync<App>(
                    MockedAppsRepository.SuccessfulRequest.Object,
                    memoryCache,
                    cacheKeys,
                    app,
                    TestObjects.GetLicense(),
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Update Difficulties
        [Test, Category("Cache")]
        public async Task UpdateDifficulties()
        {
            // Arrange
            var difficulty = (Difficulty)MockedDifficultiesRepository
                .SuccessfulRequest
                .Object
                .GetAsync(1)
                .Result
                .DomainObject;
            difficulty.DisplayName = string.Format(difficulty.DisplayName + " {0}", "UPDATED!");

            // Act
            var result = await sut.UpdateWithCacheAsync<Difficulty>(
                MockedDifficultiesRepository.SuccessfulRequest.Object,
                memoryCache,
                cacheKeys,
                difficulty,
                TestObjects.GetLicense());

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That((Difficulty)result.DomainObject, Is.InstanceOf<Difficulty>());
            Assert.That(((Difficulty)result.DomainObject).DisplayName, Is.EqualTo(difficulty.DisplayName));
        }

        [Test, Category("Cache")]
        public async Task ReturnsFalseIfUpdateDifficultiesFails()
        {
            // Arrange
            var difficulty = (Difficulty)MockedDifficultiesRepository
                .SuccessfulRequest
                .Object
                .GetAsync(1)
                .Result
                .DomainObject;
            difficulty.DisplayName = string.Format(difficulty.DisplayName + " {0}", "UPDATED!");

            // Act
            var result = await sut.UpdateWithCacheAsync<Difficulty>(
                MockedDifficultiesRepository.FailedRequest.Object,
                memoryCache,
                cacheKeys,
                difficulty,
                TestObjects.GetLicense());

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfUpdateDifficultiesRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                var difficulty = (Difficulty)MockedDifficultiesRepository
                    .SuccessfulRequest
                    .Object
                    .GetAsync(1)
                    .Result
                    .DomainObject;
                difficulty.DisplayName = string.Format(difficulty.DisplayName + " {0}", "UPDATED!");

                // Act
                var result = await sut.UpdateWithCacheAsync<Difficulty>(
                    MockedDifficultiesRepository.SuccessfulRequest.Object,
                    memoryCache,
                    cacheKeys,
                    difficulty,
                    TestObjects.GetLicense(),
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Update Roles
        [Test, Category("Cache")]
        public async Task UpdateRoles()
        {
            // Arrange
            var role = (Role)MockedRolesRepository
                .SuccessfulRequest
                .Object
                .GetAsync(1)
                .Result
                .DomainObject;
            role.Name = string.Format(role.Name + " {0}", "UPDATED!");

            // Act
            var result = await sut.UpdateWithCacheAsync<Role>(
                MockedRolesRepository.SuccessfulRequest.Object,
                memoryCache,
                cacheKeys,
                role,
                TestObjects.GetLicense());

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That((Role)result.DomainObject, Is.InstanceOf<Role>());
            Assert.That(((Role)result.DomainObject).Name, Is.EqualTo(role.Name));
        }

        [Test, Category("Cache")]
        public async Task ReturnsFalseIfUpdateRolesFails()
        {
            // Arrange
            var role = (Role)MockedRolesRepository
                .SuccessfulRequest
                .Object
                .GetAsync(1)
                .Result
                .DomainObject;
            role.Name = string.Format(role.Name + " {0}", "UPDATED!");

            // Act
            var result = await sut.UpdateWithCacheAsync<Role>(
                MockedRolesRepository.FailedRequest.Object,
                memoryCache,
                cacheKeys,
                role,
                TestObjects.GetLicense());

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfUpdateRolesRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                var role = (Role)MockedRolesRepository
                    .SuccessfulRequest
                    .Object
                    .GetAsync(1)
                    .Result
                    .DomainObject;
                role.Name = string.Format(role.Name + " {0}", "UPDATED!");

                // Act
                var result = await sut.UpdateWithCacheAsync<Role>(
                    MockedRolesRepository.SuccessfulRequest.Object,
                    memoryCache,
                    cacheKeys,
                    role,
                    TestObjects.GetLicense(),
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Update Users
        [Test, Category("Cache")]
        public async Task UpdateUsers()
        {
            // Arrange
            var user = (User)MockedUsersRepository
                .SuccessfulRequest
                .Object
                .GetAsync(1)
                .Result
                .DomainObject;
            user.UserName = string.Format(user.UserName + " {0}", "UPDATED!");

            // Act
            var result = await sut.UpdateWithCacheAsync<User>(
                MockedUsersRepository.SuccessfulRequest.Object,
                memoryCache,
                cacheKeys,
                user,
                TestObjects.GetLicense());

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That((User)result.DomainObject, Is.InstanceOf<User>());
            Assert.That(((User)result.DomainObject).UserName, Is.EqualTo(user.UserName));
        }

        [Test, Category("Cache")]
        public async Task ReturnsFalseIfUpdateUsersFails()
        {
            // Arrange
            var user = (User)MockedUsersRepository
                .SuccessfulRequest
                .Object
                .GetAsync(1)
                .Result
                .DomainObject;
            user.UserName = string.Format(user.UserName + " {0}", "UPDATED!");

            // Act
            var result = await sut.UpdateWithCacheAsync<User>(
                MockedUsersRepository.FailedRequest.Object,
                memoryCache,
                cacheKeys,
                user,
                TestObjects.GetLicense());

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfUpdateUsersRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                var user = (User)MockedUsersRepository
                    .SuccessfulRequest
                    .Object
                    .GetAsync(1)
                    .Result
                    .DomainObject;
                user.UserName = string.Format(user.UserName + " {0}", "UPDATED!");

                // Act
                var result = await sut.UpdateWithCacheAsync<User>(
                    MockedUsersRepository.SuccessfulRequest.Object,
                    memoryCache,
                    cacheKeys,
                    user,
                    TestObjects.GetLicense(),
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Update Apps
        [Test, Category("Cache")]
        public async Task DeleteApps()
        {
            // Arrange
            var app = (App)MockedAppsRepository
                .SuccessfulRequest
                .Object
                .GetAsync(1)
                .Result
                .DomainObject;

            // Act
            var result = await sut.DeleteWithCacheAsync<App>(
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                cacheKeys,
                app,
                TestObjects.GetLicense());

            // Assert
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test, Category("Cache")]
        public async Task ReturnsFalseIfDeleteAppsFails()
        {
            // Arrange
            var app = (App)MockedAppsRepository
                .SuccessfulRequest
                .Object
                .GetAsync(1)
                .Result
                .DomainObject;

            // Act
            var result = await sut.DeleteWithCacheAsync<App>(
                MockedAppsRepository.FailedRequest.Object,
                memoryCache,
                cacheKeys,
                app,
                TestObjects.GetLicense());

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfDeleteAppsRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                var app = (App)MockedAppsRepository
                    .SuccessfulRequest
                    .Object
                    .GetAsync(1)
                    .Result
                    .DomainObject;

                // Act
                var result = await sut.DeleteWithCacheAsync<App>(
                    MockedAppsRepository.SuccessfulRequest.Object,
                    memoryCache,
                    cacheKeys,
                    app,
                    TestObjects.GetLicense(),
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Update Difficulties
        [Test, Category("Cache")]
        public async Task DeleteDifficulties()
        {
            // Arrange
            var difficulty = (Difficulty)MockedDifficultiesRepository
                .SuccessfulRequest
                .Object
                .GetAsync(1)
                .Result
                .DomainObject;

            // Act
            var result = await sut.DeleteWithCacheAsync<Difficulty>(
                MockedDifficultiesRepository.SuccessfulRequest.Object,
                memoryCache,
                cacheKeys,
                difficulty,
                TestObjects.GetLicense());

            // Assert
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test, Category("Cache")]
        public async Task ReturnsFalseIfDeleteDifficultiesFails()
        {
            // Arrange
            var difficulty = (Difficulty)MockedDifficultiesRepository
                .SuccessfulRequest
                .Object
                .GetAsync(1)
                .Result
                .DomainObject;

            // Act
            var result = await sut.DeleteWithCacheAsync<Difficulty>(
                MockedDifficultiesRepository.FailedRequest.Object,
                memoryCache,
                cacheKeys,
                difficulty,
                TestObjects.GetLicense());

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfDeleteDifficultiesRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                var difficulty = (Difficulty)MockedDifficultiesRepository
                    .SuccessfulRequest
                    .Object
                    .GetAsync(1)
                    .Result
                    .DomainObject;

                // Act
                var result = await sut.DeleteWithCacheAsync<Difficulty>(
                    MockedDifficultiesRepository.SuccessfulRequest.Object,
                    memoryCache,
                    cacheKeys,
                    difficulty,
                    TestObjects.GetLicense(),
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Delete Roles
        [Test, Category("Cache")]
        public async Task DeleteRoles()
        {
            // Arrange
            var role = (Role)MockedRolesRepository
                .SuccessfulRequest
                .Object
                .GetAsync(1)
                .Result
                .DomainObject;

            // Act
            var result = await sut.DeleteWithCacheAsync<Role>(
                MockedRolesRepository.SuccessfulRequest.Object,
                memoryCache,
                cacheKeys,
                role,
                TestObjects.GetLicense());

            // Assert
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test, Category("Cache")]
        public async Task ReturnsFalseIfDeleteRolesFails()
        {
            // Arrange
            var role = (Role)MockedRolesRepository
                .SuccessfulRequest
                .Object
                .GetAsync(1)
                .Result
                .DomainObject;

            // Act
            var result = await sut.DeleteWithCacheAsync<Role>(
                MockedRolesRepository.FailedRequest.Object,
                memoryCache,
                cacheKeys,
                role,
                TestObjects.GetLicense());

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfDeleteRolesRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                var role = (Role)MockedRolesRepository
                    .SuccessfulRequest
                    .Object
                    .GetAsync(1)
                    .Result
                    .DomainObject;

                // Act
                var result = await sut.DeleteWithCacheAsync<Role>(
                    MockedRolesRepository.SuccessfulRequest.Object,
                    memoryCache,
                    cacheKeys,
                    role,
                    TestObjects.GetLicense(),
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Delete Users
        [Test, Category("Cache")]
        public async Task DeleteUsers()
        {
            // Arrange
            var user = (User)MockedUsersRepository
                .SuccessfulRequest
                .Object
                .GetAsync(1)
                .Result
                .DomainObject;

            // Act
            var result = await sut.DeleteWithCacheAsync<User>(
                MockedUsersRepository.SuccessfulRequest.Object,
                memoryCache,
                cacheKeys,
                user,
                TestObjects.GetLicense());

            // Assert
            Assert.That(result.IsSuccess, Is.True);
        }

        [Test, Category("Cache")]
        public async Task ReturnsFalseIfDeleteUsersFails()
        {
            // Arrange
            var user = (User)MockedUsersRepository
                .SuccessfulRequest
                .Object
                .GetAsync(1)
                .Result
                .DomainObject;

            // Act
            var result = await sut.DeleteWithCacheAsync<User>(
                MockedUsersRepository.FailedRequest.Object,
                memoryCache,
                cacheKeys,
                user,
                TestObjects.GetLicense());

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfDeleteUsersRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                var user = (User)MockedUsersRepository
                    .SuccessfulRequest
                    .Object
                    .GetAsync(1)
                    .Result
                    .DomainObject;

                // Act
                var result = await sut.DeleteWithCacheAsync<User>(
                    MockedUsersRepository.SuccessfulRequest.Object,
                    memoryCache,
                    cacheKeys,
                    user,
                    TestObjects.GetLicense(),
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Confirm it has anApp
        [Test, Category("Cache")]
        public async Task ConfirmItHasAnApp()
        {
            // Arrange

            // Act
            var result = await sut.HasEntityWithCacheAsync<App>(
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetAppCacheKey, 1),
                cachingStrategy.Medium,
                1);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test, Category("Cache")]
        public async Task ReturnsFalseIfConfirmItHasAnAppFails()
        {
            // Arrange

            // Act
            var result = await sut.HasEntityWithCacheAsync<App>(
                MockedAppsRepository.FailedRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetAppCacheKey, 1),
                cachingStrategy.Medium,
                1);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfConfirmItHasAnAppRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                // Act
                var result = await sut.HasEntityWithCacheAsync<App>(
                    MockedAppsRepository.SuccessfulRequest.Object,
                    memoryCache,
                    string.Format(cacheKeys.GetAppCacheKey, 1),
                    cachingStrategy.Medium,
                    1,
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Confirm it has a Difficulty
        [Test, Category("Cache")]
        public async Task ConfirmItHasADifficulty()
        {
            // Arrange


            // Act
            var result = await sut.HasEntityWithCacheAsync<Difficulty>(
                MockedDifficultiesRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetDifficultyCacheKey, 1),
                cachingStrategy.Heavy,
                1);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test, Category("Cache")]
        public async Task ReturnsFalseIfConfirmItHasADifficultyFails()
        {
            // Arrange

            // Act
            var result = await sut.HasEntityWithCacheAsync<Difficulty>(
                MockedDifficultiesRepository.FailedRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetDifficultyCacheKey, 1),
                cachingStrategy.Heavy,
                1);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfConfirmItHasAnDifficultyRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                // Act
                var result = await sut.HasEntityWithCacheAsync<Difficulty>(
                    MockedDifficultiesRepository.SuccessfulRequest.Object,
                    memoryCache,
                    string.Format(cacheKeys.GetDifficultyCacheKey, 1),
                    cachingStrategy.Heavy,
                    1,
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Confirm it has a Role
        [Test, Category("Cache")]
        public async Task ConfirmItHasARole()
        {
            // Arrange

            // Act
            var result = await sut.HasEntityWithCacheAsync<Role>(
                MockedRolesRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetRoleCacheKey, 1),
                cachingStrategy.Heavy,
                1);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test, Category("Cache")]
        public async Task ReturnsFalseIfConfirmItHasARoleFails()
        {
            // Arrange

            // Act
            var result = await sut.HasEntityWithCacheAsync<Role>(
                MockedRolesRepository.FailedRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetRoleCacheKey, 1),
                cachingStrategy.Heavy,
                1);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfConfirmItHasARoleRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                // Act
                var result = await sut.HasEntityWithCacheAsync<Role>(
                    MockedRolesRepository.SuccessfulRequest.Object,
                    memoryCache,
                    string.Format(cacheKeys.GetRoleCacheKey, 1),
                    cachingStrategy.Heavy,
                    1,
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Confirm it has a user
        [Test, Category("Cache")]
        public async Task ConfirmItHasAUser()
        {
            // Arrange

            // Act
            var result = await sut.HasEntityWithCacheAsync<User>(
                MockedUsersRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetRoleCacheKey, 1),
                cachingStrategy.Medium,
                1);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test, Category("Cache")]
        public async Task ReturnsFalseIfConfirmItHasAUserFails()
        {
            // Arrange

            // Act
            var result = await sut.HasEntityWithCacheAsync<User>(
                MockedUsersRepository.FailedRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetRoleCacheKey, 1),
                cachingStrategy.Medium,
                1);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfConfirmItHasAUserRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                // Act
                var result = await sut.HasEntityWithCacheAsync<User>(
                    MockedUsersRepository.SuccessfulRequest.Object,
                    memoryCache,
                    string.Format(cacheKeys.GetRoleCacheKey, 1),
                    cachingStrategy.Medium,
                    1,
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Get App by License
        [Test, Category("Cache")]
        public async Task GetAppByLicense()
        {
            // Arrange

            // Act
            var result = await sut.GetAppByLicenseWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetAppByLicenseCacheKey, TestObjects.GetLicense()),
                cachingStrategy.Medium,
                TestObjects.GetLicense());

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.True);
            Assert.That(((RepositoryResponse)result.Item1).DomainObject, Is.InstanceOf<App>());
        }

        [Test, Category("Cache")]
        public async Task IndicateIfGetAppByLicenseFromCache()
        {
            // Arrange
            _ = await sut.GetAppByLicenseWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetAppByLicenseCacheKey, TestObjects.GetLicense()),
                cachingStrategy.Medium,
                TestObjects.GetLicense());

            // Act
            var result = await sut.GetAppByLicenseWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetAppByLicenseCacheKey, TestObjects.GetLicense()),
                cachingStrategy.Medium,
                TestObjects.GetLicense(),
                new Result());

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.True);
            Assert.That(((RepositoryResponse)result.Item1).DomainObject, Is.InstanceOf<App>());
            Assert.That(((Result)result.Item2).IsFromCache, Is.True);
        }

        [Test, Category("Cache")]
        public async Task ReturnFalseIfGetAppByLicenseFails()
        {
            // Arrange

            // Act
            var result = await sut.GetAppByLicenseWithCacheAsync(
                MockedAppsRepository.FailedRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetAppByLicenseCacheKey, TestObjects.GetLicense()),
                cachingStrategy.Medium,
                TestObjects.GetLicense());

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfGetAppByLicenseRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                // Act
                var result = await sut.GetAppByLicenseWithCacheAsync(
                    MockedAppsRepository.SuccessfulRequest.Object,
                    memoryCache,
                    string.Format(cacheKeys.GetAppByLicenseCacheKey, TestObjects.GetLicense()),
                    cachingStrategy.Medium,
                    TestObjects.GetLicense(),
                    null,
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Get App Users
        [Test, Category("Cache")]
        public async Task GetAppUsers()
        {
            // Arrange

            // Act
            var result = await sut.GetAppUsersWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetAppUsersCacheKey, 1),
                cachingStrategy.Medium,
                1);

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.True);
            Assert.That(((RepositoryResponse)result.Item1).DomainObjects.ConvertAll(u => (User)u), Is.InstanceOf<List<User>>());
        }

        [Test, Category("Cache")]
        public async Task IndicateIfGetAppUsersFromCache()
        {
            // Arrange
            _ = await sut.GetAppUsersWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetAppUsersCacheKey, 1),
                cachingStrategy.Medium,
                1);

            // Act
            var result = await sut.GetAppUsersWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetAppUsersCacheKey, 1),
                cachingStrategy.Medium,
                1,
                new Result());

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.True);
            Assert.That(((RepositoryResponse)result.Item1).DomainObjects.ConvertAll(u => (User)u), Is.InstanceOf<List<User>>());
            Assert.That(((Result)result.Item2).IsFromCache, Is.True);
        }

        [Test, Category("Cache")]
        public async Task ReturnFalseIfGetAppUsersFails()
        {
            // Arrange

            // Act
            var result = await sut.GetAppUsersWithCacheAsync(
                MockedAppsRepository.FailedRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetAppUsersCacheKey, 1),
                cachingStrategy.Medium,
                1);

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfGetAppUsersRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                // Act
                var result = await sut.GetAppUsersWithCacheAsync(
                    MockedAppsRepository.SuccessfulRequest.Object,
                    memoryCache,
                    string.Format(cacheKeys.GetAppUsersCacheKey, 1),
                    cachingStrategy.Medium,
                    1,
                    null,
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Get Non App Users
        [Test, Category("Cache")]
        public async Task GetNonAppUsers()
        {
            // Arrange

            // Act
            var result = await sut.GetNonAppUsersWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetNonAppUsersCacheKey, 1),
                cachingStrategy.Medium,
                1);

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.True);
            Assert.That(((RepositoryResponse)result.Item1).DomainObjects.ConvertAll(u => (User)u), Is.InstanceOf<List<User>>());
        }

        [Test, Category("Cache")]
        public async Task IndicateIfGetNonAppUsersAreFromCache()
        {
            // Arrange
            _ = await sut.GetNonAppUsersWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetNonAppUsersCacheKey, 1),
                cachingStrategy.Medium,
                1);

            // Act
            var result = await sut.GetNonAppUsersWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetNonAppUsersCacheKey, 1),
                cachingStrategy.Medium,
                1,
                new Result());

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.True);
            Assert.That(((RepositoryResponse)result.Item1).DomainObjects.ConvertAll(u => (User)u), Is.InstanceOf<List<User>>());
            Assert.That(((Result)result.Item2).IsFromCache, Is.True);
        }

        [Test, Category("Cache")]
        public async Task ReturnFalseIfGetNonAppUsersFails()
        {
            // Arrange

            // Act
            var result = await sut.GetNonAppUsersWithCacheAsync(
                MockedAppsRepository.FailedRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetNonAppUsersCacheKey, 1),
                cachingStrategy.Medium,
                1);

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfGetNonAppUsersRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                // Act
                var result = await sut.GetNonAppUsersWithCacheAsync(
                    MockedAppsRepository.SuccessfulRequest.Object,
                    memoryCache,
                    string.Format(cacheKeys.GetNonAppUsersCacheKey, 1),
                    cachingStrategy.Medium,
                    1,
                    null,
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Get My Apps
        [Test, Category("Cache")]
        public async Task GetMyApps()
        {
            // Arrange

            // Act
            var result = await sut.GetMyAppsWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetMyAppsCacheKey, 1),
                cachingStrategy.Medium,
                1);

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.True);
            Assert.That(((RepositoryResponse)result.Item1).DomainObjects.ConvertAll(u => (App)u), Is.InstanceOf<List<App>>());
        }

        [Test, Category("Cache")]
        public async Task IndicateIfGetMyAppsAreFromCache()
        {
            // Arrange
            _ = await sut.GetMyAppsWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetMyAppsCacheKey, 1),
                cachingStrategy.Medium,
                1);

            // Act
            var result = await sut.GetMyAppsWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetMyAppsCacheKey, 1),
                cachingStrategy.Medium,
                1,
                new Result());

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.True);
            Assert.That(((RepositoryResponse)result.Item1).DomainObjects.ConvertAll(u => (App)u), Is.InstanceOf<List<App>>());
            Assert.That(((Result)result.Item2).IsFromCache, Is.True);
        }

        [Test, Category("Cache")]
        public async Task ReturnFalseIfGetMyAppsFails()
        {
            // Arrange

            // Act
            var result = await sut.GetMyAppsWithCacheAsync(
                MockedAppsRepository.FailedRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetMyAppsCacheKey, 1),
                cachingStrategy.Medium,
                1);

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfGetMyAppsRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                // Act
                var result = await sut.GetMyAppsWithCacheAsync(
                    MockedAppsRepository.SuccessfulRequest.Object,
                    memoryCache,
                    string.Format(cacheKeys.GetMyAppsCacheKey, 1),
                    cachingStrategy.Medium,
                    1,
                    null,
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Get My Registered Apps
        [Test, Category("Cache")]
        public async Task GetMyRegisteredApps()
        {
            // Arrange

            // Act
            var result = await sut.GetMyRegisteredAppsWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetMyRegisteredCacheKey, 1),
                cachingStrategy.Medium,
                1);

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.True);
            Assert.That(((RepositoryResponse)result.Item1).DomainObjects.ConvertAll(u => (App)u), Is.InstanceOf<List<App>>());
        }

        [Test, Category("Cache")]
        public async Task IndicateIfGetMyRegisteredAppsAreFromCache()
        {
            // Arrange
            _ = await sut.GetMyRegisteredAppsWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetMyRegisteredCacheKey, 1),
                cachingStrategy.Medium,
                1);

            // Act
            var result = await sut.GetMyRegisteredAppsWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetMyRegisteredCacheKey, 1),
                cachingStrategy.Medium,
                1,
                new Result());

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.True);
            Assert.That(((RepositoryResponse)result.Item1).DomainObjects.ConvertAll(u => (App)u), Is.InstanceOf<List<App>>());
            Assert.That(((Result)result.Item2).IsFromCache, Is.True);
        }

        [Test, Category("Cache")]
        public async Task ReturnFalseIfGetMyRegisteredAppsFails()
        {
            // Arrange

            // Act
            var result = await sut.GetMyRegisteredAppsWithCacheAsync(
                MockedAppsRepository.FailedRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetMyRegisteredCacheKey, 1),
                cachingStrategy.Medium,
                1);

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfGetMyRegisteredAppsRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                // Act
                var result = await sut.GetMyRegisteredAppsWithCacheAsync(
                    MockedAppsRepository.SuccessfulRequest.Object,
                    memoryCache,
                    string.Format(cacheKeys.GetMyRegisteredCacheKey, 1),
                    cachingStrategy.Medium,
                    1,
                    null,
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Get App License
        [Test, Category("Cache")]
        public async Task GetAppLicense()
        {
            // Arrange

            // Act
            var result = await sut.GetLicenseWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetAppLicenseCacheKey, 1),
                cachingStrategy.Medium,
                cacheKeys,
                1);

            // Assert
            Assert.That(result.Item1, Is.TypeOf<string>());
        }

        [Test, Category("Cache")]
        public async Task IndicateIfGetAppLicenseIsFromCache()
        {
            // Arrange
            _ = await sut.GetLicenseWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetAppLicenseCacheKey, 1),
                cachingStrategy.Medium,
                cacheKeys,
                1);

            // Act
            var result = await sut.GetLicenseWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetAppLicenseCacheKey, 1),
                cachingStrategy.Medium,
                cacheKeys,
                1,
                new Result());

            // Assert
            Assert.That(result.Item1, Is.TypeOf<string>());
            Assert.That(((Result)result.Item2).IsFromCache, Is.True);
        }

        [Test, Category("Cache")]
        public async Task ReturnFalseIfGetAppLicenseFails()
        {
            // Arrange

            // Act
            var result = await sut.GetLicenseWithCacheAsync(
                MockedAppsRepository.FailedRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetAppLicenseCacheKey, 1),
                cachingStrategy.Medium,
                cacheKeys,
                1);

            // Assert
            Assert.That(result.Item1, Is.TypeOf<string>());
            Assert.That(result.Item1, Is.EqualTo(string.Empty));
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfGetAppLicenseRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                // Act
                var result = await sut.GetLicenseWithCacheAsync(
                    MockedAppsRepository.SuccessfulRequest.Object,
                    memoryCache,
                    string.Format(cacheKeys.GetAppLicenseCacheKey, 1),
                    cachingStrategy.Medium,
                    cacheKeys,
                    1,
                    null,
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Reset App
        [Test, Category("Cache")]
        public async Task ResetApp()
        {
            // Arrange
            var app = MockedAppsRepository.SuccessfulRequest.Object.GetAsync(1).Result.DomainObject;

            // Act
            var result = await sut.ResetWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                cacheKeys,
                (App)app);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.DomainObject, Is.InstanceOf<App>());
        }

        [Test, Category("Cache")]
        public async Task ReturnFalseIfResetAppFails()
        {
            // Arrange
            var app = MockedAppsRepository.SuccessfulRequest.Object.GetAsync(1).Result.DomainObject;

            // Act
            var result = await sut.ResetWithCacheAsync(
                MockedAppsRepository.FailedRequest.Object,
                memoryCache,
                cacheKeys,
                (App)app);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfResetAppRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                var app = MockedAppsRepository.SuccessfulRequest.Object.GetAsync(1).Result.DomainObject;

                // Act
                var result = await sut.ResetWithCacheAsync(
                    MockedAppsRepository.SuccessfulRequest.Object,
                    memoryCache,
                    cacheKeys,
                    (App)app,
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Activate App
        [Test, Category("Cache")]
        public async Task ActivateApp()
        {
            // Arrange

            // Act
            var result = await sut.ActivatetWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                cacheKeys,
                1);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.DomainObject, Is.InstanceOf<App>());
        }

        [Test, Category("Cache")]
        public async Task ReturnFalseIfActivateAppFails()
        {
            // Arrange

            // Act
            var result = await sut.ActivatetWithCacheAsync(
                MockedAppsRepository.FailedRequest.Object,
                memoryCache,
                cacheKeys,
                1);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfActivateAppRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                // Act
                var result = await sut.ActivatetWithCacheAsync(
                    MockedAppsRepository.SuccessfulRequest.Object,
                    memoryCache,
                    cacheKeys,
                    1,
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Deactivate App
        [Test, Category("Cache")]
        public async Task DeactivateApp()
        {
            // Arrange

            // Act
            var result = await sut.DeactivatetWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                cacheKeys,
                1);

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.DomainObject, Is.InstanceOf<App>());
        }

        [Test, Category("Cache")]
        public async Task ReturnFalseIfDeactivateAppFails()
        {
            // Arrange

            // Act
            var result = await sut.DeactivatetWithCacheAsync(
                MockedAppsRepository.FailedRequest.Object,
                memoryCache,
                cacheKeys,
                1);

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfDeactivateAppRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                // Act
                var result = await sut.DeactivatetWithCacheAsync(
                    MockedAppsRepository.SuccessfulRequest.Object,
                    memoryCache,
                    cacheKeys,
                    1,
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region App License Valid
        [Test, Category("Cache")]
        public async Task ConfirmIfAppLicenseValid()
        {
            // Arrange

            // Act
            var result = await sut.IsAppLicenseValidWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.IsAppLicenseValidCacheKey, TestObjects.GetLicense()),
                cachingStrategy.Medium,
                TestObjects.GetLicense());

            // Assert
            Assert.That(result, Is.True);
        }

        [Test, Category("Cache")]
        public async Task ReturnFalseIfConfirmIfAppLicenseValidFails()
        {
            // Arrange

            // Act
            var result = await sut.IsAppLicenseValidWithCacheAsync(
                MockedAppsRepository.FailedRequest.Object,
                memoryCache,
                string.Format(cacheKeys.IsAppLicenseValidCacheKey, TestObjects.GetLicense()),
                cachingStrategy.Medium,
                TestObjects.GetLicense());

            // Assert
            Assert.That(result, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfConfirmIfAppLicenseRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                // Act
                var result = await sut.IsAppLicenseValidWithCacheAsync(
                    MockedAppsRepository.SuccessfulRequest.Object,
                    memoryCache,
                    string.Format(cacheKeys.IsAppLicenseValidCacheKey, TestObjects.GetLicense()),
                    cachingStrategy.Medium,
                    TestObjects.GetLicense(),
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Get User by User Name
        [Test, Category("Cache")]
        public async Task GetUserByUserName()
        {
            // Arrange
            var userName = "TestSuperUser";

            // Act
            var result = await sut.GetByUserNameWithCacheAsync(
                MockedUsersRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetUserByUsernameCacheKey, userName, TestObjects.GetLicense()),
                cachingStrategy.Medium,
                cacheKeys,
                userName,
                TestObjects.GetLicense());

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.True);
            Assert.That(((RepositoryResponse)result.Item1).DomainObject, Is.InstanceOf<User>());
        }

        [Test, Category("Cache")]
        public async Task IndicateIfGerUserByUserNameIsFromCache()
        {
            // Arrange
            var userName = "TestSuperUser";

            _ = await sut.GetByUserNameWithCacheAsync(
                MockedUsersRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetUserByUsernameCacheKey, userName, TestObjects.GetLicense()),
                cachingStrategy.Medium,
                cacheKeys,
                userName,
                TestObjects.GetLicense());

            // Act
            var result = await sut.GetByUserNameWithCacheAsync(
                MockedUsersRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetUserByUsernameCacheKey, userName, TestObjects.GetLicense()),
                cachingStrategy.Medium,
                cacheKeys,
                userName,
                TestObjects.GetLicense(),
                new Result());

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.True);
            Assert.That(((RepositoryResponse)result.Item1).DomainObject, Is.InstanceOf<User>());
            Assert.That(((Result)result.Item2).IsFromCache, Is.True);
        }

        [Test, Category("Cache")]
        public async Task ReturnFalseIfGetUserByUserNameFails()
        {
            // Arrange
            var userName = "TestSuperUser";

            // Act
            var result = await sut.GetByUserNameWithCacheAsync(
                MockedUsersRepository.FailedRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetUserByUsernameCacheKey, userName, TestObjects.GetLicense()),
                cachingStrategy.Medium,
                cacheKeys,
                userName,
                TestObjects.GetLicense());

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfGetUserByUserNameRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                var userName = "TestSuperUser";

                // Act
                var result = await sut.GetByUserNameWithCacheAsync(
                    MockedUsersRepository.SuccessfulRequest.Object,
                    memoryCache,
                    string.Format(cacheKeys.GetUserByUsernameCacheKey, userName, TestObjects.GetLicense()),
                    cachingStrategy.Medium,
                    cacheKeys,
                    userName,
                    TestObjects.GetLicense(),
                    null,
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Get User by Email
        [Test, Category("Cache")]
        public async Task GetUserByEmail()
        {
            // Arrange
            var email = "TestSuperUser@example.com";

            // Act
            var result = await sut.GetByEmailWithCacheAsync(
                MockedUsersRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetUserByUsernameCacheKey, email, TestObjects.GetLicense()),
                cachingStrategy.Medium,
                email);

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.True);
            Assert.That(((RepositoryResponse)result.Item1).DomainObject, Is.InstanceOf<User>());
        }

        [Test, Category("Cache")]
        public async Task IndicateIfGerUserByEmailIsFromCache()
        {
            // Arrange
            var email = "TestSuperUser@example.com";

            _ = await sut.GetByEmailWithCacheAsync(
                MockedUsersRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetUserByUsernameCacheKey, email, TestObjects.GetLicense()),
                cachingStrategy.Medium,
                email);

            // Act
            var result = await sut.GetByEmailWithCacheAsync(
                MockedUsersRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetUserByUsernameCacheKey, email, TestObjects.GetLicense()),
                cachingStrategy.Medium,
                email,
                new Result());

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.True);
            Assert.That(((RepositoryResponse)result.Item1).DomainObject, Is.InstanceOf<User>());
            Assert.That(((Result)result.Item2).IsFromCache, Is.True);
        }

        [Test, Category("Cache")]
        public async Task ReturnFalseIfGetUserByEmailFails()
        {
            // Arrange
            var email = "TestSuperUser@example.com";

            // Act
            var result = await sut.GetByEmailWithCacheAsync(
                MockedUsersRepository.FailedRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetUserByUsernameCacheKey, email, TestObjects.GetLicense()),
                cachingStrategy.Medium,
                email);

            // Assert
            Assert.That(((RepositoryResponse)result.Item1).IsSuccess, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfGetUserByEmailRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                var email = "TestSuperUser@example.com";

                // Act
                var result = await sut.GetByEmailWithCacheAsync(
                    MockedUsersRepository.SuccessfulRequest.Object,
                    memoryCache,
                    string.Format(cacheKeys.GetUserByUsernameCacheKey, email, TestObjects.GetLicense()),
                    cachingStrategy.Medium,
                    email,
                    null,
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Note When User Email is Confirmed
        [Test, Category("Cache")]
        public async Task NoteWhenUsersEmailIsConfirmed()
        {
            // Arrange
            var emailConfirmation = context.EmailConfirmations.FirstOrDefault(ec => ec.Id == 1);

            // Act
            var result = await sut.ConfirmEmailWithCacheAsync(
                MockedUsersRepository.SuccessfulRequest.Object,
                memoryCache,
                cacheKeys,
                emailConfirmation,
                TestObjects.GetLicense());

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That((User)(result.DomainObject), Is.InstanceOf<User>());
        }

        [Test, Category("Cache")]
        public async Task ReturnFalseIfNoteWhenUsersEmailIsConfirmedFails()
        {
            // Arrange
            var emailConfirmation = context.EmailConfirmations.FirstOrDefault(ec => ec.Id == 1);

            // Act
            var result = await sut.ConfirmEmailWithCacheAsync(
                MockedUsersRepository.FailedRequest.Object,
                memoryCache,
                cacheKeys,
                emailConfirmation,
                TestObjects.GetLicense());

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfNoteWhenUsersEmailIsConfirmedRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                var emailConfirmation = context.EmailConfirmations.FirstOrDefault(ec => ec.Id == 1);

                // Act
                var result = await sut.ConfirmEmailWithCacheAsync(
                    MockedUsersRepository.SuccessfulRequest.Object,
                    memoryCache,
                    cacheKeys,
                    emailConfirmation,
                    TestObjects.GetLicense(),
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Note When User Email is Confirmed
        [Test, Category("Cache")]
        public async Task NoteWhenUsersEmailIsUpdated()
        {
            // Arrange
            var emailConfirmation = context.EmailConfirmations.FirstOrDefault(ec => ec.Id == 1);

            // Act
            var result = await sut.UpdateEmailWithCacheAsync(
                MockedUsersRepository.SuccessfulRequest.Object,
                memoryCache,
                cacheKeys,
                emailConfirmation,
                TestObjects.GetLicense());

            // Assert
            Assert.That(result.IsSuccess, Is.True);
            Assert.That((User)(result.DomainObject), Is.InstanceOf<User>());
        }

        [Test, Category("Cache")]
        public async Task ReturnFalseIfNoteWhenUsersEmailIsUpdatedFails()
        {
            // Arrange
            var emailConfirmation = context.EmailConfirmations.FirstOrDefault(ec => ec.Id == 1);

            // Act
            var result = await sut.UpdateEmailWithCacheAsync(
                MockedUsersRepository.FailedRequest.Object,
                memoryCache,
                cacheKeys,
                emailConfirmation,
                TestObjects.GetLicense());

            // Assert
            Assert.That(result.IsSuccess, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfNoteWhenUsersEmailIsUpdatedRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                var emailConfirmation = context.EmailConfirmations.FirstOrDefault(ec => ec.Id == 1);

                // Act
                var result = await sut.UpdateEmailWithCacheAsync(
                    MockedUsersRepository.SuccessfulRequest.Object,
                    memoryCache,
                    cacheKeys,
                    emailConfirmation,
                    TestObjects.GetLicense(),
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Confirm User is Registered
        [Test, Category("Cache")]
        public async Task ConfirmUserIsRegistered()
        {
            // Arrange

            // Act
            var result = await sut.IsUserRegisteredWithCacheAsync(
                MockedUsersRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetUserCacheKey, 1, TestObjects.GetLicense()),
                cachingStrategy.Medium,
                1);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test, Category("Cache")]
        public async Task ReturnsFalseIfConfirmUserIsRegisteredFails()
        {
            // Arrange

            // Act
            var result = await sut.IsUserRegisteredWithCacheAsync(
                MockedUsersRepository.FailedRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetUserCacheKey, 1, TestObjects.GetLicense()),
                cachingStrategy.Medium,
                1);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfConfirmUserIsRegisteredRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                // Act
                var result = await sut.IsUserRegisteredWithCacheAsync(
                    MockedUsersRepository.SuccessfulRequest.Object,
                    memoryCache,
                    string.Format(cacheKeys.GetUserCacheKey, 1, TestObjects.GetLicense()),
                    cachingStrategy.Medium,
                    1,
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Confirm is Difficulty Level Implemented
        [Test, Category("Cache")]
        public async Task ConfirmIsDifficultyLevelImplemented()
        {
            // Arrange
            var difficulty = context.Difficulties.FirstOrDefault(d => d.DifficultyLevel == DifficultyLevel.TEST);

            // Act
            var result = await sut.HasDifficultyLevelWithCacheAsync(
                MockedDifficultiesRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetDifficultyCacheKey, difficulty.Id),
                cachingStrategy.Heavy,
                difficulty.DifficultyLevel);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test, Category("Cache")]
        public async Task ReturnFalseIfConfirmIsDifficultyLevelImplementedFails()
        {
            // Arrange
            var difficulty = context.Difficulties.FirstOrDefault(d => d.DifficultyLevel == DifficultyLevel.TEST);

            // Act
            var result = await sut.HasDifficultyLevelWithCacheAsync(
                MockedDifficultiesRepository.FailedRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetDifficultyCacheKey, difficulty.Id),
                cachingStrategy.Heavy,
                difficulty.DifficultyLevel);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfConfirmIsDifficultyLevelImplementedRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                var difficulty = context.Difficulties.FirstOrDefault(d => d.DifficultyLevel == DifficultyLevel.TEST);

                // Act
                var result = await sut.HasDifficultyLevelWithCacheAsync(
                    MockedDifficultiesRepository.SuccessfulRequest.Object,
                    memoryCache,
                    string.Format(cacheKeys.GetDifficultyCacheKey, difficulty.Id),
                    cachingStrategy.Heavy,
                    difficulty.DifficultyLevel,
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region  Confirm is Role Level Implemented
        [Test, Category("Cache")]
        public async Task ConfirmIsRoleLevelImplemented()
        {
            // Arrange
            var role = context.Roles.FirstOrDefault(r => r.RoleLevel == RoleLevel.NULL);

            // Act
            var result = await sut.HasRoleLevelWithCacheAsync(
                MockedRolesRepository.SuccessfulRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetRoleCacheKey, role.Id),
                cachingStrategy.Heavy,
                role.RoleLevel);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test, Category("Cache")]
        public async Task ReturnFalseIfConfirmIsRoleLevelImplementedFails()
        {
            // Arrange
            var role = context.Roles.FirstOrDefault(r => r.RoleLevel == RoleLevel.NULL);

            // Act
            var result = await sut.HasRoleLevelWithCacheAsync(
                MockedRolesRepository.FailedRequest.Object,
                memoryCache,
                string.Format(cacheKeys.GetRoleCacheKey, role.Id),
                cachingStrategy.Heavy,
                role.RoleLevel);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfConfirmIsRoleLevelImplementedRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                var role = context.Roles.FirstOrDefault(r => r.RoleLevel == RoleLevel.NULL);

                // Act
                var result = await sut.HasRoleLevelWithCacheAsync(
                    MockedRolesRepository.SuccessfulRequest.Object,
                    memoryCache,
                    string.Format(cacheKeys.GetRoleCacheKey, role.Id),
                    cachingStrategy.Heavy,
                    role.RoleLevel,
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Get ALl Gallery Apps
        [Test, Category("Cache")]
        public async Task GetAllGalleryApps()
        {
            // Arrange

            // Act
            var cacheResult = await sut.GetGalleryAppsWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                cacheKeys.GetGalleryAppsKey,
                cachingStrategy.Medium);
            var repositoryResponse = (RepositoryResponse)cacheResult.Item1;
            var apps = ((RepositoryResponse)cacheResult.Item1)
                .DomainObjects
                .ConvertAll(a => (GalleryApp)a);

            // Assert
            Assert.That(repositoryResponse.IsSuccess, Is.True);
            Assert.That(apps, Is.InstanceOf<List<GalleryApp>>());
        }

        [Test, Category("Cache")]
        public async Task IndicateIfGalleryAppsAreFromCache()
        {
            // Arrange
            _ = await sut.GetGalleryAppsWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                cacheKeys.GetGalleryAppsKey,
                cachingStrategy.Medium);

            // Act
            var cacheResult = await sut.GetGalleryAppsWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                cacheKeys.GetGalleryAppsKey,
                cachingStrategy.Medium,
                new Result());
            var repositoryResponse = (RepositoryResponse)cacheResult.Item1;
            var result = (Result)cacheResult.Item2;
            var galleryApps = ((RepositoryResponse)cacheResult.Item1)
                .DomainObjects
                .ConvertAll(a => (GalleryApp)a);

            // Assert
            Assert.That(repositoryResponse.IsSuccess, Is.True);
            Assert.That(result.IsFromCache, Is.True);
            Assert.That(galleryApps, Is.InstanceOf<List<GalleryApp>>());
        }

        [Test, Category("Cache")]
        public async Task ReturnFalseIfGetGalleryAppsFails()
        {
            // Arrange

            // Act
            var cacheResult = await sut.GetGalleryAppsWithCacheAsync(
                MockedAppsRepository.FailedRequest.Object,
                memoryCache,
                cacheKeys.GetGalleryAppsKey,
                cachingStrategy.Medium,
                new Result());
            var repositoryResponse = (RepositoryResponse)cacheResult.Item1;

            // Assert
            Assert.That(repositoryResponse.IsSuccess, Is.False);
        }

        [Test, Category("Cache")]
        public async Task ThrowsIOExceptionIfGetAllGalleryAppsRedisConnectionFails()
        {
            try
            {
                // Arrange
                var configVars = new List<TestVar>
                {
                    new() {
                        Name = "Url",
                        Value = "redis://:password@127.0.0.1:6379"
                    },
                    new() {
                        Name = "TLS_URL",
                        Value = "rediss://:password@127.0.0.1:6379"
                    }
                };

                using StringContent body = new(
                    JsonSerializer.Serialize<List<TestVar>>(configVars),
                    Encoding.UTF8,
                    "application/json");

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                    })
                    .Verifiable();

                MockedHttpMessageHandler.Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = body
                    })
                    .Verifiable();

                // Act
                var cacheResult = await sut.GetGalleryAppsWithCacheAsync(
                    MockedAppsRepository.SuccessfulRequest.Object,
                    memoryCache,
                    cacheKeys.GetGalleryAppsKey,
                    cachingStrategy.Medium,
                    null,
                    MockedHttpMessageHandler.Object);
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf<IOException>());
                Assert.That(ex.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
            }
        }
        #endregion

        #region Updated Values and Admin Summary
        [Test, Category("Cache")]
        public async Task GetValuesAndCacheTheResult()
        {
            var result = await sut.GetValuesAsync(
                MockedDifficultiesRepository.SuccessfulRequest.Object,
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                $"values-{Guid.NewGuid()}",
                cachingStrategy.Medium,
                [],
                [],
                []);

            Assert.That(result.Item1, Is.Not.Null);
            Assert.That(result.Item1.Difficulties, Is.Not.Empty);
            Assert.That(result.Item1.Gallery, Is.Not.Empty);
        }

        [Test, Category("Cache")]
        public async Task GetValuesFromCache()
        {
            var cacheKey = $"values-{Guid.NewGuid()}";
            _ = await sut.GetValuesAsync(
                MockedDifficultiesRepository.SuccessfulRequest.Object,
                MockedAppsRepository.SuccessfulRequest.Object,
                memoryCache,
                cacheKey,
                cachingStrategy.Medium,
                [],
                [],
                []);

            var result = await sut.GetValuesAsync(
                MockedDifficultiesRepository.FailedRequest.Object,
                MockedAppsRepository.FailedRequest.Object,
                memoryCache,
                cacheKey,
                cachingStrategy.Medium,
                [],
                [],
                [],
                new Result());

            Assert.That(result.Item1, Is.Not.Null);
            Assert.That(((Result)result.Item2).IsFromCache, Is.True);
        }

        [Test, Category("Cache")]
        public async Task GetSystemStatsAndValuesAndCacheTheResult()
        {
            var appsRepository = new Mock<IAppsRepository<App>>();
            var difficultiesRepository = new Mock<IDifficultiesRepository<Difficulty>>();
            var emailConfirmationsRepository = new Mock<IEmailConfirmationsRepository<EmailConfirmation>>();
            var gamesRepository = new Mock<IGamesRepository<Game>>();
            var passwordResetsRepository = new Mock<IPasswordResetsRepository<PasswordReset>>();
            var rolesRepository = new Mock<IRolesRepository<Role>>();
            var solutionsRepository = new Mock<ISolutionsRepository<SudokuSolution>>();
            var usersRepository = new Mock<IUsersRepository<User>>();

            appsRepository.Setup(x => x.GetCount()).Returns(new RepositoryResponse { IsSuccess = true, Object = new { TotalCount = 1 } });
            difficultiesRepository.Setup(x => x.GetCount()).Returns(new RepositoryResponse { IsSuccess = true, Object = new { TotalCount = 2 } });
            emailConfirmationsRepository.Setup(x => x.GetCount()).Returns(new RepositoryResponse { IsSuccess = true, Object = new { TotalCount = 3 } });
            gamesRepository.Setup(x => x.GetCount()).Returns(new RepositoryResponse { IsSuccess = true, Object = new { TotalCount = 4 } });
            passwordResetsRepository.Setup(x => x.GetCount()).Returns(new RepositoryResponse { IsSuccess = true, Object = new { TotalCount = 5 } });
            rolesRepository.Setup(x => x.GetCount()).Returns(new RepositoryResponse { IsSuccess = true, Object = new { TotalCount = 6 } });
            solutionsRepository.Setup(x => x.GetCount()).Returns(new RepositoryResponse { IsSuccess = true, Object = new { TotalCount = 7 } });
            usersRepository.Setup(x => x.GetCount()).Returns(new RepositoryResponse { IsSuccess = true, Object = new { TotalCount = 8 } });
            difficultiesRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(new RepositoryResponse { IsSuccess = true });
            rolesRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(new RepositoryResponse { IsSuccess = true });

            var result = await sut.GetSystemStatsAndValues(
                appsRepository.Object,
                difficultiesRepository.Object,
                emailConfirmationsRepository.Object,
                gamesRepository.Object,
                passwordResetsRepository.Object,
                rolesRepository.Object,
                solutionsRepository.Object,
                usersRepository.Object,
                memoryCache,
                $"summary-{Guid.NewGuid()}",
                cachingStrategy.Medium);

            Assert.That(result.Item1.IsSuccess, Is.True);
            Assert.That(result.Item1.Object, Is.Not.Null);
        }

        [Test, Category("Cache")]
        public async Task RemoveOnlyExistingKeys()
        {
            var cacheKey = $"remove-{Guid.NewGuid()}";
            await memoryCache.SetAsync(cacheKey, [1]);

            await sut.RemoveKeysAsync(memoryCache, [cacheKey, $"missing-{Guid.NewGuid()}"]);

            Assert.That(await memoryCache.GetAsync(cacheKey), Is.Null);
        }

        [Test, Category("Cache")]
        public async Task HandleRedisFailuresAcrossAllCacheOperations()
        {
            var app = context.Apps.First();
            var user = context.Users.First();
            var difficulty = context.Difficulties.First();
            var role = context.Roles.First();
            var emailConfirmation = context.EmailConfirmations.First();

            MockedHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new IOException("handler failure"));

            async Task AssertRedisFailure(Func<Task> operation)
            {
                Assert.That(
                    async () => await operation(),
                    Throws.Exception);
            }

            await AssertRedisFailure(() => sut.AddWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                MockedCache.Object,
                cacheKeys.GetAppCacheKey,
                cachingStrategy.Medium,
                cacheKeys,
                app,
                MockedHttpMessageHandler.Object));
            await AssertRedisFailure(() => sut.GetWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                MockedCache.Object,
                cacheKeys.GetAppCacheKey,
                cachingStrategy.Medium,
                app.Id,
                null,
                MockedHttpMessageHandler.Object));
            await AssertRedisFailure(() => sut.GetAllWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                MockedCache.Object,
                cacheKeys.GetAppsCacheKey,
                cachingStrategy.Medium,
                null,
                MockedHttpMessageHandler.Object));
            await AssertRedisFailure(() => sut.UpdateWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                MockedCache.Object,
                cacheKeys,
                app,
                null,
                MockedHttpMessageHandler.Object));
            await AssertRedisFailure(() => sut.DeleteWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                MockedCache.Object,
                cacheKeys,
                app,
                null,
                MockedHttpMessageHandler.Object));
            await AssertRedisFailure(() => sut.HasEntityWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                MockedCache.Object,
                cacheKeys.HasAppCacheKey,
                cachingStrategy.Medium,
                app.Id,
                MockedHttpMessageHandler.Object));
            await AssertRedisFailure(() => sut.GetAppByLicenseWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                MockedCache.Object,
                cacheKeys.GetAppByLicenseCacheKey,
                cachingStrategy.Medium,
                app.License,
                null,
                MockedHttpMessageHandler.Object));
            await AssertRedisFailure(() => sut.GetAppUsersWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                MockedCache.Object,
                cacheKeys.GetAppUsersCacheKey,
                cachingStrategy.Medium,
                app.Id,
                null,
                MockedHttpMessageHandler.Object));
            await AssertRedisFailure(() => sut.GetNonAppUsersWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                MockedCache.Object,
                cacheKeys.GetNonAppUsersCacheKey,
                cachingStrategy.Medium,
                app.Id,
                null,
                MockedHttpMessageHandler.Object));
            await AssertRedisFailure(() => sut.GetMyAppsWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                MockedCache.Object,
                cacheKeys.GetMyAppsCacheKey,
                cachingStrategy.Medium,
                app.OwnerId,
                null,
                MockedHttpMessageHandler.Object));
            await AssertRedisFailure(() => sut.GetMyRegisteredAppsWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                MockedCache.Object,
                cacheKeys.GetMyRegisteredCacheKey,
                cachingStrategy.Medium,
                user.Id,
                null,
                MockedHttpMessageHandler.Object));
            await AssertRedisFailure(() => sut.GetLicenseWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                MockedCache.Object,
                cacheKeys.GetAppLicenseCacheKey,
                cachingStrategy.Medium,
                cacheKeys,
                app.Id,
                null,
                MockedHttpMessageHandler.Object));
            await AssertRedisFailure(() => sut.ResetWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                MockedCache.Object,
                cacheKeys,
                app,
                MockedHttpMessageHandler.Object));
            await AssertRedisFailure(() => sut.ActivatetWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                MockedCache.Object,
                cacheKeys,
                app.Id,
                MockedHttpMessageHandler.Object));
            await AssertRedisFailure(() => sut.DeactivatetWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                MockedCache.Object,
                cacheKeys,
                app.Id,
                MockedHttpMessageHandler.Object));
            await AssertRedisFailure(() => sut.IsAppLicenseValidWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                MockedCache.Object,
                cacheKeys.IsAppLicenseValidCacheKey,
                cachingStrategy.Medium,
                app.License,
                MockedHttpMessageHandler.Object));
            await AssertRedisFailure(() => sut.GetByUserNameWithCacheAsync(
                MockedUsersRepository.SuccessfulRequest.Object,
                MockedCache.Object,
                cacheKeys.GetUserByUsernameCacheKey,
                cachingStrategy.Medium,
                cacheKeys,
                user.UserName,
                null,
                null,
                MockedHttpMessageHandler.Object));
            await AssertRedisFailure(() => sut.GetByEmailWithCacheAsync(
                MockedUsersRepository.SuccessfulRequest.Object,
                MockedCache.Object,
                cacheKeys.GetUserByEmailCacheKey,
                cachingStrategy.Medium,
                user.Email,
                null,
                MockedHttpMessageHandler.Object));
            await AssertRedisFailure(() => sut.ConfirmEmailWithCacheAsync(
                MockedUsersRepository.SuccessfulRequest.Object,
                MockedCache.Object,
                cacheKeys,
                emailConfirmation,
                TestObjects.GetLicense(),
                MockedHttpMessageHandler.Object));
            await AssertRedisFailure(() => sut.UpdateEmailWithCacheAsync(
                MockedUsersRepository.SuccessfulRequest.Object,
                MockedCache.Object,
                cacheKeys,
                emailConfirmation,
                TestObjects.GetLicense(),
                MockedHttpMessageHandler.Object));
            await AssertRedisFailure(() => sut.IsUserRegisteredWithCacheAsync(
                MockedUsersRepository.SuccessfulRequest.Object,
                MockedCache.Object,
                cacheKeys.IsUserRegisteredCacheKey,
                cachingStrategy.Medium,
                user.Id,
                MockedHttpMessageHandler.Object));
            await AssertRedisFailure(() => sut.HasDifficultyLevelWithCacheAsync(
                MockedDifficultiesRepository.SuccessfulRequest.Object,
                MockedCache.Object,
                cacheKeys.GetDifficultyCacheKey,
                cachingStrategy.Medium,
                difficulty.DifficultyLevel,
                MockedHttpMessageHandler.Object));
            await AssertRedisFailure(() => sut.HasRoleLevelWithCacheAsync(
                MockedRolesRepository.SuccessfulRequest.Object,
                MockedCache.Object,
                cacheKeys.GetRoleCacheKey,
                cachingStrategy.Medium,
                role.RoleLevel,
                MockedHttpMessageHandler.Object));
            await AssertRedisFailure(() => sut.GetValuesAsync(
                MockedDifficultiesRepository.SuccessfulRequest.Object,
                MockedAppsRepository.SuccessfulRequest.Object,
                MockedCache.Object,
                cacheKeys.GetValuesKey,
                cachingStrategy.Medium,
                [],
                [],
                [],
                null,
                MockedHttpMessageHandler.Object));
            await AssertRedisFailure(() => sut.GetGalleryAppsWithCacheAsync(
                MockedAppsRepository.SuccessfulRequest.Object,
                MockedCache.Object,
                cacheKeys.GetGalleryAppsKey,
                cachingStrategy.Medium,
                null,
                MockedHttpMessageHandler.Object));
            await AssertRedisFailure(() => sut.GetSystemStatsAndValues(
                new Mock<IAppsRepository<App>>().Object,
                new Mock<IDifficultiesRepository<Difficulty>>().Object,
                new Mock<IEmailConfirmationsRepository<EmailConfirmation>>().Object,
                new Mock<IGamesRepository<Game>>().Object,
                new Mock<IPasswordResetsRepository<PasswordReset>>().Object,
                new Mock<IRolesRepository<Role>>().Object,
                new Mock<ISolutionsRepository<SudokuSolution>>().Object,
                new Mock<IUsersRepository<User>>().Object,
                MockedCache.Object,
                cacheKeys.GetAdminSummaryKey,
                cachingStrategy.Medium,
                null,
                MockedHttpMessageHandler.Object));
        }

        [Test, Category("Cache")]
        public async Task FlushCacheAndReturnTrue()
        {
            MockedServer
                .Setup(server => server.KeysAsync(
                    It.IsAny<int>(),
                    It.Is<RedisValue>(value => value.ToString() == "SudokuCollective*"),
                    It.IsAny<int>(),
                    It.IsAny<long>(),
                    It.IsAny<int>(),
                    It.IsAny<CommandFlags>()))
                .Returns(GetRedisKeysAsync("SudokuCollective:key-1", "SudokuCollective:key-2"));

            var result = await sut.FlushCacheAsync(
                TestObjects.GetRequest(),
                MockedUsersService.Object);

            Assert.That(result, Is.True);
            MockedRedisDatabase.Verify(
                database => database.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()),
                Times.Exactly(2));
        }

        [Test, Category("Cache")]
        public async Task FlushCacheSkipDisconnectedServer()
        {
            MockedServer.SetupGet(server => server.IsConnected).Returns(false);

            var result = await sut.FlushCacheAsync(
                TestObjects.GetRequest(),
                MockedUsersService.Object,
                "SudokuCollective*");

            Assert.That(result, Is.True);
            MockedRedisDatabase.Verify(
                database => database.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()),
                Times.Never);
        }

        [Test, Category("Cache")]
        public void ThrowUnauthorizedAccessWhenRequestorIsNotSuperUser()
        {
            MockedUsersService
                .Setup(service => service.GetAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<SudokuCollective.Core.Interfaces.Models.DomainObjects.Params.IRequest>()))
                .ReturnsAsync(new Result
                {
                    IsSuccess = true,
                    Payload = [CreateUser(false)]
                });

            Assert.That(
                async () => await sut.FlushCacheAsync(
                    TestObjects.GetRequest(),
                    MockedUsersService.Object),
                Throws.TypeOf<UnauthorizedAccessException>());
        }

        private static async IAsyncEnumerable<RedisKey> GetRedisKeysAsync(params RedisKey[] keys)
        {
            foreach (var key in keys)
            {
                yield return key;
                await Task.Yield();
            }
        }

        private static UserDTO CreateUser(bool isSuperUser)
        {
            return new UserDTO
            {
                IsSuperUser = isSuperUser
            };
        }
        #endregion
    }

    internal class TestVar
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("value")]
        public string Value { get; set; }

        public TestVar()
        {
            Name = string.Empty;
            Value = string.Empty;
        }

        [JsonConstructor]
        public TestVar(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
