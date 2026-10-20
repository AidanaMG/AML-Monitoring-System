namespace Aml.Api.Dtos;
public record AlertDetailsDto(
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
            TransactionInfoDto? Transaction
            );