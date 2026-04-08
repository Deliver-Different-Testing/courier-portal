using System;
using CourierPortal.Core.DTOs.Portal.Common;

namespace CourierPortal.Core.DTOs.Portal.Auth
{
    public class CourierManagerAccessResponse : BaseResponse
    {
        public CourierManagerAccessResponse(Guid messageId) : base(messageId)
        {
        }

        public int Id { get; set; }
    }
}
