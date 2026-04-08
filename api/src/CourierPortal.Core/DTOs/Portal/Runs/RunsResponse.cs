using System;
using System.Collections.Generic;
using CourierPortal.Core.DTOs.Portal.Common;

namespace CourierPortal.Core.DTOs.Portal.Runs
{
    public class RunsResponse : BaseResponse
    {

        public RunsResponse(Guid messageId) : base(messageId)
        {
            Current = new List<RunDto>();
            //Pending = new List<RunDto>();
            Past = new List<RunDto>();
        }

        public IEnumerable<RunDto> Current { get; set; }
        //public IEnumerable<RunDto> Pending { get; set; }
        public IEnumerable<RunDto> Past { get; set; }
        public IEnumerable<RunDto> Uninvoiced { get; set; }
    }
}
