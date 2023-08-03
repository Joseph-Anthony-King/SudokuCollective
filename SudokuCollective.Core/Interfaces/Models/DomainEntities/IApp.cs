using System;
using System.Collections.Generic;
using SudokuCollective.Core.Enums;

namespace SudokuCollective.Core.Interfaces.Models.DomainEntities
{
    public interface IApp : IDomainEntity
    {
        string Name { get; set; }
        string License { get; set; }
        int OwnerId { get; set; }
        string CreatedBy { get; set; }
        string LocalUrl { get; set; }
        string StagingUrl { get; set; }
        string QaUrl { get; set; }
        string ProdUrl { get; set; }
        string SourceCodeUrl {get; set; }
        bool IsActive { get; set; }
        ReleaseEnvironment Environment { get; set; }
        bool PermitSuperUserAccess { get; set; }
        bool PermitCollectiveLogins { get; set; }
        bool UseCustomEmailConfirmationAction { get; }
        bool UseCustomPasswordResetAction { get; }
        bool DisableCustomUrls { get; set; }
        string CustomEmailConfirmationAction { get; set; }
        string CustomPasswordResetAction { get; set; }
        bool UseCustomSMTPServer { get; set; }
        ISMTPServerSettings SMTPServerSettings { get; set; }
        int UserCount { get; }
        TimeFrame TimeFrame { get; set; }
        int AccessDuration { get; set; }
        bool DisplayInGallery { get; set; }
        DateTime DateCreated { get; set; }
        DateTime DateUpdated { get; set; }
        ICollection<IUserApp> UserApps { get; set; }
        ICollection<IUserDTO> Users { get; set; }
        void ActivateApp();
        void DeactivateApp();
        string GetLicense(int id, int ownerId);
        void NullifyLicense();
        void NullifySMTPServerSettings();
    }
}
