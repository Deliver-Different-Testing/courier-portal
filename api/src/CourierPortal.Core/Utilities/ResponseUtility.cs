using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.Utilities
{
    /// <summary>
    /// Utility for responses to assist with de-cluttering other classes and code readability
    /// </summary>
    public static class ResponseUtility
    {
        public static TBaseResponse AddMessageAndReturnResponse<TBaseResponse>(TBaseResponse response, string message) where TBaseResponse : BaseResponse
        {
            response.Messages.Add(new MessageDto() { Message = message });
            return response;
        }
    }
}
