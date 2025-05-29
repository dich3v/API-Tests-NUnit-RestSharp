using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RecipesApi;
using RestSharp;
using System.Net;
using System.Runtime.Intrinsics.X86;

namespace RecipesApi
{
    [TestFixture]
    public class RecipeTests : IDisposable
    {
        private RestClient client;
        private string token;
        private Random random;
        private string recipeTitle;

        [SetUp]
        public void Setup()
        {
            client = new RestClient(GlobalConstants.BaseUrl);
            token = GlobalConstants.AuthenticateUser("john.doe@example.com", "password123");

            Assert.That(token, Is.Not.Null.Or.Empty);
            random = new Random();

        }

        [Test, Order(1)]
        public void Test_GetAllRecipes()
        {
            var request = new RestRequest("/recipe", Method.Get);
            var response = client.Execute(request);

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Response code is not OK (200)");
                Assert.That(response.Content, Is.Not.Null.Or.Empty, "Response content is not as expected");

                var recipes = JArray.Parse(response.Content);
                Assert.That(recipes.Type, Is.EqualTo(JTokenType.Array), "The response content is not array");
                Assert.That(recipes.Count, Is.GreaterThan(0), "Should contain at least 1 recipe");

                foreach(var recipe in recipes)
                {
                    Assert.That(recipe["title"]?.ToString(), Is.Not.Null.Or.Empty, "Title is null or empty");
                    Assert.That(recipe["ingredients"], Is.Not.Null.Or.Empty, "Ingredients is null or empty");
                    Assert.That(recipe["instructions"], Is.Not.Null.Or.Empty, "Instructions is null or empty");
                    Assert.That(recipe["cookingTime"], Is.Not.Null.Or.Empty, "cookingTime is null or empty");
                    Assert.That(recipe["servings"], Is.Not.Null.Or.Empty, "Servings is null or empty");
                    Assert.That(recipe["category"], Is.Not.Null.Or.Empty, "Category is null or empty");

                    Assert.That(recipe["ingredients"]?.Type, Is.EqualTo(JTokenType.Array), "Ingredients is not JSON array");
                    Assert.That(recipe["instructions"]?.Type, Is.EqualTo(JTokenType.Array), "Instructions is not JSON array");
                }       
            });
        }
        [Test, Order(2)]
        public void Test_GetRecipeByTitle()
        {
            var expectedCookingTime = 25;
            var expectedServings = 24;
            var expectedIngredientsCount = 9;
            var expectedInstructionsCount = 7;
            var title = "Chocolate Chip Cookies";

            var getAllRequest = new RestRequest("/recipe", Method.Get);
            var getAllResponse = client.Execute(getAllRequest);  
            
                Assert.That(getAllResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Response code is not OK (200)");
                Assert.That(getAllResponse.Content, Is.Not.Null.Or.Empty, "Response content is not as expected");

            var recipes = JArray.Parse(getAllResponse.Content);
            var recipe = recipes.FirstOrDefault(r => r["title"]?.ToString() == title);

            Assert.Multiple(() =>
            {
                Assert.That(recipe, Is.Not.Null, $"Recipe with title {title} is null");
                Assert.That(recipe["cookingTime"].Value<int>(), Is.EqualTo(expectedCookingTime), "CookingTime is not as expected");
                Assert.That(recipe["servings"].Value<int>(), Is.EqualTo(expectedServings), "Servings is not as expected");
                Assert.That(recipe["ingredients"].Count(), Is.EqualTo(expectedIngredientsCount), "Ingredients is not as expected");
                Assert.That(recipe["instructions"].Count(), Is.EqualTo(expectedInstructionsCount), "Instructions is not as expected");
            });
        }
        [Test, Order(3)]
        public void Test_AddRecipe()
        {
            var getAllCategoriesRequest = new RestRequest("/category", Method.Get);
            var getAllCategoriesResponse = client.Execute(getAllCategoriesRequest);

            Assert.That(getAllCategoriesResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Response code is not OK (200)");
            Assert.That(getAllCategoriesResponse.Content, Is.Not.Null.Or.Empty, "Response content is not as expected");

            var categories = JArray.Parse(getAllCategoriesResponse.Content);
            var categoryId = categories.First()["_id"]?.ToString();

            var addRequest = new RestRequest("/recipe", Method.Post);
            addRequest.AddHeader("Authorization", $"Bearer {token}");
            recipeTitle = $"recipeTitle_{random.Next(100, 999)}";
            var cookingTime = 20;
            var servings = 5;
            var ingredients = new[] { new { name = "Test", quantity = "50gr" } };
            var instructions = new[] { new { step = "test" } };

            addRequest.AddJsonBody(new
            {
                title = recipeTitle,
                cookingTime,
                servings,
                ingredients,
                instructions,
                category = categoryId
            });

            var addResponse = client.Execute(addRequest);

                Assert.That(addResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Response code is not OK (200)");
                Assert.That(addResponse.Content, Is.Not.Null.Or.Empty, "Response content is not as expected");

            var recipe = JObject.Parse(addResponse.Content);
            var recipeId = recipe["_id"]?.ToString();

            var getByIdRequest = new RestRequest($"/recipe/{recipeId}", Method.Get);
            var getByIdResponse = client.Execute(getByIdRequest);

            Assert.Multiple(() =>
            {
                Assert.That(getByIdResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Response code is not OK (200)");
                Assert.That(getByIdResponse.Content, Is.Not.Null.Or.Empty, "Response content is not as expected");

                var createdRecipe = JObject.Parse(getByIdResponse.Content);

                Assert.That(createdRecipe["title"].ToString(), Is.EqualTo(recipeTitle));
                Assert.That(createdRecipe["cookingTime"].Value<int>(), Is.EqualTo(cookingTime));
                Assert.That(createdRecipe["servings"].Value<int>(), Is.EqualTo(servings));
                Assert.That(createdRecipe["category"]?["_id"]?.ToString(), Is.EqualTo(categoryId));

                Assert.That(createdRecipe["ingredients"]?.Type, Is.EqualTo(JTokenType.Array));
                Assert.That(createdRecipe["ingredients"].Count, Is.EqualTo(ingredients.Count()));
                Assert.That(createdRecipe["ingredients"]?[0]["name"]?.ToString(), Is.EqualTo(ingredients[0].name));
                Assert.That(createdRecipe["ingredients"]?[0]["quantity"]?.ToString(), Is.EqualTo(ingredients[0].quantity));

                Assert.That(createdRecipe["instructions"]?.Type, Is.EqualTo(JTokenType.Array));
                Assert.That(createdRecipe["instructions"].Count, Is.EqualTo(instructions.Count()));
                Assert.That(createdRecipe["instructions"]?[0]["step"]?.ToString(), Is.EqualTo(instructions[0].step));
            });          
        }
        [Test, Order(4)]
        public void Test_UpdateRecipe()
        {
            var getAllRequest = new RestRequest("/recipe", Method.Get);
            var getAllResponse = client.Execute(getAllRequest);

                Assert.That(getAllResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Response code is not OK (200)");
                Assert.That(getAllResponse.Content, Is.Not.Null.Or.Empty, "Response content is not as expected");

            var recipes = JArray.Parse(getAllResponse.Content);
            var recipe = recipes.FirstOrDefault(r => r["title"]?.ToString() == recipeTitle);

                Assert.That(recipe, Is.Not.Null, $"Recipe with title {recipeTitle} is null");

            var recipeId = recipe["_id"].ToString();

            var updateRequest = new RestRequest($"/recipe/{recipeId}", Method.Put);
            updateRequest.AddHeader("Authorization", $"Bearer {token}");
            recipeTitle = recipeTitle + "_updated";
            var updatedServings = 30;
            updateRequest.AddJsonBody(new
            {
                title = recipeTitle,
                servings = updatedServings
            });
            
            var updateResponse = client.Execute(updateRequest);

            Assert.Multiple(() =>
            {
                Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Response code is not OK (200)");
                Assert.That(updateResponse.Content, Is.Not.Null.Or.Empty, "Response content is not as expected");

                var updatedRecipe = JObject.Parse(updateResponse.Content);

                Assert.That(updatedRecipe["title"]?.ToString(), Is.EqualTo(recipeTitle));
                Assert.That(updatedRecipe["servings"]?.Value<int>(), Is.EqualTo(updatedServings));
            });
        }
        [Test, Order(5)]
        public void Test_DeleteRecipe()
        {
            var getAllRequest = new RestRequest("/recipe", Method.Get);
            var getAllResponse = client.Execute(getAllRequest);

                Assert.That(getAllResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Response code is not OK (200)");
                Assert.That(getAllResponse.Content, Is.Not.Null.Or.Empty, "Response content is not as expected");

            var recipes = JArray.Parse(getAllResponse.Content);
            var recipe = recipes.FirstOrDefault(r => r["title"]?.ToString() == recipeTitle);

                Assert.That(recipe, Is.Not.Null, $"Recipe with title {recipeTitle} is null");

            var recipeId = recipe["_id"].ToString();

            var deleteRequest = new RestRequest($"recipe/{recipeId}", Method.Delete);
            deleteRequest.AddHeader("Authorization", $"Bearer {token}");

            var deleteResponse = client.Execute(deleteRequest);

            Assert.Multiple(() =>
            {
                Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Response code is not OK (200)");

                var verifyGetRequest = new RestRequest($"recipe/{recipeId}", Method.Get);
                var verifyGetResponse = client.Execute(verifyGetRequest);

                Assert.That(verifyGetResponse.Content, Is.Null.Or.EqualTo("null"));
            });
        }
        public void Dispose()
        {
            client?.Dispose();
        }
    }
}
