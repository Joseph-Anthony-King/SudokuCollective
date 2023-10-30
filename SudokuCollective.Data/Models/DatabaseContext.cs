using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;
using SudokuCollective.Core.Models;
using SudokuCollective.Encrypt;
using Microsoft.Extensions.Hosting;

namespace SudokuCollective.Data.Models
{
    public class DatabaseContext : DbContext
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly string _key;

        public DatabaseContext(
                DbContextOptions<DatabaseContext> options,
                IWebHostEnvironment environment,
                IConfiguration configuration
            ) : base(options)
        {
            _configuration = configuration;
            _environment = environment;
            
            _key = _environment.IsDevelopment() ?
                configuration.GetSection("EncryptionKey").Value :
                Environment.GetEnvironmentVariable("ENCRYPTION_KEY");
        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<Difficulty> Difficulties { get; set; }
        public DbSet<SudokuCell> SudokuCells { get; set; }
        public DbSet<SudokuMatrix> SudokuMatrices { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UsersRoles { get; set; }
        public DbSet<SudokuSolution> SudokuSolutions { get; set; }
        public DbSet<App> Apps { get; set; }
        public DbSet<SMTPServerSettings> SMTPServerSettings { get; set; }
        public DbSet<UserApp> UsersApps { get; set; }
        public DbSet<AppAdmin> AppAdmins { get; set; }
        public DbSet<EmailConfirmation> EmailConfirmations { get; set; }
        public DbSet<PasswordReset> PasswordResets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var encryptionConverter = new ValueConverter<string, string>(
                v => Encryption.EncryptString(v, _key),
                v => Encryption.DecryptString(v, _key)
            );

            var intListConverter = new ValueConverter<List<int>, string>(
                v => string.Join(",", v),
                v => v.Split(",", StringSplitOptions.RemoveEmptyEntries)
                    .Select(val => int.Parse(val))
                    .ToList()
            );

            var valueComparer = new ValueComparer<List<int>>(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()
            );

            modelBuilder.UseIdentityColumns();

            modelBuilder.Entity<Role>()
                .HasKey(role => role.Id);

            modelBuilder.Entity<Difficulty>()
                .HasKey(difficulty => difficulty.Id);

            modelBuilder.Entity<SudokuCell>()
                .HasKey(cell => cell.Id);

            modelBuilder.Entity<SudokuCell>()
                .HasOne(cell => cell.SudokuMatrix)
                .WithMany(matrix => matrix.SudokuCells)
                .HasForeignKey(cell => cell.SudokuMatrixId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SudokuCell>()
                .Ignore(cell => cell.AvailableValues);

            modelBuilder.Entity<SudokuMatrix>()
                .HasKey(matrix => matrix.Id);

            modelBuilder.Entity<SudokuMatrix>()
                .HasOne(matrix => matrix.Difficulty)
                .WithMany(difficulty => difficulty.Matrices)
                .HasForeignKey(matrix => matrix.DifficultyId);

            modelBuilder.Entity<SudokuMatrix>()
                .Ignore(matrix => matrix.FirstColumn)
                .Ignore(matrix => matrix.SecondColumn)
                .Ignore(matrix => matrix.ThirdColumn)
                .Ignore(matrix => matrix.FourthColumn)
                .Ignore(matrix => matrix.FifthColumn)
                .Ignore(matrix => matrix.SixthColumn)
                .Ignore(matrix => matrix.SeventhColumn)
                .Ignore(matrix => matrix.EighthColumn)
                .Ignore(matrix => matrix.NinthColumn)
                .Ignore(matrix => matrix.Columns)
                .Ignore(matrix => matrix.FirstRegion)
                .Ignore(matrix => matrix.SecondRegion)
                .Ignore(matrix => matrix.ThirdRegion)
                .Ignore(matrix => matrix.FourthRegion)
                .Ignore(matrix => matrix.FifthRegion)
                .Ignore(matrix => matrix.SixthRegion)
                .Ignore(matrix => matrix.SeventhRegion)
                .Ignore(matrix => matrix.EighthRegion)
                .Ignore(matrix => matrix.NinthRegion)
                .Ignore(matrix => matrix.Regions)
                .Ignore(matrix => matrix.FirstRow)
                .Ignore(matrix => matrix.SecondRow)
                .Ignore(matrix => matrix.ThirdRow)
                .Ignore(matrix => matrix.FourthRow)
                .Ignore(matrix => matrix.FifthRow)
                .Ignore(matrix => matrix.SixthRow)
                .Ignore(matrix => matrix.SeventhRow)
                .Ignore(matrix => matrix.EighthRow)
                .Ignore(matrix => matrix.NinthRow)
                .Ignore(matrix => matrix.Rows)
                .Ignore(matrix => matrix.FirstColumnValues)
                .Ignore(matrix => matrix.SecondColumnValues)
                .Ignore(matrix => matrix.ThirdColumnValues)
                .Ignore(matrix => matrix.FourthColumnValues)
                .Ignore(matrix => matrix.FifthColumnValues)
                .Ignore(matrix => matrix.SixthColumnValues)
                .Ignore(matrix => matrix.SeventhColumnValues)
                .Ignore(matrix => matrix.EighthColumnValues)
                .Ignore(matrix => matrix.NinthColumnValues)
                .Ignore(matrix => matrix.FirstRegionValues)
                .Ignore(matrix => matrix.SecondRegionValues)
                .Ignore(matrix => matrix.ThirdRegionValues)
                .Ignore(matrix => matrix.FourthRegionValues)
                .Ignore(matrix => matrix.FifthRegionValues)
                .Ignore(matrix => matrix.SixthRegionValues)
                .Ignore(matrix => matrix.SeventhRegionValues)
                .Ignore(matrix => matrix.EighthRegionValues)
                .Ignore(matrix => matrix.NinthRegionValues)
                .Ignore(matrix => matrix.FirstRowValues)
                .Ignore(matrix => matrix.SecondRowValues)
                .Ignore(matrix => matrix.ThirdRowValues)
                .Ignore(matrix => matrix.FourthRowValues)
                .Ignore(matrix => matrix.FifthRowValues)
                .Ignore(matrix => matrix.SixthRowValues)
                .Ignore(matrix => matrix.SeventhRowValues)
                .Ignore(matrix => matrix.EighthRowValues)
                .Ignore(matrix => matrix.NinthRowValues)
                .Ignore(matrix => matrix.FirstColumnDisplayedValues)
                .Ignore(matrix => matrix.SecondColumnDisplayedValues)
                .Ignore(matrix => matrix.ThirdColumnDisplayedValues)
                .Ignore(matrix => matrix.FourthColumnDisplayedValues)
                .Ignore(matrix => matrix.FifthColumnDisplayedValues)
                .Ignore(matrix => matrix.SixthColumnDisplayedValues)
                .Ignore(matrix => matrix.SeventhColumnDisplayedValues)
                .Ignore(matrix => matrix.EighthColumnDisplayedValues)
                .Ignore(matrix => matrix.NinthColumnDisplayedValues)
                .Ignore(matrix => matrix.FirstRegionDisplayedValues)
                .Ignore(matrix => matrix.SecondRegionDisplayedValues)
                .Ignore(matrix => matrix.ThirdRegionDisplayedValues)
                .Ignore(matrix => matrix.FourthRegionDisplayedValues)
                .Ignore(matrix => matrix.FifthRegionDisplayedValues)
                .Ignore(matrix => matrix.SixthRegionDisplayedValues)
                .Ignore(matrix => matrix.SeventhRegionDisplayedValues)
                .Ignore(matrix => matrix.EighthRegionDisplayedValues)
                .Ignore(matrix => matrix.NinthRegionDisplayedValues)
                .Ignore(matrix => matrix.FirstRowDisplayedValues)
                .Ignore(matrix => matrix.SecondRowDisplayedValues)
                .Ignore(matrix => matrix.ThirdRowDisplayedValues)
                .Ignore(matrix => matrix.FourthRowDisplayedValues)
                .Ignore(matrix => matrix.FifthRowDisplayedValues)
                .Ignore(matrix => matrix.SixthRowDisplayedValues)
                .Ignore(matrix => matrix.SeventhRowDisplayedValues)
                .Ignore(matrix => matrix.EighthRowDisplayedValues)
                .Ignore(matrix => matrix.NinthRowDisplayedValues)
                .Ignore(matrix => matrix.Stopwatch);

            modelBuilder.Entity<Game>()
                .HasKey(game => game.Id);

            modelBuilder.Entity<Game>()
                .HasOne(game => game.SudokuMatrix)
                .WithOne(matrix => matrix.Game)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Game>()
                .HasOne(game => game.SudokuSolution)
                .WithOne(solution => solution.Game);

            modelBuilder.Entity<Game>()
                .HasOne(game => game.User)
                .WithMany(user => user.Games)
                .HasForeignKey(game => game.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Game>()
                .Ignore(game => game.TimeToSolve);

            modelBuilder.Entity<User>()
                .HasKey(user => user.Id);

            modelBuilder.Entity<User>()
                .Property(user => user.UserName)
                .IsRequired();

            modelBuilder.Entity<User>()
                .HasIndex(user => user.UserName)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(user => user.FirstName)
                .HasConversion(encryptionConverter)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(user => user.LastName)
                .HasConversion(encryptionConverter)
                .IsRequired();

            modelBuilder.Entity<User>()
                .Property(user => user.Email)
                .HasConversion(encryptionConverter)
                .IsRequired();

            modelBuilder.Entity<User>()
                .HasIndex(user => user.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(user => user.Password);

            modelBuilder.Entity<User>()
                .Property(user => user.IsActive).HasField("_isActive")
                .IsRequired();

            modelBuilder.Entity<User>()
                .Ignore(user => user.IsAdmin)
                .Ignore(user => user.IsSuperUser);

            modelBuilder.Entity<UserRole>()
                .HasKey(userRole => userRole.Id);

            modelBuilder.Entity<UserRole>()
                .HasOne(userRole => userRole.User)
                .WithMany(user => user.Roles)
                .HasForeignKey(userRole => userRole.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(userRole => userRole.Role)
                .WithMany(role => role.Users)
                .HasForeignKey(userRole => userRole.RoleId);

            modelBuilder.Entity<SudokuSolution>()
                .HasKey(solution => solution.Id);

            modelBuilder.Entity<SudokuSolution>()
                .Property(solution => solution.SolutionList)
                .HasConversion(intListConverter)
                .Metadata
                .SetValueComparer(valueComparer);

            modelBuilder.Entity<SudokuSolution>()
                .Ignore(solution => solution.FirstRow)
                .Ignore(solution => solution.SecondRow)
                .Ignore(solution => solution.ThirdRow)
                .Ignore(solution => solution.FourthRow)
                .Ignore(solution => solution.FifthRow)
                .Ignore(solution => solution.SixthRow)
                .Ignore(solution => solution.SeventhRow)
                .Ignore(solution => solution.EighthRow)
                .Ignore(solution => solution.NinthRow)
                .Ignore(solution => solution.Game);

            modelBuilder.Entity<App>()
                .HasKey(app => app.Id);

            modelBuilder.Entity<App>()
                .Ignore(app => app.Users)
                .Ignore(app => app.UserCount)
                .Ignore(app => app.UseCustomEmailConfirmationAction)
                .Ignore(app => app.UseCustomPasswordResetAction);

            modelBuilder.Entity<App>()
                .Property(app => app.License)
                .HasConversion(encryptionConverter);

            modelBuilder.Entity<App>()
                .HasOne(app => app.SMTPServerSettings)
                .WithOne(smtpServerSettings => smtpServerSettings.App)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserApp>()
                .HasKey(userApp => userApp.Id);

            modelBuilder.Entity<UserApp>()
                .HasOne(userApp => userApp.User)
                .WithMany(user => user.Apps)
                .HasForeignKey(userApp => userApp.UserId);

            modelBuilder.Entity<UserApp>()
                .HasOne(userApp => userApp.App)
                .WithMany(app => app.UserApps)
                .HasForeignKey(userApp => userApp.AppId);

            modelBuilder.Entity<SMTPServerSettings>()
                .HasKey(settings => settings.Id);

            modelBuilder.Entity<SMTPServerSettings>()
                .Property(settings => settings.SmtpServer)
                .HasConversion(encryptionConverter);

            modelBuilder.Entity<SMTPServerSettings>()
                .Property(settings => settings.UserName)
                .HasConversion(encryptionConverter);

            modelBuilder.Entity<SMTPServerSettings>()
                .Property(settings => settings.Password)
                .HasConversion(encryptionConverter);

            modelBuilder.Entity<SMTPServerSettings>()
                .Property(settings => settings.FromEmail)
                .HasConversion(encryptionConverter);

            modelBuilder.Entity<SMTPServerSettings>()
                .Ignore(settings => settings.App);
            
            modelBuilder.Entity<AppAdmin>()
                .HasKey(appAdmin => appAdmin.Id);

            modelBuilder.Entity<EmailConfirmation>()
                .HasKey(emailConfirmation => emailConfirmation.Id);

            modelBuilder.Entity<EmailConfirmation>()
                .Property(emailConfirmation => emailConfirmation.Token)
                .IsRequired();

            modelBuilder.Entity<EmailConfirmation>()
                .HasIndex(emailConfirmation => emailConfirmation.Token)
                .IsUnique();

            modelBuilder.Entity<EmailConfirmation>()
                .Property(emailConfirmation => emailConfirmation.OldEmailAddress)
                .HasConversion(encryptionConverter);

            modelBuilder.Entity<EmailConfirmation>()
                .Property(emailConfirmation => emailConfirmation.NewEmailAddress)
                .HasConversion(encryptionConverter);

            modelBuilder.Entity<PasswordReset>()
                .HasKey(passwordReset => passwordReset.Id);
        }
    }
}
