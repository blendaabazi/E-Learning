using E_Learning.Data;
using E_Learning.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis; // Për Redis
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Konfigurimi i Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Shtoni shërbimin për lidhjen me databazën
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Shtoni mbështetje për identifikimin dhe autorizimin
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

// Konfigurimi i Redis
var redisConfig = builder.Configuration.GetConnectionString("RedisConnection")
                  ?? throw new InvalidOperationException("Connection string 'RedisConnection' not found.");
var redis = ConnectionMultiplexer.Connect(redisConfig);
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
builder.Services.AddSingleton<RedisCacheService>();

builder.Services.AddLogging(); // Injektimi i logimit



// Shtoni shërbimin për trajtimin e skedarëve
builder.Services.AddScoped<IFileService, FileService>();

// Shtoni mbështetje për kontrollorët dhe pamjet
builder.Services.AddControllersWithViews();

// Mundëso CORS për frontend-in që përdor portën tjetër
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.WithOrigins("http://localhost:3000")  // Frontend në portin 3000
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

var app = builder.Build();

// Konfigurimi i HTTPS
app.UseHttpsRedirection();

// Aktivizo shërbimin për skedarët statikë
app.UseStaticFiles();

// Konfigurimi i rrugëve
app.UseRouting();

// Aktivizo autorizimin
app.UseAuthorization();

// Aktivizo Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Learning API v1");
});

// Aktivizo CORS
app.UseCors("AllowAll");  // Aktivizo politikat CORS që mundësojnë kërkesat nga frontend

// Konfigurimi i rrugës për kontrollorët
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{action=Index}/{id?}",
    defaults: new { controller = "Admin" });

// Seed për rolet dhe administratoret
using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedRolesAndAdminAsync(scope.ServiceProvider);
}

app.Run();
