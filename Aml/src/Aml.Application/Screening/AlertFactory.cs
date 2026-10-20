using Aml.Domain.Entities;
using Aml.Domain.Enums;
using System.Text.Json;

namespace Aml.Application.Screening
{
    public static class AlertFactory
    {
        public static Alert? Create(
            ScreeningResult result, 
            Transaction transaction,
            int clientId,
            DateTime createdAt)
        {
            var triggered = result.RuleEvaluations.Where(e => e.Result.Triggered).ToList();

            if(triggered.Count == 0) return null;

            var primaryTrigger = triggered.FirstOrDefault(r => r.RuleType == RuleType.Hard)
                ?? triggered.FirstOrDefault(r => r.RuleType == RuleType.Typology)
                ?? triggered.Where(r => r.RuleType == RuleType.Scoring)
                            .OrderByDescending(r => r.Result.Contribution)
                            .FirstOrDefault();

            var riskScore = result.HardTriggered
                ? 100 
                : result.TypologyTriggered
                    ? triggered.Where(r => r.RuleType == RuleType.Typology)
                               .Max(r => r.Result.Contribution)
                    : Math.Min(result.TotalScore, 100);


            string FormatGroup(RuleType type) =>
                string.Join("; ", triggered
                    .Where(r => r.RuleType == type)
                    .Select(r => $"{r.RuleCode} - {r.Result.TriggerDetails}"));

            string details =
                $"Hard Rules: {FormatGroup(RuleType.Hard)}\n" +
                $"Typology Rules: {FormatGroup(RuleType.Typology)}\n" +
                $"Scoring Rules: {FormatGroup(RuleType.Scoring)}";


            var breakdown = triggered.Select(e => new
            {
                ruleCode = e.RuleCode,
                type = e.RuleType.ToString(),
                contribution = e.Result.Contribution,
                matched = e.Result.MatchedCount,
                total = e.Result.TotalChecks,
                details = e.Result.TriggerDetails
            }).ToList();



            return new Alert
            {
                ClientId = clientId,
                TransactionId = transaction.Id,
                PrimaryTrigger = primaryTrigger!.RuleCode,
                RiskScore = riskScore,
                TriggerDetails = details,
                ScoreBreakdownJson = JsonSerializer.Serialize(breakdown),
                CreatedAt = createdAt,
                Status = AlertStatus.New
            };
        }
    }
}
