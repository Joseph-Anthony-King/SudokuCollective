﻿using SudokuCollective.Core.Enums;

namespace SudokuCollective.Core.Interfaces.APIModels.RequestModels
{
    public interface IAppRequest : IBaseRequest
    {
        string Name { get; set; }
        string DevUrl { get; set; }
        string LiveUrl { get; set; }
        bool IsActive { get; set; }
        bool InDevelopment { get; set; }
        bool PermitSuperUserAccess { get; set; }
        bool PermitCollectiveLogins { get; set; }
        bool DisableCustomUrls { get; set; }
        string CustomEmailConfirmationAction { get; set; }
        string CustomPasswordResetAction { get; set; }
        TimeFrame TimeFrame { get; set; }
        int AccessDuration { get; set; }
    }
}
