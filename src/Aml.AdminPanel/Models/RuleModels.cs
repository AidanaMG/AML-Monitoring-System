namespace Aml.AdminPanel.Models;

public record RuleListItem(
    int Id, 
    string RuleCode,
    string Type,
    string Action,
    string Details,
    string ParametersJson,
    decimal Weight,
    bool IsActive,
    DateTime CreatedAt);


public record UpdateRuleRequest(
    decimal? Weight,
    string? ParametersJson,
    bool? IsActive);



