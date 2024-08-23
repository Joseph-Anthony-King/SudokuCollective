using Microsoft.EntityFrameworkCore;
using SudokuCollective.Core.Models;

namespace SudokuCollective.Core.Interfaces.ServiceModels
{
    public interface IDatabaseContext
    {
        DbSet<App> Apps { get; set; }
        DbSet<AppAdmin> AppAdmins { get; set; }
        DbSet<Difficulty> Difficulties { get; set; }
        DbSet<EmailConfirmation> EmailConfirmations { get; set; }
        DbSet<Game> Games { get; set; }
        DbSet<PasswordReset> PasswordResets { get; set; }
        DbSet<Role> Roles { get; set; }
        DbSet<SMTPServerSettings> SMTPServerSettings { get; set; }
        DbSet<SudokuCell> SudokuCells { get; set; }
        DbSet<SudokuMatrix> SudokuMatrices { get; set; }
        DbSet<SudokuSolution> SudokuSolutions { get; set; }
        DbSet<User> Users { get; set; }
        DbSet<UserRole> UsersRoles { get; set; }
        DbSet<UserApp> UsersApps { get; set; }
    }
}
