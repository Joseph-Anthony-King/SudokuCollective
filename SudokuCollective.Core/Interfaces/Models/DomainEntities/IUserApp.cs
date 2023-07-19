namespace SudokuCollective.Core.Interfaces.Models.DomainEntities
{
    public interface IUserApp : IDomainEntity
    {
        int UserId { get; set; }
        IUser User { get; set; }
        int AppId { get; set; }
        IApp App { get; set; }
    }
}
