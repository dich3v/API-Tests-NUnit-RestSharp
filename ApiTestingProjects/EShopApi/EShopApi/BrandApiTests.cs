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
    public class BrandApiTests : IDisposable
    {
        private RestClient client;
        private string token;
        private static string createdBrandId;

        [SetUp]

        public void Setup()
        {
            client = new RestClient(GlobalConstants.BaseUrl);
            token = GlobalConstants.AuthenticateUser("admin@gmail.com", "admin@gmail.com");

            Assert.That(token, Is.Not.Null.Or.Empty, "Authentication token is null or empty");
        }

        public void Dispose()
        {
            client?.Dispose();
        }

        [Test, Order(1)]

        public void Test_GetAllBrands()
        {
            var request = new RestRequest("brand", Method.Get);

            var response = client.Execute(request);

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code is not OK");

                Assert.That(response.Content, Is.Not.Empty, "Response content is empty");
            });

            var brands = JArray.Parse(response.Content);
            Assert.That(brands.Type, Is.EqualTo(JTokenType.Array));
            Assert.That(brands.Count, Is.GreaterThan(5));

            var brandNames = brands.Select(b => b["title"].ToString()).ToList();

            Assert.That(brandNames, Does.Contain("TechCorp"));
            Assert.That(brandNames, Does.Contain("GameMaster"));

            foreach (var blog in brands)
            {
                Assert.That(blog["title"]?.ToString(), Is.Not.Null.Or.Empty);
                Assert.That(blog["_id"]?.ToString(), Is.Not.Null.Or.Empty);
            }
        }

        [Test, Order(2)]

        public void Test_AddBrand()
        {
            var request = new RestRequest("brand", Method.Post);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddJsonBody(new
            {
                title = "New Title",
            });

            var response = client.Execute(request);

            Assert.Multiple(() =>
            {

                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code is not OK");

                Assert.That(response.Content, Is.Not.Empty, "Response content is empty");

                var content = JObject.Parse(response.Content);

                Assert.That(content["title"].ToString(), Is.EqualTo("New Title"));
                Assert.That(content["_id"].ToString(), Is.Not.Null.Or.Empty);

                createdBrandId = content["_id"].ToString();
            });
        }

        [Test, Order(3)]

        public void Test_UpdateBrand()
        {
            var udpateRequest = new RestRequest("brand/{id}", Method.Put);
            udpateRequest.AddUrlSegment("id", createdBrandId);
            udpateRequest.AddHeader("Authorization", $"Bearer {token}");
            udpateRequest.AddJsonBody(new
            {
                title = "Updated Brand Title"
            });

            GlobalConstants.UserWait();
            var updatedResponse = client.Execute(udpateRequest);

            Assert.Multiple(() =>
            {
                Assert.That(updatedResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(updatedResponse.Content, Is.Not.Empty);

                var content = JObject.Parse(updatedResponse.Content);

                Assert.That(content["title"].ToString(), Is.EqualTo("Updated Brand Title"));
                Assert.That(content["_id"].ToString(), Is.EqualTo(createdBrandId));

                Assert.That(content.ContainsKey("createdAt"), Is.True);
                Assert.That(content.ContainsKey("updatedAt"), Is.True);

                Assert.That(content["createdAt"].ToString(), Is.Not.EqualTo(content["updatedAt"].ToString()));
            });
        }
        [Test, Order(4)]

        public void Test_DeleteBrand()
        {          
            var deleteRequest = new RestRequest("brand/{id}", Method.Delete);
            deleteRequest.AddUrlSegment("id", createdBrandId);

            deleteRequest.AddHeader("Authorization", $"Bearer {token}");

            var deleteResponse = client.Execute(deleteRequest);

            Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var verifyGetRequest = new RestRequest("brand/{id}", Method.Get);

            verifyGetRequest.AddUrlSegment("id", createdBrandId);

            var verifyResponse = client.Execute(verifyGetRequest);

            Assert.That(verifyResponse.Content, Is.EqualTo("null"));
        }
    }
}
