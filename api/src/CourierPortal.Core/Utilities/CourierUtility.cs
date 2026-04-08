using CourierPortal.Core.DTOs.Admin.Common;
using CourierPortal.Core.Domain.Entities;

namespace CourierPortal.Core.Utilities
{
    public static class CourierUtility
    {
        public static CourierDetailsDto MapToCourierDto(TucCourier courier)
        {
            return new CourierDetailsDto()
            {
                Id = courier.UccrId,
                Code = courier.Code,
                Name = $"{courier.UccrName} {courier.UccrSurname}",
                FirstName = courier.UccrName,
                Surname = courier.UccrSurname,
                Phone = courier.UccrTel,
                Mobile = string.IsNullOrWhiteSpace(courier.PersonalMobile)? courier.UccrMobile : courier.PersonalMobile,
                Email = courier.UccrEmail,
                Address = courier.UccrAddress,
                GstNumber = string.IsNullOrWhiteSpace(courier.UccrGst) || (courier.WithholdingTaxPercentage.HasValue && courier.WithholdingTaxPercentage > 0) ? null : courier.UccrGst,
                IrdNumber = string.IsNullOrWhiteSpace(courier.UccrGst) || !courier.WithholdingTaxPercentage.HasValue || courier.WithholdingTaxPercentage == 0 ? null : courier.UccrGst,
                WithholdingTaxPercentage = courier.WithholdingTaxPercentage / 100 ?? 0,
                DriversLicenceNo = courier.UccrDlno,
                VehicleRegistrationNo = courier.UccrReg,
                VehicleType = courier.UccrVehicle,
                Active = courier.Active,
                Location = courier.Region?.Name ?? "Unassigned",
                ExternalId = courier.OpenForceNumber
            };
        }
    }
}
