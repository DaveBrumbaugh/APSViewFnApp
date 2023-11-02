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
    public class UploadModelForm
    {
        [FromForm(Name = "model-zip-entrypoint")]
        public string? Entrypoint { get; set; }

        [FromForm(Name = "model-file")]
        public IFormFile? File { get; set; }
    }

    public class ModelsPost
    {
        public record BucketObject(string name, string urn);

        private readonly APS _aps;

        public ModelsPost(APS aps)
        {
            _aps = aps;
        }


        [FunctionName("ModelsPost")]
        public async Task<BucketObject> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Models Post.");

            using (var stream = new MemoryStream())
            {
                var formdata = await req.ReadFormAsync();
                var file = req.Form.Files["file"];
                await file.CopyToAsync(stream);
                stream.Position = 0;
                var obj = await _aps.UploadModel(file.FileName, stream);
                var job = await _aps.TranslateModel(obj.ObjectId, string.Empty);
                return new BucketObject(obj.ObjectKey, job.Urn);
            }

            //using (var client = new HttpClient())
            //{
            //    using (var content = new MultipartFormDataContent())
            //    {
            //        var fileName = Path.GetFileName(filePath);
            //        var fileStream = System.IO.File.Open(filePath, FileMode.Open);
            //        content.Add(new StreamContent(fileStream), "file", fileName);

            //        var requestUri = baseURL;
            //        var request = new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = content };
            //        var result = await client.SendAsync(request);

            //        return;
            //    }
            //}

            //try
            //{
            //    var formdata = await req.ReadFormAsync();
            //    var file = req.Form.Files["file"];
            //    return new OkObjectResult(file.FileName + " - " + file.Length.ToString());
            //}
            //catch (Exception ex)
            //{
            //    return new BadRequestObjectResult(ex);
            //}
        }
    }
}
