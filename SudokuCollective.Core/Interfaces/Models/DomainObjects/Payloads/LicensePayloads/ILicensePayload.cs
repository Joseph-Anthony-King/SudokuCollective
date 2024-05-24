namespace SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads
{
    public interface ILicensePayload : IPayload
    {
        string Name { get; set; }
        int OwnerId { get; set; }
        string LocalUrl { get; set; }
        string TestUrl { get; set; }
        string StagingUrl { get; set; }
        string ProdUrl { get; set; }
        string SourceCodeUrl { get; set; }
    }
}
