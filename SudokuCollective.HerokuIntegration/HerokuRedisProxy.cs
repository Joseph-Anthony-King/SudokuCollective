using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SudokuCollective.HerokuIntegration.Models.Configuration;
using SudokuCollective.HerokuIntegration.Models.Requests;
using SudokuCollective.HerokuIntegration.Models.Responses;
using SudokuCollective.Logs.Utilities;

[assembly: InternalsVisibleTo("SudokuCollective.Cache")]
[assembly: InternalsVisibleTo("SudokuCollective.Test")]
namespace SudokuCollective.HerokuIntegration
{
    internal static class HerokuRedisProxy
    {
        #region Static Class Fields
        static readonly string configUrl = "https://api.heroku.com/addons/{0}/config";
        static readonly string configVarsUrl = "https://api.heroku.com/apps/{0}/config-vars";
        static readonly string dynosUrl = "https://api.heroku.com/apps/{0}/dynos";
        #endregion

        internal static async Task<HerokuRedisProxyResponse> UpdateConnectionStringAsync(HttpMessageHandler? httpMessageHandler = null, ILogger? logger = null)
        {
            try
            {
                var result = new HerokuRedisProxyResponse();

                var herokuApp = Environment.GetEnvironmentVariable("HEROKU:HEROKUAPP")!;
                var herokuRedisApp = Environment.GetEnvironmentVariable("HEROKU:HEROKUREDIS")!;

                using var httpClient = httpMessageHandler == null ? new HttpClient() : new HttpClient(httpMessageHandler);

                httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.heroku+json; version=3");
                httpClient.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", Environment.GetEnvironmentVariable("HEROKUCREDENTIALS:TOKEN")));
                httpClient.DefaultRequestHeaders.Add("user-agent", herokuApp);

                #region Obtain Heroku Redis Settings
                var getUrl = string.Format(configUrl, herokuRedisApp);

                using var getRequest = new HttpRequestMessage(HttpMethod.Get, getUrl);

                var getResponse = await httpClient.SendAsync(getRequest);

                // return false if get request fails
                if (getResponse.IsSuccessStatusCode == false)
                {
                    var response = await getResponse.Content.ReadFromJsonAsync<FailedResponse>();
                    var message = response!.Message + (response!.Url != null ? " For more info please check: " + response!.Url : "");

                    logger!.LogInformation(
                        LogsUtilities.GetHerokuProxyErrorEventId(),
                        message);

                    result.IsSuccessful = getResponse.IsSuccessStatusCode;
                    result.Message = message;

                    return result;
                }
                #endregion

                #region Get Redis Connection Values
                var redis_url = string.Empty;
                var redisConfigs = await getResponse.Content.ReadFromJsonAsync<List<ConfigVar>>();

                if (redisConfigs is not null)
                {
                    redis_url = redisConfigs.FirstOrDefault(config => config.Name.Equals("TLS_URL"))!.Value;
                }
                else
                {
                    throw new Exception("Heroku Addon Redis App returned null for redis connection values");
                }
                #endregion

                #region Update Heroku App Redis Connection
                var patchUrl = string.Format(configVarsUrl, herokuApp);

                using var patchRequest = new HttpRequestMessage(HttpMethod.Patch, patchUrl);

                using StringContent body = new(
                    JsonSerializer.Serialize<HerokuRedisConnectionStrings>(
                        new HerokuRedisConnectionStrings
                        {
                            RedisUrl = redis_url,
                        }),
                    Encoding.UTF8,
                    "application/json");

                patchRequest.Content = body;

                var patchResponse = await httpClient.SendAsync(patchRequest);

                // return false if patch request fails
                if (patchResponse.IsSuccessStatusCode == false)
                {
                    var response = await patchResponse.Content.ReadFromJsonAsync<FailedResponse>();
                    var message = response!.Message + (response!.Url != null ? " For more info please check: " + response!.Url : "");

                    logger!.LogInformation(
                        LogsUtilities.GetHerokuProxyErrorEventId(),
                        message);

                    result.IsSuccessful = patchResponse.IsSuccessStatusCode;
                    result.Message = message;

                    return result;
                }
                #endregion

                #region Restart All Dynos
                var deleteUrl = string.Format(dynosUrl, herokuApp);

                using var deleteMessage = new HttpRequestMessage(HttpMethod.Delete, deleteUrl);

                var deleteResponse = await httpClient.SendAsync(deleteMessage);

                // return false if delete request fails
                if (deleteResponse.IsSuccessStatusCode == false)
                {
                    var response = await deleteResponse.Content.ReadFromJsonAsync<FailedResponse>();
                    var message = response!.Message + (response!.Url != null ? " For more info please check: " + response!.Url : "");

                    logger!.LogInformation(
                        LogsUtilities.GetHerokuProxyErrorEventId(),
                        message);

                    result.IsSuccessful = deleteResponse.IsSuccessStatusCode;
                    result.Message = message;

                    return result;
                }

                result.IsSuccessful = deleteResponse.IsSuccessStatusCode;
                result.Message = "It was not possible to connect to the redis server, the redis server connections have been reset. Please resubmit your request.";

                return result;
                #endregion
            }
            catch (Exception ex) 
            {
                logger!.LogError(LogsUtilities.GetHerokuProxyErrorEventId(), ex.Message);
                throw;
            }
        }
    }
}
