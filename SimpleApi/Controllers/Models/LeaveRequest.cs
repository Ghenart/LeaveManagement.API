using System;

namespace SimpleApi.Models
{
    public class LeaveRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "Pending";
        public string? DocumentPath { get; set; }
        public bool IsDeleted { get; set; }
        public string LeaveType { get; set; } = "Yıllık İzin";
    }
}