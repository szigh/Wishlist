using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class Gift
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public required string Title { get; set; } = string.Empty;

        public string? Description { get; set; } 
        public string? Link { get; set; } 
        public string? Category { get; set; } 

        public bool IsTaken { get; set; } = false;

        // Foreign key
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
