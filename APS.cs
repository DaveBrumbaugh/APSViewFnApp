using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Autodesk.Forge;
using Autodesk.Forge.Model;
using Autodesk.Forge.Client;

namespace APSViewFnApp
{
    public record TranslationStatus(string Status, string Progress, IEnumerable<string>? Messages);

    public class APS
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _bucket;

        public APS(string clientId, string clientSecret, string? bucket = null)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _bucket = string.IsNullOrEmpty(bucket) ? string.Format("{0}-basic-app", _clientId.ToLower()) : bucket;
        }
        public record Token(string AccessToken, DateTime ExpiresAt);

        private Token? _internalTokenCache;
        private Token? _publicTokenCache;

        private async Task<Token> GetToken(Autodesk.Forge.Scope[] scopes)
        {
            dynamic auth = await new TwoLeggedApi().AuthenticateAsync(_clientId, _clientSecret, "client_credentials", scopes);
            return new Token(auth.access_token, DateTime.UtcNow.AddSeconds(auth.expires_in));
        }

        public async Task<Token> GetPublicToken()
        {
            if (_publicTokenCache == null || _publicTokenCache.ExpiresAt < DateTime.UtcNow)
                _publicTokenCache = await GetToken(new Autodesk.Forge.Scope[] { Autodesk.Forge.Scope.ViewablesRead });
            return _publicTokenCache;
        }

        private async Task<Token> GetInternalToken()
        {
            if (_internalTokenCache == null || _internalTokenCache.ExpiresAt < DateTime.UtcNow)
                _internalTokenCache = await GetToken(new Autodesk.Forge.Scope[] { Autodesk.Forge.Scope.BucketCreate, Autodesk.Forge.Scope.BucketRead, Autodesk.Forge.Scope.DataRead, Autodesk.Forge.Scope.DataWrite, Autodesk.Forge.Scope.DataCreate });
            return _internalTokenCache;
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes).TrimEnd('=');
        }

        public async Task<Job> TranslateModel(string objectId, string rootFilename)
        {
            var token = await GetInternalToken();
            var api = new DerivativesApi();
            api.Configuration.AccessToken = token.AccessToken;
            var formats = new List<JobPayloadItem> {
            new JobPayloadItem (JobPayloadItem.TypeEnum.Svf, new List<JobPayloadItem.ViewsEnum> { JobPayloadItem.ViewsEnum._2d, JobPayloadItem.ViewsEnum._3d })
        };
            var payload = new JobPayload(
                new JobPayloadInput(Base64Encode(objectId)),
                new JobPayloadOutput(formats)
            );
            if (!string.IsNullOrEmpty(rootFilename))
            {
                payload.Input.RootFilename = rootFilename;
                payload.Input.CompressedUrn = true;
            }
            var job = (await api.TranslateAsync(payload)).ToObject<Job>();
            return job;
        }

        public async Task<TranslationStatus> GetTranslationStatus(string urn)
        {
            var token = await GetInternalToken();
            var api = new DerivativesApi();
            api.Configuration.AccessToken = token.AccessToken;
            var json = (await api.GetManifestAsync(urn)).ToJson();

            System.Diagnostics.Debug.WriteLine($"{json}");

            var messages = new List<string>();
            foreach (var message in json.SelectTokens("$.derivatives[*].messages[?(@.type == 'error')].message"))
            {
                if (message.Type == Newtonsoft.Json.Linq.JTokenType.String)
                    messages.Add((string)message);
            }
            foreach (var message in json.SelectTokens("$.derivatives[*].children[*].messages[?(@.type == 'error')].message"))
            {
                if (message.Type == Newtonsoft.Json.Linq.JTokenType.String)
                    messages.Add((string)message);
            }
            return new TranslationStatus((string)json["status"], (string)json["progress"], messages);
        }

        private async Task EnsureBucketExists(string bucketKey)
        {
            var token = await GetInternalToken();
            var api = new BucketsApi();
            api.Configuration.AccessToken = token.AccessToken;
            try
            {
                await api.GetBucketDetailsAsync(bucketKey);
            }
            catch (ApiException e)
            {
                if (e.ErrorCode == 404)
                {
                    await api.CreateBucketAsync(new PostBucketsPayload(bucketKey, null, PostBucketsPayload.PolicyKeyEnum.Persistent));
                }
                else
                {
                    throw e;
                }
            }
        }

        public async Task<ObjectDetails> UploadModel(string objectName, Stream content)
        {
            await EnsureBucketExists(_bucket);
            var token = await GetInternalToken();
            var api = new ObjectsApi();
            api.Configuration.AccessToken = token.AccessToken;
            var results = await api.uploadResources(_bucket, new List<UploadItemDesc> {
            new UploadItemDesc(objectName, content)
        });
            if (results[0].Error)
            {
                throw new Exception(results[0].completed.ToString());
            }
            else
            {
                var json = results[0].completed.ToJson();
                return json.ToObject<ObjectDetails>();
            }
        }

        public async Task<IEnumerable<ObjectDetails>> GetObjects()
        {
            const int PageSize = 64;
            await EnsureBucketExists(_bucket);
            var token = await GetInternalToken();
            var api = new ObjectsApi();
            api.Configuration.AccessToken = token.AccessToken;
            var results = new List<ObjectDetails>();
            var response = (await api.GetObjectsAsync(_bucket, PageSize)).ToObject<BucketObjects>();
            results.AddRange(response.Items);
            while (!string.IsNullOrEmpty(response.Next))
            {
                var queryParams = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(new Uri(response.Next).Query);
                response = (await api.GetObjectsAsync(_bucket, PageSize, null, queryParams["startAt"])).ToObject<BucketObjects>();
                results.AddRange(response.Items);
            }
            return results;
        }
    }
}
