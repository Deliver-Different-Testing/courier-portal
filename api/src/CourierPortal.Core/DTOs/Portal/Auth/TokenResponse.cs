using System;
using CourierPortal.Core.DTOs.Portal.Common;

namespace CourierPortal.Core.DTOs.Portal.Auth
{
    public class TokenResponse : BaseResponse
    {
        public TokenResponse(Guid messageId) : base(messageId)
        {
        }

        public TokenDto Results { get; set; }
    }
}
