using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;

namespace APSViewFnApp
{
    public static class GetWebPage
    {
        [FunctionName("GetWebPage")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP GetWebPage request.");

            // log.LogInformation($"Client ID: {Environment.GetEnvironmentVariable("APS_CLIENT_ID")}");
            log.LogInformation($"Default Web Page: {Environment.GetEnvironmentVariable("DEFAULT_WEB_PAGE")}");

            HttpClient _httpClient = new HttpClient();

            var response = await
                _httpClient.GetAsync(Environment.GetEnvironmentVariable("DEFAULT_WEB_PAGE"));
            response.EnsureSuccessStatusCode();

            await using var ms = await response.Content.ReadAsStreamAsync();

            log.LogInformation($"Read file {ms.Length}");

            try
            {
                var response2 = new ContentResult
                {
                    Content = new StreamReader(ms).ReadToEnd(),
                    ContentType = "text/html"
                };

                log.LogInformation("About to return ContentResult");

                return response2;
            }
            catch (Exception ex)
            {
                log.LogInformation($"Error {ex.Message}");
            }

            return new OkObjectResult("No page found");

            //string name = req.Query["name"];

            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic data = JsonConvert.DeserializeObject(requestBody);
            //name = name ?? data?.name;

            //string responseMessage = string.IsNullOrEmpty(name)
            //    ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
            //    : $"Hello, {name}. This HTTP triggered function executed successfully.";

            //return new OkObjectResult(responseMessage);
        }
    }
}
