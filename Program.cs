using IssueTriage.src.Configuration;
using IssueTriage.src.Services;

var builder = WebApplication.CreateBuilder(args);


//Adding configuration
string ApiKey = builder.Configuration["AIConfiguration:ApiKey"] ?? throw new InvalidOperationException("Missing AI Api Key");
string BaseUrl = builder.Configuration["AIConfiguration:BaseUrl"] ?? throw new InvalidOperationException("Missing Base Url");
string Model = builder.Configuration["AIConfiguration:Model"] ?? throw new InvalidOperationException("Missing AI Model");
builder.Services.AddValidation();
var modelConfiguration = new ModelConfiguration
{
    ApiKey = ApiKey,
    BaseUrl = BaseUrl,
    Model = Model
};

builder.Services.AddSingleton(modelConfiguration);
builder.Services.AddHttpClient<MinimalTriageService>((_, client) =>
{
    client.BaseAddress = new Uri($"{modelConfiguration.BaseUrl.TrimEnd('/')}/");
});
builder.Services.AddHttpClient<FunctionCallingService>((_, client) =>
{
    client.BaseAddress = new Uri($"{modelConfiguration.BaseUrl.TrimEnd('/')}/");
});
// builder.Services.AddScoped<MinimalTriageService>();
builder.Services.AddControllers();
var app = builder.Build();
app.MapControllers();
app.Run();
