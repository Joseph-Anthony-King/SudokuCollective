using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using SudokuCollective.Api.Middleware;

namespace SudokuCollective.Test.TestCases.Middleware
{
    public class ExpiredTokenMiddlewareShould
    {
        private ExpiredTokenMiddleware sut;

        [SetUp]
        public void Setup()
        {
            static Task next(HttpContext hc) => Task.CompletedTask;
            sut = new ExpiredTokenMiddleware(next);
        }

        [Test, Category("Middleware")]
        public async Task RunSuccessfullyIfTokenIsntExpired()
        {
            var result = true;
            try
            {
                var headers = new Mock<IHeaderDictionary>();
                var responseMock = new Mock<HttpResponse>();
                responseMock.Setup(res => res.Headers).Returns(headers.Object);
                var httpContext = new Mock<HttpContext>();
                httpContext.Setup(x => x.Response).Returns(responseMock.Object);

                // Act
                await sut.Invoke(httpContext.Object);
            }
            catch
            {
                result = false;
            }

            // Assert
            Assert.That(result, Is.True);
        }

        [Test, Category("Middleware")]
        public async Task IssueCustomResultIfTokenExpired()
        {
            var result = true;
            try
            {
                var headers = new HeaderDictionary(new Dictionary<String, StringValues>
                    {
                        { "Token-Expired", "true"}
                    }) as IHeaderDictionary;
                var responseMock = new Mock<HttpResponse>();
                responseMock.Setup(res => res.Headers).Returns(headers);
                responseMock.Setup(res => res.Body).Returns(new MemoryStream());
                var httpContext = new Mock<HttpContext>();
                httpContext.Setup(x => x.Response).Returns(responseMock.Object);

                // Act
                await sut.Invoke(httpContext.Object);
            }
            catch
            {
                result = false;
            }

            // Assert
            Assert.That(result, Is.True);
        }
    }
}
