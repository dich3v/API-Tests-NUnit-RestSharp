using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EShopApi
{
    public static class GlobalConstants
    {
        public const string BaseUrl = "http://localhost:5000/api";

        public static string AuthenticateUser(string email, string password)
        {
            string resource = "";
            if (email == "admin@gmail.com")
            {
                resource = "user/admin-login";
            }
            else
            {
                resource = "user/login";
            }
            var restClient = new RestClient(BaseUrl);
            var authRequest = new RestRequest(resource, Method.Post);
            authRequest.AddJsonBody(new { email, password });

            var response = restClient.Execute(authRequest);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Assert.Fail($"Authentication failed with status code: {response.StatusCode}, and response content: {response.Content}");
            }

            var content = JObject.Parse(response.Content);

            return content["token"]?.ToString();
        }
        public static void UserWait()
        {
            Thread.Sleep(1000);
        }
    }
}
