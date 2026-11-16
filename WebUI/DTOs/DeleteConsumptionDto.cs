using System.ComponentModel.DataAnnotations;

namespace WebUI.DTOs;

public sealed class DeleteConsumptionDto
{
    [Range(1, long.MaxValue)]
    public long Id
    {
        get; set;
    }

    [Required]
    [RegularExpression("^0x[0-9A-Fa-f]{16}$")]
    public string Version { get; set; } = "";
}

