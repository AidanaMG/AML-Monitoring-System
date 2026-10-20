namespace Aml.Api.Dtos;
public record LoginRequest(
    string UserName,
    string Password);