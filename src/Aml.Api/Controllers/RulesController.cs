using Aml.Api.Dtos;
using Aml.Application;
using Aml.Application.Audit;
using Aml.Domain.Entities;
using Aml.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Aml.Api.Controllers
{
    [ApiController]
    [Route("api/rules")]
    [Authorize]
    public class RulesController : ControllerBase
    {
        private readonly AmlDbContext _db;
        private readonly IAuditService _auditService;
        private readonly IReferenceDataCache _referenceDataCache;

        public RulesController(
            AmlDbContext db,
            IAuditService auditService,
            IReferenceDataCache referenceDataCache)
        {
            _db = db;
            _auditService = auditService;
            _referenceDataCache = referenceDataCache;
        }


        [HttpGet]
        public async Task<IActionResult> GetList(CancellationToken ct = default)
        {
            var rules = await _db.Rules.AsNoTracking()
                .OrderBy(r => r.Type).ThenBy(r => r.RuleCode)
                .Select(r => new RuleListItemDto(
                    r.Id,
                    r.RuleCode,
                    r.Type.ToString(),
                    r.Action.ToString(),
                    r.Details ?? "",
                    r.ParametersJson,
                    r.Weight,
                    r.IsActive,
                    r.CreatedAt))
                .ToListAsync(ct);

            return Ok(rules);

        }


        [HttpPatch("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRule(
            int id,
            [FromBody] UpdateRuleRequest request,
            CancellationToken ct = default)
        {
            var userName = User.Identity?.Name;
            if(string.IsNullOrEmpty(userName)) return Unauthorized();

            var rule = await _db.Rules.FirstOrDefaultAsync(r => r.Id == id, ct);
            if(rule is null) return NotFound();

            var changes = new List<string>();

            if(request.Weight is not null)
            {
                if(request.Weight < 0 || request.Weight > 100) return BadRequest("Weight must be between 0 and 100");

                changes.Add($"weight: {rule.Weight} -> {request.Weight.Value}");

                rule.Weight = request.Weight.Value;
            }


            if(request.ParametersJson is not null)
            {
                try 
                {
                    JsonSerializer.Deserialize<Dictionary<string, decimal>>(request.ParametersJson);
                }
                catch (JsonException)
                {
                    return BadRequest("Invalid ParametersJson");
                }

                changes.Add($"parametersJson: {rule.ParametersJson} -> {request.ParametersJson}");

                rule.ParametersJson = request.ParametersJson;
            }


            if(request.IsActive is not null)
            {
                changes.Add($"isActive: {rule.IsActive} -> {request.IsActive}");

                rule.IsActive = request.IsActive.Value;
            }


            if(changes.Count == 0) return BadRequest("Nothing to update");

            var newValue = string.Join("; ", changes);
            if(newValue.Length > 1000) newValue = newValue[..997] + "...";

            await _db.SaveChangesAsync(ct);
            await _auditService.LogAsync(userName, AuditAction.RuleUpdated, nameof(Rule), rule.Id, oldValue: null, newValue: newValue, ct);
            await _referenceDataCache.RefreshAsync(ct); 

            return Ok(new { 
                rule.Id,
                rule.RuleCode,
                Type = rule.Type.ToString(),
                Action = rule.Action.ToString(),
                rule.Details,
                rule.ParametersJson,
                rule.Weight,
                rule.IsActive,
                rule.CreatedAt});

        }
    }
}
