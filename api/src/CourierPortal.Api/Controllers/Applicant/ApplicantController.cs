using CourierPortal.Api.Core.Domain;
using CourierPortal.Api.Core.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Api.Controllers.Applicant;

[ApiController]
[Route("api/[controller]")]
public class ApplicantController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await db.Applicants.AsNoTracking().OrderByDescending(a => a.AppliedDate).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var applicant = await db.Applicants
            .AsNoTracking()
            .Include(a => a.Documents).ThenInclude(d => d.DocumentType)
            .Include(a => a.Stages)
            .Include(a => a.RecruitmentNotes)
            .Include(a => a.QuizAttempts)
            .FirstOrDefaultAsync(a => a.Id == id);
        return applicant is null ? NotFound() : Ok(applicant);
    }

    [HttpGet("status/{email}")]
    public async Task<IActionResult> GetByEmail(string email)
    {
        var applicant = await db.Applicants
            .AsNoTracking()
            .Include(a => a.Documents)
            .Include(a => a.QuizAttempts)
            .FirstOrDefaultAsync(a => a.Email == email);
        return applicant is null ? NotFound(new { error = "No application found" }) : Ok(applicant);
    }

    [HttpPost]
    public async Task<IActionResult> Apply([FromBody] Applicant applicant)
    {
        applicant.AppliedDate = DateTime.UtcNow;
        applicant.StatusChangedDate = DateTime.UtcNow;
        applicant.Status = ApplicantStatus.Applied;
        db.Applicants.Add(applicant);

        db.RecruitmentStages.Add(new RecruitmentStage
        {
            Applicant = applicant,
            Stage = "Applied",
            CreatedBy = "System"
        });

        await db.SaveChangesAsync();
        return Ok(applicant);
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        var applicant = await db.Applicants.FindAsync(id);
        if (applicant is null) return NotFound();

        applicant.Status = request.Status;
        applicant.StatusChangedDate = DateTime.UtcNow;

        db.RecruitmentStages.Add(new RecruitmentStage
        {
            ApplicantId = id,
            Stage = request.Status.ToString(),
            Notes = request.Notes,
            CreatedBy = request.CreatedBy ?? "Admin"
        });

        await db.SaveChangesAsync();
        return Ok(applicant);
    }

    [HttpPost("{id:int}/step-data")]
    public async Task<IActionResult> SaveStepData(int id, [FromBody] SaveStepDataRequest request)
    {
        var applicant = await db.Applicants.FindAsync(id);
        if (applicant is null) return NotFound();

        var existing = await db.ApplicantStepData
            .FirstOrDefaultAsync(d => d.ApplicantId == id && d.PortalStepId == request.PortalStepId);

        if (existing != null)
        {
            existing.FieldData = request.FieldData;
            existing.AiExtractedData = request.AiExtractedData;
            existing.AiConfirmedFields = request.AiConfirmedFields;
            existing.UpdatedDate = DateTime.UtcNow;
        }
        else
        {
            db.ApplicantStepData.Add(new ApplicantStepData
            {
                ApplicantId = id,
                PortalStepId = request.PortalStepId,
                StepType = request.StepType,
                FieldData = request.FieldData,
                AiExtractedData = request.AiExtractedData,
                AiConfirmedFields = request.AiConfirmedFields
            });
        }

        await db.SaveChangesAsync();
        return Ok();
    }
}

public record SaveStepDataRequest(int PortalStepId, string StepType, string? FieldData, string? AiExtractedData = null, string? AiConfirmedFields = null);
public record UpdateStatusRequest(ApplicantStatus Status, string? Notes, string? CreatedBy);
