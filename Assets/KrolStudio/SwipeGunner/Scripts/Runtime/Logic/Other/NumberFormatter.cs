
namespace KrolStudio
{
    public static class NumberFormatter
    {
        public static string Format(long value)
        {
            if (value >= 1_000_000_000_000)
                return (value / 1_000_000_000_000f).ToString("0.###") + "T";
            if (value >= 1_000_000_000)
                return (value / 1_000_000_000f).ToString("0.###") + "B";
            if (value >= 1_000_000)
                return (value / 1_000_000f).ToString("0.###") + "M";
            if (value >= 1_000)
                return (value / 1_000f).ToString("0.###") + "K";

            return value.ToString();
        }
    }
}