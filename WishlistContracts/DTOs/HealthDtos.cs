namespace WishlistContracts.DTOs
{
    // For returning health check info
    public class HealthResponseDto
    {
        public string Status { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
