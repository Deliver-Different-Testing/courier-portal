using CourierPortal.Core.DTOs.Portal.Common;
using Microsoft.AspNetCore.Http;

namespace CourierPortal.Core.DTOs.Portal.Applications
{
    public class UploadRequest : BaseRequest
    {
        public int DocumentId { get; set; }
        public IFormFile File { get; set; }
    }
}
