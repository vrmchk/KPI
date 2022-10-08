namespace Lab1;

public static class ByteConverter
{
    public static long MegabytesToBytes(int megabytes) => (long) Math.Pow(2, 20) * megabytes;
}