using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models;

namespace SudokuCollective.Test.TestData
{
    public static class TestDatabase
    {
        public static async Task<DatabaseContext> GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            Mock<IWebHostEnvironment> env = new Mock<IWebHostEnvironment>();

            env.Setup(x => x.EnvironmentName).Returns("Development");

            Mock<IConfigurationSection> mockSection = new Mock<IConfigurationSection>();
            
            mockSection.Setup(x=>x.Value).Returns(TestObjects.GetEncryptionKey());

            Mock<IConfiguration> config = new Mock<IConfiguration>();

            config
                .Setup(x => x.GetSection(It.Is<string>(k => k == "EncryptionKey")))
                .Returns(mockSection.Object);

            var databaseContext = new DatabaseContext(
                options,
                env.Object,
                config.Object);

            databaseContext.Database.EnsureCreated();

            var dateCreated = DateTime.UtcNow;

            if (!await databaseContext.Roles.AnyAsync())
            {
                databaseContext.Roles.AddRange(
                    new Role(1, "Null", RoleLevel.NULL),
                    new Role(2, "Super User", RoleLevel.SUPERUSER),
                    new Role(3, "Admin", RoleLevel.ADMIN),
                    new Role(4, "User", RoleLevel.USER)
                );

                await databaseContext.SaveChangesAsync();
            }

            if (!await databaseContext.Difficulties.AnyAsync())
            {
                databaseContext.Difficulties.AddRange(
                    new Difficulty(1, "Null", "Null", DifficultyLevel.NULL),
                    new Difficulty(2, "Test", "Test", DifficultyLevel.TEST),
                    new Difficulty(3, "Easy", "Steady Sloth", DifficultyLevel.EASY),
                    new Difficulty(4, "Medium", "Leaping Lemur", DifficultyLevel.MEDIUM),
                    new Difficulty(5, "Hard", "Mighty Mountain Lion", DifficultyLevel.HARD),
                    new Difficulty(6, "Evil", "Sneaky Shark", DifficultyLevel.EVIL)
                );

                await databaseContext.SaveChangesAsync();
            }

            if (!await databaseContext.Users.AnyAsync())
            {
                var salt = BCrypt.Net.BCrypt.GenerateSalt();

                databaseContext.Users.AddRange(
                    new User(
                        1,
                        "TestSuperUser",
                        "Test Super",
                        "User",
                        "Test Super User",
                        "TestSuperUser@example.com",
                        true,
                        false,
                        BCrypt.Net.BCrypt.HashPassword("T3stPass0rd?1", salt),
                        false,
                        true,
                        dateCreated,
                        DateTime.MinValue),
                    new User(
                        2,
                        "TestUser",
                        "Test",
                        "User",
                        "Test User",
                        "TestUser@example.com",
                        false,
                        true,
                        BCrypt.Net.BCrypt.HashPassword("T3stPass0rd?2", salt),
                        true,
                        true,
                        dateCreated,
                        DateTime.MinValue),
                    new User(
                        3,
                        "TestUser2",
                        "Test",
                        "User2",
                        "Test User 2",
                        "TestUser2@example.com",
                        false,
                        true,
                        BCrypt.Net.BCrypt.HashPassword("T3stPass0rd?3", salt),
                        true,
                        true,
                        dateCreated,
                        DateTime.MinValue)
                );

                await databaseContext.SaveChangesAsync();
            }

            if (!await databaseContext.UsersRoles.AnyAsync())
            {
                databaseContext.UsersRoles.AddRange(
                    new UserRole(1, 1, 2),
                    new UserRole(2, 1, 3),
                    new UserRole(3, 1, 4),
                    new UserRole(4, 2, 3),
                    new UserRole(5, 2, 4),
                    new UserRole(6, 3, 4)
                );

                await databaseContext.SaveChangesAsync();
            }

            if (!await databaseContext.Apps.AnyAsync())
            {
                databaseContext.Apps.AddRange(
                    new App(
                        1,
                        "Test App 1",
                        TestObjects.GetLicense(),
                        1,
                        "TestSuperUser",
                        "https://localhost:4200",
                        "https://testapp.dev.com",
                        "https://testapp.qa.com",
                        "https://testapp.com",
                        string.Empty,
                        true,
                        true,
                        true,
                        ReleaseEnvironment.PROD,
                        true,
                        string.Empty,
                        string.Empty,
                        false,
                        TimeFrame.DAYS,
                        1,
                        true,
                        dateCreated,
                        DateTime.MinValue),
                    new App(
                        2,
                        "Test App 2",
                        TestObjects.GetSecondLicense(),
                        1,
                        "TestSuperUser",
                        "https://localhost:8080",
                        "https://testapp2.dev.com",
                        "https://testapp2.qa.com",
                        "https://testapp2.com",
                        string.Empty,
                        true,
                        false,
                        true,
                        ReleaseEnvironment.LOCAL,
                        true,
                        string.Empty,
                        string.Empty,
                        false,
                        TimeFrame.DAYS,
                        1,
                        false,
                        dateCreated,
                        DateTime.MinValue),
                    new App(
                        3,
                        "Test App 3",
                        TestObjects.GetThirdLicense(),
                        2,
                        "TestUser",
                        "https://localhost:8080",
                        "https://testapp3.dev.com",
                        "https://testapp3.qa.com",
                        "https://testapp3.com",
                        string.Empty,
                        true,
                        false,
                        true,
                        ReleaseEnvironment.LOCAL,
                        true,
                        string.Empty,
                        string.Empty,
                        false,
                        TimeFrame.DAYS,
                        1,
                        false,
                        dateCreated,
                        DateTime.MinValue)
                );

                await databaseContext.SaveChangesAsync();
            }

            if (!await databaseContext.SMTPServerSettings.AnyAsync())
            {
                databaseContext.SMTPServerSettings.AddRange(
                    new SMTPServerSettings(
                        1, 
                        string.Empty, 
                        0, 
                        string.Empty, 
                        string.Empty, 
                        string.Empty, 
                        1),
                    new SMTPServerSettings(
                        2, 
                        string.Empty, 
                        0, 
                        string.Empty, 
                        string.Empty, 
                        string.Empty, 
                        2),
                    new SMTPServerSettings(
                        3, 
                        string.Empty, 
                        0, 
                        string.Empty, 
                        string.Empty, 
                        string.Empty,
                        3)
                );

                await databaseContext.SaveChangesAsync();
            }

            if (!await databaseContext.UsersApps.AnyAsync())
            {
                databaseContext.UsersApps.AddRange(
                    new UserApp(1, 1, 1),
                    new UserApp(3, 2, 1),
                    new UserApp(4, 2, 2),
                    new UserApp(5, 3, 2),
                    new UserApp(6, 2, 3)
                );

                await databaseContext.SaveChangesAsync();
            }

            if (!await databaseContext.AppAdmins.AnyAsync())
            {
                databaseContext.AppAdmins.AddRange(
                    new AppAdmin(1, 1, 1, true),
                    new AppAdmin(2, 2, 1, true),
                    new AppAdmin(3, 3, 2, true)
                );
            }
            if (!await databaseContext.SudokuMatrices.AnyAsync())
            {
                databaseContext.SudokuMatrices.AddRange(
                    new SudokuMatrix(1, 4),
                    new SudokuMatrix(2, 4)
                );

                await databaseContext.SaveChangesAsync();
            }

            if (!await databaseContext.SudokuCells.AnyAsync())
            {
                databaseContext.SudokuCells.AddRange(
                    new SudokuCell(81, 1, 1, 1, 1, 4, 4, false, 1),
                    new SudokuCell(58, 2, 2, 1, 1, 2, 0, true, 1),
                    new SudokuCell(57, 3, 3, 1, 1, 8, 8, false, 1),
                    new SudokuCell(56, 4, 4, 2, 1, 6, 0, true, 1),
                    new SudokuCell(55, 5, 5, 2, 1, 1, 0, true, 1),
                    new SudokuCell(54, 6, 6, 2, 1, 5, 0, true, 1),
                    new SudokuCell(53, 7, 7, 3, 1, 9, 0, true, 1),
                    new SudokuCell(52, 8, 8, 3, 1, 3, 0, true, 1),
                    new SudokuCell(51, 9, 9, 3, 1, 7, 0, true, 1),
                    new SudokuCell(50, 10, 1, 1, 2, 3, 3, false, 1),
                    new SudokuCell(49, 11, 2, 1, 2, 9, 0, true, 1),
                    new SudokuCell(48, 12, 3, 1, 2, 6, 6, false, 1),
                    new SudokuCell(47, 13, 4, 2, 2, 2, 0, true, 1),
                    new SudokuCell(46, 14, 5, 2, 2, 8, 0, true, 1),
                    new SudokuCell(45, 15, 6, 2, 2, 7, 0, true, 1),
                    new SudokuCell(44, 16, 7, 3, 2, 1, 1, false, 1),
                    new SudokuCell(43, 17, 8, 3, 2, 4, 4, false, 1),
                    new SudokuCell(42, 18, 9, 3, 2, 5, 0, false, 1),
                    new SudokuCell(59, 19, 1, 1, 3, 1, 0, true, 1),
                    new SudokuCell(61, 20, 2, 1, 3, 5, 0, true, 1),
                    new SudokuCell(80, 21, 3, 1, 3, 7, 7, false, 1),
                    new SudokuCell(62, 22, 4, 2, 3, 3, 0, true, 1),
                    new SudokuCell(79, 23, 5, 2, 3, 4, 0, true, 1),
                    new SudokuCell(78, 24, 6, 2, 3, 9, 9, false, 1),
                    new SudokuCell(77, 25, 7, 3, 3, 6, 0, true, 1),
                    new SudokuCell(76, 26, 8, 3, 3, 8, 0, true, 1),
                    new SudokuCell(75, 27, 9, 3, 3, 2, 2, false, 1),
                    new SudokuCell(74, 28, 1, 4, 4, 5, 0, true, 1),
                    new SudokuCell(73, 29, 2, 4, 4, 8, 8, false, 1),
                    new SudokuCell(72, 30, 3, 4, 4, 4, 4, false, 1),
                    new SudokuCell(71, 31, 4, 5, 4, 9, 0, true, 1),
                    new SudokuCell(70, 32, 5, 5, 4, 3, 0, true, 1),
                    new SudokuCell(69, 33, 6, 5, 4, 6, 0, true, 1),
                    new SudokuCell(68, 34, 7, 6, 4, 2, 0, true, 1),
                    new SudokuCell(67, 35, 8, 6, 4, 7, 0, true, 1),
                    new SudokuCell(66, 36, 9, 6, 4, 1, 0, true, 1),
                    new SudokuCell(65, 37, 1, 4, 5, 7, 7, false, 1),
                    new SudokuCell(64, 38, 2, 4, 5, 6, 0, true, 1),
                    new SudokuCell(63, 39, 3, 4, 5, 1, 0, true, 1),
                    new SudokuCell(41, 40, 4, 5, 5, 8, 8, false, 1),
                    new SudokuCell(40, 41, 5, 5, 5, 2, 2, false, 1),
                    new SudokuCell(39, 42, 6, 5, 5, 4, 4, false, 1),
                    new SudokuCell(38, 43, 7, 6, 5, 5, 5, false, 1),
                    new SudokuCell(16, 44, 8, 6, 5, 9, 0, true, 1),
                    new SudokuCell(15, 45, 9, 6, 5, 3, 0, true, 1),
                    new SudokuCell(14, 46, 1, 4, 6, 2, 2, false, 1),
                    new SudokuCell(13, 47, 2, 4, 6, 3, 0, true, 1),
                    new SudokuCell(12, 48, 3, 4, 6, 9, 9, false, 1),
                    new SudokuCell(11, 49, 4, 5, 6, 7, 7, false, 1),
                    new SudokuCell(10, 50, 5, 5, 6, 5, 5, false, 1),
                    new SudokuCell(17, 51, 6, 5, 6, 1, 0, true, 1),
                    new SudokuCell(9, 52, 7, 6, 6, 4, 4, false, 1),
                    new SudokuCell(7, 53, 8, 6, 6, 6, 6, false, 1),
                    new SudokuCell(6, 54, 9, 6, 6, 8, 0, true, 1),
                    new SudokuCell(5, 55, 1, 7, 7, 9, 0, true, 1),
                    new SudokuCell(4, 56, 2, 7, 7, 7, 0, true, 1),
                    new SudokuCell(3, 57, 3, 7, 7, 5, 0, true, 1),
                    new SudokuCell(2, 58, 4, 8, 7, 1, 0, true, 1),
                    new SudokuCell(1, 59, 5, 8, 7, 6, 0, true, 1),
                    new SudokuCell(8, 60, 6, 8, 7, 8, 8, false, 1),
                    new SudokuCell(18, 61, 7, 9, 7, 3, 3, false, 1),
                    new SudokuCell(19, 62, 8, 9, 7, 2, 2, false, 1),
                    new SudokuCell(20, 63, 9, 9, 7, 4, 0, true, 1),
                    new SudokuCell(37, 64, 1, 7, 8, 6, 0, true, 1),
                    new SudokuCell(36, 65, 2, 7, 8, 1, 0, true, 1),
                    new SudokuCell(35, 66, 3, 7, 8, 2, 2, false, 1),
                    new SudokuCell(34, 67, 4, 8, 8, 4, 0, true, 1),
                    new SudokuCell(33, 68, 5, 8, 8, 7, 7, false, 1),
                    new SudokuCell(32, 69, 6, 8, 8, 3, 0, true, 1),
                    new SudokuCell(31, 70, 7, 9, 8, 8, 0, true, 1),
                    new SudokuCell(30, 71, 8, 9, 8, 5, 0, true, 1),
                    new SudokuCell(29, 72, 9, 9, 8, 9, 0, true, 1),
                    new SudokuCell(28, 73, 1, 7, 9, 8, 0, true, 1),
                    new SudokuCell(27, 74, 2, 7, 9, 4, 0, true, 1),
                    new SudokuCell(26, 75, 3, 7, 9, 3, 0, true, 1),
                    new SudokuCell(25, 76, 4, 8, 9, 5, 0, true, 1),
                    new SudokuCell(24, 77, 5, 8, 9, 9, 0, true, 1),
                    new SudokuCell(23, 78, 6, 8, 9, 2, 0, true, 1),
                    new SudokuCell(22, 79, 7, 9, 9, 7, 7, false, 1),
                    new SudokuCell(21, 80, 8, 9, 9, 1, 0, true, 1),
                    new SudokuCell(60, 81, 9, 9, 9, 6, 6, false, 1),
                    new SudokuCell(162, 1, 1, 1, 1, 3, 3, true, 2),
                    new SudokuCell(139, 2, 2, 1, 1, 8, 8, false, 2),
                    new SudokuCell(138, 3, 3, 1, 1, 1, 1, false, 2),
                    new SudokuCell(137, 4, 4, 2, 1, 9, 9, false, 2),
                    new SudokuCell(136, 5, 5, 2, 1, 2, 2, true, 2),
                    new SudokuCell(135, 6, 6, 2, 1, 6, 6, false, 2),
                    new SudokuCell(134, 7, 7, 3, 1, 4, 4, true, 2),
                    new SudokuCell(133, 8, 8, 3, 1, 5, 5, true, 2),
                    new SudokuCell(132, 9, 9, 3, 1, 7, 7, false, 2),
                    new SudokuCell(131, 10, 1, 1, 2, 9, 9, true, 2),
                    new SudokuCell(130, 11, 2, 1, 2, 7, 7, false, 2),
                    new SudokuCell(129, 12, 3, 1, 2, 5, 5, true, 2),
                    new SudokuCell(128, 13, 4, 2, 2, 1, 1, true, 2),
                    new SudokuCell(127, 14, 5, 2, 2, 3, 3, false, 2),
                    new SudokuCell(126, 15, 6, 2, 2, 4, 4, false, 2),
                    new SudokuCell(125, 16, 7, 3, 2, 2, 2, false, 2),
                    new SudokuCell(124, 17, 8, 3, 2, 8, 8, false, 2),
                    new SudokuCell(123, 18, 9, 3, 2, 6, 6, false, 2),
                    new SudokuCell(140, 19, 1, 1, 3, 4, 4, false, 2),
                    new SudokuCell(142, 20, 2, 1, 3, 2, 2, false, 2),
                    new SudokuCell(161, 21, 3, 1, 3, 6, 6, true, 2),
                    new SudokuCell(143, 22, 4, 2, 3, 8, 8, true, 2),
                    new SudokuCell(160, 23, 5, 2, 3, 5, 5, true, 2),
                    new SudokuCell(159, 24, 6, 2, 3, 7, 7, true, 2),
                    new SudokuCell(158, 25, 7, 3, 3, 1, 1, false, 2),
                    new SudokuCell(157, 26, 8, 3, 3, 3, 3, true, 2),
                    new SudokuCell(156, 27, 9, 3, 3, 9, 9, true, 2),
                    new SudokuCell(155, 28, 1, 4, 4, 1, 1, false, 2),
                    new SudokuCell(154, 29, 2, 4, 4, 3, 3, true, 2),
                    new SudokuCell(153, 30, 3, 4, 4, 7, 7, true, 2),
                    new SudokuCell(152, 31, 4, 5, 4, 4, 4, false, 2),
                    new SudokuCell(151, 32, 5, 5, 4, 9, 9, true, 2),
                    new SudokuCell(150, 33, 6, 5, 4, 2, 2, true, 2),
                    new SudokuCell(149, 34, 7, 6, 4, 8, 8, true, 2),
                    new SudokuCell(148, 35, 8, 6, 4, 6, 6, true, 2),
                    new SudokuCell(147, 36, 9, 6, 4, 5, 4, false, 2),
                    new SudokuCell(146, 37, 1, 4, 5, 5, 5, true, 2),
                    new SudokuCell(145, 38, 2, 4, 5, 4, 4, true, 2),
                    new SudokuCell(144, 39, 3, 4, 5, 2, 2, false, 2),
                    new SudokuCell(122, 40, 4, 5, 5, 3, 3, true, 2),
                    new SudokuCell(121, 41, 5, 5, 5, 6, 6, false, 2),
                    new SudokuCell(120, 42, 6, 5, 5, 8, 8, false, 2),
                    new SudokuCell(119, 43, 7, 6, 5, 9, 9, true, 2),
                    new SudokuCell(97, 44, 8, 6, 5, 7, 0, true, 2),
                    new SudokuCell(96, 45, 9, 6, 5, 1, 1, true, 2),
                    new SudokuCell(95, 46, 1, 4, 6, 6, 6, true, 2),
                    new SudokuCell(94, 47, 2, 4, 6, 9, 9, false, 2),
                    new SudokuCell(93, 48, 3, 4, 6, 8, 8, true, 2),
                    new SudokuCell(92, 49, 4, 5, 6, 5, 5, false, 2),
                    new SudokuCell(91, 50, 5, 5, 6, 7, 7, true, 2),
                    new SudokuCell(98, 51, 6, 5, 6, 1, 1, true, 2),
                    new SudokuCell(90, 52, 7, 6, 6, 3, 3, true, 2),
                    new SudokuCell(88, 53, 8, 6, 6, 4, 4, true, 2),
                    new SudokuCell(87, 54, 9, 6, 6, 2, 2, false, 2),
                    new SudokuCell(86, 55, 1, 7, 7, 2, 2, true, 2),
                    new SudokuCell(85, 56, 2, 7, 7, 6, 6, true, 2),
                    new SudokuCell(84, 57, 3, 7, 7, 3, 3, true, 2),
                    new SudokuCell(83, 58, 4, 8, 7, 7, 7, false, 2),
                    new SudokuCell(82, 59, 5, 8, 7, 4, 4, true, 2),
                    new SudokuCell(89, 60, 6, 8, 7, 9, 9, true, 2),
                    new SudokuCell(99, 61, 7, 9, 7, 5, 5, false, 2),
                    new SudokuCell(100, 62, 8, 9, 7, 1, 1, true, 2),
                    new SudokuCell(101, 63, 9, 9, 7, 8, 8, true, 2),
                    new SudokuCell(118, 64, 1, 7, 8, 7, 7, true, 2),
                    new SudokuCell(117, 65, 2, 7, 8, 1, 1, true, 2),
                    new SudokuCell(116, 66, 3, 7, 8, 4, 4, true, 2),
                    new SudokuCell(115, 67, 4, 8, 8, 2, 2, true, 2),
                    new SudokuCell(114, 68, 5, 8, 8, 8, 8, true, 2),
                    new SudokuCell(113, 69, 6, 8, 8, 5, 5, true, 2),
                    new SudokuCell(112, 70, 7, 9, 8, 6, 6, true, 2),
                    new SudokuCell(111, 71, 8, 9, 8, 9, 9, false, 2),
                    new SudokuCell(110, 72, 9, 9, 8, 3, 3, true, 2),
                    new SudokuCell(109, 73, 1, 7, 9, 8, 8, false, 2),
                    new SudokuCell(108, 74, 2, 7, 9, 5, 5, true, 2),
                    new SudokuCell(107, 75, 3, 7, 9, 9, 9, false, 2),
                    new SudokuCell(106, 76, 4, 8, 9, 6, 6, true, 2),
                    new SudokuCell(105, 77, 5, 8, 9, 1, 1, true, 2),
                    new SudokuCell(104, 78, 6, 8, 9, 3, 3, false, 2),
                    new SudokuCell(103, 79, 7, 9, 9, 7, 7, true, 2),
                    new SudokuCell(102, 80, 8, 9, 9, 2, 2, true, 2),
                    new SudokuCell(141, 81, 9, 9, 9, 4, 4, true, 2)
                );

                await databaseContext.SaveChangesAsync();
            }

            if (!await databaseContext.SudokuSolutions.AnyAsync())
            {
                databaseContext.SudokuSolutions.AddRange(
                    new SudokuSolution(1, new List<int>(), dateCreated, DateTime.MinValue),
                    new SudokuSolution(2, new List<int>(), dateCreated, DateTime.MinValue),
                    new SudokuSolution(3, new List<int>() { 1, 2, 9, 5, 6, 7, 8, 3, 4, 8, 3, 4, 9, 2, 1, 7, 6, 5, 5, 7, 6, 8, 4, 3, 2, 9, 1, 4, 1, 8, 2, 5, 6, 3, 7, 9, 9, 6, 7, 3, 1, 4, 5, 8, 2, 3, 5, 2, 7, 8, 9, 1, 4, 6, 2, 9, 1, 4, 3, 8, 6, 5, 7, 6, 4, 3, 1, 7, 5, 9, 2, 8, 7, 8, 5, 6, 9, 2, 4, 1, 3 }, dateCreated, DateTime.MinValue)
                );

                await databaseContext.SaveChangesAsync();
            }

            if (!await databaseContext.Games.AnyAsync())
            {
                databaseContext.Games.AddRange(
                    new Game(1, 1, 1, 1, 1, true, 0, true, dateCreated, DateTime.MinValue, DateTime.MinValue),
                    new Game(2, 2, 2, 2, 2, true, 0, true, dateCreated, DateTime.MinValue, DateTime.MinValue)
                );

                await databaseContext.SaveChangesAsync();
            }

            if (!await databaseContext.EmailConfirmations.AnyAsync())
            {
                databaseContext.EmailConfirmations.AddRange(
                    new EmailConfirmation(1, 1, 1, "f5d6377e-b170-4335-a425-25066a112987", "oldEmail@example.com", "newEmail@example.com", false, DateTime.UtcNow),
                    new EmailConfirmation(2, 3, 1, "4667a7d3-70c7-4da1-b472-8c34c8280723", "oldEmail@example.com", "newEmail@example.com", false, DateTime.UtcNow)
                );

                await databaseContext.SaveChangesAsync();
            }

            if (!await databaseContext.PasswordResets.AnyAsync())
            {
                databaseContext.PasswordResets.AddRange(
                    new PasswordReset(1, 1),
                    new PasswordReset(3, 1)
                );

                await databaseContext.SaveChangesAsync();
            }

            return databaseContext;
        }
    }
}
