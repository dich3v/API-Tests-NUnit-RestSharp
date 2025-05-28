using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RestSharp;
using System.Net;
using TravelersApi;

namespace TravelersApi
{
    [TestFixture]
    public class CategoryTests : IDisposable
    {
        private RestClient client;
        private string token;

        [SetUp]
        public void Setup()
        {
            client = new RestClient(GlobalConstants.BaseUrl);
            token = GlobalConstants.AuthenticateUser("john.doe@example.com", "password123");

            Assert.That(token, Is.Not.Null.Or.Empty, "Authentication token should not be null or empty");
        }

        [Test]
        public void Test_CategoryLifecycle()
        {
            // Step 1: Create a new category
            var createCategoryRequest = new RestRequest("category", Method.Post);
            createCategoryRequest.AddHeader("Authorization", $"Bearer {token}");
            createCategoryRequest.AddJsonBody(new
            {
                name = "New category"
            });

            var createCategoryResponse = client.Execute(createCategoryRequest);

            Assert.That(createCategoryResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code is not OK (200)");
            Assert.That(createCategoryResponse.Content, Is.Not.Null.Or.Empty, "Response content should not be null or empty");

            var createCategory = JObject.Parse(createCategoryResponse.Content);
            var categoryId = createCategory["_id"]?.ToString();

            Assert.That(categoryId, Is.Not.Null.Or.Empty);

            // Step 2: Get all categories
            var getAllCategoriesRequest = new RestRequest("category", Method.Get);

            var getAllCategoriesResponse = client.Execute(getAllCategoriesRequest);

            Assert.Multiple(() =>
            {
                Assert.That(getAllCategoriesResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code is not OK (200)");
                Assert.That(getAllCategoriesResponse.Content, Is.Not.Null.Or.Empty, "Response content should not be null or empty");

                var categories = JArray.Parse(getAllCategoriesResponse.Content);

                Assert.That(categories.Type, Is.EqualTo(JTokenType.Array));

                Assert.That(categories.Count, Is.GreaterThan(0));

                var createdCategory = categories.FirstOrDefault(c => c["name"]?.ToString() == "New category");

                Assert.That(createdCategory, Is.Not.Null);
            });

            // Step 3: Get category by ID
            var getCategoryById = new RestRequest($"category/{categoryId}", Method.Get);

            var getCategoryByIdResponse = client.Execute(getCategoryById);

            Assert.Multiple(() =>
            {
                Assert.That(getCategoryByIdResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code is not OK (200)");
                Assert.That(getCategoryByIdResponse.Content, Is.Not.Null.Or.Empty, "Response content should not be null or empty");

                var category = JObject.Parse(getCategoryByIdResponse.Content);

                Assert.That(category["_id"]?.ToString(), Is.EqualTo(categoryId));

                Assert.That(category["name"]?.ToString(), Is.EqualTo("New category"));
            });

            // Step 4: Update the category
            var updateRequest = new RestRequest($"category/{categoryId}", Method.Put);
            updateRequest.AddHeader("Authorization", $"Bearer {token}");
            updateRequest.AddJsonBody(new
            {
                name = "Updated new category"
            });
            var updateResponse = client.Execute(updateRequest);

            Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code is not OK (200)");

            // Step 5: Verify update
            var getEditedCategoryRequest = new RestRequest($"category/{categoryId}", Method.Get);

            var getEditedCategoryResponse = client.Execute(getEditedCategoryRequest);

            Assert.Multiple(() =>
            {
                Assert.That(getEditedCategoryResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code is not OK (200)");
            });
            Assert.That(getEditedCategoryResponse.Content, Is.Not.Null.Or.Empty, "Response content should not be null or empty");

            var updatedCategoryGet = JObject.Parse(getEditedCategoryResponse.Content);

            Assert.That(updatedCategoryGet["name"]?.ToString(), Is.EqualTo("Updated new category"));

            // Step 6: Delete the category
            var deleteCategory = new RestRequest($"category/{categoryId}", Method.Delete);
            deleteCategory.AddHeader("Authorization", $"Bearer {token}");
            var deleteResponse = client.Execute(deleteCategory);

            Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code is not OK (200)");

            // Step 7: Verify that the deleted category cannot be found
            var verifyDeleteRequest = new RestRequest($"category/{categoryId}", Method.Get);

            var verifyResponse = client.Execute(verifyDeleteRequest);

            Assert.That(verifyResponse.Content, Is.EqualTo("null"));
        }
        public void Dispose()
        {
            client?.Dispose();
        }
    }
}