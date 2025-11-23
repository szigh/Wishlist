namespace WishlistModels
{
    public class Volunteer
    {
        public int Id { get; set; }

        // Foreign keys
        public int GiftId { get; set; }
        public Gift Gift { get; set; } = null!;

        public int VolunteerUserId { get; set; }
        public User VolunteerUser { get; set; } = null!;
    }
}
