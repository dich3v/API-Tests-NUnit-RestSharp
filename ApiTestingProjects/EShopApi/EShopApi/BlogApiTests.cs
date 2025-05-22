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
    [TestFixture]
    public class BlogApiTests : IDisposable
    {
        private RestClient client;
        private string token;
        private static string createdProductId;

        public void Dispose()
        {
            client?.Dispose();
        }
        [SetUp]

        public void Setup()
        {
            client = new RestClient(GlobalConstants.BaseUrl);
            token = GlobalConstants.AuthenticateUser("admin@gmail.com", "admin@gmail.com");

            Assert.That(token, Is.Not.Null.Or.Empty, "Authentication token is null or empty");
        }
        [Test, Order(1)]

        public void Test_GetAllBlogs()
        {
            var request = new RestRequest("blog", Method.Get);

            var response = client.Execute(request);

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code is not OK");

                Assert.That(response.Content, Is.Not.Empty, "Response content is empty");
            });

            var blogs = JArray.Parse(response.Content);

            Assert.That(blogs.Count, Is.GreaterThan(0));

            foreach (var blog in blogs)
            {
                Assert.That(blog["title"]?.ToString(), Is.Not.Null.Or.Empty);
                Assert.That(blog["description"]?.ToString(), Is.Not.Null.Or.Empty);
                Assert.That(blog["author"]?.ToString(), Is.Not.Null.Or.Empty);
                Assert.That(blog["category"]?.ToString(), Is.Not.Null.Or.Empty);
            }
        }
        [Test, Order(2)]

        public void Test_AddBlog()
        {
            var request = new RestRequest("blog", Method.Post);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddJsonBody(new
            {
                title = "New Blog Title",
                description = "New Blog Description",
                category = "New Blog Category",
            });

            var response = client.Execute(request);

            Assert.Multiple(() =>
            {

                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code is not OK");

                Assert.That(response.Content, Is.Not.Empty, "Response content is empty");

                var content = JObject.Parse(response.Content);
                createdProductId = content["_id"]?.ToString();

                Assert.That(content["title"].ToString(), Is.EqualTo("New Blog Title"));
                Assert.That(content["description"].ToString(), Is.EqualTo("New Blog Description"));
                Assert.That(content["category"].ToString(), Is.EqualTo("New Blog Category"));
                Assert.That(content["author"].ToString, Is.Null.Or.Not.Empty);
            });
        }
        [Test, Order(3)]

        public void Test_UpdateBlog()
        {
            var updateRequest = new RestRequest("blog/{id}", Method.Put);
            updateRequest.AddUrlSegment("id", createdProductId);
            updateRequest.AddHeader("Authorization", $"Bearer {token}");
            updateRequest.AddJsonBody(new
            {
                title = "Updated Blog Title",
                description = "Updated Blog Description",
                category = "Updated Blog Category",
            });

            var updatedResponse = client.Execute(updateRequest);

            Assert.Multiple(() =>
            {
                Assert.That(updatedResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code is not OK");

                Assert.That(updatedResponse.Content, Is.Not.Empty, "Response content is empty");

                var content = JObject.Parse(updatedResponse.Content);

                Assert.That(content["title"].ToString(), Is.EqualTo("Updated Blog Title"));
                Assert.That(content["description"].ToString(), Is.EqualTo("Updated Blog Description"));
                Assert.That(content["category"].ToString(), Is.EqualTo("Updated Blog Category"));
                Assert.That(content["author"].ToString, Is.Null.Or.Not.Empty);
            });
        }
        [Test, Order(4)]
        public void Test_GetBlogById()
        {
            var request = new RestRequest("blog/{id}", Method.Get);
            request.AddUrlSegment("id", createdProductId);

            var response = client.Execute(request);

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code is not OK");

                Assert.That(response.Content, Is.Not.Empty, "Response content is empty");

                var content = JObject.Parse(response.Content);

                Assert.That(content["title"].ToString(), Is.EqualTo("Updated Blog Title"));
                Assert.That(content["description"].ToString(), Is.EqualTo("Updated Blog Description"));
                Assert.That(content["category"].ToString(), Is.EqualTo("Updated Blog Category"));
            });
        }
        [Test, Order(5)]

        public void Test_DeleteBlog()
        {
            var deleteRequest = new RestRequest("blog/{id}", Method.Delete);
            deleteRequest.AddUrlSegment("id", createdProductId);
            deleteRequest.AddHeader("Authorization", $"Bearer {token}");

            var deleteResponse = client.Execute(deleteRequest);

            Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var verifyGetRequest = new RestRequest("blog/{id}", Method.Get);

            verifyGetRequest.AddUrlSegment("id", createdProductId);

            var verifyResponse = client.Execute(verifyGetRequest);

            Assert.That(verifyResponse.Content, Is.EqualTo("null"));
        }
    }
}
