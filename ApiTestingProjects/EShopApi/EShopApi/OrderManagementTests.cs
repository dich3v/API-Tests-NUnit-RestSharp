using RestSharp;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace EShopApi
{
    [TestFixture]
    public class OrderManagementTests
    {
        private RestClient client;
        private string adminToken;
        private string userToken;

        [SetUp]
        public void Setup()
        {
            client = new RestClient(GlobalConstants.BaseUrl);
            adminToken = GlobalConstants.AuthenticateUser("admin@gmail.com", "admin@gmail.com");
            userToken = GlobalConstants.AuthenticateUser("john.doe@example.com", "password123");
        }

        [TearDown]
        public void Dispose()
        {
            client.Dispose();
        }

        [Test]
        public void Test_ComplexOrderLifecycle()
        {
            //Step 1: Get all products and pick one by id
            var getAllProductsRequest = new RestRequest("/product", Method.Get);
            getAllProductsRequest.AddHeader("Authorization", $"Bearer {adminToken}");

            var getAllProductsResponse = client.Execute(getAllProductsRequest);
            Assert.That(getAllProductsResponse.IsSuccessful, Is.True, "Get request failed");

            var products = JArray.Parse(getAllProductsResponse.Content);
            Assert.That(products.Count, Is.GreaterThan(0), "Should have at least 1 product in the system");

            var productId = products.First()["_id"]?.ToString();
            Assert.That(productId, Is.Not.Null.Or.Empty,
                "Product ID should not be null or empty");

            //Step 2: Add the product to the user cart
            var addToCartRequest = new RestRequest("/user/cart", Method.Post);
            addToCartRequest.AddHeader("Authorization", $"Bearer {userToken}");
            addToCartRequest.AddJsonBody(new
            {
                cart = new[]
                {
                    new { _id = productId, count = 2, color = "Red" }
                }
            });
            var addToCartResponse = client.Execute(addToCartRequest);
            Assert.That(addToCartResponse.IsSuccessful, Is.True,
                "Adding product to the cart failed");

            //Step 3: Apply coupon to the cart
            var applyCouponRequest = new RestRequest("/user/cart/applycoupon", Method.Post);
            applyCouponRequest.AddHeader("Authorization", $"Bearer {userToken}");
            applyCouponRequest.AddJsonBody(new
            {
                Coupon = "BLACKFRIDAY"
            });
            var applyCouponResponse = client.Execute(applyCouponRequest);
            Assert.That(applyCouponResponse.IsSuccessful, Is.True, "Applying coupon failed");

            //Step 4: Place the order
            var placeOrderRequest = new RestRequest("/user/cart/cash-order", Method.Post);
            placeOrderRequest.AddHeader("Authorization", $"Bearer {userToken}");
            placeOrderRequest.AddJsonBody(JsonConvert.SerializeObject(new
            {
                COD = true
            }));
            var placeOrderResponse = client.Execute(placeOrderRequest);
            Assert.That(placeOrderResponse.IsSuccessful, Is.True, "Placing order failed");

            //Step 5: Get all user orders
            var getOrderRequest = new RestRequest($"/user/get-orders", Method.Get);
            getOrderRequest.AddHeader("Authorization", $"Bearer {userToken}");
            var getOrderResponse = client.Execute(getOrderRequest);
            Assert.That(getOrderResponse.IsSuccessful, Is.True,
                "Failed to retrieve order details");

            var order = JObject.Parse(getOrderResponse.Content);
            var orderId = order["_id"]?.ToString();
            Assert.That(orderId, Is.Not.Null.Or.Empty, "Order ID should not be null or empty");

            //Step 6: Update the order status to cancelled
            var cancelOrderRequest = new RestRequest($"/user/order/update-order/{orderId}",
                Method.Put);
            cancelOrderRequest.AddHeader("Authorization", $"Bearer {adminToken}");
            cancelOrderRequest.AddJsonBody(new
            {
                Status = "Cancelled"
            });
            var cancelOrderResponse = client.Execute(cancelOrderRequest);
            Assert.That(cancelOrderResponse.IsSuccessful, Is.True, "Order cancellation failed");
        }
    }
}
