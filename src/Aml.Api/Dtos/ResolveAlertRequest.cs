using Aml.Domain.Enums;

namespace Aml.Api.Dtos;
public record ResolveAlertRequest(
    AlertStatus Status, 
    string? Comment);   