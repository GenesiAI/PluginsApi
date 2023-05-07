using Azure.Identity;
using AutoMapper;
using AiPlugin.Infrastructure;
using Microsoft.EntityFrameworkCore;


using AiPlugin.Application;
using AiPlugin.Application.OpenAi.Models;

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
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IPluginRepository, PluginRepository>();

// add database
builder.Services.AddDbContext<AiPluginDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

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
