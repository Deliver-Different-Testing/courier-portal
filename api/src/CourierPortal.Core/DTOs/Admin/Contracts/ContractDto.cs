using System;

namespace CourierPortal.Core.DTOs.Admin.Contracts
{
    public class ContractDto
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public string FileName { get; set; }
        public string Type { get; set; }
        public long Length { get; set; }
    }
}
