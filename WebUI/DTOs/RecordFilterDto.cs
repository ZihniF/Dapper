namespace WebUI.DTOs;

public sealed class RecordFilterDto
{
    public long? Id
    {
        get; set;
    }
    public int? CityId
    {
        get; set;
    }
    public int? CategoryId
    {
        get; set;
    }
    public DateTime? From
    {
        get; set;
    }
    public DateTime? To
    {
        get; set;
    }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

