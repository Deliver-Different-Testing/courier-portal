using System.ComponentModel.DataAnnotations;
using CourierPortal.Api.Core.Application.Dtos.Common;
using CourierPortal.Core.DTOs.Portal.Common;

namespace CourierPortal.Core.DTOs.Portal.Applications
{
    public class RegisterRequest : BaseRequest
    {
        public NameId Location { get; set; }
        public string VehicleType { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        [Phone]
        public string Phone { get; set; }
        [Phone]
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
