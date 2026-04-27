var builder = WebApplication.CreateBuilder(args);

// Ajouter Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Activer Swagger en mode développement
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// (optionnel) redirection HTTPS
app.UseHttpsRedirection();

// Endpoint simple test
app.MapGet("/test", () => "Backend is working");

// Endpoint exemple (users)
app.MapGet("/users", () =>
{
    var users = new[]
    {
        new { Id = 1, Name = "Yahya" },
        new { Id = 2, Name = "Nada" }
    };

    return users;
});

app.Run();