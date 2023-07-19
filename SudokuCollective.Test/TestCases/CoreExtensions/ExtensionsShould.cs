using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using NUnit.Framework;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Extensions;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads;
using SudokuCollective.Data.Extensions;
using SudokuCollective.Data.Models.Payloads;
using SudokuCollective.Data.Models.Requests;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Extensions
{
    public class ExtensionsShould
    {
        [Test, Category("Extensions")]
        public void ShuffleLists()
        {
            // Arrange
            var sut = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var random = new Random();

            // Act
            CoreExtensions.Shuffle(sut, random);

            // Assert
            Assert.That(sut, Is.InstanceOf<List<int>>());
        }

        [Test, Category("Extensions")]
        public void CheckIfListsAreEqual()
        {
            // Arrange
            var sut = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var secondList = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var random = new Random();
            CoreExtensions.Shuffle(secondList, random);

            // Act
            var result = sut.IsThisListEqual(secondList);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test, Category("Extensions")]
        public void RemoveSubLists()
        {
            // Arrange
            var firstList = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var secondList = new List<int> { 1, 2, 3, 4 };

            // Act
            var sut = firstList.RemoveSubList(secondList).ToList();

            // Assert
            Assert.That(sut.Count, Is.EqualTo(5));
        }

        [Test, Category("Extensions")]
        public void ConvertJsonElementsToAppPayload()
        {
            // Arrange
            JsonElement json = new AppPayload();

            // Act
            var result = json.ConvertToPayloadSuccessful(typeof(AppPayload), out IPayload convertedPayload);
            var payload = (AppPayload)convertedPayload;

            // Assert
            Assert.That(result, Is.True);
            Assert.That(payload, Is.InstanceOf<AppPayload>());
        }

        [Test, Category("Extensions")]
        public void ReturnFalseIfConvertJsonElementsToAppPayloadThrowsKeyNotFoundException()
        {
            // Arrange
            JsonElement json = JsonSerializer.SerializeToElement(
                new
                {
                    LocalUrl = "LocalUrl",
                    StagingUrl = "StagingUrl",
                    QaUrl = "QaUrl",
                    ProdUrl = "PodUrl",
                    IsActive = true,
                    Environment = ReleaseEnvironment.LOCAL,
                    PermitSuperUserAccess = true,
                    PermitCollectiveLogins = true,
                    DisableCustomUrls = true,
                    CustomEmailConfirmationAction = "CustomEmailConfirmationAction",
                    CustomPasswordResetAction = "CustomPasswordResetAction",
                    TimeFrame = TimeFrame.HOURS,
                    AccessDuration = 0
                }, 
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            // Act
            var result = json.ConvertToPayloadSuccessful(typeof(AppPayload), out IPayload convertedPayload);
            var payload = (AppPayload)convertedPayload;

            // Assert
            Assert.That(result, Is.False);
            Assert.That(payload, Is.Null);
        }

        [Test, Category("Extensions")]
        public void ConvertJsonElementsToCreateDifficultyPayload()
        {
            // Arrange
            JsonElement json = new CreateDifficultyPayload();

            // Act
            var result = json.ConvertToPayloadSuccessful(typeof(CreateDifficultyPayload), out IPayload convertedPayload);
            var payload = (CreateDifficultyPayload)convertedPayload;

            // Assert
            Assert.That(result, Is.True);
            Assert.That(payload, Is.InstanceOf<CreateDifficultyPayload>());
        }

        [Test, Category("Extensions")]
        public void ReturnFalseIfConvertJsonElementsToCreateDifficultyPayloadThrowsKeyNotFoundException()
        {
            // Arrange
            JsonElement json = JsonSerializer.SerializeToElement(
                new
                {
                    Name = "Name",
                    DisplayName = "DisplayName"
                },
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            // Act
            var result = json.ConvertToPayloadSuccessful(typeof(CreateDifficultyPayload), out IPayload convertedPayload);
            var payload = (CreateDifficultyPayload)convertedPayload;

            // Assert
            Assert.That(result, Is.False);
            Assert.That(payload, Is.Null);
        }

        [Test, Category("Extensions")]
        public void ConvertJsonElementsToUpdateDifficultyPayload()
        {
            // Arrange
            JsonElement json = new UpdateDifficultyPayload();

            // Act
            var result = json.ConvertToPayloadSuccessful(typeof(UpdateDifficultyPayload), out IPayload convertedPayload);
            var payload = (UpdateDifficultyPayload)convertedPayload;

            // Assert
            Assert.That(result, Is.True);
            Assert.That(payload, Is.InstanceOf<UpdateDifficultyPayload>());
        }

        [Test, Category("Extensions")]
        public void ReturnFalseIfConvertJsonElementsToUpdateDifficultyPayloadThrowsKeyNotFoundException()
        {
            // Arrange
            JsonElement json = JsonSerializer.SerializeToElement(
                new
                {
                    Name = "Name",
                    DisplayName = "DisplayName"
                },
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            // Act
            var result = json.ConvertToPayloadSuccessful(typeof(UpdateDifficultyPayload), out IPayload convertedPayload);
            var payload = (UpdateDifficultyPayload)convertedPayload;

            // Assert
            Assert.That(result, Is.False);
            Assert.That(payload, Is.Null);
        }

        [Test, Category("Extensions")]
        public void ReturnFalseIfConvertJsonElementsToAnnonymousCheckPayloadThrowsKeyNotFoundException()
        {
            // Arrange
            JsonElement json = JsonSerializer.SerializeToElement(
                new
                {
                    SecondRow = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                    ThirdRow = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                    FourthRow = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                    FifthRow = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                    SixthRow = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                    SeventhRow = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                    EighthRow = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                    NinthRow = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 }
                },
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            // Act
            var result = json.ConvertToPayloadSuccessful(typeof(AnnonymousCheckRequest), out IPayload convertedPayload);
            var payload = (AnnonymousCheckRequest)convertedPayload;

            // Assert
            Assert.That(result, Is.False);
            Assert.That(payload, Is.Null);
        }

        [Test, Category("Extensions")]
        public void ReturnFalseIfConvertJsonElementsToAnnonymousGamePayloadThrowsKeyNotFoundException()
        {
            // Arrange
            JsonElement json = JsonSerializer.SerializeToElement(
                new {},
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            // Act
            var result = json.ConvertToPayloadSuccessful(typeof(AnnonymousGameRequest), out IPayload convertedPayload);
            var payload = (AnnonymousGameRequest)convertedPayload;

            // Assert
            Assert.That(result, Is.False);
            Assert.That(payload, Is.Null);
        }

        [Test, Category("Extensions")]
        public void ConvertJsonElementsToCreateGamePayload()
        {
            // Arrange
            JsonElement json = new CreateGamePayload();

            // Act
            var result = json.ConvertToPayloadSuccessful(typeof(CreateGamePayload), out IPayload convertedPayload);
            var payload = (CreateGamePayload)convertedPayload;

            // Assert
            Assert.That(result, Is.True);
            Assert.That(payload, Is.InstanceOf<CreateGamePayload>());
        }

        [Test, Category("Extensions")]
        public void ReturnFalseIfConvertJsonElementsToCreateGamePayloadThrowsKeyNotFoundException()
        {
            // Arrange
            JsonElement json = JsonSerializer.SerializeToElement(
                new 
                {
                    UserId = 1
                },
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            // Act
            var result = json.ConvertToPayloadSuccessful(typeof(CreateGamePayload), out IPayload convertedPayload);
            var payload = (CreateGamePayload)convertedPayload;

            // Assert
            Assert.That(result, Is.False);
            Assert.That(payload, Is.Null);
        }

        [Test, Category("Extensions")]
        public void ConvertJsonElementsToGamesPayload()
        {
            // Arrange
            JsonElement json = new GamesPayload();

            // Act
            var result = json.ConvertToPayloadSuccessful(typeof(GamesPayload), out IPayload convertedPayload);
            var payload = (GamesPayload)convertedPayload;

            // Assert
            Assert.That(result, Is.True);
            Assert.That(payload, Is.InstanceOf<GamesPayload>());
        }

        [Test, Category("Extensions")]
        public void ReturnFalseIfConvertJsonElementsToGamesPayloadThrowsKeyNotFoundException()
        {
            // Arrange
            JsonElement json = JsonSerializer.SerializeToElement(
                new {},
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            // Act
            var result = json.ConvertToPayloadSuccessful(typeof(GamesPayload), out IPayload convertedPayload);
            var payload = (GamesPayload)convertedPayload;

            // Assert
            Assert.That(result, Is.False);
            Assert.That(payload, Is.Null);
        }

        [Test, Category("Extensions")]
        public void ConvertJsonElementsToGamePayload()
        {
            // Arrange
            JsonElement json = new GamePayload()
            {
                SudokuCells = TestObjects.GetSolvedSudokuCells()
            };

            // Act
            var result = json.ConvertToPayloadSuccessful(typeof(GamePayload), out IPayload convertedPayload);
            var payload = (GamePayload)convertedPayload;

            // Assert
            Assert.That(result, Is.True);
            Assert.That(payload, Is.InstanceOf<GamePayload>());
        }

        [Test, Category("Extensions")]
        public void ReturnFalseIfConvertJsonElementsToGamePayloadThrowsKeyNotFoundException()
        {
            // Arrange
            JsonElement json = JsonSerializer.SerializeToElement(
                new 
                {
                    GameId = 1
                },
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            // Act
            var result = json.ConvertToPayloadSuccessful(typeof(GamePayload), out IPayload convertedPayload);
            var payload = (GamePayload)convertedPayload;

            // Assert
            Assert.That(result, Is.False);
            Assert.That(payload, Is.Null);
        }

        [Test, Category("Extensions")]
        public void ConvertJsonElementsToCreateRolePayload()
        {
            // Arrange
            JsonElement json = new CreateRolePayload();

            // Act
            var result = json.ConvertToPayloadSuccessful(typeof(CreateRolePayload), out IPayload convertedPayload);
            var payload = (CreateRolePayload)convertedPayload;

            // Assert
            Assert.That(result, Is.True);
            Assert.That(payload, Is.InstanceOf<CreateRolePayload>());
        }

        [Test, Category("Extensions")]
        public void ReturnFalseIfConvertJsonElementsToCreateRolePayloadThrowsKeyNotFoundException()
        {
            // Arrange
            JsonElement json = JsonSerializer.SerializeToElement(
                new
                {
                    name = "Name"
                },
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            // Act
            var result = json.ConvertToPayloadSuccessful(typeof(CreateRolePayload), out IPayload convertedPayload);
            var payload = (CreateRolePayload)convertedPayload;

            // Assert
            Assert.That(result, Is.False);
            Assert.That(payload, Is.Null);
        }

        [Test, Category("Extensions")]
        public void ConvertJsonElementsToUpdateRolePayload()
        {
            // Arrange
            JsonElement json = new UpdateRolePayload();

            // Act
            var result = json.ConvertToPayloadSuccessful(typeof(UpdateRolePayload), out IPayload convertedPayload);
            var payload = (UpdateRolePayload)convertedPayload;

            // Assert
            Assert.That(result, Is.True);
            Assert.That(payload, Is.InstanceOf<UpdateRolePayload>());
        }

        [Test, Category("Extensions")]
        public void ReturnFalseIfConvertJsonElementsToUpdateRolePayloadThrowsKeyNotFoundException()
        {
            // Arrange
            JsonElement json = JsonSerializer.SerializeToElement(
                new
                {
                    Id = 1,
                    RoleName = "Name"
                },
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            // Act
            var result = json.ConvertToPayloadSuccessful(typeof(UpdateRolePayload), out IPayload convertedPayload);
            var payload = (UpdateRolePayload)convertedPayload;

            // Assert
            Assert.That(result, Is.False);
            Assert.That(payload, Is.Null);
        }

        [Test, Category("Extensions")]
        public void ConvertJsonElementsToAddSolutionsPayload()
        {
            // Arrange
            JsonElement json = new AddSolutionsPayload();

            // Act
            var result = json.ConvertToPayloadSuccessful(typeof(AddSolutionsPayload), out IPayload convertedPayload);
            var payload = (AddSolutionsPayload)convertedPayload;

            // Assert
            Assert.That(result, Is.True);
            Assert.That(payload, Is.InstanceOf<AddSolutionsPayload>());
        }

        [Test, Category("Extensions")]
        public void ReturnFalseIfConvertJsonElementsToAddSolutionsPayloadThrowsKeyNotFoundException()
        {
            // Arrange
            JsonElement json = JsonSerializer.SerializeToElement(
                new {},
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            // Act
            var result = json.ConvertToPayloadSuccessful(typeof(AddSolutionsPayload), out IPayload convertedPayload);
            var payload = (AddSolutionsPayload)convertedPayload;

            // Assert
            Assert.That(result, Is.False);
            Assert.That(payload, Is.Null);
        }

        [Test, Category("Extensions")]
        public void ConvertJsonElementsToSolutionPayload()
        {
            // Arrange
            JsonElement json = new SolutionPayload()
            {
                FirstRow = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                SecondRow = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                ThirdRow = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                FourthRow = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                FifthRow = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                SixthRow = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                SeventhRow = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                EighthRow = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                NinthRow = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 }
            };

            // Act
            var result = json.ConvertToPayloadSuccessful(typeof(SolutionPayload), out IPayload convertedPayload);
            var payload = (SolutionPayload)convertedPayload;

            // Assert
            Assert.That(result, Is.True);
            Assert.That(payload, Is.InstanceOf<SolutionPayload>());
        }

        [Test, Category("Extensions")]
        public void ReturnFalseIfConvertJsonElementsToSolutionPayloadThrowsKeyNotFoundException()
        {
            // Arrange
            JsonElement json = JsonSerializer.SerializeToElement(
                new
                {
                    SecondRow = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                    ThirdRow = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                    FourthRow = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                    FifthRow = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                    SixthRow = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                    SeventhRow = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                    EighthRow = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                    NinthRow = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 }
                },
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            // Act
            var result = json.ConvertToPayloadSuccessful(typeof(SolutionPayload), out IPayload convertedPayload);
            var payload = (SolutionPayload)convertedPayload;

            // Assert
            Assert.That(result, Is.False);
            Assert.That(payload, Is.Null);
        }

        [Test, Category("Extensions")]
        public void ConvertJsonElementsToPasswordResetPayload()
        {
            // Arrange
            JsonElement json = new PasswordResetPayload()
            {
                UserId = 1,
                NewPassword = "T3stP@ssw0rd"
            };

            // Act
            var result = json.ConvertToPayloadSuccessful(typeof(PasswordResetPayload), out IPayload convertedPayload);
            var payload = (PasswordResetPayload)convertedPayload;

            // Assert
            Assert.That(result, Is.True);
            Assert.That(payload, Is.InstanceOf<PasswordResetPayload>());
        }

        [Test, Category("Extensions")]
        public void ReturnFalseIfConvertJsonElementsToPasswordResetPayloadThrowsKeyNotFoundException()
        {
            // Arrange
            JsonElement json = JsonSerializer.SerializeToElement(
                new
                {
                    NewPassword = "T3stP@ssw0rd"
                },
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            // Act
            var result = json.ConvertToPayloadSuccessful(typeof(PasswordResetPayload), out IPayload convertedPayload);
            var payload = (PasswordResetPayload)convertedPayload;

            // Assert
            Assert.That(result, Is.False);
            Assert.That(payload, Is.Null);
        }

        [Test, Category("Extensions")]
        public void ConvertJsonElementsToRequestPasswordResetPayload()
        {
            // Arrange
            JsonElement json = new RequestPasswordResetPayload()
            {
                License = TestObjects.GetLicense(),
                Email = "email@example.com"
            };

            // Act
            var result = json.ConvertToPayloadSuccessful(typeof(RequestPasswordResetPayload), out IPayload convertedPayload);
            var payload = (RequestPasswordResetPayload)convertedPayload;

            // Assert
            Assert.That(result, Is.True);
            Assert.That(payload, Is.InstanceOf<RequestPasswordResetPayload>());
        }

        [Test, Category("Extensions")]
        public void ReturnFalseIfConvertJsonElementsToRequestPasswordResetPayloadThrowsKeyNotFoundException()
        {
            // Arrange
            JsonElement json = JsonSerializer.SerializeToElement(
                new
                {
                    Email = "email@example.com"
                },
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            // Act
            var result = json.ConvertToPayloadSuccessful(typeof(RequestPasswordResetPayload), out IPayload convertedPayload);
            var payload = (RequestPasswordResetPayload)convertedPayload;

            // Assert
            Assert.That(result, Is.False);
            Assert.That(payload, Is.Null);
        }

        [Test, Category("Extensions")]
        public void ConvertJsonElementsToUpdateUserPayload()
        {
            // Arrange
            JsonElement json = new UpdateUserPayload()
            {
                UserName = "TestUser",
                FirstName = "Test",
                LastName = "User",
                NickName = "",
                Email = "testUser@example.com"
            };

            // Act
            var result = json.ConvertToPayloadSuccessful(typeof(UpdateUserPayload), out IPayload convertedPayload);
            var payload = (UpdateUserPayload)convertedPayload;

            // Assert
            Assert.That(result, Is.True);
            Assert.That(payload, Is.InstanceOf<UpdateUserPayload>());
        }

        [Test, Category("Extensions")]
        public void ReturnFalseIfConvertJsonElementsToUpdateUserPayloadThrowsKeyNotFoundException()
        {
            // Arrange
            JsonElement json = JsonSerializer.SerializeToElement(
                new
                {
                    UserName = "TestUser",
                    FirstName = "Test",
                    LastName = "User",
                    NickName = ""
                },
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            // Act
            var result = json.ConvertToPayloadSuccessful(typeof(UpdateUserPayload), out IPayload convertedPayload);
            var payload = (UpdateUserPayload)convertedPayload;

            // Assert
            Assert.That(result, Is.False);
            Assert.That(payload, Is.Null);
        }

        [Test, Category("Extensions")]
        public void ConvertJsonElementsToUpdateUserRolePayload()
        {
            // Arrange
            JsonElement json = new UpdateUserRolePayload();

            // Act
            var result = json.ConvertToPayloadSuccessful(typeof(UpdateUserRolePayload), out IPayload convertedPayload);
            var payload = (UpdateUserRolePayload)convertedPayload;

            // Assert
            Assert.That(result, Is.True);
            Assert.That(payload, Is.InstanceOf<UpdateUserRolePayload>());
        }

        [Test, Category("Extensions")]
        public void ReturnFalseIfConvertJsonElementsToUpdateUserRolePayloadThrowsKeyNotFoundException()
        {
            // Arrange
            JsonElement json = JsonSerializer.SerializeToElement(
                new {},
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            // Act
            var result = json.ConvertToPayloadSuccessful(typeof(UpdateUserRolePayload), out IPayload convertedPayload);
            var payload = (UpdateUserRolePayload)convertedPayload;

            // Assert
            Assert.That(result, Is.False);
            Assert.That(payload, Is.Null);
        }
    }
}
