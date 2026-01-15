using WishlistModels;

namespace WishlistContracts
{
    public static class GiftDaysExtensions
    {
        extension(GiftDays gd)
        {
            /// <summary>
            /// Gets the next occurrence of this gift day starting from today.
            /// For Feb 29, returns Feb 28 in non-leap years.
            /// </summary>
            public DateOnly NextOccurance
            {
                get
                {
                    var today = DateOnly.FromDateTime(DateTime.Today);
                    var currentYear = today.Year;

                    // Try to create date in current year
                    var thisYearDate = GetDateForYear(gd, currentYear);

                    // If the date hasn't passed this year, return it
                    if (thisYearDate >= today)
                        return thisYearDate;

                    // Otherwise return next year's date
                    return GetDateForYear(gd, currentYear + 1);
                }
            }
        }

        /// <summary>
        /// Gets the date for this gift day in a specific year.
        /// For Feb 29 in non-leap years, returns Feb 28.
        /// </summary>
        public static DateOnly GetDateForYear(GiftDays gd, int currentYear)
        {
            // Handle Feb 29 in non-leap years
            if (gd.Month == 2 && gd.Day == 29 && !DateTime.IsLeapYear(currentYear))
            {
                return new DateOnly(currentYear, 2, 28);
            }

            return new DateOnly(currentYear, 2, 29);
        }
    }
}
