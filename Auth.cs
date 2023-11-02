using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace APSViewFnApp
{
    public class Auth
    {
        public record AccessToken(string access_token, long expires_in);

        private readonly APS _aps;

        public Auth(APS aps)
        {
            _aps = aps;
        }

        [FunctionName("Auth")]
        public async Task<AccessToken> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "auth/token")] HttpRequest req,
            ILogger log)
        {
            var token = await _aps.GetPublicToken();
            return new AccessToken(
                token.AccessToken,
                (long)Math.Round((token.ExpiresAt - DateTime.UtcNow).TotalSeconds)
            );
        }
    }
}
