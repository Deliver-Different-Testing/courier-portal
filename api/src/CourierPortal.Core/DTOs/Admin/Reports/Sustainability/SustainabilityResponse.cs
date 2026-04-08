//using System;
//using System.Collections.Generic;
//using CourierPortal.Core.DTOs.Admin.Common;

//namespace CourierPortal.Core.DTOs.Admin.Reports.Sustainability
//{
//    public class SustainabilityResponse : BaseResponse
//    {
//        public SustainabilityResponse(Guid messageId) : base(messageId)
//        {
//        }

//        public decimal Total { get; set; }
//        public decimal MinPerHour { get; set; }
//        public decimal MaxPerHour { get; set; }
//        public decimal AveragePerHour { get; set; }
//        public double MinHours { get; set; }
//        public double MaxHours { get; set; }
//        public double AverageHours { get; set; }

//        public IEnumerable<SustainabilitySummaryDto> Couriers { get; set; }
//        public IEnumerable<SustainabilitySummaryDto> Vehicles { get; set; }
//        public IEnumerable<SustainabilitySummaryRegionDto> Regions { get; set; }
//    }
//}
