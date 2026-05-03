using SecureVaultInterface;
using SecureVaultApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register IKeyVault as Singleton - connects once at startup, reuses for all requests
builder.Services.AddSingleton<IKeyVault>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    
    var vault = VaultFactory.CreateVault(configuration);
    
    // Connect at application startup
    try
    {
        vault.ConnectAsync().GetAwaiter().GetResult();
        logger.LogInformation("Successfully connected to vault at startup");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to connect to vault at startup");
        throw;
    }
    
    return vault;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Disconnect vault on application shutdown
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(() =>
{
    var vault = app.Services.GetRequiredService<IKeyVault>();
    vault.DisconnectAsync().GetAwaiter().GetResult();
});

app.Run();
