using System;
using CourierPortal.Core.DTOs.Portal.Common;

namespace CourierPortal.Core.DTOs.Portal.Applications
{
    public class ProfileResponse : BaseResponse
    {
        public ProfileResponse(Guid messageId) : base(messageId)
        {
        }

        public ProfileDto Profile { get; set; }
    }
}
