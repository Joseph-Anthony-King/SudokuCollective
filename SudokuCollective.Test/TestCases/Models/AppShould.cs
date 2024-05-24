using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Models;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Enums;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Models
{
    public class AppShould
    {
        private IApp sut;

        [SetUp]
        public void Setup()
        {
            sut = new App();
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
        public void HaveExpectedProperties()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.Name, Is.TypeOf<string>());
            Assert.That(sut.License, Is.TypeOf<string>());
            Assert.That(sut.OwnerId, Is.TypeOf<int>());
            Assert.That(sut.CreatedBy, Is.TypeOf<string>());
            Assert.That(sut.LocalUrl, Is.TypeOf<string>());
            Assert.That(sut.StagingUrl, Is.TypeOf<string>());
            Assert.That(sut.TestUrl, Is.TypeOf<string>());
            Assert.That(sut.ProdUrl, Is.TypeOf<string>());
            Assert.That(sut.SourceCodeUrl, Is.TypeOf<string>());
            Assert.That(sut.IsActive, Is.TypeOf<bool>());
            Assert.That(sut.Environment, Is.TypeOf<ReleaseEnvironment>());
            Assert.That(sut.PermitSuperUserAccess, Is.TypeOf<bool>());
            Assert.That(sut.PermitCollectiveLogins, Is.TypeOf<bool>());
            Assert.That(sut.UseCustomEmailConfirmationAction, Is.TypeOf<bool>());
            Assert.That(sut.UseCustomPasswordResetAction, Is.TypeOf<bool>());
            Assert.That(sut.DisableCustomUrls, Is.TypeOf<bool>());
            Assert.That(sut.CustomEmailConfirmationAction, Is.TypeOf<string>());
            Assert.That(sut.CustomPasswordResetAction, Is.TypeOf<string>());
            Assert.That(sut.UseCustomSMTPServer, Is.TypeOf<bool>());
            Assert.That(sut.SMTPServerSettings, Is.TypeOf<SMTPServerSettings>());
            Assert.That(sut.UserCount, Is.TypeOf<int>());
            Assert.That(sut.TimeFrame, Is.TypeOf<TimeFrame>());
            Assert.That(sut.AccessDuration, Is.TypeOf<int>());
            Assert.That(sut.DisplayInGallery, Is.TypeOf<bool>());
            Assert.That(sut.DateCreated, Is.TypeOf<DateTime>());
            Assert.That(sut.DateUpdated, Is.TypeOf<DateTime>());
            Assert.That(sut
                .UserApps
                .ToList()
                .ConvertAll(u => (UserApp)u), Is.InstanceOf<List<UserApp>>());
        }

        [Test, Category("Models")]
        public void DefaultToDeactiveStatus()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.IsActive, Is.False);
        }

        [Test, Category("Models")]
        public void ProvideLicenseByCallingGetLicense()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.GetLicense(0, 0), Is.TypeOf<string>());
            Assert.That(sut.GetLicense(0, 0), Is.EqualTo(sut.License));
        }

        [Test, Category("Models")]
        public void HaveATrueActiveStatusIfActivateAppCalled()
        {
            // Arrange and Act
            sut.ActivateApp();

            // Assert
            Assert.That(sut.IsActive, Is.True);
        }

        [Test, Category("Models")]
        public void HaveAFalseActiveStatusIfDeactivateAppCalled()
        {
            // Arrange and Act
            sut.DeactivateApp();

            // Assert
            Assert.That(sut.IsActive, Is.False);
        }

        [Test, Category("Models")]
        public void HaveAReleaseEnvironmentThatDefaultsToLocal()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.Environment, Is.InstanceOf<ReleaseEnvironment>());
            Assert.That(sut.Environment, Is.EqualTo(ReleaseEnvironment.LOCAL));
        }

        [Test, Category("Models")]
        public void HaveTheAbilityToDisableCustomUrlsThatDefaultsToTrue()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.DisableCustomUrls, Is.InstanceOf<bool>());
            Assert.That(sut.DisableCustomUrls, Is.True);
        }

        [Test, Category("Models")]
        public void UseCustomEmailConfirmationAction()
        {
            // Arrange
            var localUrl = "http://localhost:5001";
            var customAction = "customurl";

            // Act
            sut.LocalUrl = localUrl;
            sut.CustomEmailConfirmationAction = customAction;
            sut.DisableCustomUrls = false;

            // Assert
            Assert.That(sut.UseCustomEmailConfirmationAction, Is.True);
            Assert.That(sut.CustomEmailConfirmationAction, Is.EqualTo(customAction));
        }

        [Test, Category("Models")]
        public void UseCustomPasswordResetAction()
        {
            // Arrange
            var localUrl = "http://localhost:5001";
            var customAction = "customurl";

            // Act
            sut.LocalUrl = localUrl;
            sut.CustomPasswordResetAction = customAction;
            sut.DisableCustomUrls = false;

            // Assert
            Assert.That(sut.UseCustomPasswordResetAction, Is.True);
            Assert.That(sut.CustomPasswordResetAction, Is.EqualTo(customAction));
        }

        [Test, Category("Models")]
        public void TrackUserCountThatDefaultsToZero()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.UserCount, Is.InstanceOf<int>());
            Assert.That(sut.UserCount, Is.EqualTo(0));
        }

        [Test, Category("Models")]
        public void TrackUserCount()
        {
            // Arrange and Act
            var initialUserCount = sut.UserCount;

            var user = new User();
            ((App)sut).UserApps.Add(new UserApp { App = (App)sut, User = user });
            ((App)sut).Users.Add((UserDTO)user.Cast<UserDTO>());

            var finalUserCount = ((App)sut).UserCount;

            // Assert
            Assert.That(initialUserCount, Is.EqualTo(0));
            Assert.That(finalUserCount, Is.EqualTo(1));
        }

        [Test, Category("Models")]
        public void DefaultAllowSuperUserAccessToFalse()
        {
            // Arrange and Act

            // Assert
            Assert.That(sut.PermitSuperUserAccess, Is.InstanceOf<bool>());
            Assert.That(sut.PermitSuperUserAccess, Is.False);
        }

        [Test, Category("Models")]
        public void HaveAConstructorWhichAcceptsProperties()
        {
            // Arrange
            string name = "name";
            string license = TestObjects.GetLicense();
            int ownerId = 0;
            string ownerUserName = "CreatedBy";
            string localUrl = "http://localhost:5173";
            string TestUrl = "";
            string stagingUrl = "https://example-dev.com";
            string liveUrl = "https://www.example.com";

            // Act
            var app = new App(
                name, 
                license, 
                ownerId, 
                ownerUserName, 
                localUrl,
                TestUrl,
                stagingUrl, 
                liveUrl,
                string.Empty);

            // Assert
            Assert.That(app, Is.TypeOf<App>());
        }

        [Test, Category("Models")]
        public void HasAJsonConstructor()
        {
            // Arrange

            // Act
            sut = new App(
                0,
                "name",
                TestObjects.GetLicense(),
                0,
                string.Empty,
                "http://localhost:5173",
                "https://example-dev.com",
                "https://example-test.com",
                "https://www.example.com",
                string.Empty,
                true,
                false,
                true,
                ReleaseEnvironment.LOCAL,
                true,
                null,
                null,
                TimeFrame.DAYS,
                1,
                false,
                DateTime.Now,
                DateTime.MinValue,
                false);

            // Assert
            Assert.That(sut, Is.InstanceOf<App>());
        }

        [Test, Category("Models")]
        public void NullifyLicenses()
        {
            // Arrange and Act
            sut.NullifyLicense();

            // Assert
            Assert.That(string.IsNullOrEmpty(sut.License), Is.True);
        }

        [Test, Category("Models")]
        public void NullifySMTPServerSettings()
        {
            // Arrange and Act
            sut.NullifySMTPServerSettings();

            // Assert
            Assert.That(sut.SMTPServerSettings, Is.Null);
        }

        [Test, Category("Models")]
        public void OutputGalleryApp()
        {
            // Arrange and Act
            var result = sut.Cast<GalleryApp>();

            // Assert
            Assert.That(result, Is.InstanceOf<GalleryApp>());
        }
    }
}
