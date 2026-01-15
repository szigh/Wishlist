using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WishlistModels
{
    //may represent special gift-giving days like birthdays, holidays, anniversaries, etc. for a given user
    public class GiftDays
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public required string Title { get; set; } = string.Empty;

        [Range(1, 31)]
        public int Day { get; set; }

        [Range(1, 12)]
        public int Month { get; set; }

        public bool Protected { get; set; } = false;

        //foreign key
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        
    }
}
