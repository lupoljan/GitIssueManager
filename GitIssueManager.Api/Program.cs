using GitIssueManager.Core.Factories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient("github", client =>
{
    client.BaseAddress = new Uri("https://api.github.com/");
    client.DefaultRequestHeaders.Add("User-Agent", "GitIssueManager");
    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
});

builder.Services.AddHttpClient("gitlab", client =>
{
    client.BaseAddress = new Uri("https://gitlab.com/api/v4/");
});


builder.Services.AddSingleton<GitServiceFactory>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
