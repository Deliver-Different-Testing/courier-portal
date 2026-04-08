using System;
using System.Collections.Generic;
using System.Linq;
using CourierPortal.Core.Services.Admin;
using CourierPortal.Core.Domain.Entities;

namespace CourierPortal.Core.Utilities
{
    public static class ScheduleUtility
    {
        public static bool HasStarted(CourierSchedule schedule, DateTime tenantTime)
        {
            return schedule.BookDate.Date < tenantTime.Date || (schedule.BookDate.Date == tenantTime.Date && schedule.StartTime.ToTimeSpan() <= tenantTime.TimeOfDay);
        }

        public static bool HasNotEnded(CourierSchedule schedule, DateTime tenantTime)
        {
            return schedule.BookDate.Date > tenantTime.Date || !(schedule.BookDate.Date == tenantTime.Date && schedule.EndTime.ToTimeSpan() <= tenantTime.TimeOfDay);
        }

        public static bool HasConflictingSchedule(CourierSchedule schedule, IEnumerable<CourierSchedule> schedules, bool checkNotificationSent = false)
        {
            return checkNotificationSent
                ? schedules.Any(s => s.NotificationSent.HasValue
                                     && s.Id != schedule.Id
                                     && ((s.StartTime >= schedule.StartTime && s.StartTime < schedule.EndTime)
                                         || (s.EndTime > schedule.StartTime && s.EndTime <= schedule.EndTime)
                                         || (s.StartTime <= schedule.StartTime && s.EndTime >= schedule.EndTime)))
                : schedules.Any(s => s.Id != schedule.Id
                                     && s.BookDate.Date == schedule.BookDate.Date
                                     && s.BookDate.Date == schedule.BookDate.Date
                                     && ((s.StartTime >= schedule.StartTime && s.StartTime < schedule.EndTime)
                                         || (s.EndTime > schedule.StartTime && s.EndTime <= schedule.EndTime)
                                         || (s.StartTime <= schedule.StartTime && s.EndTime >= schedule.EndTime)));
        }

        public static bool IsScheduleConflicting(CourierSchedule schedule, CourierSchedule schedule2)
        {
            return schedule.Id != schedule2.Id
                   && schedule.BookDate.Date == schedule2.BookDate.Date
                   && ((schedule.StartTime >= schedule2.StartTime && schedule.StartTime < schedule2.EndTime)
                       || (schedule.EndTime > schedule2.StartTime && schedule.EndTime <= schedule2.EndTime)
                       || (schedule.StartTime <= schedule2.StartTime && schedule.EndTime >= schedule2.EndTime));
        }
    }
}
