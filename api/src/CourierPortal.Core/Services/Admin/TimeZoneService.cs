using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace CourierPortal.Core.Services.Admin
{
    public class AdminTimeZoneService(IHttpContextAccessor httpContextAccessor)
    {        // Mapping from IANA timezone names to Windows timezone names
        private static readonly Dictionary<string, string> IanaToWindowsTimezoneMap = new()
        {
            { "Pacific/Auckland", "New Zealand Standard Time" },
            { "Australia/Sydney", "AUS Eastern Standard Time" },
            { "Australia/Melbourne", "AUS Eastern Standard Time" },
            { "Australia/Brisbane", "E. Australia Standard Time" },
            { "Australia/Perth", "W. Australia Standard Time" },
            { "America/New_York", "Eastern Standard Time" },
            { "America/Chicago", "Central Standard Time" },
            { "America/Denver", "Mountain Standard Time" },
            { "America/Los_Angeles", "Pacific Standard Time" },
            { "Europe/London", "GMT Standard Time" },
            { "UTC", "UTC" }
        };

        public string GetTenantTimeZone()
        {
            var timeZone = httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == "TimeZone")?.Value;

            if (string.IsNullOrEmpty(timeZone))
            {
                Log.Warning("TimeZone claim not found, defaulting to UTC");
                return "UTC";
            }

            // Check if it's already a Windows timezone or if we need to convert from IANA
            if (IanaToWindowsTimezoneMap.TryGetValue(timeZone, out var windowsTimeZone))
            {
                Log.Information("Converted IANA timezone {IanaTimeZone} to Windows timezone {WindowsTimeZone}", timeZone, windowsTimeZone);
                return windowsTimeZone;
            }

            // Assume it's already in Windows format
            Log.Information("Using timezone from claims: {TimeZone}", timeZone);
            return timeZone;
        }

        public DateTime GetTenantTime()
        {
            var timeZone = GetTenantTimeZone();
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo);
        }
        public DateTime ConvertUtcToTenantTime(DateTime time)
        {
            if (time.Kind != DateTimeKind.Utc)
                throw new Exception("Must be UTC Kind");

            var timeZone = GetTenantTimeZone();
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            return TimeZoneInfo.ConvertTimeFromUtc(time, timeZoneInfo);
        }
    }
}
