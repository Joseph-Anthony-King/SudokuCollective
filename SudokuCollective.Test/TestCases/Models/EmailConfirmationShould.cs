using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Models
{
    public class EmailConfirmationShould
    {
        private EmailConfirmation sut;
        private DatabaseContext context;

        [SetUp]
        public async Task Setup()
        {
            sut = InitializeSut();
            context = await InitializeDB();
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
        public void DistinguishBetweenNewAndUpdateRequests()
        {
            // Arrange and Act
            var sutNewEmailConfirmation = TestObjects.GetNewEmailConfirmation();
            var sutUpdateEmailConfirmation = TestObjects.GetUpdateEmailConfirmation();

            // Assert
            Assert.That(sutNewEmailConfirmation.ConfirmationType, Is.EqualTo(EmailConfirmationType.NEWEMAILCONFIRMED));
            Assert.That(sutUpdateEmailConfirmation.ConfirmationType, Is.EqualTo(EmailConfirmationType.OLDEMAILCONFIRMED));
        }

        [Test, Category("Models")]
        public void HasAReferenceToAUser()
        {
            // Arrange and Act
            var user = context
                .Users
                .Where(u => u.Id == sut.UserId)
                .FirstOrDefault();

            // Assert
            Assert.That(user, Is.InstanceOf<User>());
        }

        [Test, Category("Models")]
        public void HasAReferenceToAnApp()
        {
            // Arrange and Act
            var app = context
                .Apps
                .Where(a => a.Id == sut.AppId)
                .FirstOrDefault();

            // Assert
            Assert.That(app, Is.InstanceOf<App>());
        }

        [Test, Category("Models")]
        public void SetsOldEmailAddressConfirmedToFalseWhenSettingOldEmailAddress()
        {
            // Arrange and Act
            sut.OldEmailAddress = "example@example.com";

            // Assert
            Assert.That(sut.OldEmailAddressConfirmed, Is.False);
        }

        [Test, Category("Models")]
        public void HasAValueToTrackTheNewEmailAddress()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.NewEmailAddress, Is.Null);
        }

        [Test, Category("Models")]
        public void TrackDateCreated()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.ExpirationDate, Is.InstanceOf<DateTime>());
        }

        [Test, Category("Models")]
        public void HasADefaultConstructor()
        {
            // Arrange and Act
            sut = new EmailConfirmation();

            // Assert
            Assert.That(sut, Is.InstanceOf<EmailConfirmation>());
        }

        [Test, Category("Models")]
        public void HaveAConstructorForSignUps()
        {
            // Arrange
            var userId = 2;
            var appId = 1;

            // Act
            sut = new EmailConfirmation(EmailConfirmationType.NEWPROFILECONFIRMED, userId, appId);

            // Assert
            Assert.That(sut, Is.InstanceOf<EmailConfirmation>());
        }

        [Test, Category("Models")]
        public void HaveAConstructorForEmailUpdates()
        {
            // Arrange
            var userId = 2;
            var appId = 1;
            var oldEmailAddress = "old@example.com";
            var newEmailAddress = "new@example.com";

            // Act
            sut = new EmailConfirmation(
                EmailConfirmationType.OLDEMAILCONFIRMED,
                userId,
                appId,
                oldEmailAddress,
                newEmailAddress);

            // Assert
            Assert.That(sut, Is.InstanceOf<EmailConfirmation>());
        }

        [Test, Category("Models")]
        public void HasAJsonConstructor()
        {
            // Arrange
            var id = 0;
            var userId = 2;
            var appId = 1;
            var token = TestObjects.GetToken();
            var oldEmailAddress = "old@example.com";
            var newEmailAddress = "new@example.com";
            var oldEmailAddressConfirmed = false;
            var dateCreated = DateTime.Now;

            // Act
            sut = new EmailConfirmation(
                id,
                EmailConfirmationType.OLDEMAILCONFIRMED,
                token,
                userId,
                appId,
                oldEmailAddress,
                newEmailAddress,
                oldEmailAddressConfirmed,
                dateCreated);

            // Assert
            Assert.That(sut, Is.InstanceOf<EmailConfirmation>());
        }

        private static EmailConfirmation InitializeSut()
        {
            return TestObjects.GetNewEmailConfirmation();
        }

        private async static Task<DatabaseContext> InitializeDB()
        {
            return await TestDatabase.GetDatabaseContext();
        }
    }
}