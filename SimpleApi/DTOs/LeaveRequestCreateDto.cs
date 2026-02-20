using System;

namespace SimpleApi.DTOs
{
    public class LeaveRequestCreateDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string LeaveType { get; set; } = "Yıllık İzin";
    }
}