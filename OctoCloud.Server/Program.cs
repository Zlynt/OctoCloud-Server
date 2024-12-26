using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using MusicSettings = OctoCloud.Settings.Music;

var builder = WebApplication.CreateBuilder(args);
if(builder.Environment.EnvironmentName == "Development")
    builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
else 
    builder.Configuration.AddJsonFile("appsettings.json", optional: false);
builder.Configuration.AddEnvironmentVariables();
builder.Services.Configure<MusicSettings>(builder.Configuration.GetSection("Music"));

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

// Health Check
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
// Allow Reserve Proxy
app.UseForwardedHeaders();
// Add health check
app.MapHealthChecks("/healthz");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
