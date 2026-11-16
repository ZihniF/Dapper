namespace WebUI.DTOs;

public sealed class MetricDto
{
    public string Label { get; set; } = "";
    public decimal Value
    {
        get; set;
    }
}

