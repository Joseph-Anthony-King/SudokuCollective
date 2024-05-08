using System;
using StackExchange.Redis;
using NUnit.Framework;
using SudokuCollective.HerokuIntegration;

namespace SudokuCollective.Test.TestCases.HerokuIntegration
{
    public class HerokuConfigurationShould
    {
        [Test, Category("HerokuIntegration")]
        public void ReturnAPostgresConnectionString()
        {
            // Arrange and Act
            Environment.SetEnvironmentVariable("DATABASE_URL", "postgres://user-id:user-password@host:5432/test-database");
            var result = HerokuConfiguration.ConfigureHerokuPostgresConnection();

            // Assert
            Assert.That(result, Is.TypeOf<string>());
        }

        [Test, Category("HerokuIntegration")]
        public void ReturnARedisConfigurationObject()
        {
            // Arrange and Act
            Environment.SetEnvironmentVariable("REDIS:URL", "rediss://:user-pasword@host:15790");
            var result = HerokuConfiguration.ConfigureHerokuRedisConnection();

            // Assert
            Assert.That(result, Is.TypeOf<ConfigurationOptions>());
        }
    }
}
