using System.ComponentModel.DataAnnotations;

namespace WebUI.DTOs;

public sealed class UpdateConsumptionDto
{
    [Range(1, long.MaxValue)]
    public long Id
    {
        get; set;
    }

    [Range(typeof(decimal), "0.01", "999999999999", ParseLimitsInInvariantCulture = true)]
    public decimal Kwh
    {
        get; set;
    }

    [Range(1, 10000000)]
    public int Subscribers
    {
        get; set;
    }

    [Required]
    [RegularExpression("^0x[0-9A-Fa-f]{16}$")]
    public string Version { get; set; } = "";
}

