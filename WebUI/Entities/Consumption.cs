namespace WebUI.Entities;

public sealed class Consumption
{
    public long Id
    {
        get; set;
    }
    public int DistrictId
    {
        get; set;
    }
    public DateTime Date
    {
        get; set;
    }
    public int CategoryId
    {
        get; set;
    }
    public decimal Kwh
    {
        get; set;
    }
    public int Subscribers
    {
        get; set;
    }
    public byte[] Version { get; set; } = [];
}

