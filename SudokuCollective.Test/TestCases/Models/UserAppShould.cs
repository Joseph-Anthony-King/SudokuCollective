using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Models;

namespace SudokuCollective.Test.TestCases.Models
{
    public class UserAppShould
    {
        private IUserApp sut;

        [SetUp]
        public void Setup()
        {
            sut = new UserApp();
        }

        [Test, Category("Models")]
        public void ImplementIDomainEntity()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut, Is.InstanceOf<IDomainEntity>());
        }

        [Test, Category("Models")]
        public void HaveAnID()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.Id, Is.TypeOf<int>());
            Assert.That(sut.Id, Is.EqualTo(0));
        }

        [Test, Category("Models")]
        public void UserIdAndAppIdAreInts()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.UserId, Is.TypeOf<int>());
            Assert.That(sut.AppId, Is.TypeOf<int>());
        }

        [Test, Category("Models")]
        public void AcceptsReferenceToUserObjects()
        {
            // Arrange
            var user = new User();

            // Act
            sut.User = user;

            // Assert
            Assert.That(user, Is.InstanceOf<User>());
            Assert.That(((UserApp)sut).User, Is.InstanceOf<User>());
        }

        [Test, Category("Models")]
        public void AcceptsReferenceToAppObjects()
        {
            // Arrange
            var app = new App();

            // Act
            sut.App = app;

            // Assert
            Assert.That(app, Is.InstanceOf<App>());
            Assert.That(((UserApp)sut).App, Is.InstanceOf<App>());
        }

        [Test, Category("Models")]
        public void AcceptsUserIdAndAppIdInConstructor()
        {
            // Arrange
            var userId = 2;
            var appId = 3;

            // Act
            sut = new UserApp(userId, appId);

            // Assert
            Assert.That(sut, Is.InstanceOf<UserApp>());
            Assert.That(sut.UserId, Is.EqualTo(userId));
            Assert.That(sut.AppId, Is.EqualTo(appId));
        }

        [Test, Category("Models")]
        public void HasAJsonConstructor()
        {
            // Arrange
            var id = 1;
            var userId = 2;
            var appId = 3;

            // Act
            sut = new UserApp(id, userId, appId);

            // Assert
            Assert.That(sut, Is.InstanceOf<UserApp>());
            Assert.That(sut.Id, Is.EqualTo(id));
            Assert.That(sut.UserId, Is.EqualTo(userId));
            Assert.That(sut.AppId, Is.EqualTo(appId));
        }
    }
}
