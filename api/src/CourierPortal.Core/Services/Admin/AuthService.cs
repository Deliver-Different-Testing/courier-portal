using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using CourierPortal.Core.DTOs.Admin.Auth;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.Services.Admin
{
    public class AdminAuthService
    {
        private readonly object _courierPortalAccessKeysLock = new object();
        private readonly List<CourierPortalAccessKeyDto> _courierPortalAccessKeys = new List<CourierPortalAccessKeyDto>();

        public CourierPortalAccessValidationResponse CourierPortalAccessValidation(CourierPortalAccessValidationRequest request)
        {
            CourierPortalAccessValidationResponse response = new CourierPortalAccessValidationResponse(request.MessageId);

            lock (_courierPortalAccessKeysLock)
            {
                if (_courierPortalAccessKeys.Any(x=>x.AccessKey == request.AccessKey))
                {
                    CourierPortalAccessKeyDto courierPortalAccessKey = _courierPortalAccessKeys.First(x => x.AccessKey == request.AccessKey);
                    //Remove this access key as it is one time use only.
                    _courierPortalAccessKeys.Remove(courierPortalAccessKey);
                    //Check if access key is expired.
                    if (courierPortalAccessKey.Expiry > DateTime.Now)
                    {
                        response.Id = courierPortalAccessKey.Id;
                        response.Success = true;
                        return response;
                    }
                }
            }

            response.Success = false;
            response.Messages.Add(new MessageDto() { Message = "Invalid access key." });
            return response;
        }

        public CreateCourierPortalAccessKeyResponse CreateCourierPortalAccessKey(IdentifierRequest request)
        {
            CreateCourierPortalAccessKeyResponse response = new CreateCourierPortalAccessKeyResponse(request.MessageId);

            string accessKey = string.Empty;
            lock (_courierPortalAccessKeysLock)
            {
                while (string.IsNullOrWhiteSpace(accessKey) || _courierPortalAccessKeys.Any(x => x.AccessKey == accessKey))
                {
                    var randomNumber = new byte[32];
                    using (var rng = RandomNumberGenerator.Create())
                    {
                        rng.GetBytes(randomNumber);
                        accessKey = BitConverter.ToUInt32(randomNumber).ToString();
                    }
                }

                _courierPortalAccessKeys.Add(new CourierPortalAccessKeyDto() { AccessKey = accessKey, Expiry = DateTime.Now.AddSeconds(10), Id = request.Id });
            }

            response.AccessKey = accessKey;
            response.Success = true;

            return response;
        }
    }
}
