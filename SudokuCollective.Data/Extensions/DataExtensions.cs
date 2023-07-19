using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models.Payloads;

[assembly:InternalsVisibleTo("SudokuCollective.Test")]
namespace SudokuCollective.Data.Extensions
{
    internal static class DataExtensions
    {
        internal static bool ConvertToPayloadSuccessful(this JsonElement element, Type type, out IPayload result)
        {
            result = null;

            if (type == typeof(AppPayload))
            {
                try
                {
                    result = new AppPayload()
                    {
                        Name = element.GetProperty("name").ToString(),
                        IsActive = Convert.ToBoolean(element.GetProperty("isActive").ToString()),
                        Environment = (ReleaseEnvironment)Convert.ToInt32(element.GetProperty("environment").ToString()),
                        PermitSuperUserAccess = Convert.ToBoolean(element.GetProperty("permitSuperUserAccess").ToString()),
                        PermitCollectiveLogins = Convert.ToBoolean(element.GetProperty("permitCollectiveLogins").ToString()),
                        DisableCustomUrls = Convert.ToBoolean(element.GetProperty("disableCustomUrls").ToString()),
                        CustomEmailConfirmationAction = element.GetProperty("customEmailConfirmationAction").ToString(),
                        CustomPasswordResetAction = element.GetProperty("customPasswordResetAction").ToString(),
                        UseCustomSMTPServer = Convert.ToBoolean(element.GetProperty("useCustomSMTPServer").ToString()),
                        TimeFrame = (TimeFrame)Convert.ToInt32(element.GetProperty("timeFrame").ToString()),
                        AccessDuration = Convert.ToInt32(element.GetProperty("accessDuration").ToString())
                    };

                    if (!string.IsNullOrEmpty(element.GetProperty("localUrl").ToString()))
                    {
                        ((AppPayload)result).LocalUrl = element.GetProperty("localUrl").ToString();
                    }

                    if (!string.IsNullOrEmpty(element.GetProperty("stagingUrl").ToString()))
                    {
                        ((AppPayload)result).StagingUrl = element.GetProperty("stagingUrl").ToString();
                    }

                    if (!string.IsNullOrEmpty(element.GetProperty("qaUrl").ToString()))
                    {
                        ((AppPayload)result).QaUrl = element.GetProperty("qaUrl").ToString();
                    }

                    if (!string.IsNullOrEmpty(element.GetProperty("prodUrl").ToString()))
                    {
                        ((AppPayload)result).ProdUrl = element.GetProperty("prodUrl").ToString();
                    }

                    if (((AppPayload)result).UseCustomSMTPServer && !string.IsNullOrEmpty(element.GetProperty("smtpServerSettings").ToString()))
                    {
                        ((AppPayload)result).SMTPServerSettings.SmtpServer = element
                            .GetProperty("smtpServerSettings")
                            .GetProperty("smtpServer").ToString();
                        ((AppPayload)result).SMTPServerSettings.Port = Convert.ToInt32(
                            element
                                .GetProperty("smtpServerSettings")
                                .GetProperty("port").ToString());
                        ((AppPayload)result).SMTPServerSettings.UserName = element
                            .GetProperty("smtpServerSettings")
                            .GetProperty("userName").ToString();
                        ((AppPayload)result).SMTPServerSettings.Password = element
                            .GetProperty("smtpServerSettings")
                            .GetProperty("password").ToString();
                        ((AppPayload)result).SMTPServerSettings.FromEmail = element
                            .GetProperty("smtpServerSettings")
                            .GetProperty("fromEmail").ToString();
                    }

                    return true;
                }
                catch (KeyNotFoundException)
                {
                    return false;
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else if (type == typeof(CreateDifficultyPayload))
            {
                try
                {
                    result = new CreateDifficultyPayload()
                    {
                        Name = element.GetProperty("name").ToString(),
                        DisplayName = element.GetProperty("displayName").ToString(),
                        DifficultyLevel = (DifficultyLevel)Convert.ToInt32(element.GetProperty("difficultyLevel").ToString())
                    };

                    return true;
                }
                catch (KeyNotFoundException)
                {
                    return false;
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else if (type == typeof(UpdateDifficultyPayload))
            {
                try
                {
                    result = new UpdateDifficultyPayload()
                    {
                        Id = Convert.ToInt32(element.GetProperty("id").ToString()),
                        Name = element.GetProperty("name").ToString(),
                        DisplayName = element.GetProperty("displayName").ToString()
                    };

                    return true;
                }
                catch (KeyNotFoundException)
                {
                    return false;
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else if (type == typeof(CreateGamePayload))
            {
                try
                {
                    result = new CreateGamePayload()
                    {
                        DifficultyId = Convert.ToInt32(element.GetProperty("difficultyId").ToString())
                    };

                    return true;
                }
                catch (KeyNotFoundException)
                {
                    return false;
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else if (type == typeof(GamesPayload))
            {
                try
                {
                    result = new GamesPayload()
                    {
                        UserId = Convert.ToInt32(element.GetProperty("userId").ToString())
                    };

                    return true;
                }
                catch (KeyNotFoundException)
                {
                    return false;
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else if (type == typeof(GamePayload))
            {
                try
                {
                    var cells = new List<SudokuCell>();

                    var cellsArray = element.GetProperty("sudokuCells");

                    foreach (JsonElement number in cellsArray.EnumerateArray())
                    {
                        var cell = new SudokuCell(
                            Convert.ToInt32(number.GetProperty("id").ToString()),
                            Convert.ToInt32(number.GetProperty("index").ToString()),
                            Convert.ToInt32(number.GetProperty("column").ToString()),
                            Convert.ToInt32(number.GetProperty("region").ToString()),
                            Convert.ToInt32(number.GetProperty("row").ToString()),
                            Convert.ToInt32(number.GetProperty("value").ToString()),
                            Convert.ToInt32(number.GetProperty("displayedValue").ToString()),
                            Convert.ToBoolean(number.GetProperty("hidden").ToString()),
                            Convert.ToInt32(number.GetProperty("sudokuMatrixId").ToString())
                            );

                        cells.Add(cell);
                    }
                    // TO DO: add logic to convert sudoku cells
                    result = new GamePayload()
                    {
                        SudokuCells = cells
                    };

                    return true;
                }
                catch (KeyNotFoundException)
                {
                    return false;
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else if (type == typeof(CreateRolePayload))
            {
                try
                {
                    result = new CreateRolePayload()
                    {
                        Name = element.GetProperty("name").ToString(),
                        RoleLevel = (RoleLevel)Convert.ToInt32(element.GetProperty("roleLevel").ToString())
                    };

                    return true;
                }
                catch (KeyNotFoundException)
                {
                    return false;
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else if (type == typeof(UpdateRolePayload))
            {
                try
                {
                    result = new UpdateRolePayload()
                    {
                        Id = Convert.ToInt32(element.GetProperty("id").ToString()),
                        Name = element.GetProperty("name").ToString()
                    };

                    return true;
                }
                catch (KeyNotFoundException)
                {
                    return false;
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else if (type == typeof(AddSolutionsPayload))
            {
                try
                {
                    result = new AddSolutionsPayload()
                    {
                        Limit = Convert.ToInt32(element.GetProperty("limit").ToString())
                    };

                    return true;
                }
                catch (KeyNotFoundException)
                {
                    return false;
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else if (type == typeof(SolutionPayload))
            {
                try
                {
                    var firstRow = new List<int>();
                    var secondRow = new List<int>();
                    var thirdRow = new List<int>();
                    var fourthRow = new List<int>();
                    var fifthRow = new List<int>();
                    var sixthRow = new List<int>();
                    var seventhRow = new List<int>();
                    var eighthRow = new List<int>();
                    var ninthRow = new List<int>();

                    var firstArray = element.GetProperty("firstRow");

                    foreach (JsonElement number in firstArray.EnumerateArray())
                    {
                        firstRow.Add(number.GetInt32());
                    }

                    var secondArray = element.GetProperty("secondRow");

                    foreach (JsonElement number in secondArray.EnumerateArray())
                    {
                        secondRow.Add(number.GetInt32());
                    }

                    var thirdArray = element.GetProperty("thirdRow");

                    foreach (JsonElement number in thirdArray.EnumerateArray())
                    {
                        thirdRow.Add(number.GetInt32());
                    }

                    var fourthArray = element.GetProperty("fourthRow");

                    foreach (JsonElement number in fourthArray.EnumerateArray())
                    {
                        fourthRow.Add(number.GetInt32());
                    }

                    var fifthArray = element.GetProperty("fifthRow");

                    foreach (JsonElement number in fifthArray.EnumerateArray())
                    {
                        fifthRow.Add(number.GetInt32());
                    }

                    var sixthArray = element.GetProperty("sixthRow");

                    foreach (JsonElement number in sixthArray.EnumerateArray())
                    {
                        sixthRow.Add(number.GetInt32());
                    }

                    var seventhArray = element.GetProperty("seventhRow");

                    foreach (JsonElement number in seventhArray.EnumerateArray())
                    {
                        seventhRow.Add(number.GetInt32());
                    }

                    var eighthArray = element.GetProperty("eighthRow");

                    foreach (JsonElement number in eighthArray.EnumerateArray())
                    {
                        eighthRow.Add(number.GetInt32());
                    }

                    var ninthArray = element.GetProperty("ninthRow");

                    foreach (JsonElement number in ninthArray.EnumerateArray())
                    {
                        ninthRow.Add(number.GetInt32());
                    }

                    result = new SolutionPayload()
                    {
                        FirstRow = firstRow,
                        SecondRow = secondRow,
                        ThirdRow = thirdRow,
                        FourthRow = fourthRow,
                        FifthRow = fifthRow,
                        SixthRow = sixthRow,
                        SeventhRow = seventhRow,
                        EighthRow = eighthRow,
                        NinthRow = ninthRow
                    };

                    return true;
                }
                catch (KeyNotFoundException)
                {
                    return false;
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else if (type == typeof(PasswordResetPayload))
            {
                try
                {
                    result = new PasswordResetPayload()
                    {
                        UserId = Convert.ToInt32(element.GetProperty("userId").ToString()),
                        NewPassword = element.GetProperty("newPassword").ToString()
                    };

                    return true;
                }
                catch (KeyNotFoundException)
                {
                    return false;
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else if (type == typeof(RequestPasswordResetPayload))
            {
                try
                {
                    result = new RequestPasswordResetPayload()
                    {
                        License = element.GetProperty("license").ToString(),
                        Email = element.GetProperty("email").ToString()
                    };

                    return true;
                }
                catch (KeyNotFoundException)
                {
                    return false;
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else if (type == typeof(UpdateUserPayload))
            {
                try
                {
                    result = new UpdateUserPayload()
                    {
                        UserName = element.GetProperty("userName").ToString(),
                        FirstName = element.GetProperty("firstName").ToString(),
                        LastName = element.GetProperty("lastName").ToString(),
                        NickName = element.GetProperty("nickName").ToString(),
                        Email = element.GetProperty("email").ToString()
                    };

                    return true;
                }
                catch (KeyNotFoundException)
                {
                    return false;
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else if (type == typeof(LicensePayload))
            {
                try
                {
                    result = new LicensePayload()
                    {
                        Name = element.GetProperty("name").ToString(),
                        OwnerId = Convert.ToInt32(element.GetProperty("ownerId").ToString()),
                        LocalUrl = element.GetProperty("localUrl").ToString(),
                        StagingUrl = element.GetProperty("stagingUrl").ToString(),
                        QaUrl = element.GetProperty("qaUrl").ToString(),
                        ProdUrl = element.GetProperty("prodUrl").ToString()
                    };

                    return true;
                }
                catch (KeyNotFoundException)
                {
                    return false;
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                try
                {
                    result = new UpdateUserRolePayload()
                    {
                        RoleIds = element.GetProperty("roleIds").ToString().ToCharArray().ToList().ConvertAll(c => Convert.ToInt32(c))
                    };

                    return true;
                }
                catch (KeyNotFoundException)
                {
                    return false;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }
}
