using AiPlugin.Application.Old.OpenAi.Models;
using AiPlugin.Application.Plugins;
using AiPlugin.Domain;
using AiPlugin.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

//deserialize and inject GPTSettings fron "GPTSettings" section of appsettings.json
var gPTSettings = builder.Configuration.GetSection("GPTSettings").Get<GPTSettings>()!;
builder.Services.AddSingleton(gPTSettings);

//var keyVaultEndpoint = new Uri(Environment.GetEnvironmentVariable("VaultUri"));
//builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var version = "1.2.1"; //subdomain management without userId query param
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<OpenApiParameterIgnoreFilter>();
    options.SwaggerDoc(version, new OpenApiInfo
    {
        Title = "Genesi AI Plugin API",
        Version = version,
        Description = "API set to get and manage Plugins. routes are intended to be accessed on subdomains in the format {userId}.Genesi.AI. The subdomain is used as the userId."
    });
});


builder.Services.AddScoped<IBaseRepository<Plugin>, AiPlugin.Application.Plugins.PluginRepository>();

// add database
builder.Services.AddDbContext<AiPluginDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint($"/swagger/{version}/swagger.json", $"AiPlugin API {version}");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
