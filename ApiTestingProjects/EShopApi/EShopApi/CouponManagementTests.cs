using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net;

namespace EShopApi
{
    [TestFixture]
    public class CouponManagementTests
    {
        private RestClient client;
        private string adminToken;
        private string userToken;
        private Random random;

        [SetUp]
        public void Setup()
        {
            client = new RestClient(GlobalConstants.BaseUrl);
            adminToken = GlobalConstants.AuthenticateUser("admin@gmail.com", "admin@gmail.com");
            userToken = GlobalConstants.AuthenticateUser("john.doe@example.com", "password123");
            random = new Random();
        }

        [TearDown]
        public void Dispose()
        {
            client.Dispose();
        }

        [Test]
        public void Test_CouponLifecycle()
        {
            //Step 1: Get all products
            var getAllProductsRequest = new RestRequest("/product", Method.Get);
            getAllProductsRequest.AddHeader("Authorization", $"Bearer {adminToken}");

            var getAllProductsResponse = client.Execute(getAllProductsRequest);
            Assert.That(getAllProductsResponse.IsSuccessful, Is.True, "Get request failed");

            var products = JArray.Parse(getAllProductsResponse.Content);
            Assert.That(products.Count, Is.GreaterThanOrEqualTo(2),
                "Products should be more or equal to 2");

            //Step 2: Get two random products

            var productIds = products.Select(p => p["_id"]?.ToString()).ToList();
            var firstProductId = productIds[random.Next(productIds.Count)];
            var secondProductId = productIds[random.Next(productIds.Count)];

            //Step 3: Ensure the two selected products are not the same
            while (firstProductId == secondProductId)
            {
                secondProductId = productIds[random.Next(productIds.Count)];
            }

            //Step 4: Create a random coupon
            var couponName = "DISCOUNT20-" + random.Next(999, 9999).ToString();
            var createCouponRequest = new RestRequest("/coupon", Method.Post);
            createCouponRequest.AddHeader("Authorization", $"Bearer {adminToken}");
            createCouponRequest.AddJsonBody(new
            {
                Name = couponName,
                Discount = 10,
                Expiry = "2025-06-04"
            });

            var createCouponResponse = client.Execute(createCouponRequest);
            Assert.That(createCouponResponse.IsSuccessful, Is.True, "Coupon creation failed");

            var couponId = JObject.Parse(createCouponResponse.Content)["_id"]?.ToString();
            Assert.That(couponId, Is.Not.Null.Or.Empty, "Coupon ID should not be null or empty");

            //Step 5: Create shopping cart and add the two random products inside
            var createCartRequest = new RestRequest("/user/cart", Method.Post);
            createCartRequest.AddHeader("Authorization", $"Bearer {adminToken}");
            createCartRequest.AddJsonBody(new
            {
                cart = new[]
                {
                    new { _id = firstProductId, count = 1, color = "red" },
                    new { _id = secondProductId, count = 2, color = "blue" }
                }
            });

            var createCartResponse = client.Execute(createCartRequest);
            Assert.That(createCartResponse.IsSuccessful, Is.True, "Cart creation failed");

            //Step 6: Apply the coupon to the user cart
            var applyCouponRequest = new RestRequest("/user/cart/applycoupon", Method.Post);
            applyCouponRequest.AddHeader("Authorization", $"Bearer {adminToken}");
            applyCouponRequest.AddJsonBody(new
            {
                Coupon = couponName
            });

            var applyCouponResponse = client.Execute(applyCouponRequest);

            Assert.That(applyCouponResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK),
                "Status code is not OK (200)");

            //Step 7: Delete the coupon
            var deleteCouponRequest = new RestRequest($"/coupon/{couponId}", Method.Delete);
            deleteCouponRequest.AddHeader("Authorization", $"Bearer {adminToken}");
            var deleteCouponResponse = client.Execute(deleteCouponRequest);
            Assert.That(deleteCouponResponse.IsSuccessful, Is.True, "Deletion of the coupon failed");

            //Step 8: Verify coupon is deleted
            var verifyCouponRequest = new RestRequest($"/coupon/{couponId}", Method.Get);
            verifyCouponRequest.AddHeader("Authorization", $"Bearer {adminToken}");
            var verifyCouponResponse = client.Execute(verifyCouponRequest);
            Assert.That(verifyCouponResponse.Content, Is.Null.Or.EqualTo("null"),
                "Coupon is not deleted");
        }

        [Test]
        public void Test_CouponApplicationToOrder()
        {
            //Step 1: Get all products and pick a product by ID
            var getAllProductsRequest = new RestRequest("/product", Method.Get);
            getAllProductsRequest.AddHeader("Authorization", $"Bearer {adminToken}");

            var getAllProductsResponse = client.Execute(getAllProductsRequest);
            Assert.That(getAllProductsResponse.IsSuccessful, Is.True, "Get request failed");

            var products = JArray.Parse(getAllProductsResponse.Content);
            Assert.That(products.Count, Is.GreaterThan(0), "There should be at least 1 product");

            var productId = products.First()["_id"]?.ToString();
            Assert.That(productId, Is.Not.Null.Or.Empty, "Product ID should not be null or empty");

            //Step 2: Create a new coupon
            var couponName = "SAVE10-" + random.Next(999, 9999).ToString();
            var createCouponRequest = new RestRequest("/coupon", Method.Post);
            createCouponRequest.AddHeader("Authorization", $"Bearer {adminToken}");
            createCouponRequest.AddJsonBody(new
            {
                Name = couponName,
                Discount = 20,
                Expiry = "2026-02-18"
            });

            var createCouponResponse = client.Execute(createCouponRequest);
            Assert.That(createCouponResponse.IsSuccessful, Is.True, "Coupon creation failed");

            //Step 3: Add the product to the user cart
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
            Assert.That(addToCartResponse.IsSuccessful, Is.True, "Adding product to cart failed");

            //Step 4: Apply the coupon to the cart
            var applyCouponRequest = new RestRequest("/user/cart/applycoupon", Method.Post);
            applyCouponRequest.AddHeader("Authorization", $"Bearer {userToken}");
            applyCouponRequest.AddJsonBody(new
            {
                Coupon = couponName
            });

            var applyCouponResponse = client.Execute(applyCouponRequest);
            Assert.That(applyCouponResponse.IsSuccessful, Is.True, "Applying coupon to cart failed");

            //Step 5: Place the order with the applied coupon
            var placeOrderRequest = new RestRequest("/user/cart/cash-order", Method.Post);
            placeOrderRequest.AddHeader("Authorization", $"Bearer {userToken}");
            placeOrderRequest.AddJsonBody(JsonConvert.SerializeObject(new
            {
                COD = true,
                couponApplied = false
            }));

            var placeOrderResponse = client.Execute(placeOrderRequest);
            Assert.That(placeOrderResponse.IsSuccessful, Is.True, "Placing order failed");
        }
    }
}
