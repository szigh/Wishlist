namespace WishlistContracts.DTOs
{
    // For creating a gift day
    public class GiftDaysCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public int Day { get; set; }
        public int Month { get; set; }
        public bool Protected { get; set; } = false;
        // UserId is populated from JWT claims for non-admin users
        public int? UserId { get; set; } // Only used by admins
    }

    // For updating a gift day
    public class GiftDaysUpdateDto
    {
        public string Title { get; set; } = string.Empty;
        public int Day { get; set; }
        public int Month { get; set; }
        public bool Protected { get; set; } = false;
    }

    // For returning gift day info
    public class GiftDaysReadDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Day { get; set; }
        public int Month { get; set; }
        public bool Protected { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
    }
}