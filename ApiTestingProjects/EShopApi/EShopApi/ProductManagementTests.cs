using RestSharp;
using Newtonsoft.Json.Linq;
using System.Net;

namespace EShopApi
{
    [TestFixture]
    public class ProductManagementTests
    {
        private RestClient client;
        private string adminToken;
        private string userToken;
        private Random random;

        [TearDown]
        public void Dispose()
        {
            client.Dispose();
        }

        [SetUp]
        public void Setup()
        {
            client = new RestClient(GlobalConstants.BaseUrl);
            adminToken = GlobalConstants.AuthenticateUser("admin@gmail.com", "admin@gmail.com");
            userToken = GlobalConstants.AuthenticateUser("john.doe@example.com", "password123");
            random = new Random();
        }

        [Test]
        public void ProductLifecycleTest()
        {
            //Step 1: Create product
            var productTitle = "Test Product" + random.Next(999, 9999).ToString();
            var createProductRequest = new RestRequest("/product", Method.Post);
            createProductRequest.AddHeader("Authorization", $"Bearer {adminToken}");
            createProductRequest.AddJsonBody(new
            {
                Title = productTitle,
                Description = "This is test product",
                Slug = "test-product",
                Price = 9.99,
                Category = "Electronics",
                Brand = "Apple",
                Quantity = 10
            });

            var createProductResponse = client.Execute(createProductRequest);
            Assert.That(createProductResponse.IsSuccessful, Is.True,
                "Product creation failed");

            var productId = JObject.Parse(createProductResponse.Content)["_id"]?.ToString();
            Assert.That(productId, Is.Not.Null.Or.Empty,
                "Product ID should not be null or empty");

            //Step 2: Get the created product
            var getProductRequest = new RestRequest($"/product/{productId}", Method.Get);
            var getProductResponse = client.Execute(getProductRequest);
            Assert.That(getProductResponse.IsSuccessful, Is.True,
                "Failed to get product details");
            Assert.That(getProductResponse.Content, Is.Not.Null.Or.Empty,
                "Product details should not be null or empty");

            //Step 3: Update the created product
            var updateProductTitle = "Updated Product" + random.Next(999, 9999).ToString();
            var updateProductRequest = new RestRequest($"/product/{productId}", Method.Put);
            updateProductRequest.AddHeader("Authorization", $"Bearer {adminToken}");
            updateProductRequest.AddJsonBody(new
            {
                Title = updateProductTitle,
                Description = "Updated product description",
                Price = 39.99
            });

            var updateProductResponse = client.Execute(updateProductRequest);
            Assert.That(updateProductResponse.IsSuccessful, Is.True, "Product update failed");

            //Step 4: Delete the created product
            var deleteProductRequest = new RestRequest($"/product/{productId}", Method.Delete);
            deleteProductRequest.AddHeader("Authorization", $"Bearer {adminToken}");
            var deleteProductResponse = client.Execute(deleteProductRequest);
            Assert.That(deleteProductResponse.IsSuccessful, Is.True, "Product deletion failed");

            //Step 5: Verify the product was deleted
            var verifyDeleteRequest = new RestRequest($"/product/{productId}", Method.Get);
            var verifyDeleteResponse = client.Execute(verifyDeleteRequest);
            Assert.That(verifyDeleteResponse.Content, Is.Null.Or.EqualTo("null"),
                "Product still exists after deletion");
        }

        [Test]
        public void ProductRatingsLifecycleTest()
        {
            //Step 1: Get random product and extract its id
            var getProductListRequest = new RestRequest("/product", Method.Get);
            var getProductListResponse = client.Execute(getProductListRequest);
            Assert.That(getProductListResponse.IsSuccessful, Is.True,
                "Failed to get product list");

            var products = JArray.Parse(getProductListResponse.Content);
            Assert.That(products.Count, Is.GreaterThan(0), "No products found");

            var randomProduct = products[new Random().Next(products.Count)];
            var productId = randomProduct["_id"]?.ToString();
            Assert.That(productId, Is.Not.Null.Or.Empty, "Product ID should not be null or empty");

            //Step 2: Add review to the product and check its added
            var addReviewRequest = new RestRequest("/product/rating", Method.Put);
            addReviewRequest.AddHeader("Authorization", $"Bearer {userToken}");
            addReviewRequest.AddJsonBody(new
            {
                star = 5,
                prodId = productId,
                comment = "Excellent product!"
            });

            var addReviewResponse = client.Execute(addReviewRequest);
            Assert.That(addReviewResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK),
                "Adding rating failed");

            //Step 3: Check if the product is added to the wishlist
            var addToWishlistRequest = new RestRequest("/product/wishlist", Method.Put);
            addToWishlistRequest.AddHeader("Authorization", $"Bearer {userToken}");
            addToWishlistRequest.AddJsonBody(new
            {
                prodId = productId
            });

            var addToWishlistResponse = client.Execute(addToWishlistRequest);
            Assert.That(addToWishlistResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK),
                "Adding product to wishlist failed");
        }
    }
}
