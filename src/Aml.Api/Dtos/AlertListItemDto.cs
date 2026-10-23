namespace Aml.Api.Dtos;
public record AlertListItemDto(
            int Id,
            int ClientId,
            int? TransactionId,
            string PrimaryTrigger,
            decimal RiskScore,
            string Status,
            DateTime CreatedAt);