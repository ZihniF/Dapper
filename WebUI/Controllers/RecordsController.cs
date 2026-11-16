using System.ComponentModel.DataAnnotations;
using WebUI.Services;
using WebUI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;

namespace WebUI.Controllers;

public class RecordsController(
    EnergyService service,
    IMemoryCache cache,
    ILogger<RecordsController> logger) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        RecordFilterDto filter,
        long? editId,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Filtre değerleri geçersiz.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var rows = await service.ListAsync(filter, ct);
            var selected = editId.HasValue
                ? await service.FindAsync(editId.Value, ct)
                : null;

            if (editId.HasValue && selected is null)
            {
                TempData["Error"] = "Kayıt bulunamadı.";
            }

            var dto = new RecordsDto(rows, await service.CitiesAsync(ct), selected);
            return View(dto);
        }
        catch (ValidationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Kayıtlar alınamadı");
            Response.StatusCode = 503;
            return View("~/Views/Home/Setup.cshtml");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(UpdateConsumptionDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Tüketim, abone sayısı veya kayıt sürümü geçersiz.";
            return RedirectToAction(nameof(Index), new
            {
                editId = dto.Id
            });
        }

        try
        {
            var changed = await service.UpdateAsync(dto, ct);

            if (changed)
            {
                cache.Remove("dashboard");
            }

            TempData[changed ? "Success" : "Error"] = changed
                ? "Kayıt güncellendi."
                : "Kayıt değişmiş veya silinmiş. Yeniden açın.";
        }
        catch (ValidationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Güncelleme başarısız");
            TempData["Error"] = "Veritabanına ulaşılamadı. İşlem tamamlanamadı.";
        }

        return RedirectToAction(nameof(Index), new
        {
            dto.Id
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(DeleteConsumptionDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Kayıt bilgileri geçersiz. Listeyi yenileyin.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var changed = await service.DeleteAsync(dto, ct);

            if (changed)
            {
                cache.Remove("dashboard");
            }

            TempData[changed ? "Success" : "Error"] = changed
                ? "Kayıt silindi."
                : "Kayıt değişmiş veya silinmiş. Listeyi yenileyin.";
        }
        catch (ValidationException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (SqlException ex)
        {
            logger.LogError(ex, "Silme başarısız");
            TempData["Error"] = "Veritabanına ulaşılamadı. İşlem tamamlanamadı.";
        }

        return RedirectToAction(nameof(Index));
    }
}

