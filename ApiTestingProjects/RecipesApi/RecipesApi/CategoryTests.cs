using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RestSharp;
using System.Net;

namespace RecipesApi
{
    [TestFixture]
    public class CategoryTests : IDisposable
    {
        private RestClient client;
        private string token;
        private Random random;
        private string name;

        [SetUp]
        public void Setup()
        {
            client = new RestClient(GlobalConstants.BaseUrl);
            token = GlobalConstants.AuthenticateUser("john.doe@example.com", "password123");

            Assert.That(token, Is.Not.Null.Or.Empty, "Authentication token should not be null or empty");

            random = new Random();
        }
        [Test]
        public void Test_CategoryLifecycle()
        {
            // Step 1: Create a new category
            name = $"Name_{random.Next(1, 100)}";

            var createRequest = new RestRequest("category", Method.Post);
            createRequest.AddHeader("Authorization", $"Bearer {token}");
            createRequest.AddJsonBody(new
            {
                name = name,
            });

            var createResponse = client.Execute(createRequest);
            Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code is not OK (200)");

            var createdCategory = JObject.Parse(createResponse.Content);
            string categoryId = createdCategory["_id"]?.ToString();
            Assert.That(categoryId, Is.Not.Null.And.Not.Empty, "Category ID is null or empty");

            // Step 2: Get all categories
            var getAllRequest = new RestRequest("category", Method.Get);
            var getAllResponse = client.Execute(getAllRequest);

            Assert.Multiple(() =>
            {
                Assert.That(getAllResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code is not OK (200)");
                Assert.That(getAllResponse.Content, Is.Not.Null.Or.Empty, "Response content is null or empty");

                var categories = JArray.Parse(getAllResponse.Content);
                Assert.That(categories.Type, Is.EqualTo(JTokenType.Array), "Expected response content is not JSON array");
                Assert.That(categories.Count, Is.GreaterThan(0), "Expected at least one category in the response");
            });

            // Step 3: Get category by ID
            var getByIdRequest = new RestRequest($"category/{categoryId}", Method.Get);
            var getByIdResponse = client.Execute(getByIdRequest);

            Assert.Multiple(() =>
            {
                Assert.That(getByIdResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code is not OK (200)");
                Assert.That(getByIdResponse.Content, Is.Not.Null.Or.Empty, "Response content is null or empty");

                var category = JObject.Parse(getByIdResponse.Content);
                Assert.That(category["_id"]?.ToString(), Is.EqualTo(categoryId), "Expected the same category ID");
                Assert.That(category["name"]?.ToString(), Is.EqualTo(name), "Expected the same category name");
            });

            // Step 4: Update the category
            var updateRequest = new RestRequest($"category/{categoryId}", Method.Put);
            var updatedCategoryName = name + "_updated";
            updateRequest.AddHeader("Authorization", $"Bearer {token}");
            updateRequest.AddJsonBody(new
            {
                name = updatedCategoryName
            });

            var updateResponse = client.Execute(updateRequest);
            Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code is not OK (200)");

            // Step 5: Verify the category is updated
            var getUpdatedCategoryRequest = new RestRequest($"category/{categoryId}", Method.Get);
            var getUpdatedCategoryResponse = client.Execute(getUpdatedCategoryRequest);

            Assert.Multiple(() =>
            {
                Assert.That(getUpdatedCategoryResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code is not OK (200)");
                Assert.That(getUpdatedCategoryResponse.Content, Is.Not.Null.Or.Empty, "Response content is null or empty");

                var updatedCategory = JObject.Parse(getUpdatedCategoryResponse.Content);
                Assert.That(updatedCategory["name"]?.ToString(), Is.EqualTo(updatedCategoryName), "Expected the same updated category name");
            });

            // Step 6: Delete the category
            var deleteRequest = new RestRequest($"category/{categoryId}", Method.Delete);
            deleteRequest.AddHeader("Authorization", $"Bearer {token}");

            var deleteResponse = client.Execute(deleteRequest);
            Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code is not OK (200)");

            // Step 7: Verify that the deleted category cannot be found
            var getDeletedCategoryRequest = new RestRequest($"category/{categoryId}", Method.Get);
            var getDeletedCategoryResponse = client.Execute(getDeletedCategoryRequest);

            Assert.That(getDeletedCategoryResponse.Content, Is.Empty.Or.EqualTo("null"), "Deleted category should not be found");
        }
        public void Dispose()
        {
            client?.Dispose();
        }
    }
}
