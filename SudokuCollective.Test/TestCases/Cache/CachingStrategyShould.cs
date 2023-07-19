using System;
using NUnit.Framework;
using SudokuCollective.Cache;
using SudokuCollective.Core.Interfaces.Cache;

namespace SudokuCollective.Test.TestCases.Cache
{
    public class CachingStrategyShould
    {
        private ICachingStrategy sut;

        [SetUp]
        public void Setup()
        {
            sut = new CachingStrategy();
        }

        [Test, Category("Cache")]
        public void Return15MinutesForLightStrategy()
        {
            // Arrange

            // Act
            var now = DateTime.Now;
            var lightStrategy = sut.Light;

            // Assert
            Assert.That(lightStrategy, Is.GreaterThan(now));
            Assert.That(lightStrategy, Is.GreaterThan(now.AddMinutes(15)));
            Assert.That(lightStrategy, Is.LessThan(now.AddMinutes(16)));
        }

        [Test, Category("Cache")]
        public void Return1HourForMediumStrategy()
        {
            // Arrange

            // Act
            var now = DateTime.Now;
            var mediumStrategy = sut.Medium;

            // Assert
            Assert.That(mediumStrategy, Is.GreaterThan(now));
            Assert.That(mediumStrategy, Is.GreaterThan(now.AddHours(1)));
            Assert.That(mediumStrategy, Is.LessThan(now.AddHours(1.1)));
        }

        [Test, Category("Cache")]
        public void Return1DayForHeavyStrategy()
        {
            // Arrange

            // Act
            var now = DateTime.Now;
            var heavyStrategy = sut.Heavy;

            // Assert
            Assert.That(heavyStrategy, Is.GreaterThan(now));
            Assert.That(heavyStrategy, Is.GreaterThan(now.AddDays(1)));
            Assert.That(heavyStrategy, Is.LessThan(now.AddDays(1.1)));
        }
    }
}
