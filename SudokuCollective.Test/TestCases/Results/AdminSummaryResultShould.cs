using System.Collections.Generic;
using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Results;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Values;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Data.Models.Results;

namespace SudokuCollective.Test.TestCases.Results
{
    public class AdminSummaryResultShould
    {
        private IAdminSummaryResult sut;

        [SetUp]
        public void Setup()
        {
            sut = new AdminSummaryResult();
        }

        [Test, Category("Results")]
        public void HaveDefaultValues()
        {
            Assert.That(sut.ApplicationsCount, Is.Zero);
            Assert.That(sut.DifficultiesCount, Is.Zero);
            Assert.That(sut.EmailConfirmationsCount, Is.Zero);
            Assert.That(sut.GamesCount, Is.Zero);
            Assert.That(sut.PasswordResetsCount, Is.Zero);
            Assert.That(sut.RolesCount, Is.Zero);
            Assert.That(sut.SolutionsCount, Is.Zero);
            Assert.That(sut.UsersCount, Is.Zero);
            Assert.That(sut.Difficulties, Is.Empty);
            Assert.That(sut.Roles, Is.Empty);
            Assert.That(sut.ReleaseEnvironments, Is.Empty);
            Assert.That(sut.SortValues, Is.Empty);
            Assert.That(sut.TimeFrames, Is.Empty);
        }

        [Test, Category("Results")]
        public void SetRequiredProperties()
        {
            var difficulties = new List<IDifficulty>();
            var roles = new List<IRole>();
            var releaseEnvironments = new List<IEnumListItem>();
            var sortValues = new List<IEnumListItem>();
            var timeFrames = new List<IEnumListItem>();

            sut.ApplicationsCount = 1;
            sut.DifficultiesCount = 2;
            sut.EmailConfirmationsCount = 3;
            sut.GamesCount = 4;
            sut.PasswordResetsCount = 5;
            sut.RolesCount = 6;
            sut.SolutionsCount = 7;
            sut.UsersCount = 8;
            sut.Difficulties = difficulties;
            sut.Roles = roles;
            sut.ReleaseEnvironments = releaseEnvironments;
            sut.SortValues = sortValues;
            sut.TimeFrames = timeFrames;

            Assert.That(sut.ApplicationsCount, Is.EqualTo(1));
            Assert.That(sut.DifficultiesCount, Is.EqualTo(2));
            Assert.That(sut.EmailConfirmationsCount, Is.EqualTo(3));
            Assert.That(sut.GamesCount, Is.EqualTo(4));
            Assert.That(sut.PasswordResetsCount, Is.EqualTo(5));
            Assert.That(sut.RolesCount, Is.EqualTo(6));
            Assert.That(sut.SolutionsCount, Is.EqualTo(7));
            Assert.That(sut.UsersCount, Is.EqualTo(8));
            Assert.That(sut.Difficulties, Is.SameAs(difficulties));
            Assert.That(sut.Roles, Is.SameAs(roles));
            Assert.That(sut.ReleaseEnvironments, Is.SameAs(releaseEnvironments));
            Assert.That(sut.SortValues, Is.SameAs(sortValues));
            Assert.That(sut.TimeFrames, Is.SameAs(timeFrames));
        }
    }
}
