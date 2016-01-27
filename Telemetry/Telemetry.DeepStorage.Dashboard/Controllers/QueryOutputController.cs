namespace Telemetry.DeepStorage.Dashboard.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Http;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Newtonsoft.Json.Linq;
    using Telemetry.Core;

    public class QueryOutputController : ApiController
    {
        public IHttpActionResult Get(string name)
        {
            var json = string.Empty;
            var account = CloudStorageAccount.Parse(Config.Get("DeepStorage.OutputConnectionString"));
            var blobClient = account.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(Config.Get("DeepStorage.HdpContainerName"));

            var prefix = string.Format("{0}.json/part-r-", name);
            var matchingBlobs = container.ListBlobs(prefix, true);
            foreach (var part in matchingBlobs.OfType<CloudBlockBlob>())
            {
                json += part.DownloadText() + Environment.NewLine;
            }

            var outputArray = new JArray();
            foreach (var line in json.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries))
            {
                dynamic raw = JObject.Parse(line);
                var formatted = new JArray();
                formatted.Add((string)raw.eventName);
                formatted.Add((long)raw.count);
                outputArray.Add(formatted);
            }

            return Ok(outputArray);
        }
    }
}
