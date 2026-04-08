using System;
using CourierPortal.Core.DTOs.Portal.Common;

namespace CourierPortal.Core.DTOs.Portal.Runs
{
    public class RunRequest : BaseRequest
    {
        public DateTime BookDate { get; set; }
        public string RunName { get; set; }
    }
}
