namespace Aml.AdminPanel.Models;

public record AlertListItem(
    int Id,
    int ClientId,
    int? TransactionId,
    string PrimaryTrigger,
    decimal RiskScore,
    string Status,
    DateTime CreatedAt);

public record AlertListResponse(
    int Total,
    int Page,
    int PageSize,
    AlertListItem[] Items);

public record AlertDetails(
    int Id,
    int ClientId,
    string ClientName,
    int? TransactionId,
    string PrimaryTrigger,
    decimal RiskScore,
    string Status,
    string TriggerDetails,
    string ScoreBreakdownJson,
    DateTime CreatedAt,
    DateTime? ResolvedAt,
    string? ResolvedBy,
    string? ResolutionComment,
    string? AssignedTo,
    TransactionInfo? Transaction);

public record RuleBreakdown(
    string RuleCode,
    string Type,
    decimal Contribution,
    int Matched,
    int Total,
    string Details);


public record TransactionInfo(
    int Id,
    string Type,
    string Direction,
    decimal Amount,
    string Currency,
    DateTime TransactionDate,
    string? CounterpartyName,
    string? MerchantName,
    string? TransactionCountry,
    string? CounterpartyCountry);