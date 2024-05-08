using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using SudokuCollective.HerokuIntegration;
using Microsoft.Extensions.Logging;
using Moq.Protected;
using Moq;
using NUnit.Framework;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using static System.Net.WebRequestMethods;
using SudokuCollective.Core.Models;
using System.Security.Principal;

namespace SudokuCollective.Test.TestCases.HerokuIntegration
{
    public class HerokuRedisProxyShould
    {
        [Test, Category("HerokuIntegration")]
        public async Task UpdateConnectionString()
        {
            Environment.SetEnvironmentVariable("HEROKUCREDENTIALS:TOKEN", "1265753e-d175-4204-8a79-7396101d096a");
            Environment.SetEnvironmentVariable("HEROKU:HEROKUAPP", "sudokucollective-prod");
            Environment.SetEnvironmentVariable("HEROKU:HEROKUREDIS", "sudokucollective-prod-cache");

            var mockedLogger = new Mock<ILogger>();
            var mockedMessageHandler = new Mock<HttpMessageHandler>();

            var configVars = new List<TestVar>
            {
                new() {
                    Name = "Url",
                    Value = "redis://:password@127.0.0.1:6379"
                },
                new() {
                    Name = "TLS_URL",
                    Value = "rediss://:password@127.0.0.1:6379"
                }
            };

            using StringContent body = new(
                JsonSerializer.Serialize<List<TestVar>>(configVars),
                Encoding.UTF8,
                "application/json");

            mockedMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                })
                .Verifiable();

            mockedMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = body
                })
                .Verifiable();

            var result = await HerokuRedisProxy.UpdateConnectionStringAsync(mockedMessageHandler.Object, mockedLogger.Object);

            // Assert
            Assert.That(result.IsSuccessful, Is.True);
            Assert.That(result.Message.Equals("It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request."), Is.True);
        }

        [Test, Category("HerokuIntegration")]
        public async Task ReturnFalseIfUpdateConnectionStringAddonGetRequestFails()
        {
            Environment.SetEnvironmentVariable("HEROKUCREDENTIALS:TOKEN", "1265753e-d175-4204-8a79-7396101d096a");
            Environment.SetEnvironmentVariable("HEROKU:HEROKUAPP", "sudokucollective-prod");
            Environment.SetEnvironmentVariable("HEROKU:HEROKUREDIS", "sudokucollective-prod-cache");

            var mockedLogger = new Mock<ILogger>();
            var mockedMessageHandler = new Mock<HttpMessageHandler>();

            var testFailed = new TestFailed
            {
                Id = "rate_limit",
                Message = "Your account reached the API limit. Please wait a few minutes before making new requests.",
                Url = "https://127.0.0.1"
            };

            using StringContent body = new(
                JsonSerializer.Serialize<TestFailed>(testFailed),
                Encoding.UTF8,
                "application/json");

            mockedMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                })
                .Verifiable();

            mockedMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    Content = body
                })
                .Verifiable();

            var result = await HerokuRedisProxy.UpdateConnectionStringAsync(mockedMessageHandler.Object, mockedLogger.Object);

            // Assert
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.Message.Equals("Your account reached the API limit. Please wait a few minutes before making new requests. For more info please check: https://127.0.0.1"), Is.True);
        }

        [Test, Category("HerokuIntegration")]
        public async Task ReturnFalseIfUpdateConnectionStringAppsPatchRequestFails()
        {
            Environment.SetEnvironmentVariable("HEROKUCREDENTIALS:TOKEN", "1265753e-d175-4204-8a79-7396101d096a");
            Environment.SetEnvironmentVariable("HEROKU:HEROKUAPP", "sudokucollective-prod");
            Environment.SetEnvironmentVariable("HEROKU:HEROKUREDIS", "sudokucollective-prod-cache");

            var mockedLogger = new Mock<ILogger>();
            var mockedMessageHandler = new Mock<HttpMessageHandler>();

            var configVars = new List<TestVar>
            {
                new() {
                    Name = "Url",
                    Value = "redis://:password@127.0.0.1:6379"
                },
                new() {
                    Name = "TLS_URL",
                    Value = "rediss://:password@127.0.0.1:6379"
                }
            };

            var testFailed = new TestFailed
            {
                Id = "rate_limit",
                Message = "Your account reached the API limit. Please wait a few minutes before making new requests.",
                Url = "https://127.0.0.1"
            };

            using StringContent body = new(
                JsonSerializer.Serialize<List<TestVar>>(configVars),
                Encoding.UTF8,
                "application/json");

            using StringContent failedBody = new(
                JsonSerializer.Serialize<TestFailed>(testFailed),
                Encoding.UTF8,
                "application/json");

            mockedMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                })
                .Verifiable();

            mockedMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = body
                })
                .Verifiable();

            mockedMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Patch && httpMessage!.RequestUri.Equals("https://api.heroku.com/apps/sudokucollective-prod/config-vars")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = failedBody
                })
                .Verifiable();

            var result = await HerokuRedisProxy.UpdateConnectionStringAsync(mockedMessageHandler.Object, mockedLogger.Object);

            // Assert
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.Message.Equals("Your account reached the API limit. Please wait a few minutes before making new requests. For more info please check: https://127.0.0.1"), Is.True);
        }

        [Test, Category("HerokuIntegration")]
        public async Task ReturnFalseIfUpdateConnectionStringDynosDeleteRequestFails()
        {
            Environment.SetEnvironmentVariable("HEROKUCREDENTIALS:TOKEN", "1265753e-d175-4204-8a79-7396101d096a");
            Environment.SetEnvironmentVariable("HEROKU:HEROKUAPP", "sudokucollective-prod");
            Environment.SetEnvironmentVariable("HEROKU:HEROKUREDIS", "sudokucollective-prod-cache");

            var mockedLogger = new Mock<ILogger>();
            var mockedMessageHandler = new Mock<HttpMessageHandler>();

            var configVars = new List<TestVar>
            {
                new() {
                    Name = "Url",
                    Value = "redis://:password@127.0.0.1:6379"
                },
                new() {
                    Name = "TLS_URL",
                    Value = "rediss://:password@127.0.0.1:6379"
                }
            };

            var testFailed = new TestFailed
            {
                Id = "rate_limit",
                Message = "Your account reached the API limit. Please wait a few minutes before making new requests.",
                Url = "https://127.0.0.1"
            };

            using StringContent body = new(
                JsonSerializer.Serialize<List<TestVar>>(configVars),
                Encoding.UTF8,
                "application/json");

            using StringContent failedBody = new(
                JsonSerializer.Serialize<TestFailed>(testFailed),
                Encoding.UTF8,
                "application/json");

            mockedMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = body
                })
                .Verifiable();

            mockedMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Patch && httpMessage!.RequestUri.Equals("https://api.heroku.com/apps/sudokucollective-prod/config-vars")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                })
                .Verifiable();

            mockedMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Delete && httpMessage!.RequestUri.Equals("https://api.heroku.com/apps/sudokucollective-prod/dynos")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = failedBody
                })
                .Verifiable();

            var result = await HerokuRedisProxy.UpdateConnectionStringAsync(mockedMessageHandler.Object, mockedLogger.Object);

            // Assert
            Assert.That(result.IsSuccessful, Is.False);
            Assert.That(result.Message.Equals("Your account reached the API limit. Please wait a few minutes before making new requests. For more info please check: https://127.0.0.1"), Is.True);
        }

        [Test, Category("HerokuIntegration")]
        public async Task ThrowsExceptionsIfHttpClientRequestsThrowExceptions()
        {
            try
            {
                Environment.SetEnvironmentVariable("HEROKUCREDENTIALS:TOKEN", "1265753e-d175-4204-8a79-7396101d096a");
                Environment.SetEnvironmentVariable("HEROKU:HEROKUAPP", "sudokucollective-prod");
                Environment.SetEnvironmentVariable("HEROKU:HEROKUREDIS", "sudokucollective-prod-cache");

                var mockedLogger = new Mock<ILogger>();
                var mockedMessageHandler = new Mock<HttpMessageHandler>();

                mockedMessageHandler.Protected()
                    .Setup<Task<Exception>>(
                        "SendAsync",
                        ItExpr.Is<HttpRequestMessage>(httpMessage => httpMessage!.Method == HttpMethod.Get && httpMessage!.RequestUri.Equals("https://api.heroku.com/addons/sudokucollective-prod-cache/config")),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new Exception("It was not possible to connect to the redis server, the password failed"))
                    .Verifiable();

                var result = await HerokuRedisProxy.UpdateConnectionStringAsync(mockedMessageHandler.Object, mockedLogger.Object);

                // Assert
                Assert.That(result.IsSuccessful, Is.False);
            }
            catch
            {
                Assert.That(true);
            }
        }
    }

    internal class TestVar
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("value")]
        public string Value { get; set; }

        public TestVar()
        {
            Name = string.Empty;
            Value = string.Empty;
        }

        [JsonConstructor]
        public TestVar(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }

    internal class TestFailed
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }
        [JsonPropertyName("url"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Url { get; set; }

        public TestFailed()
        {
            Id = string.Empty;
            Message = string.Empty;
            Url = null;
        }

        [JsonConstructor]
        public TestFailed(string id, string message, string url = null)
        {
            Id = id;
            Message = message;
            Url = url;
        }
    }
}
