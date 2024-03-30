using System;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using System.IO;

namespace AVILOT.backend
{
    public static class Sync
    {
        private static string backendUrl = "https://backend.lusifly.eu";

        private class DatasetUpdateRequest
        {
            public string app { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string? dataset { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string? updated { get; set; }
        }

        public class DatasetUpdateResponse
        {
            public string action { get; set; }
            public string? url { get; set; }
            public string? filename { get; set; }
        }

        public async static Task<DatasetUpdateResponse?> requestCheckDatasetUpdate(string appVersion, string? datasetVersion = null, DateTime? lastUpdated = null)
        {
            var requestJson = new DatasetUpdateRequest
            {
                app = appVersion,
                dataset = datasetVersion,
                updated = lastUpdated == null ? null : lastUpdated?.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };
            var requestString = JsonConvert.SerializeObject(requestJson); 
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(
                    $"{backendUrl}/api/check_dataset_update",
                     new StringContent(requestString, Encoding.UTF8, "application/json")
                );
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine(response.Content);
                    Console.WriteLine(response.StatusCode.ToString());
                    Console.WriteLine(response.Headers.ToString());

                }
                response.EnsureSuccessStatusCode();
                var responseString = await response.Content.ReadAsStringAsync();
                var responseJson = JsonConvert.DeserializeObject<DatasetUpdateResponse>(responseString);
                
                if (responseJson == null)
                {
                    throw new Exception("Failed to parse response");
                }
                else if (responseJson.action == "update")
                {
                    if (responseJson.filename == null || responseJson.url == null)
                    {
                        throw new Exception("Fields missing in response");
                    }
                    return responseJson;
                }
                else
                {
                    return null;
                }
            }
        }
        public async static Task<DatasetUpdateResponse?> forceCheckDatasetUpdate(string appVersion)
        {
            return await requestCheckDatasetUpdate(appVersion, null, null);
        }

        public static async Task<string> downloadDatasetFile(string downloadUrl, string filename)
        {
            var downloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "temp", filename);
            if (File.Exists(downloadPath))
            {
                File.Delete(downloadPath);
            }
            if (!Directory.Exists(Path.GetDirectoryName(downloadPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(downloadPath));
            }
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(downloadUrl);
                response.EnsureSuccessStatusCode();
                using (var fileStream = new FileStream(downloadPath, FileMode.Create, FileAccess.Write))
                {
                    await response.Content.CopyToAsync(fileStream);
                }
            }
            return downloadPath;
        }

        public static bool deleteDatasetFile(string datasetPath)
        {
            if (File.Exists(datasetPath))
            {
                File.Delete(datasetPath);
                return true;
            }
            else
            {
                return false;
            }
        }
        

    }
}
