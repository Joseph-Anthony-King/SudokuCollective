namespace SudokuCollective.Core.Interfaces.Models.DomainEntities
{
    public interface IAppAdmin : IDomainEntity
    {
        int AppId { get; set; }
        int UserId { get; set; }
        bool IsActive { get; set; }
    }
}
