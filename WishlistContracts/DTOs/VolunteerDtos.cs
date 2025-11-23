namespace WishlistContracts.DTOs
{
    // For volunteering for a gift
    public class VolunteerCreateDto
    {
        public int GiftId { get; set; }
        // VolunteerUserId is populated from JWT claims, not from request body
    }

    // For returning volunteer info
    public class VolunteerReadDto
    {
        public int Id { get; set; }
        public int GiftId { get; set; }
        public int VolunteerUserId { get; set; }
    }
}
