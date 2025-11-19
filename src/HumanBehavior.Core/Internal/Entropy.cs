using System.Security.Cryptography;

namespace HumanBehavior.Core.Internal;

internal static class Entropy
{
    private static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();

    public static double NextDouble()
    {
        var value = NextUInt32();
        return value / (double)uint.MaxValue;
    }

    public static double NextDouble(double min, double max)
    {
        if (min > max)
        {
            (min, max) = (max, min);
        }

        return min + (max - min) * NextDouble();
    }

    public static int NextInt(int minInclusive, int maxExclusive)
    {
        if (minInclusive >= maxExclusive)
        {
            throw new ArgumentOutOfRangeException(nameof(maxExclusive), "maxExclusive must be greater than minInclusive");
        }

        var range = (long)maxExclusive - minInclusive;
        var limit = uint.MaxValue - (uint.MaxValue % (uint)range);
        uint value;

        do
        {
            value = NextUInt32();
        } while (value >= limit);

        return (int)(minInclusive + (value % range));
    }

    public static TimeSpan NextTimeSpan(TimeSpan min, TimeSpan max)
    {
        if (min > max)
        {
            (min, max) = (max, min);
        }

        var ticks = NextDouble(min.Ticks, max.Ticks);
        return TimeSpan.FromTicks((long)ticks);
    }

    private static uint NextUInt32()
    {
        var buffer = new byte[4];
        Rng.GetBytes(buffer);
        return BitConverter.ToUInt32(buffer, 0);
    }
}

