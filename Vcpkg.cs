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

    // A class for accessing to vcpkg
    public class Vcpkg
    {
        private const string repoAddress = "microsoft/vcpkg";
        private static HttpClient httpClient;

        // A class that callback the Repo and Ref from the portfile.cmake file
        private class RepoRef
        {
            public string Repo {get; set;}
            public string Ref {get; set;}
        }

        /// <summary>
        /// Set the BaseAddress and DefaultRequestHeaders on httpClient
        /// </summary>  
        private void SetHttpClient()
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
        private async Task<IEnumerable<string>> GetPorts()
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
                VcpkgMessage.ErrorMessage("Can't connect to the vcpkg GitHub repo :(");
                return null;
            }
        }

        /// <summary>
        /// Get the content of portfile.cmake and return it as a string
        /// </summary> 
        private async Task<RepoRef> GetPortFileCMake(string portName)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            var url = $"https://github.com/{repoAddress}/blob/master/ports/{portName}/portfile.cmake";
            
            HttpClient httpClient = new HttpClient();
            string portFileCMake = await httpClient.GetStringAsync(url);
            RepoRef repoRef = GetRepoAndRef(portFileCMake);
            
            return repoRef;
        }

        /// <summary>
        /// Get the REPO and REF property from portfile.cmake and return it as a string array
        /// </summary>
        private RepoRef GetRepoAndRef(string portFileCMake)
        {
            RepoRef repoRef = new RepoRef();

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

            return repoRef;
        }

        /// <summary>
        /// Search a port and get the latest release of it
        /// </summary>
        private async Task<string> SearchLatestRelease(RepoRef port)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                var url = $"https://github.com/{port.Repo}/releases";
                
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

        public async Task GetVcpkgRepo()
        {
            SetHttpClient();

            IEnumerable<string> lst = await GetPorts();
            
            foreach(string item in lst)
            {
                //Console.WriteLine(item);

                RepoRef a = await GetPortFileCMake(item);

                if(a.Repo != null && a.Ref != null)
                {
                    Console.WriteLine(a.Repo);
                    Console.WriteLine(a.Ref);
                    string b = await SearchLatestRelease(a);
                    Console.WriteLine(b);
                }
            }
        }
    }
}