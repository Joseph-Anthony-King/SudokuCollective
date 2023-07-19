using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;

namespace SudokuCollective.Core.Interfaces.Services
{
    public interface IRequestService : IService
    {
        IRequest Request { get; set; }
        IRequest Get();
        void Update(IRequest request);
    }
}
