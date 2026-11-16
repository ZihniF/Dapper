namespace WebUI.DTOs;

public sealed record PagedRecordsDto(
    IReadOnlyList<ConsumptionDto> Items,
    long Total,
    RecordFilterDto Filter)
{
    public int Pages => Math.Max(1, (int)Math.Ceiling((double)Total / Filter.PageSize));
}

