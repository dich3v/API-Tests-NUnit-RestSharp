using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RestSharp;
using System.Net;

namespace BookStoreApi
{
    [TestFixture]
    public class BookCategoryTests : IDisposable
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
        public void Test_BookCategoryLifecycle()
        {
            // Step 1: Create a new book category
            var createRequest = new RestRequest("category", Method.Post);
            createRequest.AddHeader("Authorization", $"Bearer {token}");
            createRequest.AddJsonBody(new
            {
                title = "Fictional Literature"
            });
            var createResponse = client.Execute(createRequest);
            Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Response status code is not as axpected(200)");

            var createdCategory = JObject.Parse(createResponse.Content);

            var categoryId = createdCategory["_id"]?.ToString();
            Assert.That(categoryId, Is.Not.Null.Or.Empty, "Category id is null or empty");

            var categoryTitle = createdCategory["title"]?.ToString();
            Assert.That(categoryTitle, Is.EqualTo("Fictional Literature"), "Response does not contain title - Fictional Literature");
            // Step 2: Retrieve all book categories and verify the newly created category is present
            var getAllCategoriesRequest = new RestRequest("category", Method.Get);
            var getAllCategoriesResponse = client.Execute(getAllCategoriesRequest);

            Assert.Multiple(() =>
            {
                Assert.That(getAllCategoriesResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Response status code is not as expected(200)");
                Assert.That(getAllCategoriesResponse.Content, Is.Not.Empty, "Response content is empty");

                var categories = JArray.Parse(getAllCategoriesResponse.Content);
                Assert.That(categories.Type, Is.EqualTo(JTokenType.Array), "Expected response content should be a JSON array");
                Assert.That(categories.Count, Is.GreaterThan(0), "Expected at least one category in the response");

                var newCategory = categories.FirstOrDefault(c => c["title"]?.ToString() == "Fictional Literature");
                Assert.That(newCategory, Is.Not.Null, "Category with title Fictional Literature not found in the response");

                Assert.That(newCategory["_id"], Is.Not.Null, "Category id is not found in the response");
                Assert.That(newCategory["_id"]?.ToString(), Is.EqualTo(categoryId), "Category ID is not the same");
            });
            // Step 3: Update the category title
            var updateRequest = new RestRequest($"category/{categoryId}", Method.Put);
            updateRequest.AddHeader("Authorization", $"Bearer {token}");
            updateRequest.AddJsonBody(new
            {
                title = "Updated Fictional Literature"
            });

            var updateResponse = client.Execute(updateRequest);
            Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Response status code is not as expected(200)");
            // Step 4: Verify that the category details have been updated
            var verifyUpdatedRequest = new RestRequest($"category/{categoryId}", Method.Get);
            var verifyUpdateResponse = client.Execute(verifyUpdatedRequest);

            Assert.Multiple(() =>
            {
                Assert.That(verifyUpdateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Response status code is not as expected(200)");
                Assert.That(verifyUpdateResponse.Content, Is.Not.Empty, "Response content is empty");

                var verifyCategory = JObject.Parse(verifyUpdateResponse.Content);
                Assert.That(verifyCategory["title"]?.ToString(), Is.EqualTo("Updated Fictional Literature"), "Expected updated category name");
            });
            // Step 5: Delete the category and validate it's no longer accessible
            var deleteRequest = new RestRequest($"category/{categoryId}", Method.Delete);
            deleteRequest.AddHeader("Authorization", $"Bearer {token}");

            var deleteResponse = client.Execute(deleteRequest);
            Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Response status code is not as expected(200)");
            // Step 6: Verify that the deleted category cannot be found
            var verifyDeletedRequest = new RestRequest($"category/{categoryId}", Method.Get);
            var verifyDeletedResponse = client.Execute(verifyDeletedRequest);

            Assert.That(verifyDeletedResponse.Content, Is.Empty.Or.EqualTo("null"), "Deleted category should not exist");
        }
        public void Dispose()
        {
            client?.Dispose();
        }
    }
}