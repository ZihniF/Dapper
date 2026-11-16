namespace WebUI.DTOs;

public sealed class DashboardDto
{
    public long RecordCount
    {
        get; set;
    }
    public decimal TotalKwh
    {
        get; set;
    }
    public decimal AveragePerSubscriber
    {
        get; set;
    }
    public int Cities
    {
        get; set;
    }
    public DateTime? FirstDate
    {
        get; set;
    }
    public DateTime? LastDate
    {
        get; set;
    }
    public List<MetricDto> Months { get; set; } = [];
    public List<MetricDto> Categories { get; set; } = [];
    public List<MetricDto> TopCities { get; set; } = [];
}

