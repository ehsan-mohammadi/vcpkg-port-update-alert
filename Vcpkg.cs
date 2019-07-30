using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace VCPKG
{
    // A class for accessing to vcpkg
    public class Vcpkg
    {
        private const string repoAddress = "microsoft/vcpkg";
        private static HttpClient httpClient;

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
                Console.WriteLine("Can't connect to the server :(");
                return null;
            }
        }

        /// <summary>
        /// Get the content of portfile.cmake and return it as a string
        /// </summary> 
        private async Task<string> GetPortFileCMake(string portName)
        {
            //HttpClient httpClient = new HttpClient();
            string portFileCMake = await httpClient.GetStringAsync($"https://github.com/{repoAddress}/ports/{portName}/portfile.cmake");
            Console.WriteLine(portFileCMake);
            return portFileCMake;
        }

        public async Task GetVcpkgRepo()
        {
            SetHttpClient();

            IEnumerable<string> lst = await GetPorts();
            
            foreach(string item in lst)
            {
                Console.WriteLine(item);
                string a = await GetPortFileCMake(item);
            }
        }
    }
}