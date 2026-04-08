using System.ComponentModel.DataAnnotations;
using CourierPortal.Core.DTOs.Portal.Common;

namespace CourierPortal.Core.DTOs.Portal.Couriers
{
    public class CourierUpdateRequest : BaseRequest
    {
        public string FirstName { get; set; }
        public string Surname { get; set; }
        [Phone]
        public string Phone { get; set; }
        [Phone]
        public string Mobile { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public string AddressLine5 { get; set; }
        public string AddressLine6 { get; set; }
        public string AddressLine7 { get; set; }
        public string AddressLine8 { get; set; }
        public string DriversLicenceNo { get; set; }
        public string VehicleRegistrationNo { get; set; }
        public string BankRoutingNumber { get; set; }
        public string BankAccountNo { get; set; }
        public string TaxNo { get; set; }
    }
}
