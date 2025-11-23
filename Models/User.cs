using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class User
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public required string Name { get; set; }

        public required string PasswordHash { get; set; }

        [MaxLength(20)]
        public required string Role { get; set; } = "User"; //"Admin" or "User"

        //navigation properties
        public ICollection<Gift> Gifts { get; set; } = new List<Gift>();
    }
}
