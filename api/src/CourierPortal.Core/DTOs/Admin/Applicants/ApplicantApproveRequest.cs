using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Applicants
{
    public class ApplicantApproveRequest : BaseRequest
    {
        public int Id { get; set; }
        public int CourierTypeId { get; set; }
        public int? MasterCourierId { get; set; }
        public string Code { get; set; }
        public string Fleet { get; set; }
    }
}
