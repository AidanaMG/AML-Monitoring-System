namespace Aml.Api.Dtos
{
    public record UpdateRuleRequest(
        decimal? Weight,
        string? ParametersJson,
        bool? IsActive);
}
