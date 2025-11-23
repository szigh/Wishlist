namespace WishlistContracts.DTOs
{
    // For creating a gift
    public class GiftCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Link { get; set; }
        public string? Category { get; set; }
        public int UserId { get; set; } // owner
    }

    // For updating a gift
    public class GiftUpdateDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Link { get; set; }
        public string? Category { get; set; }
        public bool IsTaken { get; set; }
    }

    // For returning gift info
    public class GiftReadDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Link { get; set; }
        public string? Category { get; set; }
        public bool IsTaken { get; set; }
        public int UserId { get; set; }
    }
}
