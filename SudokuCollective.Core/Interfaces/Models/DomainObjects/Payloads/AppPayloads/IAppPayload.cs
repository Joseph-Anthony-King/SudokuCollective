using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;

namespace SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads
{
    public interface IAppPayload : IPayload
    {
        string Name { get; set; }
        string LocalUrl { get; set; }
        string StagingUrl { get; set; }
        string QaUrl { get; set; }
        string ProdUrl { get; set; }
        string SourceCodeUrl { get; set; }
        bool IsActive { get; set; }
        ReleaseEnvironment Environment { get; set; }
        bool PermitSuperUserAccess { get; set; }
        bool PermitCollectiveLogins { get; set; }
        bool DisableCustomUrls { get; set; }
        string CustomEmailConfirmationAction { get; set; }
        string CustomPasswordResetAction { get; set; }
        bool UseCustomSMTPServer { get; set; }
        ISMTPServerSettings SMTPServerSettings { get; set; }
        TimeFrame TimeFrame { get; set; }
        int AccessDuration { get; set; }
        bool DisplayInGallery { get; set; }
    }
}
