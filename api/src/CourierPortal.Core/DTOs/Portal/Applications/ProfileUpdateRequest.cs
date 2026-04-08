using System;
using System.ComponentModel.DataAnnotations;
using CourierPortal.Api.Core.Application.Dtos.Common;
using CourierPortal.Core.DTOs.Portal.Common;

namespace CourierPortal.Core.DTOs.Portal.Applications
{
    public class ProfileUpdateRequest : BaseRequest
    {
        public NameId Location { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public DateTime? DateOfBirth { get; set; }
        [Phone]
        public string Phone { get; set; }
        [Phone]
        public string Mobile { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public string AddressLine5 { get; set; }
        public string AddressLine6 { get; set; }
        public string AddressLine7 { get; set; }
        public string AddressLine8 { get; set; }
        public string DriversLicenceNo { get; set; }
        public string VehicleType { get; set; }
        public string VehicleRegistrationNo { get; set; }
        public bool GstRegistered { get; set; }
        public string TaxNo { get; set; }
        public string BankRoutingNumber { get; set; }
        public string BankAccountNo { get; set; }
        public string NextOfKin { get; set; }
        public string NextOfKinRelationship { get; set; }
        [Phone]
        public string NextOfKinPhone { get; set; }
        public string NextOfKinAddress { get; set; }
    }
}
