namespace WebUI.DTOs;

public sealed class ConsumptionDto
{
    public long Id
    {
        get; set;
    }
    public int DistrictId
    {
        get; set;
    }
    public string City { get; set; } = "";
    public string District { get; set; } = "";
    public DateTime Date
    {
        get; set;
    }
    public int CategoryId
    {
        get; set;
    }
    public string Category { get; set; } = "";
    public decimal Kwh
    {
        get; set;
    }
    public int Subscribers
    {
        get; set;
    }
    public string Version { get; set; } = "";
}

