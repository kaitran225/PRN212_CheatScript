namespace PerfumeRepository.DTOs
{
    public class UserDTO
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Note { get; set; }
        public int? Role { get; set; }
        public bool IsAuthenticated { get; set; }
        
        // Helper properties for role-based checks
        public bool IsManager => Role == 2;
        public bool IsStaff => Role == 3;
    }
} 