using Moq;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Logs.Models;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.Services
{
    public class MockedRequestService
    {
        internal Mock<IRequestService> SuccessfulRequest { get; set; }
        internal Mock<IRequestService> FailedRequest { get; set; }

        public MockedRequestService()
        {
            SuccessfulRequest = new Mock<IRequestService>();
            FailedRequest = new Mock<IRequestService>();

            SuccessfulRequest.Setup(service =>
                service.Get())
                .Returns((Request)TestObjects.GetRequest());

            SuccessfulRequest.Setup(service =>
                service.Update(It.IsAny<IRequest>()));

            FailedRequest.Setup(service =>
                service.Get())
                .Returns(new Request());

            FailedRequest.Setup(service =>
                service.Update(It.IsAny<IRequest>()));
        }
    }
}
