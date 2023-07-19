using System;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace SudokuCollective.Test.HtttpContext
{
    internal class MockWebContext
    {
        public Mock<HttpContext> Http { get; private set; }

        public MockWebContext(string message = null)
        {
            Http = new Mock<HttpContext>(MockBehavior.Loose);

            ExceptionHandlerFeature exceptionHandler;

            if (!string.IsNullOrEmpty(message) && message.Equals("Parameter Invalid")) {

                exceptionHandler = new ExceptionHandlerFeature
                {
                    Error = new ArgumentException(message)
                };
            } 
            else 
            {
                exceptionHandler = new ExceptionHandlerFeature
                {
                    Error = new NullReferenceException("Null Reference Exception")
                };
            }

            Http.SetupAllProperties();
            Http.Setup(h => h.Features.Get<IExceptionHandlerFeature>()).Returns(exceptionHandler);
        }

        public static ControllerContext BasicContext(string message = null)
        {
            return new ControllerContext
            {
                HttpContext = new MockWebContext(message).Http.Object
            };
        }

    }
}
