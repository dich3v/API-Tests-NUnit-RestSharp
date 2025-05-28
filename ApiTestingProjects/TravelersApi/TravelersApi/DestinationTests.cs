using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RestSharp;
using System.Net;
using System.Xml.Linq;
using TravelersApi;

namespace TravelersApi
{
    [TestFixture]
    public class DestinationTests : IDisposable
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
        public void Test_GetAllDestinations()
        {
            var request = new RestRequest("destination", Method.Get);
            var response = client.Execute(request);

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code is not OK (200)");            
                Assert.That(response.Content, Is.Not.Null.Or.Empty, "Response content should not be null or empty");

                var destination = JArray.Parse(response.Content);

                Assert.That(destination.Type, Is.EqualTo(JTokenType.Array), "The response content is not JSON array");
                Assert.That(destination.Count, Is.GreaterThan(0), "Destinations count should not be 0");

                foreach (var dest in destination)
                {
                    Assert.That(dest["name"]?.ToString(), Is.Not.Null.Or.Empty, "Property name is not as expected");
                    Assert.That(dest["location"]?.ToString(), Is.Not.Null.Or.Empty, "Property location is not as expected");
                    Assert.That(dest["description"]?.ToString(), Is.Not.Null.Or.Empty, "Property description is not as expected");
                    Assert.That(dest["category"]?.ToString(), Is.Not.Null.Or.Empty, "Property category is not as expected");
                    Assert.That(dest["attractions"]?.Type, Is.EqualTo(JTokenType.Array), "Attractions property is not JSON array");
                    Assert.That(dest["bestTimeToVisit"]?.ToString(), Is.Not.Null.Or.Empty, "Property bestTimeToVisit is not as expected");
                }
            });
        }

        [Test]
        public void Test_GetDestinationByName()
        {
            var getRequest = new RestRequest("destination", Method.Get);
            var getResponse = client.Execute(getRequest);

            Assert.Multiple(() =>
            {
                Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Response status code is not as expected");
                Assert.That(getResponse.Content, Is.Not.Null.Or.Empty, "Response content is not as expected");

                var destinations = JArray.Parse(getResponse.Content);
                var destination = destinations.FirstOrDefault(d => d["name"]?.ToString() == "New York City");

                Assert.That(destination["location"]?.ToString(), Is.EqualTo("New York, USA"), "Location property does not have the correct value");
                Assert.That(destination["description"]?.ToString(), Is.EqualTo("The largest city in the USA, known for its skyscrapers, culture, and entertainment."), "Destination property does not have the correct value");
            });
        }

        [Test]
        public void Test_AddDestination()
        {
            var getCategoryRequest = new RestRequest("category", Method.Get);
            var getCategoryResponse = client.Execute(getCategoryRequest);
            var categories = JArray.Parse(getCategoryResponse.Content);
            var firstcategory = categories.First();
            var categoryId = firstcategory["_id"]?.ToString();

            var addRequest = new RestRequest("destination", Method.Post);
            addRequest.AddHeader("Authorization", $"Bearer {token}");

            var name = "New name";
            var location = "New location";
            var description = "New description";
            var bestTimeToVisit = "Summer";
            var attractions = new[] { "Attraction1", "Attraction2" };

            addRequest.AddJsonBody(new
            {
                name,
                location,
                description,
                bestTimeToVisit,
                attractions,
                category = categoryId
            });

            var addResponse = client.Execute(addRequest);

            Assert.Multiple(() =>
            {
                Assert.That(addResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Response status code is not as expected");
                Assert.That(addResponse.Content, Is.Not.Null.Or.Empty, "Response content is not as expected");
            });

            var createdDestination = JObject.Parse(addResponse.Content);
            Assert.That(createdDestination["_id"]?.ToString(), Is.Not.Empty);

            var createdDestinationId = createdDestination["_id"]?.ToString();
            var getDestinationRequest = new RestRequest($"/destination/{createdDestinationId}", Method.Get);

            var getResponse = client.Execute(getDestinationRequest);

            Assert.Multiple(() =>
            {
                Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code is not as expected");
                Assert.That(getResponse.Content, Is.Not.Null.Or.Empty, "Response content is not as expected");

                var destination = JObject.Parse(getResponse.Content);

                Assert.That(destination["name"]?.ToString(), Is.EqualTo(name));
                Assert.That(destination["location"]?.ToString(), Is.EqualTo(location));
                Assert.That(destination["description"]?.ToString(), Is.EqualTo(description));
                Assert.That(destination["bestTimeToVisit"]?.ToString(), Is.EqualTo(bestTimeToVisit));
                Assert.That(destination["category"]?.ToString(), Is.Not.Null.Or.Empty);
                Assert.That(destination["category"]["_id"]?.ToString(), Is.EqualTo(categoryId));
                Assert.That(destination["attractions"].Count, Is.EqualTo(2));
                Assert.That(destination["attractions"]?.Type, Is.EqualTo(JTokenType.Array));
                Assert.That(destination["attractions"][0]?.ToString(), Is.EqualTo("Attraction1"));
                Assert.That(destination["attractions"][1]?.ToString(), Is.EqualTo("Attraction2"));
            });
        }
        [Test]
        public void Test_UpdateDestination()
        {
            var getRequest = new RestRequest("destination", Method.Get);
            var getResponse = client.Execute(getRequest);

            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status is not OK");
            Assert.That(getResponse.Content, Is.Not.Null.Or.Empty, "Response content is null or empty");

            var destinations = JArray.Parse(getResponse.Content);

            var destinationToUpdate = destinations.FirstOrDefault(d => d["name"]?.ToString() == "New name");

            Assert.That(destinationToUpdate, Is.Not.Null);

            var destinationId = destinationToUpdate["_id"]?.ToString();

            var updateRequest = new RestRequest($"destination/{destinationId}", Method.Put);
            updateRequest.AddHeader("Authorization", $"Bearer {token}");
            updateRequest.AddJsonBody(new
            {
                name = "Updated name",
                bestTimeToVisit = "Winter"
            });

            var updateResponse = client.Execute(updateRequest);

            Assert.Multiple(() =>
            {
                Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status is not OK");
                Assert.That(updateResponse.Content, Is.Not.Null.Or.Empty, "Response content is null or empty");

                var updatedDestination = JObject.Parse(updateResponse.Content);

                Assert.That(updatedDestination["name"]?.ToString(), Is.EqualTo("Updated name"));
                Assert.That(updatedDestination["bestTimeToVisit"]?.ToString(), Is.EqualTo("Winter"));
            });
        }
        [Test]
        public void Test_DeleteDestination()
        {
            var getRequest = new RestRequest("destination", Method.Get);
            var getResponse = client.Execute(getRequest);

            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status is not OK");
            Assert.That(getResponse.Content, Is.Not.Null.Or.Empty, "Response content is null or empty");

            var destinations = JArray.Parse(getResponse.Content);

            var destinationToDelete = destinations.FirstOrDefault(d => d["name"]?.ToString() == "Updated name");

            Assert.That(destinationToDelete, Is.Not.Null);

            var destinationId = destinationToDelete["_id"]?.ToString();
            var deleteRequest = new RestRequest($"destination/{destinationId}", Method.Delete);

            deleteRequest.AddHeader("Authorization", $"Bearer {token}");

            var deleteResponse = client.Execute(deleteRequest);

            Assert.Multiple(() =>
            {
                Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

                var verifyRequest = new RestRequest($"destination/{destinationId}");

                var verifyResponse = client.Execute(verifyRequest);

                Assert.That(verifyResponse.Content, Is.EqualTo("null"));
            });
        }
        public void Dispose()
        {
            client?.Dispose();
        }
    }
}
