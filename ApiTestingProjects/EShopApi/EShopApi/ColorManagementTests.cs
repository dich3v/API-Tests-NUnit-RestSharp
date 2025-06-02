using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;

namespace EShopApi
{
    [TestFixture]
    public class ColorManagementTests
    {
        private RestClient restClient;
        private string token;
        private Random random;

        [TearDown]

        public void Dispose()
        {
            restClient.Dispose();
        }

        [SetUp]

        public void Setup()
        {
            restClient = new RestClient(GlobalConstants.BaseUrl);
            token = GlobalConstants.AuthenticateUser("admin@gmail.com", "admin@gmail.com");
            random = new Random();
        }

        [Test]

        public void Test_ColorLifecycleTest()
        {
            var addColorRequest = new RestRequest("/color", Method.Post);
            addColorRequest.AddHeader("Authorization", $"Bearer {token}");
            addColorRequest.AddJsonBody(new
            {
                title = $"Color_{random.Next(999, 9999)}"
            });

            var addColorResponse = restClient.Execute(addColorRequest);

            Assert.That(addColorResponse.IsSuccessful, Is.True, "Color is not created");

            var colorId = JObject.Parse(addColorResponse.Content)["_id"]?.ToString();
            Assert.That(colorId, Is.Not.Null.Or.Empty, "Response content is null or empty");

            var getColorRequest = new RestRequest($"/color/{colorId}", Method.Get);

            var getColorResponse = restClient.Execute(getColorRequest);

            Assert.IsTrue(getColorResponse.IsSuccessful);

            var deleteColorRequest = new RestRequest($"/color/{colorId}", Method.Delete);
            deleteColorRequest.AddHeader("Authorization", $"Bearer {token}");
            var deleteResponse = restClient.Execute(deleteColorRequest);

            Assert.IsTrue(deleteResponse.IsSuccessful, "Color is not deleted");

            var verifyRequest = new RestRequest($"/color/{colorId}", Method.Get);

            var verifyResponse = restClient.Execute(verifyRequest);

            Assert.That(verifyResponse.Content, Is.Null.Or.EqualTo("null"), "Content is not null");
        }

        [Test]

        public void Test_ColorLifecycleNegativeTest()
        {
            var invalidToken = "Invalid Token";
            var addColorRequest = new RestRequest("/color", Method.Post);
            addColorRequest.AddHeader("Authorization", $"Bearer {invalidToken}");
            addColorRequest.AddJsonBody(new
            {
                title = $"Color_{random.Next(999, 9999)}"
            });

            var addColorResponse = restClient.Execute(addColorRequest);

            Assert.IsFalse(addColorResponse.IsSuccessful, "Color is created");
            Assert.That(addColorResponse.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));

            var invalidColorId = "InvalidID";
            var getColorRequest = new RestRequest($"/color/{invalidColorId}", Method.Get);

            var getColorResponse = restClient.Execute(getColorRequest);

            Assert.IsFalse(getColorResponse.IsSuccessful);
            Assert.That(getColorResponse.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));

            var deleteColorRequest = new RestRequest($"/color/{invalidColorId}", Method.Delete);
            deleteColorRequest.AddHeader("Authorization", $"Bearer {invalidToken}");
            var deleteResponse = restClient.Execute(deleteColorRequest);

            Assert.IsFalse(deleteResponse.IsSuccessful);
            Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        }
    }
}
