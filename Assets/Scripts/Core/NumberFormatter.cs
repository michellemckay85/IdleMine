namespace GoldAndGoblins.Core
{
    /// <summary>Formats big idle-game numbers as e.g. 12.3K / 4.56M / 7.89B for UI display.</summary>
    public static class NumberFormatter
    {
        private static readonly string[] Suffixes = { "", "K", "M", "B", "T", "Qa", "Qi" };

        public static string Format(double value)
        {
            if (value < 1000) return value < 0 ? "0" : value.ToString("0");

            int suffixIndex = 0;
            while (value >= 1000 && suffixIndex < Suffixes.Length - 1)
            {
                value /= 1000;
                suffixIndex++;
            }

            return $"{value:0.##}{Suffixes[suffixIndex]}";
        }
    }
}
