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
    }
}
