using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;
using System.IO;

namespace VCPKG
{
    using VCPKG.Message;
    using VCPKG.CallBack;

    // A class for accessing to vcpkg
    public class Vcpkg
    {
        private const string repoAddress = "microsoft/vcpkg";
        private static HttpClient httpClient;
        private static List<string> log = new List<string>();

        /// <summary>
        /// Set the BaseAddress and DefaultRequestHeaders on httpClient
        /// </summary>  
        public void SetHttpClient()
        {
            httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.github.com") ,
                DefaultRequestHeaders = {
                    {"User-Agent", "Github-API-Test"}
                }
            };
        }

        /// <summary>
        /// Get the vcpkg ports from github repo and store it in a list
        /// </summary> 
        public async Task<IEnumerable<string>> GetPorts()
        {
            try
            {
                var response = await httpClient.GetAsync($"repos/{repoAddress}/contents/ports");
                var contentAsString = await response.Content.ReadAsStringAsync();
                var contentAsJson = JToken.Parse(contentAsString);

                return contentAsJson.SelectTokens("$.[*].name").Select(token => token.Value<string>());
            }
            catch(Exception)
            {
                VcpkgMessage.ErrorMessage("Error: Can't connect to the vcpkg GitHub repo :(\n");
                Console.ResetColor();
                return null;
            }
        }

        /// <summary>
        /// Get the url of portfile.cmake and return it as a string
        /// </summary> 
        private string GetPortFileCMake(string portName)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            var url = $"https://github.com/{repoAddress}/blob/master/ports/{portName}/portfile.cmake";
            return url;
        }

        /// <summary>
        /// Get the REPO and REF property from portfile.cmake and return it as a RepoRef
        /// </summary>
        public async Task<RepoRef> GetRepoAndRef(string portName)
        {
            RepoRef repoRef = new RepoRef();

            try
            {
                string portFileUrl = GetPortFileCMake(portName);

                HtmlWeb htmlWeb = new HtmlWeb();
                htmlWeb.UsingCache = false;
                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument = await htmlWeb.LoadFromWebAsync(portFileUrl);
                List<HtmlNode> htmlNodeList = htmlDocument.DocumentNode.SelectNodes("//td")
                        .Where(node => node.GetAttributeValue("class", "")
                        .Equals("blob-code blob-code-inner js-file-line")).ToList();

                foreach(HtmlNode node in htmlNodeList)
                {
                    // Find REPO and REF lines
                    if(node.InnerText.Contains(" REPO "))
                        repoRef.Repo = node.InnerText.Replace(" ", "").Substring(4);
                    else if(node.InnerText.Contains(" REF "))
                        repoRef.Ref = node.InnerText.Replace(" ", "").Substring(3);

                    if(repoRef.Repo != null && repoRef.Ref != null)
                        break;
                }
            }
            catch(Exception)
            {
                return null;
            }
            

            return repoRef;
        }

        /// <summary>
        /// Check a port has both REPO and REF property
        /// </summary>
        public bool HasRepoAndRef(RepoRef port)
        {
            return (port.Repo != null && port.Ref != null) ? true : false;
        }

        /// <summary>
        /// Search a port and get the latest release of it
        /// </summary>
        public async Task<string> SearchLatestRelease(string repo)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                var release = $"https://github.com/{repo}/releases";
                
                HtmlWeb htmlWeb = new HtmlWeb();
                htmlWeb.UsingCache = false;
                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument = await htmlWeb.LoadFromWebAsync(release);
                HtmlNode htmlNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='css-truncate-target']");
                string latestRelease = htmlNode.InnerText;

                return latestRelease;
            }
            catch(Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Compare the current release and latest release versions
        /// </summary>
        public bool PortNeedToUpdate(string currentRelease, string latestRelease)
        {
            if(latestRelease != null && currentRelease != latestRelease)
                return true;
            
            return false;
        }

        /// <summary>
        /// Add each need-to-be-update port to the log list
        /// </summary>
        public void AddToLog(string updatePort)
        {
            log.Add(updatePort);
        }

        /// <summary>
        /// Get the length of log list
        /// </summary>
        public int LogLength()
        {
            return log.Count;
        }

        /// <summary>
        /// Save the log list to the path as .log file
        /// </summary>
        public bool SaveLog(string path)
        {
            try
            {
                // Check the path exists or not. If not exist, try to create the path
                if(!Directory.Exists(Path.GetDirectoryName(path)))
                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                // Save log list to the path
                File.WriteAllLines(path, log.ToArray());

                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }
    }
}