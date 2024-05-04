using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SudokuCollective.Heroku.Models;
using SudokuCollective.Logs.Utilities;

[assembly: InternalsVisibleTo("SudokuCollective.Api")]
[assembly: InternalsVisibleTo("SudokuCollective.Test")]
namespace SudokuCollective.Heroku
{
    public static class HerokuProxy
    {
        #region Static Class Fields
        static readonly string configUrl = "https://api.heroku.com/addons/{0}/config";
        static readonly string configVarsUrl = "https://api.heroku.com/apps/{0}/config-vars";
        static readonly string dynosUrl = "https://api.heroku.com/apps/{0}/dynos";
        #endregion

        public static async Task<bool> UpdateHerokuRedisConnectionStringAsync(bool isStaging, ILogger logger, HttpMessageHandler? httpMessageHandler = null)
        {
            string herokuApp, herokuRedisApp;

            if (isStaging)
            {
                herokuApp = Environment.GetEnvironmentVariable("HEROKU:STAGING:HEROKUAPP")!;
                herokuRedisApp = Environment.GetEnvironmentVariable("HEROKU:STAGING:HEROKUREDIS")!;
            }
            else
            {
                herokuApp = Environment.GetEnvironmentVariable("HEROKU:PROD:HEROKUAPP")!;
                herokuRedisApp = Environment.GetEnvironmentVariable("HEROKU:PROD:HEROKUREDIS")!;
            }

            using var httpClient = httpMessageHandler == null ? new HttpClient() : new HttpClient(httpMessageHandler);

            #region Obtain Heroku Redis Settings
            var getUrl = string.Format(configUrl, herokuRedisApp);

            using var getRequest = new HttpRequestMessage(HttpMethod.Get, getUrl);

            getRequest.Headers.Add("Accept", "application/vnd.heroku+json; version=3");
            getRequest.Headers.Add("Authorization", string.Format("Bearer {0}", Environment.GetEnvironmentVariable("HEROKUCREDENTIALS:TOKEN")));

            var getResponse = await httpClient.SendAsync(getRequest);

            // return false if get request fails
            if (getResponse.IsSuccessStatusCode == false)
            {
                var response = await getResponse.Content.ReadFromJsonAsync<FailedResponse>();
                logger.LogInformation(LogsUtilities.GetHerokuProxyErrorEventId(), response!.Message);
                return false;
            }
            #endregion

            #region Get Redis Connection Values
            var redisConfigs = await getResponse.Content.ReadFromJsonAsync<List<ConfigVar>>();
            var redis_tls_url = redisConfigs.FirstOrDefault(config => config.Name.Equals("TLS_URL")).Value;
            var redis_url = redis_tls_url.Replace("rediss", "redis");
            #endregion

            #region Update Heroku App Redis Connection
            var patchUrl = string.Format(configVarsUrl, herokuApp);

            using var patchRequest = new HttpRequestMessage(HttpMethod.Patch, patchUrl);

            patchRequest.Headers.Add("Accept", "application/vnd.heroku+json; version=3");
            patchRequest.Headers.Add("Authorization", string.Format("Bearer {0}", Environment.GetEnvironmentVariable("HEROKUCREDENTIALS:TOKEN")));

            using StringContent body = new(
                JsonSerializer.Serialize<HerokuConnectionStrings>(
                    new HerokuConnectionStrings
                    {
                        RedisTlsUrl = redis_tls_url,
                        RedisUrl = redis_url,
                    }),
                Encoding.UTF8,
                "application/json");

            patchRequest.Content = body;

            var patchResponse = await httpClient.SendAsync(patchRequest);

            // return false if patch request fails
            if (patchResponse.IsSuccessStatusCode == false)
            {
                var response = await getResponse.Content.ReadFromJsonAsync<FailedResponse>();
                logger.LogInformation(LogsUtilities.GetHerokuProxyErrorEventId(), response!.Message);
                return false;
            }
            #endregion

            #region Restart All Dynos
            var deleteUrl = string.Format(dynosUrl, herokuApp);

            using var deleteMessage = new HttpRequestMessage(HttpMethod.Delete, deleteUrl);

            var deleteResponse = await httpClient.SendAsync(deleteMessage);

            deleteResponse.Headers.Add("Accept", "application/vnd.heroku+json; version=3");
            deleteResponse.Headers.Add("Authorization", string.Format("Bearer {0}", Environment.GetEnvironmentVariable("HEROKUCREDENTIALS:TOKEN")));

            if (deleteResponse.IsSuccessStatusCode)
            {
                return deleteResponse.IsSuccessStatusCode;
            }
            else
            {
                var response = await deleteResponse.Content.ReadFromJsonAsync<FailedResponse>();
                logger.LogInformation(LogsUtilities.GetHerokuProxyErrorEventId(), response!.Message);

                return false;
            }
            #endregion
        }
    }
}
