using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Contracts;
using CourierPortal.Core.Domain.Entities;

namespace CourierPortal.Core.DTOs.Admin.Applicants
{
    public class ApplicantDto
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public string Location { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string EmailVerificationCode { get; set; }
        public bool EmailVerified { get; set; }
        public string Address { get; set; }
        public string DriversLicenceNo { get; set; }
        public string VehicleType { get; set; }
        public string VehicleRegistrationNo { get; set; }
        public bool GstRegistered { get; set; }
        public string TaxNo { get; set; }
        public string BankRoutingNumber { get; set; }
        public string BankAccountNo { get; set; }
        public string NextOfKin { get; set; }
        public string NextOfKinRelationship { get; set; }
        public string NextOfKinPhone { get; set; }
        public string NextOfKinAddress { get; set; }
        public bool TrainingCompleted { get; set; }
        public DateTime? RejectDate { get; set; }
        public string RejectReason { get; set; }
        public int? CourierId { get; set; }
        public ContractDto Contract { get; set; }

        public DeclarationDto Declaration { get; set; }

        public IEnumerable<UploadDto> Uploads { get; set; }
    }
}
