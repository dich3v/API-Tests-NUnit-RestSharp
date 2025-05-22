using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NUnit.Framework;

namespace EShopApi
{
    [TestFixture]
    public class ProductApiTests : IDisposable
    {
        private RestClient client;
        private string token;

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
        public void Test_GetAllProducts()
        {
            var request = new RestRequest("product", Method.Get);
            var response = client.Execute(request);

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code is not OK");
                Assert.That(response.Content, Is.Not.Empty, "Response content is empty");

                var content = JArray.Parse(response.Content);

                var productTitles = new[]
                {
                    "Smartphone Alpha", "Wireless Headphones", "Gaming Laptop", "4K Ultra HD TV", "Smartwatch Pro"
                };
                foreach (var title in productTitles)
                {
                    Assert.That(content.ToString(), Does.Contain(title));
                }

                var expectedPrices = new Dictionary<string, decimal>
                {
                    { "Smartphone Alpha", 999},
                    { "Wireless Headphones", 199},
                    { "Gaming Laptop", 1499},
                    { "4K Ultra HD TV", 899},
                    { "Smartwatch Pro", 299}
                };

                foreach (var product in content)
                {
                    var title = product["title"].ToString();
                    if (expectedPrices.ContainsKey(title))
                    {
                        Assert.That(product["price"].Value<decimal>(), Is.EqualTo(expectedPrices[title]));
                    }
                }
            });
        }

        [Test, Order(2)]
        public void Test_AddProduct()
        {
            var request = new RestRequest("product", Method.Post);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddJsonBody(new
            {
                title = "New Product",
                slug = "new-product",
                description = "New Description",
                price = 99.99,
                category = "test",
                brand = "test",
                quantity = 50
            });

            var response = client.Execute(request);

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code is not OK");
                Assert.That(response.Content, Is.Not.Empty, "Response content is empty");

                var content = JObject.Parse(response.Content);

                Assert.That(content["title"].ToString(), Is.EqualTo("New Product"));
                Assert.That(content["slug"].ToString(), Is.EqualTo("new-product"));
                Assert.That(content["description"].ToString(), Is.EqualTo("New Description"));
                Assert.That(content["price"].Value<decimal>(), Is.EqualTo(99.99));
                Assert.That(content["category"].ToString(), Is.EqualTo("test"));
                Assert.That(content["brand"].ToString(), Is.EqualTo("test"));
                Assert.That(content["quantity"].Value<int>(), Is.EqualTo(50));
            });
        }

        [Test, Order(3)]
        public void Test_GetProductInvalidId()
        {
            var request = new RestRequest("product/{id}", Method.Get);
            request.AddUrlSegment("id", "InvalidId");

            var response = client.Execute(request);

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError).Or.EqualTo(HttpStatusCode.BadRequest));
                Assert.That(response.Content, Does.Contain("This id is not valid or not Found"));
            });
        }

        [Test, Order(4)]
        public void Test_UpdateProduct()
        {
            var getRequest = new RestRequest("product", Method.Get);
            var getResponse = client.Execute(getRequest);

            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code is not OK");
            Assert.That(getResponse.Content, Is.Not.Empty, "Response content is empty");

            var products = JArray.Parse(getResponse.Content);
            var productToUpdate = products.FirstOrDefault(p => p["slug"].ToString() == "new-product");

            Assert.That(productToUpdate, Is.Not.Null);

            var productId = productToUpdate["_id"]?.ToString();

            var request = new RestRequest("product/{id}", Method.Put);
            request.AddUrlSegment("id", productId);
            request.AddHeader("Authorization", $"Bearer {token}");
            request.AddJsonBody(new
            {
                title = "Updated title",
                slug = "updated-product-123",
                description = "Updated description",
                price = 1000.0,
                quantity = 10,
            });

            var response = client.Execute(request);

            Assert.That(response.IsSuccessful);
        }

        [Test, Order(5)]
        public void Test_UpdateProductInvalidId()
        {
            var updateRequest = new RestRequest("product/{id}", Method.Put);
            updateRequest.AddUrlSegment("id", "InvalidId");
            updateRequest.AddHeader("Authorization", $"Bearer {token}");
            updateRequest.AddJsonBody(new 
            { 
                title = "Title", 
                description = "Description", 
                price = 1 
            });

            var updatedResponse = client.Execute(updateRequest);

            Assert.Multiple(() =>
            {
                Assert.That(updatedResponse.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError).Or.EqualTo(HttpStatusCode.BadRequest));
                Assert.That(updatedResponse.Content, Does.Contain("This id is not valid or not Found").Or.Contain("Invalid ID"));
            });
        }

        [Test, Order(6)]
        public void Test_DeleteProduct()
        {
            var getRequest = new RestRequest("product", Method.Get);
            var getResponse = client.Execute(getRequest);

            Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code is not OK");
            Assert.That(getResponse.Content, Is.Not.Empty, "Response content is empty");

            var products = JArray.Parse(getResponse.Content);
            var productToDelete = products.FirstOrDefault(p => p["slug"].ToString() == "new-product");

            Assert.That(productToDelete, Is.Not.Null);

            var productId = productToDelete["_id"]?.ToString();

            var deleteRequest = new RestRequest("product/{id}", Method.Delete);
            deleteRequest.AddUrlSegment("id", productId);

            deleteRequest.AddHeader("Authorization", $"Bearer {token}");

            var deleteResponse = client.Execute(deleteRequest);

            Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var verifyGetRequest = new RestRequest("product/{id}", Method.Get);

            verifyGetRequest.AddUrlSegment("id", productId);

            var verifyResponse = client.Execute(verifyGetRequest);

            Assert.That(verifyResponse.Content, Is.EqualTo("null"));
        }
    }
}