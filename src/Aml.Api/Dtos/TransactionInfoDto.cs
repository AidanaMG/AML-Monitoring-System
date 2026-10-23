
namespace Aml.Api.Dtos;

public record TransactionInfoDto
(
    int Id,
    string Type,
    string Direction,
    decimal Amount,
    string Currency,
    DateTime TransactionDate,
    string? CounterpartyName,
    string? MerchantName,
    string? TransactionCountry,
    string? CounterpartyCountry
);