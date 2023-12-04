using Microsoft.Extensions.Options;
using MongoDB.Driver;
using UserAPISample2.DAL;
using UserAPISample2.Filters;
using UserAPISample2.Services;
using UserAPISample2.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Configuration
    .AddEnvironmentVariables().Build();

builder.Services.Configure<ApplicationData>(
    builder.Configuration.GetSection(nameof(ApplicationData)));
builder.Services.Configure<UsersDatabaseSettings>(
    builder.Configuration.GetSection(nameof(UsersDatabaseSettings)));

builder.Services.AddSingleton<IApplicationData>(sp =>
    sp.GetRequiredService<IOptions<ApplicationData>>().Value);
builder.Services.AddSingleton<IUsersDatabaseSettings>(sp =>
    sp.GetRequiredService<IOptions<UsersDatabaseSettings>>().Value);

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var usersDatabaseSettings = sp.GetRequiredService<IUsersDatabaseSettings>();
    var applicationData = sp.GetRequiredService<IApplicationData>();

    var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? usersDatabaseSettings.DbHost;
    var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? usersDatabaseSettings.DbPort;
    var applicationName = applicationData.ApplicationName;
    var connectionString = string.Format(usersDatabaseSettings.ConnectionString! , dbHost, dbPort, applicationName);
    return new MongoClient(connectionString);
});

builder.Services.AddSingleton(sp =>
{
    var usersDatabaseSettings = sp.GetRequiredService<IUsersDatabaseSettings>();

    var client = sp.GetRequiredService<IMongoClient>();
    var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? usersDatabaseSettings.DatabaseName;
    return client.GetDatabase(dbName);
});

builder.Services.AddScoped<IMongoDbContext, MongoDbContext>();
builder.Services.AddScoped<IUserService, UserService>();


builder.Services.AddControllers(configure => 
    configure.Filters.Add(typeof(GlobalExceptionFilter)));

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

//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
