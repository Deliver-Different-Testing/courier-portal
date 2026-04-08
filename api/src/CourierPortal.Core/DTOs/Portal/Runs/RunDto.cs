using System;
using System.Collections.Generic;

namespace CourierPortal.Core.DTOs.Portal.Runs
{
    public class RunDto
    {
        public int Id { get; set; }
        public int RunType { get; set; }
        public DateTime BookDate { get; set; }
        public string RunName { get; set; }
        public int Time { get; set; }
        public double Kms { get; set; }
        public decimal Amount { get; set; }
        public decimal MasterAmount { get; set; }
        public IEnumerable<RunMasterDto> Masters { get; set; }
        public string Cities { get; set; }
        public string States { get; set; }
        public IEnumerable<JobDto> Jobs { get; set; }
        public string DateDisplay { get; set; }
    }
}
