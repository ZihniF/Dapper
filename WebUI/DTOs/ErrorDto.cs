namespace WebUI.DTOs;

public sealed class ErrorDto
{
    public string? RequestId
    {
        get; set;
    }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}

