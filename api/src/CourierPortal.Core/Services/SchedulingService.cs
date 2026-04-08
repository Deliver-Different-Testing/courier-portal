using CourierPortal.Core.Domain;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Core.Services;

public class SchedulingService : ISchedulingService
{
    private readonly IDbContextFactory<DespatchContext> _contextFactory;

    public SchedulingService(IDbContextFactory<DespatchContext> contextFactory) => _contextFactory = contextFactory;

    public async Task<IReadOnlyList<CourierSchedule>> GetSchedulesAsync(int locationId, DateTime date)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.CourierSchedules
            .Include(s => s.Courier)
            .Where(s => s.LocationId == locationId && s.Date.Date == date.Date)
            .OrderBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<CourierSchedule>> GetCourierScheduleAsync(int courierId, DateTime startDate, DateTime endDate)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.CourierSchedules
            .Where(s => s.CourierId == courierId && s.Date >= startDate.Date && s.Date <= endDate.Date)
            .OrderBy(s => s.Date).ThenBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<CourierSchedule> CreateScheduleAsync(CourierSchedule schedule)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        schedule.CreatedDate = DateTime.UtcNow;
        db.CourierSchedules.Add(schedule);
        await db.SaveChangesAsync();
        return schedule;
    }

    public async Task<CourierSchedule?> UpdateScheduleAsync(int id, CourierSchedule schedule)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var existing = await db.CourierSchedules.FindAsync(id);
        if (existing is null) return null;

        existing.CourierId = schedule.CourierId;
        existing.LocationId = schedule.LocationId;
        existing.Date = schedule.Date;
        existing.StartTime = schedule.StartTime;
        existing.EndTime = schedule.EndTime;
        existing.Status = schedule.Status;
        existing.Notes = schedule.Notes;

        await db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteScheduleAsync(int id)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        var existing = await db.CourierSchedules.FindAsync(id);
        if (existing is null) return false;

        db.CourierSchedules.Remove(existing);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<int> CopyScheduleAsync(int locationId, DateTime fromDate, DateTime toDate)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();

        var sourceSchedules = await db.CourierSchedules
            .Where(s => s.LocationId == locationId && s.Date.Date == fromDate.Date)
            .ToListAsync();

        var dayOffset = (toDate.Date - fromDate.Date).Days;
        var copies = sourceSchedules.Select(s => new CourierSchedule
        {
            CourierId = s.CourierId,
            LocationId = s.LocationId,
            Date = s.Date.AddDays(dayOffset),
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            Status = "Scheduled",
            Notes = s.Notes,
            CreatedDate = DateTime.UtcNow
        }).ToList();

        db.CourierSchedules.AddRange(copies);
        await db.SaveChangesAsync();
        return copies.Count;
    }

    public async Task<IReadOnlyList<NpCourier>> GetAvailableCouriersAsync(int locationId, DateTime date)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();

        // Get couriers who are NOT already scheduled at this location on this date
        var scheduledCourierIds = await db.CourierSchedules
            .Where(s => s.LocationId == locationId && s.Date.Date == date.Date)
            .Select(s => s.CourierId)
            .ToListAsync();

        return await db.Set<NpCourier>()
            .Where(c => c.Status == "Active" && c.RecordStatusId == 1
                         && !scheduledCourierIds.Contains(c.CourierId))
            .OrderBy(c => c.Code)
            .ToListAsync();
    }

    // Templates
    public async Task<IReadOnlyList<ScheduleTemplate>> GetTemplatesAsync(int tenantId)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        return await db.Set<ScheduleTemplate>()
            .Where(t => t.TenantId == tenantId)
            .OrderBy(t => t.DayOfWeek).ThenBy(t => t.StartTime)
            .ToListAsync();
    }

    public async Task<ScheduleTemplate> CreateTemplateAsync(ScheduleTemplate template)
    {
        await using var db = await _contextFactory.CreateDbContextAsync();
        db.Set<ScheduleTemplate>().Add(template);
        await db.SaveChangesAsync();
        return template;
    }
}
