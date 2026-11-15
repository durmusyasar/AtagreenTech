namespace AtaGreenTech.UserService.Api.Contracts;

/// <summary>API hata yanıtı için tutarlı gövde.</summary>
public sealed record ErrorResponse(string Code, string Message);
