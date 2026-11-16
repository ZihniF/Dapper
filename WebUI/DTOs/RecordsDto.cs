namespace WebUI.DTOs;

public sealed record RecordsDto(
    PagedRecordsDto Records,
    List<LookupDto> Cities,
    ConsumptionDto? Selected);

