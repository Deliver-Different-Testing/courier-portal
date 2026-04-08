using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CourierPortal.Core.DTOs.Portal.Reports
{
    public class ReportParametersDto
    {
        //public enum ReportFormat
        //{
        //    CSV,
        //    EXCEL,
        //    PDF
        //}
        public int ReportId { get; set; }
        //[JsonConverter(typeof(StringEnumConverter))]
        public string Format { get; set; }
        public DateTime? StartDate  { get; set; }
        public DateTime? EndDate { get; set; }
        public int? YearMonth { get; set; }
    }
}
