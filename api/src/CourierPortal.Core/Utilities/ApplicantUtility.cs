using CourierPortal.Core.Domain.Entities;

namespace CourierPortal.Core.Utilities
{
    public class ApplicantUtility
    {

        public static bool HasCompletedProfile(CourierApplicant applicant)
        {
            return !string.IsNullOrWhiteSpace(applicant.FirstName)
                   && !string.IsNullOrWhiteSpace(applicant.Surname)
                   && applicant.DateOfBirth.HasValue
                   && !string.IsNullOrWhiteSpace(applicant.Mobile)
                   && !string.IsNullOrWhiteSpace(applicant.Email)
                   && !string.IsNullOrWhiteSpace(applicant.Address)
                   && !string.IsNullOrWhiteSpace(applicant.DriversLicenceNo)
                   && !string.IsNullOrWhiteSpace(applicant.VehicleType)
                   && !string.IsNullOrWhiteSpace(applicant.VehicleRegistrationNo)
                   && !string.IsNullOrWhiteSpace(applicant.TaxNo)
                   && !string.IsNullOrWhiteSpace(applicant.BankAccountNo)
                   && !string.IsNullOrWhiteSpace(applicant.NextOfKin)
                   && !string.IsNullOrWhiteSpace(applicant.NextOfKinRelationship)
                   && !string.IsNullOrWhiteSpace(applicant.NextOfKinPhone)
                   && !string.IsNullOrWhiteSpace(applicant.NextOfKinAddress);
        }

        public static bool HasCompletedDeclaration(CourierApplicant applicant)
        {
            return applicant.ContractId.HasValue
                   && applicant.DeclarationDate.HasValue
                   && applicant.DeclarationAgree
                   && !string.IsNullOrWhiteSpace(applicant.DeclarationName)
                   && !string.IsNullOrWhiteSpace(applicant.DeclarationSignatureFileName)
                   && !string.IsNullOrWhiteSpace(applicant.DeclarationSignatureType)
                   && applicant.DeclarationSignatureLength.HasValue
                   && applicant.DeclarationSignatureLength.Value > 0
                   && applicant.DeclarationSignature != null;
        }

        public static void ClearDeclarationNoSave(CourierApplicant applicant)
        {
            applicant.DeclarationText = null;
            applicant.DeclarationDate = null;
            applicant.DeclarationAgree = false;
            applicant.DeclarationName = null;
            applicant.DeclarationSignatureFileName = null;
            applicant.DeclarationSignatureType = null;
            applicant.DeclarationSignatureLength = null;
            applicant.DeclarationSignature = null;
        }
    }
}
