using System.ComponentModel.DataAnnotations;
using WebUI.Repositories;
using WebUI.DTOs;

namespace WebUI.Services;

public sealed class EnergyService(ConsumptionRepository repository)
{
    public Task<List<LookupDto>> CitiesAsync(CancellationToken ct)
    {
        return repository.CitiesAsync(ct);
    }

    public Task<DashboardDto> DashboardAsync(CancellationToken ct)
    {
        return repository.DashboardAsync(ct);
    }

    public Task<ConsumptionDto?> FindAsync(long id, CancellationToken ct)
    {
        return repository.FindAsync(id, ct);
    }

    public Task<PagedRecordsDto> ListAsync(RecordFilterDto filter, CancellationToken ct)
    {
        filter.Page = Math.Max(1, filter.Page);
        filter.PageSize = filter.PageSize is 25 or 50 or 100 ? filter.PageSize : 25;

        if (filter.From > filter.To)
        {
            throw new ValidationException("Başlangıç tarihi bitiş tarihinden büyük olamaz.");
        }

        return repository.ListAsync(filter, ct);
    }

    public Task<bool> UpdateAsync(UpdateConsumptionDto dto, CancellationToken ct)
    {
        Validator.ValidateObject(dto, new ValidationContext(dto), true);
        return repository.UpdateAsync(dto, ct);
    }

    public Task<bool> DeleteAsync(DeleteConsumptionDto dto, CancellationToken ct)
    {
        Validator.ValidateObject(dto, new ValidationContext(dto), true);
        return repository.DeleteAsync(dto.Id, dto.Version, ct);
    }
}


