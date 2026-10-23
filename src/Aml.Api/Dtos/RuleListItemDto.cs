namespace Aml.Api.Dtos
{
    public record RuleListItemDto(
        int Id,
        string RuleCode,
        string Type,
        string Action,
        string Details,
        string ParametersJson,
        decimal Weight,
        bool IsActive,
        DateTime CreatedAt);

}
