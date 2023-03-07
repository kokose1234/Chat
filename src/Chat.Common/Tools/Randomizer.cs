namespace Chat.Common.Tools;

public static class Randomizer
{
    private static readonly Random Random = new Random();

    public static int NextInt() => Random.Next();

    public static int NextInt(int bound) => Random.Next(bound);

    public static long NextLong()
    {
        var buf = new byte[8];
        Random.NextBytes(buf);
        var longRand = BitConverter.ToInt64(buf, 0);

        return (Math.Abs(longRand % (long.MaxValue - 0)) + 0);
    }

    public static bool IsSuccess(int rate) => rate > NextInt(100);

    public static byte[] NextBytes(short count)
    {
        var array = new byte[count];
        Random.NextBytes(array);

        return array;
    }
}