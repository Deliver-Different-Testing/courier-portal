using CourierPortal.Core.Domain.Entities;

namespace CourierPortal.Core.Interfaces;

/// <summary>Courier scheduling and template management.</summary>
public interface ISchedulingService
{
    Task<IReadOnlyList<CourierSchedule>> GetSchedulesAsync(int locationId, DateTime date);
    Task<IReadOnlyList<CourierSchedule>> GetCourierScheduleAsync(int courierId, DateTime startDate, DateTime endDate);
    Task<CourierSchedule> CreateScheduleAsync(CourierSchedule schedule);
    Task<CourierSchedule?> UpdateScheduleAsync(int id, CourierSchedule schedule);
    Task<bool> DeleteScheduleAsync(int id);
    Task<int> CopyScheduleAsync(int locationId, DateTime fromDate, DateTime toDate);
    Task<IReadOnlyList<NpCourier>> GetAvailableCouriersAsync(int locationId, DateTime date);

    // Templates
    Task<IReadOnlyList<ScheduleTemplate>> GetTemplatesAsync(int tenantId);
    Task<ScheduleTemplate> CreateTemplateAsync(ScheduleTemplate template);
}
