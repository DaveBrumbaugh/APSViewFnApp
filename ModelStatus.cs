using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Autodesk.Forge.Client;
using System.Collections.Generic;
using System.Linq;

namespace APSViewFnApp
{
    public class ModelStatus
    {
        private readonly APS _aps;

        public ModelStatus(APS aps)
        {
            _aps = aps;
        }

        [FunctionName("ModelStatus")]
        public async Task<TranslationStatus> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            string urn = req.Query["urn"];

            log.LogInformation($"Get Models Status '{urn}'");

            try
            {
                var status = await _aps.GetTranslationStatus(urn);
                return status;
            }
            catch (ApiException ex)
            {
                if (ex.ErrorCode == 404)
                    return new TranslationStatus("n/a", "", new List<string>());
                else
                    throw ex;
            }
        }
    }
}
