using AiPlugin.Api.Settings;
using AiPlugin.Application.OpenAI;
using AiPlugin.Application.Plugins;
using AiPlugin.Application.Users;
using AiPlugin.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

var version = "1.2.7"; // chat
AddServices(builder, version);
AddConfigrations(builder);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint($"/swagger/{version}/swagger.json", $"AiPlugin API {version}");
    });
    app.UseDeveloperExceptionPage();
    app.UseCors("MyDevelopmentPolicy");
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseMiddleware<PortalSubdomainRerouterMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static void AddServices(WebApplicationBuilder builder, string version)
{

    //var keyVaultEndpoint = new Uri(Environment.GetEnvironmentVariable("VaultUri"));
    //builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());

    // Add services to the container.
    builder.Services.AddHttpClient();
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: "MyDevelopmentPolicy",
            builder =>
            {
                builder.WithOrigins("http://localhost:3000")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
            });
    });

    builder.Services.AddSwaggerGen(options =>
    {
        options.OperationFilter<OpenApiParameterIgnoreFilter>();
        options.SwaggerDoc(version, new OpenApiInfo
        {
            Title = "Genesi AI Plugin API",
            Version = version,
            Description = "API set to get and manage Plugins. routes are intended to be accessed on subdomains in the format {pluginId}.Genesi.AI."
        });
    });

    var contactUrl = builder.Configuration.GetValue<string>("ContactLogicApp:Url");
    ArgumentNullException.ThrowIfNull(contactUrl);
    builder.Services.AddSingleton(new ContactSetting() { Url = contactUrl });

    builder.Services.AddScoped<UserRepository>();
    builder.Services.AddScoped<IPluginRepository, PluginRepository>();
    builder.Services.AddScoped<SubscriptionRepository>();
    builder.Services.AddScoped<IChatService, ChatService>();

    builder.Services.AddAuthentication(options => { options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; })
          .AddJwtBearer(options =>
          {
              //options.AutomaticAuthenticate = true;
              options.IncludeErrorDetails = true;
              options.Authority = "https://securetoken.google.com/genesi-ai";
              options.TokenValidationParameters = new TokenValidationParameters
              {
                  ValidateIssuer = true,
                  ValidIssuer = "https://securetoken.google.com/genesi-ai",
                  ValidateAudience = true,
                  ValidAudience = "genesi-ai",
                  ValidateLifetime = true
              };
          });

    // add database
    builder.Services.AddDbContext<AiPluginDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
}

void AddConfigrations(WebApplicationBuilder builder)
{
    var stripeSettings = new StripeSettings();
    builder.Configuration.GetSection("StripeSettings").Bind(stripeSettings);
    builder.Services.AddSingleton(stripeSettings);

    var gptSettings = new GPTSettings();
    builder.Configuration.GetSection("GPTSettings").Bind(gptSettings);
    builder.Services.AddSingleton(gptSettings);

    builder.Services.AddSingleton(x => x.GetRequiredService<IConfiguration>().GetValue<string>("FrontendDomain")!); //make this an obj
}