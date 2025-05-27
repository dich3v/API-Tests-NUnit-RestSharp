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
    public class ColorApiTests : IDisposable
    {
        private RestClient client;
        private string token;
        private static string createdColorId;

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
         public void GetAllColors()
        {
            var request = new RestRequest("color", Method.Get);

            var response = client.Execute(request);

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code is not OK");

                Assert.That(response.Content, Is.Not.Empty, "Response content is empty");           

            var colors = JArray.Parse(response.Content);
            Assert.That(colors.Type, Is.EqualTo(JTokenType.Array), "Expected response content to be a JSON array");
            Assert.That(colors.Count, Is.GreaterThan(0), "Expected at least one color in the response");

            var colorTitles = colors.Select(c => c["title"].ToString()).ToList();

            Assert.That(colorTitles, Does.Contain("Silver"));
            Assert.That(colorTitles, Does.Contain("Blue"));

            foreach (var color in colors)
            {
                Assert.That(color["title"]?.ToString(), Is.Not.Null.Or.Empty, "Color title should not be null or empty");
                Assert.That(color["_id"]?.ToString(), Is.Not.Null.Or.Empty, "Color ID should not be null or empty");
            }
                Assert.That(colors.Count, Is.EqualTo(10), "Expected exactly 10 colors in the response");
            });
        }
        [Test, Order(2)]
        public void Test_AddColor()
        {
            var request = new RestRequest("color", Method.Post);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddJsonBody(new
            {
                title = "New Color",
            });

            var response = client.Execute(request);

            Assert.Multiple(() =>
            {

                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code is not OK");

                Assert.That(response.Content, Is.Not.Empty, "Response content is empty");

                var content = JObject.Parse(response.Content);

                Assert.That(content["title"].ToString(), Is.EqualTo("New Color"));
                Assert.That(content["_id"].ToString(), Is.Not.Null.Or.Empty);
                Assert.That(content.ContainsKey("createdAt"), Is.True, "Color should have createdAt field");
                Assert.That(content.ContainsKey("updatedAt"), Is.True, "Color should have updatedAt field");
                Assert.That(content["createdAt"]?.ToString(), Is.EqualTo(content["updatedAt"]?.ToString()), "createdAt and updatedAt should be equal on creation");

                createdColorId = content["_id"].ToString();
            });
        }
        [Test, Order(3)]
        public void Test_UpdateColor()
        {
            var updateRequest = new RestRequest("color/{id}", Method.Put);
            updateRequest.AddUrlSegment("id", createdColorId);
            updateRequest.AddHeader("Authorization", $"Bearer {token}");
            updateRequest.AddJsonBody(new
            {
                title = "Updated Color"
            });
            GlobalConstants.UserWait();
            var updatedResponse = client.Execute(updateRequest);

            Assert.Multiple(() =>
            {
                Assert.That(updatedResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(updatedResponse.Content, Is.Not.Empty);

                var content = JObject.Parse(updatedResponse.Content);

                Assert.That(content["title"].ToString(), Is.EqualTo("Updated Color"));
                Assert.That(content["_id"].ToString(), Is.EqualTo(createdColorId));

                Assert.That(content.ContainsKey("createdAt"), Is.True);
                Assert.That(content.ContainsKey("updatedAt"), Is.True);
                Assert.That(content["updatedAt"]?.ToString(), Is.Not.EqualTo(content["createdAt"]?.ToString()), "createdAt and updatedAt should be different after update");
            });
        }
        [Test, Order(4)]
        public void Test_DeleteColor()
        {
            var deleteRequest = new RestRequest("color/{id}", Method.Delete);
            deleteRequest.AddUrlSegment("id", createdColorId);

            deleteRequest.AddHeader("Authorization", $"Bearer {token}");

            var deleteResponse = client.Execute(deleteRequest);

            Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code OK (200) after deletion");

            var verifyGetRequest = new RestRequest("color/{id}", Method.Get);

            verifyGetRequest.AddUrlSegment("id", createdColorId);

            var verifyResponse = client.Execute(verifyGetRequest);

            Assert.That(verifyResponse.Content, Is.EqualTo("null"), "Get response after deletion should be 'null'");
        }
    }
}
