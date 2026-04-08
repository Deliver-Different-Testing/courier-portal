using System;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Invoices
{
    public class DownloadResponse : BaseResponse
    {
        public DownloadResponse(Guid messageId) : base(messageId)
        {
        }

        public string FileName { get; set; }
        public string Link { get; set; }
    }
}
