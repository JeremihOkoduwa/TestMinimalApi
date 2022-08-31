using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors(x => x.AllowAnyHeader().AllowAnyHeader().AllowAnyOrigin());
app.UseHttpsRedirection();

var books = new List<Book>();
void InitializeBooks() => books = Enumerable.Range(1, 5)
    .Select(index => new Book(index, $"Awesome book #{index}"))
    .ToList();

    app.MapGet("/getall", ()=>{
        InitializeBooks();
        return Results.Ok(books);

    }).WithName("Book");
    app.MapPost("/books", (Book book) =>
{
    books.Add(book);
    return Results.Created($"/books/{book.BookId}", book);
});
app.MapPut("/books", (Book book) =>
{
    books.RemoveAll(book => book.BookId == book.BookId);
    books.Add(book);
    return Results.Ok(book);
});
app.MapDelete("/state", () =>
{
    InitializeBooks();
    return Results.NoContent();
});
app.MapGet("/admin", ([FromHeader(Name = "X-Api-Key")] string apiKey) =>
{
    if (apiKey == "SuperSecretApiKey")
    {
        return Results.Ok("Hi admin!");
    }
    return Results.Unauthorized();
});



app.Run();

internal record Book(int BookId, string Title);