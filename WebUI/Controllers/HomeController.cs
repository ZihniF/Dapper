using WebUI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;

namespace WebUI.Controllers;

public class HomeController(
    EnergyService service,
    IMemoryCache cache,
    ILogger<HomeController> logger) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        try
        {
            var dashboard = await cache.GetOrCreateAsync("dashboard", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);
                return await service.DashboardAsync(ct);
            });

            return View(dashboard);
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Dashboard verileri alınamadı");
            Response.StatusCode = 503;
            return View("Setup");
        }
    }

    public IActionResult Error()
    {
        Response.StatusCode = 500;
        return View("Setup");
    }
}

