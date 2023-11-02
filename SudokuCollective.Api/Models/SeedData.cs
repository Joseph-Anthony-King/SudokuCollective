using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models;

namespace SudokuCollective.Api.Models
{
    /// <summary>
    /// Seed Data Class...
    /// </summary>
    public class SeedData
    {
        /// <summary>
        /// This method is run if the database contains no records.
        /// </summary>
        public static void EnsurePopulated(
            IApplicationBuilder app, 
            IConfiguration config,
            IWebHostEnvironment env)
        {
            using (var servicesScope = app.ApplicationServices.CreateScope())
            {
                var createdDate = DateTime.UtcNow;

                DatabaseContext context = servicesScope.ServiceProvider.GetRequiredService<DatabaseContext>();
                context.Database.Migrate();

                if (!context.Roles.Any())
                {
                    context.Roles.Add(
                        new Role(0, "Null", RoleLevel.NULL)
                    );
                    context.SaveChanges();

                    context.Roles.Add(
                        new Role(0, "Super User", RoleLevel.SUPERUSER)
                    );
                    context.SaveChanges();

                    context.Roles.Add(
                        new Role(0, "Admin", RoleLevel.ADMIN)
                    );
                    context.SaveChanges();

                    context.Roles.Add(
                        new Role(0, "User", RoleLevel.USER)
                    );
                    context.SaveChanges();
                }

                if (!context.Difficulties.Any())
                {
                    context.Difficulties.Add(
                        new Difficulty(0, "Null", "Null", DifficultyLevel.NULL)
                    );
                    context.SaveChanges();

                    context.Difficulties.Add(
                        new Difficulty(0, "Test", "Test", DifficultyLevel.TEST)
                    );
                    context.SaveChanges();

                    context.Difficulties.Add(
                        new Difficulty(0, "Easy", "Steady Sloth", DifficultyLevel.EASY)
                    );
                    context.SaveChanges();

                    context.Difficulties.Add(
                        new Difficulty(0, "Medium", "Leaping Lemur", DifficultyLevel.MEDIUM)
                    );
                    context.SaveChanges();

                    context.Difficulties.Add(
                        new Difficulty(0, "Hard", "Mighty Mountain Lion", DifficultyLevel.HARD)
                    );
                    context.SaveChanges();

                    context.Difficulties.Add(
                        new Difficulty(0, "Evil", "Sneaky Shark", DifficultyLevel.EVIL)
                    );
                    context.SaveChanges();
                }

                if (!context.Users.Any())
                {
                    var salt = BCrypt.Net.BCrypt.GenerateSalt();

                    context.Users.Add(
                        new User(
                            0,
                            env.IsDevelopment() ? 
                                config.GetValue<string>("DefaultUserAccounts:SuperUser:UserName") : 
                                Environment.GetEnvironmentVariable("SUPER_USER_USERNAME"),
                            env.IsDevelopment() ? 
                                config.GetValue<string>("DefaultUserAccounts:SuperUser:FirstName") : 
                                Environment.GetEnvironmentVariable("SUPER_USER_FIRSTNAME"),
                            env.IsDevelopment() ? 
                                config.GetValue<string>("DefaultUserAccounts:SuperUser:LastName") : 
                                Environment.GetEnvironmentVariable("SUPER_USER_LASTNAME"),
                            env.IsDevelopment() ? 
                                config.GetValue<string>("DefaultUserAccounts:SuperUser:NickName") : 
                                Environment.GetEnvironmentVariable("SUPER_USER_NICKNAME"),
                            env.IsDevelopment() ? 
                                config.GetValue<string>("DefaultUserAccounts:SuperUser:Email") : 
                                Environment.GetEnvironmentVariable("SUPER_USER_EMAIL"),
                            true,
                            false,
                            env.IsDevelopment() ? 
                                BCrypt.Net.BCrypt.HashPassword(config.GetValue<string>("DefaultUserAccounts:SuperUser:Password", salt)) : 
                                BCrypt.Net.BCrypt.HashPassword(config.GetValue<string>("SUPER_USER_PASSWORD", salt)),
                            false,
                            true,
                            createdDate,
                            DateTime.MinValue)
                    );
                    context.SaveChanges();

                    context.Users.AddRange(
                        new User(
                            0,
                            env.IsDevelopment() ? 
                                config.GetValue<string>("DefaultUserAccounts:AdminUser:UserName") : 
                                Environment.GetEnvironmentVariable("ADMIN_USER_USERNAME"),
                            env.IsDevelopment() ? 
                                config.GetValue<string>("DefaultUserAccounts:AdminUser:FirstName") : 
                                Environment.GetEnvironmentVariable("ADMIN_USER_FIRSTNAME"),
                            env.IsDevelopment() ? 
                                config.GetValue<string>("DefaultUserAccounts:AdminUser:LastName") : 
                                Environment.GetEnvironmentVariable("ADMIN_USER_LASTNAME"),
                            env.IsDevelopment() ? 
                                config.GetValue<string>("DefaultUserAccounts:AdminUser:NickName") : 
                                Environment.GetEnvironmentVariable("ADMIN_USER_NICKNAME"),
                            env.IsDevelopment() ? 
                                config.GetValue<string>("DefaultUserAccounts:AdminUser:Email") : 
                                Environment.GetEnvironmentVariable("ADMIN_USER_EMAIL"),
                            true,
                            false,
                            env.IsDevelopment() ? 
                                BCrypt.Net.BCrypt.HashPassword(config.GetValue<string>("DefaultUserAccounts:AdminUser:Password", salt)) : 
                                BCrypt.Net.BCrypt.HashPassword(config.GetValue<string>("ADMIN_USER_PASSWORD", salt)),
                            false,
                            true,
                            createdDate,
                            DateTime.MinValue)
                    );
                    context.SaveChanges();

                }
                if (!context.Apps.Any())
                {
                    context.Apps.Add(
                        new App(
                            0,
                            env.IsDevelopment() ? 
                                config.GetValue<string>("DefaultAdminApp:Name") : 
                                Environment.GetEnvironmentVariable("ADMIN_APP_NAME"),
                            env.IsDevelopment() ? 
                                config.GetValue<string>("DefaultAdminApp:License") : 
                                Environment.GetEnvironmentVariable("ADMIN_APP_LICENSE"),
                            1,
                            env.IsDevelopment() ? 
                                config.GetValue<string>("DefaultUserAccounts:SuperUser:UserName") : 
                                Environment.GetEnvironmentVariable("SUPER_USER_USERNAME"),
                            env.IsDevelopment() ? 
                                config.GetValue<string>("DefaultAdminApp:LocalUrl") : 
                                Environment.GetEnvironmentVariable("ADMIN_APP_LOCAL_URL"),
                            env.IsDevelopment() ?
                                config.GetValue<string>("DefaultAdminApp:QaUrl") :
                                Environment.GetEnvironmentVariable("ADMIN_APP_QA_URL"),
                            env.IsDevelopment() ? 
                                config.GetValue<string>("DefaultAdminApp:StagingUrl") : 
                                Environment.GetEnvironmentVariable("ADMIN_APP_STAGING_URL"),
                            env.IsDevelopment() ? 
                                config.GetValue<string>("DefaultAdminApp:ProdUrl") : 
                                Environment.GetEnvironmentVariable("ADMIN_APP_PROD_URL"),
                            env.IsDevelopment() ?
                                config.GetValue<string>("DefaultAdminApp:SourceCodeUrl") :
                                Environment.GetEnvironmentVariable("ADMIN_APP_SOURCE_CODE_URL"),
                            true,
                            true,
                            true,
                            env.IsDevelopment() ?
                                config.GetValue<ReleaseEnvironment>("DefaultAdminApp:ReleaseEnvironment") :
                                Enum.Parse<ReleaseEnvironment>(Environment.GetEnvironmentVariable("ADMIN_APP_RELEASE_ENVIRONMENT")),
                            env.IsDevelopment() ?
                                config.GetValue<bool>("DefaultAdminApp:DisableCustomUrls") :
                                bool.Parse(Environment.GetEnvironmentVariable("ADMIN_APP_DISABLE_CUSTOM_URLS")),
                            env.IsDevelopment() ? 
                                config.GetValue<string>("DefaultAdminApp:CustomEmailAction") : 
                                Environment.GetEnvironmentVariable("ADMIN_APP_CUSTOM_EMAIL_ACTION"),
                            env.IsDevelopment() ? 
                                config.GetValue<string>("DefaultAdminApp:CustomPasswordAction") : 
                                Environment.GetEnvironmentVariable("ADMIN_APP_CUSTOM_PASSWORD_ACTION"),
                            false,
                            env.IsDevelopment() ?
                                config.GetValue<TimeFrame>("DefaultAdminApp:TimeFrame") :
                                Enum.Parse<TimeFrame>(Environment.GetEnvironmentVariable("ADMIN_APP_TIME_FRAME")),
                            env.IsDevelopment() ?
                                config.GetValue<int>("DefaultAdminApp:AccessDuration") :
                                int.Parse(Environment.GetEnvironmentVariable("ADMIN_APP_ACCESS_DURATION")),
                            true,
                            createdDate,
                            DateTime.MinValue)
                    );
                    context.SaveChanges();

                    context.Apps.Add(
                        new App(
                            0,
                            env.IsDevelopment() ? 
                                config.GetValue<string>("DefaultClientApp:Name") : 
                                Environment.GetEnvironmentVariable("CLIENT_APP_NAME"),
                            env.IsDevelopment() ? 
                                config.GetValue<string>("DefaultClientApp:License") : 
                                Environment.GetEnvironmentVariable("CLIENT_APP_LICENSE"),
                            2,
                            env.IsDevelopment() ? 
                                config.GetValue<string>("DefaultUserAccounts:AdminUser:UserName") : 
                                Environment.GetEnvironmentVariable("ADMIN_USER_USERNAME"),
                            env.IsDevelopment() ? 
                                config.GetValue<string>("DefaultClientApp:LocalUrl") : 
                                Environment.GetEnvironmentVariable("CLIENT_APP_LOCAL_URL"),
                            env.IsDevelopment() ? 
                                config.GetValue<string>("DefaultClientApp:QaUrl") : 
                                Environment.GetEnvironmentVariable("CLIENT_APP_QA_URL"),
                            env.IsDevelopment() ?
                                config.GetValue<string>("DefaultClientApp:StagingUrl") :
                                Environment.GetEnvironmentVariable("CLIENT_APP_STAGING_URL"),
                            env.IsDevelopment() ? 
                                config.GetValue<string>("DefaultClientApp:ProdUrl") : 
                                Environment.GetEnvironmentVariable("CLIENT_APP_PROD_URL"),
                            env.IsDevelopment() ?
                                config.GetValue<string>("DefaultClientApp:SourceCodeUrl") :
                                Environment.GetEnvironmentVariable("CLIENT_APP_SOURCE_CODE_URL"),
                            false,
                            true,
                            true,
                            env.IsDevelopment() ?
                                config.GetValue<ReleaseEnvironment>("DefaultClientApp:ReleaseEnvironment") :
                                Enum.Parse<ReleaseEnvironment>(Environment.GetEnvironmentVariable("CLIENT_APP_RELEASE_ENVIRONMENT")),
                            env.IsDevelopment() ?
                                config.GetValue<bool>("DefaultClientApp:DisableCustomUrls") :
                                bool.Parse(Environment.GetEnvironmentVariable("CLIENT_APP_DISABLE_CUSTOM_URLS")),
                            env.IsDevelopment() ? 
                                config.GetValue<string>("DefaultClientApp:CustomEmailAction") : 
                                Environment.GetEnvironmentVariable("CLIENT_APP_CUSTOM_EMAIL_ACTION"),
                            env.IsDevelopment() ? 
                                config.GetValue<string>("DefaultClientApp:CustomPasswordAction") : 
                                Environment.GetEnvironmentVariable("CLIENT_APP_CUSTOM_PASSWORD_ACTION"),
                            false,
                            env.IsDevelopment() ?
                                config.GetValue<TimeFrame>("DefaultClientApp:TimeFrame") : 
                                Enum.Parse<TimeFrame>(Environment.GetEnvironmentVariable("CLIENT_APP_TIME_FRAME")),
                            env.IsDevelopment() ?
                                config.GetValue<int>("DefaultClientApp:AccessDuration") : 
                                int.Parse(Environment.GetEnvironmentVariable("CLIENT_APP_ACCESS_DURATION")),
                            false,
                            createdDate,
                            DateTime.MinValue)
                    );
                    context.SaveChanges();

                    context.Apps.Add(
                        new App(
                            0,
                            env.IsDevelopment() ? 
                                config.GetValue<string>("DefaultSandboxApp:Name") : 
                                Environment.GetEnvironmentVariable("SANDBOX_APP_NAME"),
                            env.IsDevelopment() ? 
                                config.GetValue<string>("DefaultSandboxApp:License") : 
                                Environment.GetEnvironmentVariable("SANDBOX_APP_LICENSE"),
                            1,
                            env.IsDevelopment() ? 
                                config.GetValue<string>("DefaultUserAccounts:SuperUser:UserName") : 
                                Environment.GetEnvironmentVariable("SUPER_USER_USERNAME"),
                            env.IsDevelopment() ? 
                                config.GetValue<string>("DefaultSandboxApp:LocalUrl") : 
                                Environment.GetEnvironmentVariable("SANDBOX_APP_LOCAL_URL"),
                            env.IsDevelopment() ? 
                                config.GetValue<string>("DefaultSandboxApp:QaUrl") : 
                                Environment.GetEnvironmentVariable("SANDBOX_APP_QA_URL"),
                            env.IsDevelopment() ?
                                config.GetValue<string>("DefaultSandboxApp:StagingUrl") :
                                Environment.GetEnvironmentVariable("SANDBOX_APP_STAGING_URL"),
                            env.IsDevelopment() ? 
                                config.GetValue<string>("DefaultSandboxApp:ProdUrl") : 
                                Environment.GetEnvironmentVariable("SANDBOX_APP_PROD_URL"),
                            string.Empty,
                            true,
                            true,
                            true,
                            ReleaseEnvironment.LOCAL,
                            true,
                            string.Empty,
                            string.Empty,
                            false,
                            TimeFrame.DAYS,
                            1,
                            false,
                            createdDate,
                            DateTime.MinValue)
                    );
                    context.SaveChanges();
                }

                if (!context.SMTPServerSettings.Any())
                {
                    context.SMTPServerSettings.Add(
                        new SMTPServerSettings(
                            0, 
                            string.Empty, 
                            0, 
                            string.Empty, 
                            string.Empty, 
                            string.Empty, 
                            1)
                    );
                    context.SaveChanges();

                    context.SMTPServerSettings.Add(
                        new SMTPServerSettings(
                            0, 
                            string.Empty, 
                            0, 
                            string.Empty, 
                            string.Empty, 
                            string.Empty, 
                            2)
                    );
                    context.SaveChanges();

                    context.SMTPServerSettings.Add(
                        new SMTPServerSettings(
                            0, 
                            string.Empty, 
                            0, 
                            string.Empty, 
                            string.Empty, 
                            string.Empty, 
                            3)
                    );
                    context.SaveChanges();
                }
                

                if (!context.UsersApps.Any())
                {
                    context.UsersApps.Add(
                        new UserApp(0, 1, 1)
                    );
                    context.SaveChanges();

                    context.UsersApps.Add(
                        new UserApp(0, 2, 1)
                    );
                    context.SaveChanges();

                    context.UsersApps.Add(
                        new UserApp(0, 1, 2)
                    );
                    context.SaveChanges();

                    context.UsersApps.Add(
                        new UserApp(0, 2, 2)
                    );
                    context.SaveChanges();

                    context.UsersApps.Add(
                        new UserApp(0, 1, 3)
                    );
                    context.SaveChanges();

                    context.UsersApps.Add(
                        new UserApp(0, 2, 3)
                    );
                    context.SaveChanges();
                }

                if (!context.UsersRoles.Any())
                {
                    context.UsersRoles.Add(
                        new UserRole(0, 1, 2)
                    );
                    context.SaveChanges();

                    context.UsersRoles.Add(
                        new UserRole(0, 1, 3)
                    );
                    context.SaveChanges();

                    context.UsersRoles.Add(
                        new UserRole(0, 1, 4)
                    );
                    context.SaveChanges();

                    context.UsersRoles.Add(
                        new UserRole(0, 2, 3)
                    );
                    context.SaveChanges();

                    context.UsersRoles.Add(
                        new UserRole(0, 2, 4)
                    );
                    context.SaveChanges();
                }

                if (!context.AppAdmins.Any())
                {
                    context.AppAdmins.Add(
                        new AppAdmin(0, 1, 1, true)
                    );
                    context.SaveChanges();

                    context.AppAdmins.Add(
                        new AppAdmin(0, 1, 2, true)
                    );
                    context.SaveChanges();

                    context.AppAdmins.Add(
                        new AppAdmin(0, 2, 2, true)
                    );
                    context.SaveChanges();

                    context.AppAdmins.Add(
                        new AppAdmin(0, 3, 1, true)
                    );
                    context.SaveChanges();
                }
            }
        }
    }
}
