using System.Text.Json.Serialization;
using Api.Swazy.Common;
using Api.Swazy.DataSeeding;
using Api.Swazy.Extensions;
using Api.Swazy.Modules;
using Api.Swazy.Options;
using Api.Swazy.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, configuration) => 
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

const string SwazyCorsPolicy = "SwazyCorsPolicy";
builder.Services.AddCors(options => 
{
    options.AddPolicy(name: SwazyCorsPolicy, policyBuilder =>
    {
        policyBuilder.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(SwazyConstants.JwtOptionsSectionName));
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.WriteIndented = true;
});
builder.Services.AddDbContext<SwazyDbContext>(options => 
    options
        .UseLazyLoadingProxies()
        .UseNpgsql(builder.Configuration.GetConnectionString(SwazyConstants.DatabaseOptionsSectionName)));

builder.Services.InjectDependencies();

var app = builder.Build();

Log.Information("[------] Swazy Building Process [------]");
Log.Information("[Swazy] Build started...");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.EnableTryItOutByDefault());

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<SwazyDbContext>();
    FakeSeeder.SeedAsync(dbContext).Wait();
}

app.UseCors(SwazyCorsPolicy);
Log.Information($"[Swazy] CORS Policy: '{SwazyCorsPolicy}' has been applied...");
Log.Information($"[Swazy] Endpoints mapping...");
app.MapBusinessEndpoints();
app.MapServiceEndpoints();
app.MapBookingEndpoints();
app.MapUserEndpoints();
app.MapBusinessServiceEndpoints(); // Added this line
Log.Information($"[Swazy] Endpoints mapped, setting up HTTPS Redirection...");
app.UseHttpsRedirection();
Log.Information("[Swazy] Everything is setup, the app starts to run!");
Log.Information("[------] Swazy Building Process [------]");
app.Run();
