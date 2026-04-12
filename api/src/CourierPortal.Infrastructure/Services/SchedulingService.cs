using CourierPortal.Core.Interfaces;
using CourierPortal.Core.Domain.Entities;
using CourierPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Infrastructure.Services;

/// <summary>
/// Courier scheduling — CRUD for schedules, copy between dates, template management.
/// </summary>
public class SchedulingService : ISchedulingService
{
    private readonly AgentsDbContext _db;

    public SchedulingService(AgentsDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task<IReadOnlyList<CourierSchedule>> GetSchedulesAsync(int locationId, DateTime date)
    {
        return await _db.CourierSchedules
            .Include(s => s.Courier)
            .Where(s => s.LocationId == locationId && s.Date.Date == date.Date)
            .OrderBy(s => s.StartTime)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CourierSchedule>> GetCourierScheduleAsync(int courierId, DateTime startDate, DateTime endDate)
    {
        return await _db.CourierSchedules
            .Where(s => s.CourierId == courierId && s.Date >= startDate.Date && s.Date <= endDate.Date)
            .OrderBy(s => s.Date).ThenBy(s => s.StartTime)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<CourierSchedule> CreateScheduleAsync(CourierSchedule schedule)
    {
        _db.CourierSchedules.Add(schedule);
        await _db.SaveChangesAsync();
        return schedule;
    }

    /// <inheritdoc />
    public async Task<CourierSchedule?> UpdateScheduleAsync(int id, CourierSchedule update)
    {
        var schedule = await _db.CourierSchedules.FindAsync(id);
        if (schedule is null) return null;

        schedule.CourierId = update.CourierId;
        schedule.LocationId = update.LocationId;
        schedule.Date = update.Date;
        schedule.StartTime = update.StartTime;
        schedule.EndTime = update.EndTime;
        schedule.Status = update.Status;
        schedule.Notes = update.Notes;

        await _db.SaveChangesAsync();
        return schedule;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteScheduleAsync(int id)
    {
        var schedule = await _db.CourierSchedules.FindAsync(id);
        if (schedule is null) return false;

        _db.CourierSchedules.Remove(schedule);
        await _db.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<int> CopyScheduleAsync(int locationId, DateTime fromDate, DateTime toDate)
    {
        var source = await _db.CourierSchedules
            .Where(s => s.LocationId == locationId && s.Date.Date == fromDate.Date)
            .ToListAsync();

        var copies = source.Select(s => new CourierSchedule
        {
            CourierId = s.CourierId,
            LocationId = s.LocationId,
            Date = toDate.Date,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            Status = "Scheduled",
            Notes = s.Notes
        }).ToList();

        _db.CourierSchedules.AddRange(copies);
        await _db.SaveChangesAsync();
        return copies.Count;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<NpCourier>> GetAvailableCouriersAsync(int locationId, DateTime date)
    {
        var scheduledCourierIds = await _db.CourierSchedules
            .Where(s => s.LocationId == locationId && s.Date.Date == date.Date)
            .Select(s => s.CourierId)
            .ToListAsync();

        return await _db.NpCouriers
            .Where(c => c.RecordStatusId == 1 && c.Status == "Active" && !scheduledCourierIds.Contains(c.CourierId))
            .OrderBy(c => c.SurName)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ScheduleTemplate>> GetTemplatesAsync(int tenantId)
    {
        return await _db.ScheduleTemplates
            .Where(t => t.TenantId == tenantId)
            .OrderBy(t => t.DayOfWeek).ThenBy(t => t.StartTime)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<ScheduleTemplate> CreateTemplateAsync(ScheduleTemplate template)
    {
        _db.ScheduleTemplates.Add(template);
        await _db.SaveChangesAsync();
        return template;
    }
}
