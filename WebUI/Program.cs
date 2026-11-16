using System.Globalization;
using WebUI.Services;
using WebUI.Repositories;
using WebUI.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.SqlClient;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("tr-TR");

var builder = WebApplication.CreateBuilder(args);
var connection = builder.Configuration.GetConnectionString("EnergyPulse")
    ?? throw new InvalidOperationException("EnergyPulse bağlantısı tanımlanmalı.");

if (args.Contains("--initialize"))
{
    try
    {
        var scriptsDirectory = Path.Combine(AppContext.BaseDirectory, "Database");
        await DatabaseInitializer.InitializeAsync(connection, scriptsDirectory);
    }
    catch (SqlException ex)
    {
        Console.Error.WriteLine($"Veritabanı kurulamadı: {ex.Message}");
        Console.Error.WriteLine("SSMS sunucu adını ve bağlantı yetkilerini kontrol edin.");
        Environment.ExitCode = 1;
    }

    return;
}

builder.Services.AddControllersWithViews();

var keysDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "keys");
var keys = builder.Services.AddDataProtection()
    .SetApplicationName("EnergyPulse")
    .PersistKeysToFileSystem(new DirectoryInfo(keysDirectory));

if (OperatingSystem.IsWindows())
{
    keys.ProtectKeysWithDpapi();
}

builder.Services.AddSingleton(new SqlConnectionFactory(connection));
builder.Services.AddScoped<ConsumptionRepository>();
builder.Services.AddScoped<EnergyService>();
builder.Services.AddMemoryCache();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.Run();


