namespace Kart.Project_Files.Scripts.Extensions
{
    public static class IntExtensions
    {
        public static string ToOrdinal(this int number)
        {
            if (number <= 0) return number.ToString();
            int rem100 = number % 100;
            if (rem100 >= 11 && rem100 <= 13)
                return number + "th";
            return (number % 10) switch
            {
                1 => number + "st",
                2 => number + "nd",
                3 => number + "rd",
                _ => number + "th"
            };
        }
    }
}