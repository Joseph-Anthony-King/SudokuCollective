using Moq;
using SudokuCollective.Core.Interfaces.Services;

namespace SudokuCollective.Test.Services
{
    public class MockedEmailService
    {
        internal Mock<IEmailService> SuccessfulRequest { get; set; }
        internal Mock<IEmailService> FailedRequest { get; set; }

        public MockedEmailService()
        {
            SuccessfulRequest = new Mock<IEmailService>();
            FailedRequest = new Mock<IEmailService>();

            SuccessfulRequest.Setup(service =>
                service.SendAsync(
                    It.IsAny<string>(), 
                    It.IsAny<string>(), 
                    It.IsAny<string>(),
                    It.IsAny<int>()))
                .ReturnsAsync(true);

            FailedRequest.Setup(service =>
                service.SendAsync(
                    It.IsAny<string>(), 
                    It.IsAny<string>(), 
                    It.IsAny<string>(),
                    It.IsAny<int>()))
                .ReturnsAsync(false);
        }
    }
}
