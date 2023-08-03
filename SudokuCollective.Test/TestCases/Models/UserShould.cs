using System;
using System.Collections.Generic;
using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Models;

namespace SudokuCollective.Test.TestCases.Models
{
    public class UserShould
    {
        private IUser sut;

        [SetUp]
        public void Setup()
        {
            sut = new User();
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
            Assert.That(((User)sut).Id, Is.TypeOf<int>());
            Assert.That(((User)sut).Id, Is.EqualTo(0));
        }

        [Test, Category("Models")]
        public void AcceptFirstAndLastName()
        {
            // Arrange and Act
            sut = new User(
                "John",
                "Doe",
                "T3stPass0rd?1");

            // Assert
            Assert.That(((User)sut).FirstName, Is.EqualTo("John"));
            Assert.That(((User)sut).LastName, Is.EqualTo("Doe"));
            Assert.That(((User)sut).FullName, Is.EqualTo("John Doe"));
            Assert.That(((User)sut).Password, Is.EqualTo("T3stPass0rd?1"));
        }

        [Test, Category("Models")]
        public void HaveWorkingProperties()
        {
            // Arrange and Act
            sut.NickName = "Johnny";
            sut.Email = "JohnDoe@example.com";

            // Assert
            Assert.That(((User)sut).NickName, Is.EqualTo("Johnny"));
            Assert.That(((User)sut).Email, Is.EqualTo("JohnDoe@example.com"));
        }

        [Test, Category("Models")]
        public void HaveMinimumDateTimeValueWhenCreated()
        {
            // Arrange and Act

            // Assert
            Assert.That(((User)sut).DateCreated, Is.EqualTo(DateTime.MinValue));
        }

        [Test, Category("Models")]
        public void HaveAGameList()
        {
            // Arrange and Act

            // Assert
            Assert.That(((User)sut).Games.Count, Is.EqualTo(0));
            Assert.That(((User)sut).Games, Is.TypeOf<List<Game>>());
        }

        [Test, Category("Models")]
        public void HaveARolesList()
        {
            // Arrange and Act

            // Assert
            Assert.That(((User)sut).Roles.Count, Is.EqualTo(0));
            Assert.That(((User)sut).Roles, Is.TypeOf<List<UserRole>>());
        }

        [Test, Category("Models")]
        public void HaveAnAppList()
        {
            // Arrange and Act

            // Assert
            Assert.That(((User)sut).Apps.Count, Is.EqualTo(0));
            Assert.That(((User)sut).Apps, Is.TypeOf<List<UserApp>>());
        }

        [Test, Category("Models")]
        public void HaveAPassword()
        {
            // Arrange and Act

            // Assert
            Assert.That(((User)sut).Password, Is.TypeOf<string>());
        }

        [Test, Category("Models")]
        public void HaveAnActiveStatus()
        {
            // Arrange and Act

            // Assert
            Assert.That(((User)sut).IsActive, Is.TypeOf<bool>());
        }

        [Test, Category("Models")]
        public void HaveAMethodToActivate()
        {
            // Arrange and Act
            sut.ActivateUser();

            // Assert
            Assert.That(((User)sut).IsActive, Is.True);
        }

        [Test, Category("Models")]
        public void HaveAMethodToDeactivate()
        {
            // Arrange and Act
            sut.DeactiveUser();

            // Assert
            Assert.That(((User)sut).IsActive, Is.False);
        }

        [Test, Category("Models")]
        public void HaveAUserNameThatAcceptsAlphaNumericCharacters()
        {
            // Arrange and Act
            sut.UserName = "Good.User-Name";

            // Assert
            Assert.That(((User)sut).UserName, Is.EqualTo("Good.User-Name"));
        }

        [Test, Category("Models")]
        public void HaveAUserNameThatAcceptsSpecialCharacters()
        {
            // Arrange and Act
            sut.UserName = "G@@dUs3rN$m#";

            // Assert
            Assert.That(((User)sut).UserName, Is.EqualTo("G@@dUs3rN$m#"));
        }

        [Test, Category("Models")]
        public void ProvideSuperUserStatus()
        {
            // Arrange and Act

            // Assert
            Assert.That(((User)sut).IsSuperUser, Is.False);
        }

        [Test, Category("Models")]
        public void ProvideAdminStatus()
        {
            // Arrange and Act

            // Assert
            Assert.That(((User)sut).IsAdmin, Is.False);
        }

        [Test, Category("Models")]
        public void TrackEmailConfirmation()
        {
            // Arrange and Act

            // Assert
            Assert.That(((User)sut).IsEmailConfirmed, Is.InstanceOf<bool>());
        }

        [Test, Category("Models")]
        public void DefaultEmailTrackingConfirmationToFalse()
        {
            // Arrange and Act

            // Assert
            Assert.That(((User)sut).IsEmailConfirmed, Is.False);
        }

        [Test, Category("Models")]
        public void ResetEmailTrackingValueIfEmailChanges()
        {
            // Arrange
            sut.Email = "test@example.com";
            sut.IsEmailConfirmed = true;

            var emailTrackingStatePriorToUpdate = sut.IsEmailConfirmed;

            // Act
            sut.Email = "test.UPDATED@example.com";

            // Assert
            Assert.That(emailTrackingStatePriorToUpdate, Is.True);
            Assert.That(((User)sut).IsEmailConfirmed, Is.False);
        }

        [Test, Category("Models")]
        public void InvalidEmailThrowException()
        {
            // Arrange
            var originalEmail = "test@example.com";
            var updatedEmail = "testUPDATED@example";

            sut.Email = originalEmail;
            sut.IsEmailConfirmed = true;

            // Act and Assert
            Assert.Throws<ArgumentException>(() => sut.Email = updatedEmail);
        }

        [Test, Category("Models")]
        public void CanNullifyEmail()
        {
            // Arrange and Act
            sut.NullifyEmail();

            // Assert
            Assert.That(sut.Email, Is.Null);
        }

        [Test, Category("Models")]
        public void CanNullifyPassword()
        {
            // Arrange and Act
            sut.NullifyPassword();

            // Assert
            Assert.That(sut.Password, Is.Null);
        }

        [Test, Category("Models")]
        public void HaveAConstructorWhichAcceptsProperties()
        {
            // Arrange
            string firstName = "name";
            string lastName = "license";
            string password = "T3stPassw0rd!";

            // Act
            var user = new User(firstName, lastName, password);

            // Assert
            Assert.That(user, Is.TypeOf<User>());
            Assert.That(user.FirstName, Is.EqualTo(firstName));
            Assert.That(user.LastName, Is.EqualTo(lastName));
            Assert.That(user.Password, Is.EqualTo(password));
        }

        [Test, Category("Models")]
        public void HasAJsonConstructor()
        {
            // Arrange and Act
            sut = new User(
                0,
                "userName",
                "firstName",
                "lastName",
                "nickName",
                "user@example.com",
                true,
                false,
                "T3stPassw0rd!",
                false,
                true,
                DateTime.Now,
                DateTime.MinValue);

            // Assert
            Assert.That(sut, Is.InstanceOf<User>());
        }

        [Test, Category("Models")]
        public void CastToTranslatedUser()
        {
            // Arrange
            sut = new User(
                0,
                "userName",
                "firstName",
                "lastName",
                "nickName",
                "user@example.com",
                true,
                false,
                "T3stPassw0rd!",
                false,
                true,
                DateTime.Now,
                DateTime.MinValue);
            
            // Act
            var result = (UserDTO)sut.Cast<UserDTO>();


            // Assert
            Assert.That(result, Is.InstanceOf<UserDTO>());
        }
    }
}
