using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;

namespace VCPKG
{
    using VCPKG.Message;
    using VCPKG.CallBack;

    // A class for accessing to vcpkg
    public class Vcpkg
    {
        private const string repoAddress = "microsoft/vcpkg";
        private static HttpClient httpClient;

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
        /// Get the content of portfile.cmake and return it as a string
        /// </summary> 
        private async Task<string> GetPortFileCMake(string portName)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                var url = $"https://github.com/{repoAddress}/blob/master/ports/{portName}/portfile.cmake";
                
                HttpClient httpClient = new HttpClient();
                string portFileCMake = await httpClient.GetStringAsync(url);
                
                return portFileCMake;
            }
            catch(Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Get the REPO and REF property from portfile.cmake and return it as a RepoRef
        /// </summary>
        public async Task<RepoRef> GetRepoAndRef(string portName)
        {
            RepoRef repoRef = new RepoRef();

            try
            {
                string portFileCMake = await GetPortFileCMake(portName);

                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(portFileCMake);
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
                // Do nothing!
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
                var url = $"https://github.com/{repo}/releases";
                
                HttpClient httpClient = new HttpClient();
                string releases = await httpClient.GetStringAsync(url);

                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(releases);
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
    }
}