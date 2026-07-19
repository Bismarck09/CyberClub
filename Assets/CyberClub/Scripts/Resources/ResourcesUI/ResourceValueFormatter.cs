using System;
using System.Globalization;

public static class ResourceValueFormatter
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    public static string Format(int value)
    {
        return Format((long)value);
    }

    public static string Format(long value)
    {
        long absValue = Math.Abs(value);
        string sign = value < 0 ? "-" : string.Empty;

        if (absValue < 1000)
            return value.ToString(Culture);

        if (absValue < 1_000_000)
            return sign + FormatShort(absValue / 1000d, "k");

        if (absValue < 1_000_000_000)
            return sign + FormatShort(absValue / 1_000_000d, "m");

        if (absValue < 1_000_000_000_000)
            return sign + FormatShort(absValue / 1_000_000_000d, "b");

        return sign + FormatShort(absValue / 1_000_000_000_000d, "t");
    }

    public static string FormatSigned(int value)
    {
        if (value > 0)
            return "+" + Format(value);

        return Format(value);
    }

    private static string FormatShort(double value, string suffix)
    {
        string format;

        if (value >= 100d)
            format = "0";
        else if (value >= 10d)
            format = "0.#";
        else
            format = "0.##";

        return value.ToString(format, Culture) + suffix;
    }
}
