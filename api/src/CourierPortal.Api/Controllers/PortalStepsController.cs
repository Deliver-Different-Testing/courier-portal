using CourierPortal.Core.Domain;
using CourierPortal.Core.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Api.Controllers;

[ApiController]
[Route("api/portal-steps")]
public class PortalStepsController(CourierPortalContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var steps = await db.PortalSteps.OrderBy(s => s.SortOrder).ToListAsync();
        return Ok(steps);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PortalStep step)
    {
        if (step.SortOrder == 0)
            step.SortOrder = (await db.PortalSteps.MaxAsync(s => (int?)s.SortOrder) ?? 0) + 1;
        db.PortalSteps.Add(step);
        await db.SaveChangesAsync();
        return Ok(step);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] PortalStep updated)
    {
        var step = await db.PortalSteps.FindAsync(id);
        if (step is null) return NotFound();
        step.Title = updated.Title;
        step.Description = updated.Description;
        step.IsRequired = updated.IsRequired;
        step.IsActive = updated.IsActive;
        step.SortOrder = updated.SortOrder;
        step.Config = updated.Config;
        step.DocumentTypeId = updated.DocumentTypeId;
        step.StepType = updated.StepType;
        await db.SaveChangesAsync();
        return Ok(step);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var step = await db.PortalSteps.FindAsync(id);
        if (step is null) return NotFound();
        db.PortalSteps.Remove(step);
        await db.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("reorder")]
    public async Task<IActionResult> Reorder([FromBody] List<ReorderItem> items)
    {
        foreach (var item in items)
        {
            var step = await db.PortalSteps.FindAsync(item.Id);
            if (step != null) step.SortOrder = item.SortOrder;
        }
        await db.SaveChangesAsync();
        return Ok();
    }
}

public record ReorderItem(int Id, int SortOrder);
