using System.Threading.Tasks;

namespace SudokuCollective.Core.Interfaces.Services
{
    public interface IEmailService : IService
    {
        Task<bool> SendAsync(string to, string subject, string html, int appId);
    }
}
