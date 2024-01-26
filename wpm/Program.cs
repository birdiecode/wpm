using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace wpm
{
    internal class Program
    {
        const string DOWNLOAD_PATH = "C:\\ProgramData\\wpm\\";
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {

            }
            else
            {
                if (args[0] == "install")
                {
                    if (args.Length > 1 && args[1] != "")
                    {
                        var res = Install(args[1]);
                        Console.WriteLine(res.Item2);
                    }
                   
                }
                else
                {
                    Console.WriteLine($"Error: {args[0]}");
                }
            }
        }

        static (bool, string ,string) Install(string name)
        {
            var a = TryGetPackage(name).Result;
            var data = GetDowloadUrl(a.Item1, a.Item2).Result;
            var durl = data.Item1;
            if (name != a.Item1)
            {
                Console.WriteLine($"Error: did u mean {a.Item1}");
                return default;
            }
            var path = DOWNLOAD_PATH + $"{a.Item1}\\{a.Item2}";
            var path_file = path + $"\\{durl.Split('/').Last()}";
            CreateFolder(path);
            DowloadPackage(durl, path_file);
            return (true, data.Item2, path_file);
        }

        static async Task<String> GetInfo()
        {
            string apiUrl = "https://repo.v2t2.ru/23af2517-f6a5-43df-9f90-2f7c56143909/information";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        // Read and print the response content as a string
                        string responseBody = await response.Content.ReadAsStringAsync();
                        dynamic result = JsonConvert.DeserializeObject(responseBody);
                        return result.Data.SourceIdentifier;
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
                return "";
            }
        }

        static async Task<(string, string)> TryGetPackage(string name)
        {
            string apiUrl = "https://repo.v2t2.ru/23af2517-f6a5-43df-9f90-2f7c56143909/manifestSearch";
            
            using (HttpClient client = new HttpClient())
            {
                string jsonData = $@"
                {{
                    ""Inclusions"": [
                        {{ ""PackageMatchField"": ""PackageFamilyName"", ""RequestMatch"": {{ ""KeyWord"": ""{name}"", ""MatchType"": ""Exact"" }} }},
                        {{ ""PackageMatchField"": ""ProductCode"", ""RequestMatch"": {{ ""KeyWord"": ""{name}"", ""MatchType"": ""Exact"" }} }},
                        {{ ""PackageMatchField"": ""PackageIdentifier"", ""RequestMatch"": {{ ""KeyWord"": ""{name}"", ""MatchType"": ""CaseInsensitive"" }} }},
                        {{ ""PackageMatchField"": ""PackageName"", ""RequestMatch"": {{ ""KeyWord"": ""{name}"", ""MatchType"": ""CaseInsensitive"" }} }},
                        {{ ""PackageMatchField"": ""Moniker"", ""RequestMatch"": {{ ""KeyWord"": ""{name}"", ""MatchType"": ""CaseInsensitive"" }} }}
                    ]
                }}";

                try
                {
                    StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                   

                    if (response.IsSuccessStatusCode)
                    {
                        // Read and print the response content as a string
                        string responseBody = await response.Content.ReadAsStringAsync();
                        dynamic result = JsonConvert.DeserializeObject(responseBody);
                        return (result.Data[0].PackageIdentifier, result.Data[0].Versions[0].PackageVersion);
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
                return ("", "");
            }
        }

        static async Task<(string, string)> GetDowloadUrl(string name, string version)
        {
            string apiUrl = $"https://repo.v2t2.ru/23af2517-f6a5-43df-9f90-2f7c56143909/packageManifests/{name}?Version={version}";
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        // Read and print the response content as a string
                        string responseBody = await response.Content.ReadAsStringAsync();
                        dynamic result = JsonConvert.DeserializeObject(responseBody);
                        return (result.Data.Versions[0].Installers[0].InstallerUrl, result.Data.Versions[0].Installers[0].InstallerType);
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
                return default;
            }
        }

        static void CreateFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                try
                {
                    Directory.CreateDirectory(folderPath);
                    Console.WriteLine("Folder created successfully!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating folder: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Folder already exists.");
            }
        }
        static async void DowloadPackage(string url, string path)
        {
            using (WebClient webClient = new WebClient())
            {
                try
                {
                    webClient.DownloadFile(url, path);
                    Console.WriteLine("File downloaded successfully.");
                }
                catch (WebException ex)
                {
                    // Display the specific error message and status code
                    Console.WriteLine($"Error downloading file: {ex.Message}");

                    if (ex.Response is HttpWebResponse response)
                    {
                        Console.WriteLine($"HTTP Status Code: {response.StatusCode}");
                    }
                }
            }
        }
    }
}
