namespace SudokuCollective.Core.Interfaces.Models.DomainEntities
{
    public interface IUserRole : IDomainEntity
    {
        int UserId { get; set; }
        IUser User { get; set; }
        int RoleId { get; set; }
        IRole Role { get; set; }
    }
}
