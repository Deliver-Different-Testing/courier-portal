using System;

namespace CourierPortal.Core.DTOs.Portal.Common
{
    public class FileResponse : BaseResponse
    {
        public FileResponse(Guid messageId) : base(messageId)
        {
        }

        public string FileName { get; set; }
        public string FileType { get; set; }
        public byte[] File { get; set; }
    }
}
