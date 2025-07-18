﻿using Newtonsoft.Json.Linq;
using RestSharp;

namespace EShopApi
{
    [TestFixture]
    public class UserManagementTests
    {
        private RestClient client;
        private string token;
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
            token = GlobalConstants.AuthenticateUser("admin@gmail.com", "admin@gmail.com");
            random = new Random();
        }

        [Test]
        public void UserLoginTest()
        {
            //Step 1: Login the user
            var loginRequest = new RestRequest("/user/login", Method.Post);
            loginRequest.AddJsonBody(new
            {
                Email = "john.doe@example.com",
                Password = "password123"
            });

            var loginResponse = client.Execute(loginRequest);
            Assert.That(loginResponse.IsSuccessful, Is.True, "Login failed");
            Assert.That(loginResponse.Content, Is.Not.Null, "Login response data is null");

            //Step 2: Extract user token
            string userToken = JObject.Parse(loginResponse.Content)["token"]?.ToString();
            Assert.That(userToken, Is.Not.Null.Or.Empty, "Login token is null or empty");

            //Step 3: Register random user
            var randomUserEmail = $"dichev{random.Next(999, 9999)}@example.com";
            var signupRequest = new RestRequest("/user/register", Method.Post);
            signupRequest.AddJsonBody(new
            {
                Firstname = "Radoslav" + random.Next(999, 9999),
                Lastname = "Dichev",
                Password = "NewUser@123",
                Mobile = $"+1234567{random.Next(999, 9999)}",
                Email = randomUserEmail
            });

            var signupResponse = client.Execute(signupRequest);
            Assert.That(signupResponse.IsSuccessful, Is.True, "Signup failed");

            //Step 4: Password reset request
            var forgotPasswordRequest = new RestRequest("/user/forgot-password-token", Method.Post);
            forgotPasswordRequest.AddJsonBody(new
            {
                Email = randomUserEmail
            });

            var forgotPasswordResponse = client.Execute(forgotPasswordRequest);
            Assert.That(forgotPasswordResponse.IsSuccessful, Is.True,
                "Forgot password request failed");
            Assert.That(forgotPasswordResponse.Content, Is.Not.Null,
                "Forgot password response data is null");
        }

        [Test]
        public void UserSignupLoginUpdateAndDeleteTest()
        {
            //Step 1: Sign up new random user
            var randomUserEmail = $"petrov{+random.Next(999, 9999)}@example.com";
            var signupRequest = new RestRequest("/user/register", Method.Post);
            signupRequest.AddJsonBody(new
            {
                Firstname = "Petar",
                Lastname = "Petrov",
                Password = "NewUser@123",
                Mobile = "+1234567955",
                Email = randomUserEmail
            });

            var signupResponse = client.Execute(signupRequest);
            Assert.That(signupResponse.IsSuccessful, Is.True, "Signup failed");
            Assert.That(signupResponse.Content, Is.Not.Null, "Signup response data is null");

            //Step 2: Login the newly created user
            var loginRequest = new RestRequest("/user/login", Method.Post);
            loginRequest.AddJsonBody(new
            {
                Email = randomUserEmail,
                Password = "NewUser@123"
            });

            var loginResponse = client.Execute(loginRequest);
            Assert.That(loginResponse.IsSuccessful, Is.True, "Login failed");
            Assert.That(loginResponse.Content, Is.Not.Null, "Login response data is null");

            //Step 3: Extract user token and user id
            var userToken = JObject.Parse(loginResponse.Content)["token"]?.ToString();
            Assert.That(userToken, Is.Not.Null.Or.Empty, "Login token is null or empty");

            var userId = JObject.Parse(loginResponse.Content)["_id"]?.ToString();
            Assert.That(userId, Is.Not.Null.Or.Empty, "User id is null or empty");

            //Step 4: Update the created user
            var updatedUserEmail = $"georgiev{random.Next(999, 9999)}@example.com";
            var updateUserRequest = new RestRequest("/user/edit-user", Method.Put);
            updateUserRequest.AddHeader("Authorization", $"Bearer {userToken}");
            updateUserRequest.AddJsonBody(new
            {
                Firstname = "Georgi",
                Lastname = "Georgiev",
                Mobile = "+1234567998",
                Email = updatedUserEmail
            });

            var updateUserResponse = client.Execute(updateUserRequest);
            Assert.That(updateUserResponse.IsSuccessful, Is.True, "User update failed");
            Assert.That(updateUserResponse.Content, Is.Not.Null,
                "Update response data is null");

            //Step 5: Delete the user
            var deleteUserRequest = new RestRequest($"/user/{userId}", Method.Delete);
            deleteUserRequest.AddHeader("Authorization", $"Bearer {token}");

            var deleteUserResponse = client.Execute(deleteUserRequest);
            Assert.That(deleteUserResponse.IsSuccessful, Is.True, "User deletion failed");
        }

        [Test]
        public void ProductAndUserCartTest()
        {
            //Step 1: Create new product
            var productTitle = "ProductTitle" + random.Next(999, 9999).ToString();
            var createProductRequest = new RestRequest("/product", Method.Post);
            createProductRequest.AddHeader("Authorization", $"Bearer {token}");
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
            Assert.That(createProductResponse.IsSuccessful, Is.True, "Product creation failed");

            var productContent = JObject.Parse(createProductResponse.Content);
            var productId = productContent["_id"]?.ToString();
            Assert.That(productId, Is.Not.Null.Or.Empty, "Product ID should not be null or empty");

            //Step 2: Login a user and get user token
            var loginRequest = new RestRequest("/user/login", Method.Post);
            loginRequest.AddJsonBody(new
            {
                Email = "john.doe@example.com",
                Password = "password123"
            });

            var loginResponse = client.Execute(loginRequest);
            Assert.That(loginResponse.IsSuccessful, Is.True, "Login failed");
            Assert.That(loginResponse.Content, Is.Not.Null, "Login response data is null");

            var userToken = JObject.Parse(loginResponse.Content)["token"]?.ToString();
            Assert.That(userToken, Is.Not.Null.Or.Empty, "Login token is null or empty");

            //Step 3: Add the newly created product to the user cart
            var addToCartRequest = new RestRequest($"/user/cart", Method.Post);
            addToCartRequest.AddHeader("Authorization", $"Bearer {userToken}");
            addToCartRequest.AddJsonBody(new
            {
                cart = new[]
                {
                    new { _id = productId, count = 1, color = "Red" }
                }
            });

            var addToCartResponse = client.Execute(addToCartRequest);
            Assert.That(addToCartResponse.IsSuccessful, Is.True, "Adding product to cart failed");

            //Step 4: Apply coupon to the cart
            var addCouponRequest = new RestRequest($"/user/cart/applycoupon", Method.Post);
            addCouponRequest.AddHeader("Authorization", $"Bearer {userToken}");
            addCouponRequest.AddJsonBody(new
            {
                Coupon = "BLACKFRIDAY"
            });

            var addCouponResponse = client.Execute(addCouponRequest);
            Assert.That(addCouponResponse.IsSuccessful, Is.True, "Adding coupon to cart failed");

            //Step 5: Delete the product
            var deleteProductRequest = new RestRequest($"/product/{productId}", Method.Delete);
            deleteProductRequest.AddHeader("Authorization", $"Bearer {token}");
            var deleteProductResponse = client.Execute(deleteProductRequest);
            Assert.That(deleteProductResponse.IsSuccessful, Is.True, "Product deletion failed");
        }
    }
}
