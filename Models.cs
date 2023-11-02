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
    public class Models
    {
        public record BucketObject(string name, string urn);

        private readonly APS _aps;

        public Models(APS aps)
        {
            _aps = aps;
        }

        //public class UploadModelForm
        //{
        //    [FromForm(Name = "model-zip-entrypoint")]
        //    public string? Entrypoint { get; set; }

        //    [FromForm(Name = "model-file")]
        //    public IFormFile? File { get; set; }
        //}

        //[HttpPost()]
        //public async Task<BucketObject> UploadAndTranslateModel([FromForm] UploadModelForm form)
        //{
        //    using (var stream = new MemoryStream())
        //    {
        //        await form.File.CopyToAsync(stream);
        //        stream.Position = 0;
        //        var obj = await _aps.UploadModel(form.File.FileName, stream);
        //        var job = await _aps.TranslateModel(obj.ObjectId, form.Entrypoint);
        //        return new BucketObject(obj.ObjectKey, job.Urn);
        //    }
        //}

        [FunctionName("Models")]
        public async Task<IEnumerable<BucketObject>> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Get Models.");

            var objects = await _aps.GetObjects();
            return from o in objects
                   select new BucketObject(o.ObjectKey, APS.Base64Encode(o.ObjectId));
        }
    }
}
