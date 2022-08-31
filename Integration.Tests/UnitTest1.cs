using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Integration.Tests;

public class UnitTest1 : IDisposable
{
     private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly HttpClient _httpClient = new() { BaseAddress = new Uri("http://localhost:5120") };
   [Fact]
public async Task GivenARequest_WhenCallingGetBooks_ThenTheAPIReturnsExpectedResponse()
{
    // Arrange.
    var expectedStatusCode = System.Net.HttpStatusCode.OK;
    var expectedContent = new[]
    {
        new Book(1, "Awesome book #1"),
        new Book(2, "Awesome book #2"),
        new Book(3, "Awesome book #3"),
        new Book(4, "Awesome book #4"),
        new Book(5, "Awesome book #5")
    };
    var stopwatch = Stopwatch.StartNew();
    // Act.
    var response = await _httpClient.GetAsync("/getall");
    // Assert.
    await TestHelpers.AssertResponseWithContentAsync(stopwatch, response, expectedStatusCode, expectedContent);
}
[Theory]
[InlineData(null)]
[InlineData("")]
[InlineData(" ")]
[InlineData("WrongApiKey")]
public async Task GivenAnUnauthenticatedRequest_WhenCallingAdmin_ThenTheAPIReturnsUnauthorized(string apiKey)
{
    // Arrange.
    var expectedStatusCode = System.Net.HttpStatusCode.Unauthorized;
    var stopwatch = Stopwatch.StartNew();
    var request = new HttpRequestMessage(HttpMethod.Get, "/admin");
    request.Headers.Add("X-Api-Key", apiKey);
    // Act.
    var response = await _httpClient.SendAsync(request);
    // Assert.
     TestHelpers.AssertCommonResponseParts(stopwatch, response, expectedStatusCode);
}
[Fact]
public async Task GivenARequest_WhenCallingPutBooks_ThenTheAPIReturnsExpectedResponseAndUpdatesBook()
{
    // Arrange.
    var expectedStatusCode = System.Net.HttpStatusCode.OK;
    var updatedBook = new Book(6, "Awesome book #6 - Updated");
    var stopwatch = Stopwatch.StartNew();
    // Act.
    var response = await _httpClient.PutAsync("/books", TestHelpers.GetJsonStringContent(updatedBook));
    // Assert.
    TestHelpers.AssertCommonResponseParts(stopwatch, response, expectedStatusCode);
}
[Fact]
public async Task GivenARequest_WhenCallingPostBooks_ThenTheAPIReturnsExpectedResponseAndAddsBook()
{
    // Arrange.
    var expectedStatusCode = System.Net.HttpStatusCode.Created;
    var expectedContent = new Book(6, "Awesome book #6");
    var stopwatch = Stopwatch.StartNew();
    // Act.
    var response = await _httpClient.PostAsync("/books", TestHelpers.GetJsonStringContent(expectedContent));
    // Assert.
    await TestHelpers.AssertResponseWithContentAsync(stopwatch, response, expectedStatusCode, expectedContent);
}
    public void Dispose()
{
    _httpClient.DeleteAsync("/state").GetAwaiter().GetResult();
}
}
internal record Book(int BookId, string Title);

public static class TestHelpers
{
    private const string _jsonMediaType = "application/json";
    private const int _expectedMaxElapsedMilliseconds = 1000;
      private static readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    public static async Task AssertResponseWithContentAsync<T>(Stopwatch stopwatch,
    HttpResponseMessage response, System.Net.HttpStatusCode expectedStatusCode,
    T expectedContent)
{
    AssertCommonResponseParts(stopwatch, response, expectedStatusCode);
    Assert.Equal(_jsonMediaType, response.Content.Headers.ContentType?.MediaType);
    Assert.Equal(expectedContent, await JsonSerializer.DeserializeAsync<T?>(
        await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions));
}
public static void AssertCommonResponseParts(Stopwatch stopwatch,
    HttpResponseMessage response, System.Net.HttpStatusCode expectedStatusCode)
{
    Assert.Equal(expectedStatusCode, response.StatusCode);
    Assert.True(stopwatch.ElapsedMilliseconds < _expectedMaxElapsedMilliseconds);
}

public static StringContent GetJsonStringContent<T>(T model)
    => new(JsonSerializer.Serialize(model), Encoding.UTF8, _jsonMediaType);
   
}