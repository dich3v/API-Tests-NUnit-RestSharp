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
    }
}
