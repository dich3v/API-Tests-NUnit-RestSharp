using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RestSharp;
using System.Net;

namespace BookStoreApi
{
    [TestFixture]
    public class BookTests : IDisposable
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

        [Test, Order(1)]
        public void Test_GetAllBooks()
        {
            var request = new RestRequest("book", Method.Get);
            var response = client.Execute(request);

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code is not OK (200)");
                Assert.That(response.Content, Is.Not.Empty, "Response content is empty");

                var books = JArray.Parse(response.Content);

                Assert.That(books.Type, Is.EqualTo(JTokenType.Array), "Expected response is not JSON Array");
                Assert.That(books.Count, Is.GreaterThan(0), "Expected at least one book in the response");

                foreach (var book in books)
                {
                    Assert.That(book["title"]?.ToString(), Is.Not.Null.Or.Empty, "Book title should not be null or empty");
                    Assert.That(book["author"]?.ToString(), Is.Not.Null.Or.Empty, "Book author should not be null or empty");
                    Assert.That(book["description"]?.ToString(), Is.Not.Null.Or.Empty, "Book description should not be null or empty");
                    Assert.That(book["price"], Is.Not.Null.Or.Empty, "Book price should not be null or empty");
                    Assert.That(book["pages"], Is.Not.Null.Or.Empty, "Book pages should not be null or empty");
                    Assert.That(book["category"], Is.Not.Null.Or.Empty, "Book category should not be null or empty");
                }
            });
        }

        [Test, Order(2)]
        public void Test_GetBookByTitle()
        {
            var request = new RestRequest("book", Method.Get);
            var response = client.Execute(request);

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code is not OK (200)");
                Assert.That(response.Content, Is.Not.Empty, "Response content is empty");

                var books = JArray.Parse(response.Content);
                var book = books.FirstOrDefault(b => b["title"]?.ToString() == "The Great Gatsby");
                Assert.That(book, Is.Not.Null, "The book with the title The Great Gatsby does not exist in the response");

                Assert.That(book["author"]?.ToString(), Is.EqualTo("F. Scott Fitzgerald"),
                    "Author of the book should be F. Scott Fitzgerald");
            });
        }

        [Test, Order(3)]
        public void Test_AddBook()
        {
            var getCategoryRequest = new RestRequest("category", Method.Get);
            var getCategoryResponse = client.Execute(getCategoryRequest);

            Assert.Multiple(() =>
            {
                Assert.That(getCategoryResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code is not OK (200)");
                Assert.That(getCategoryResponse.Content, Is.Not.Empty, "Response content is empty");
            });

            var categories = JArray.Parse(getCategoryResponse.Content);
            var category = categories.First();
            var categoryId = category["_id"]?.ToString();

            var createBookRequest = new RestRequest("book", Method.Post);
            createBookRequest.AddHeader("Authorization", $"Bearer {token}");

            var title = "King Rat";
            var author = "James Clavell";
            var description = "The novel describes the struggle for survival of American, Australian, British, Dutch and New Zealander prisoners of war in a Japanese camp in Singapore.";
            var price = 20;
            var pages = 400;

            createBookRequest.AddJsonBody(new
            {
                title,
                author,
                description,
                price,
                pages,
                category = categoryId
            });

            var createBookResponse = client.Execute(createBookRequest);

            Assert.Multiple(() =>
            {
                Assert.That(createBookResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code OK (200)");
                Assert.That(createBookResponse.Content, Is.Not.Empty, "Response content should not be empty");
            });

            var createdBook = JObject.Parse(createBookResponse.Content);
            var createdBookId = createdBook["_id"]?.ToString();

            var getBookRequest = new RestRequest($"book/{createdBookId}", Method.Get);
            getBookRequest.AddHeader("Authorization", $"Bearer {token}");
            var getBookResponse = client.Execute(getBookRequest);

            Assert.Multiple(() =>
            {
                Assert.That(getBookResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code is not OK (200)");
                Assert.That(getBookResponse.Content, Is.Not.Empty, "Response content is empty");

                var newBook = JObject.Parse(getBookResponse.Content);

                Assert.That(newBook["title"]?.ToString(), Is.EqualTo(title));
                Assert.That(newBook["author"]?.ToString(), Is.EqualTo(author));
                Assert.That(newBook["description"]?.ToString(), Is.EqualTo(description));
                Assert.That(newBook["price"]?.Value<int>(), Is.EqualTo(price));
                Assert.That(newBook["pages"]?.Value<int>(), Is.EqualTo(pages));
                Assert.That(newBook["category"]?["_id"]?.ToString(), Is.EqualTo(categoryId));
            });
        }

        [Test, Order(4)]
        public void Test_UpdateBook()
        {
            var request = new RestRequest("book", Method.Get);
            var response = client.Execute(request);

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code is not OK (200)");
                Assert.That(response.Content, Is.Not.Empty, "Response content is empty");
            });

            var books = JArray.Parse(response.Content);
            var bookToUpdate = books.FirstOrDefault(b => b["title"]?.ToString() == "King Rat");
            Assert.That(bookToUpdate, Is.Not.Null, "Book with title King Rat not found in the response");

            var bookId = bookToUpdate["_id"].ToString();

            var updateRequest = new RestRequest("book/{id}", Method.Put);
            updateRequest.AddHeader("Authorization", $"Bearer {token}");
            updateRequest.AddUrlSegment("id", bookId);
            updateRequest.AddJsonBody(new
            {
                title = "Asian Saga",
                author = "Charles Edmond Clavell"
            });
            var updateResponse = client.Execute(updateRequest);

            Assert.Multiple(() =>
            {
                Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code is not OK (200)");
                Assert.That(updateResponse.Content, Is.Not.Empty, "Response content is empty");

                var updatedBook = JObject.Parse(updateResponse.Content);

                Assert.That(updatedBook["title"]?.ToString(), Is.EqualTo("Asian Saga"), "Book title should be updated");
                Assert.That(updatedBook["author"]?.ToString(), Is.EqualTo("Charles Edmond Clavell"), "Book author should be updated");
            });
        }

        [Test, Order(5)]
        public void Test_DeleteBook()
        {
            var request = new RestRequest("book", Method.Get);
            var response = client.Execute(request);

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code is not OK (200)");
                Assert.That(response.Content, Is.Not.Empty, "Response content is empty");
            });

            var books = JArray.Parse(response.Content);
            var bookToDelete = books.FirstOrDefault(b => b["title"]?.ToString() == "Asian Saga");
            Assert.That(bookToDelete, Is.Not.Null, "Book with title Asian Saga not found in the response");

            var bookId = bookToDelete["_id"].ToString();

            var deleteRequest = new RestRequest("book/{id}", Method.Delete);
            deleteRequest.AddHeader("Authorization", $"Bearer {token}");
            deleteRequest.AddUrlSegment("id", bookId);

            var deleteResponse = client.Execute(deleteRequest);

            Assert.Multiple(() =>
            {
                Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code is not OK (200)");

                var getDeletedBookRequest = new RestRequest("book/{id}", Method.Get);
                getDeletedBookRequest.AddUrlSegment("id", bookId);

                var getDeletedBookResponse = client.Execute(getDeletedBookRequest);

                Assert.That(getDeletedBookResponse.Content, Is.Null.Or.EqualTo("null"), "Book should not exist");
            });
        }

        public void Dispose()
        {
            client?.Dispose();
        }
    }
}
