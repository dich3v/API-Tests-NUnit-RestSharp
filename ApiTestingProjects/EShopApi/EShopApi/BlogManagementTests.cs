using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EShopApi
{
    [TestFixture]
    public class BlogManagementTests
    {
        private RestClient restClient;
        private string token;
        private Random random;

        [SetUp]

        public void Setup()
        {
            restClient = new RestClient(GlobalConstants.BaseUrl);
            token = GlobalConstants.AuthenticateUser("admin@gmail.com", "admin@gmail.com");
            random = new Random();
        }

        [Test]
        public void Test_BlogPostLifecycleTest()
        {
            //Step 1: Create blog post
            var createBlogPostRequest = new RestRequest("/blog", Method.Post);
            createBlogPostRequest.AddHeader("Authorization", $"Bearer {token}");
            createBlogPostRequest.AddJsonBody(new
            {
                Title = $"BlogTitle_{random.Next(999, 9999).ToString()}",
                Description = "New description",
                Category = "Action"
            });

            var createBlogResponse = restClient.Execute(createBlogPostRequest);

            var blogId = JObject.Parse(createBlogResponse.Content)["id"]?.ToString();

            Assert.That(createBlogResponse.IsSuccessful, Is.True, "Blog post is not created");
            Assert.That(blogId, Is.Not.Null.Or.Empty, "Blog id is null or empty");

            //Step2: Update blog post

            var updateRequest = new RestRequest($"/blog/{blogId}", Method.Put);
            updateRequest.AddHeader("Authorization", $"Bearer {token}");
            updateRequest.AddJsonBody(new
            {
                Title = $"UpdatedTitle_{random.Next(999, 9999)}",
                Description = " Updated Description"
            });

            var updateResponse = restClient.Execute(updateRequest);

            Assert.That(updateResponse.IsSuccessful, Is.True, "Response body is not updated");

            //Step 3: Delete blog post

            var deleteRequest = new RestRequest($"/blog/{blogId}", Method.Delete);
            deleteRequest.AddHeader("Authorization", $"Bearer {token}");

            var deleteResponse = restClient.Execute(deleteRequest);

            Assert.That(deleteResponse.IsSuccessful, Is.True);

            //Step 4: Verify the blog post is deleted
            var verifyRequest = new RestRequest($"blog/{blogId}", Method.Get);
            var verifyResponse = restClient.Execute(verifyRequest);

            Assert.That(verifyResponse.Content, Is.Null.Or.EqualTo("null"), "Response content is not null");
        }
        public void Dispose()
        {
            restClient.Dispose();
        }
    }
}
