using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Admin.Common;

namespace CourierPortal.Core.DTOs.Admin.Contracts
{
    public class ContractsResponse : BaseResponse
    {
        public ContractsResponse(Guid messageId) : base(messageId)
        {
        }

        public IEnumerable<ContractDto> Contracts { get; set; }
    }
}
