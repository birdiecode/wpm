using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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
               
                Console.ReadKey();
            }
            else
            {
                if (args[0] == "install")
                {
                    if (args.Length > 1 && args[1] != "")
                    {
                        var res = Install("kyosera_upd");
                        if (res.Item1)
                        {
                            string param = "";
                            try
                            {
                                param = res.Item2.InstallerSwitches.Silent;
                            }
                            catch (Exception ex) { }
                            Console.WriteLine(res.Item2);
                            if (res.Item2.InstallerType == "msi")
                            {
                                InstallMsi(res.Item3, param);
                            }
                            else if (res.Item2.InstallerType == "exe")
                            {
                                InstallExe(res.Item3, param);
                            }
                            else if (res.Item2.InstallerType == "zip")
                            {
                                (string, string) run_file = (res.Item2.NestedInstallerFiles[0].RelativeFilePath, res.Item2.NestedInstallerType);
                                InstallZip(res.Item3, param, run_file);
                            }
                        }
                    }

                } else if (args[0] == "unzip")
                {
                    //UnZip("C:\\ProgramData\\wpm\\kyosera_upd\\8.3.0815\\KX_Universal_Printer_Driver-KTeV1aM12W.zip", "C:\\ProgramData\\wpm\\kyosera_upd\\8.3.0815\\extr");
                }
                else
                {
                    Console.WriteLine($"Error: {args[0]}");
                }
            }
        }

        static (bool, dynamic, string) Install(string name)
        {
            var a = TryGetPackage(name).Result;
            var data = GetDowloadUrl(a.Item1, a.Item2).Result;
            var durl = (string)data.InstallerUrl;
            if (name != a.Item1)
            {
                Console.WriteLine($"Error: did u mean {a.Item1}");
                return default;
            }
            var path = DOWNLOAD_PATH + $"{a.Item1}\\{a.Item2}";
            var path_file = path + $"\\{durl.Split('/').Last()}";
            CreateFolder(path);
            DownloadPackage(durl, path_file);
            return (true, data, path_file);
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

        static async Task<dynamic> GetDowloadUrl(string name, string version)
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
                        return result.Data.Versions[0].Installers[0];
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
        static async void DownloadPackage(string url, string path)
        {
            Console.WriteLine("Download");
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

        static void InstallMsi(string file, string param)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = "msiexec.exe";
                process.StartInfo.Arguments = $"{param} /i \"{file}\" ";
                process.Start();
                process.WaitForExit();

                Console.WriteLine("MSI installation completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void InstallExe(string file, string param)
        {
            try
            {
                Process process = new Process();
                process.StartInfo.FileName = file;
                process.StartInfo.Arguments = param;
                process.Start();
                process.WaitForExit();

                Console.WriteLine("MSI installation completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void InstallZip(string file, string param, (string, string) run_file)
        {
            UnZip(file, Path.GetDirectoryName(file));
            if(run_file.Item2 == "exe")
            {
                InstallExe(Path.GetDirectoryName(file) + $"\\{run_file.Item1}", param);
            }
            else if (run_file.Item2 == "msi")
            {
                InstallMsi(Path.GetDirectoryName(file) + $"\\{run_file.Item1}", param);
            }
        }
        static void UnZip(string zipFilePath, string extractPath)
        {
            Console.WriteLine("UnZip");
            try
            {

                if (!Directory.Exists(extractPath))
                {
                    Directory.CreateDirectory(extractPath);
                }

                // Extract the contents of the ZIP file
                ZipFile.ExtractToDirectory(zipFilePath, extractPath);

                Console.WriteLine("ZIP file has been successfully extracted.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        } 
    }
}
