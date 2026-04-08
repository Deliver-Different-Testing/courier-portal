using System.Collections;
using System.Collections.Generic;

namespace CourierPortal.Core.DTOs.Admin.Openforce
{
    public class OfBadRequest
    {
        public IEnumerable<string> Errors { get; set; }
    }
}
