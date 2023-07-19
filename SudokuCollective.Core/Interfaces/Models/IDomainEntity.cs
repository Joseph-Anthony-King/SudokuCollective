namespace SudokuCollective.Core.Interfaces.Models
{
    public interface IDomainEntity
    {
        int Id { get; set; }
        string ToJson();
        IDomainEntity Cast<T>();
    }
}
