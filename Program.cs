using ApiHealthMonitor.Configuration;
using ApiHealthMonitor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure HealthCheckOptions from appsettings
builder.Services.Configure<HealthCheckOptions>(
    builder.Configuration.GetSection(HealthCheckOptions.SectionName));

var healthCheckOptions = builder.Configuration
    .GetSection(HealthCheckOptions.SectionName)
    .Get<HealthCheckOptions>() ?? new HealthCheckOptions();

// Configure HttpClient for health checks
builder.Services.AddHttpClient<IHealthCheckService, HealthCheckService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(healthCheckOptions.TimeoutSeconds);
    client.DefaultRequestHeaders.Add("User-Agent", healthCheckOptions.UserAgent);
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
