using System.Collections.Generic;

namespace SimpleApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int LeaveBalance { get; set; } = 14;
        public bool IsActive { get; set; } = true;

        public ICollection<LeaveRequest>? LeaveRequests { get; set; }
    }
}