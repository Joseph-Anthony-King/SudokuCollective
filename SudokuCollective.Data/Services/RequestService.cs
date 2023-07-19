using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Logs.Models;

namespace SudokuCollective.Data.Services
{
    public class RequestService : IRequestService
    {
        IRequest IRequestService.Request
        {
            get
            {
                return Request;
            }
            set
            {
                Request = (Request)value;
            }
        }

        public Request Request { get; set; }

        public RequestService()
        {
            Request = new Request();
        }

        public IRequest Get()
        {
            return Request;
        }

        public void Update(IRequest request)
        {
            Request.License = request.License;
            Request.AppId = request.AppId;
            Request.RequestorId = request.RequestorId;
        }
    }
}
