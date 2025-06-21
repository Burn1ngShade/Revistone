namespace Revistone.Modules;

public struct Byte3
{
    public byte x;
    public byte y;
    public byte z;

    public Byte3(byte x, byte y, byte z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Byte3(Byte3 other)
    {
        x = other.x;
        y = other.y;
        z = other.z;
    }

    public static Byte3 Min => new(0, 0, 0);
    public static Byte3 Max => new(255, 255, 255);

    ///<summary> Magnitude of a byte3 without square rooting, use for comparison. </summary>
    public readonly double MagnitudeSquared()
    {
        return x * x + y * y + z * z;
    }

    ///<summary> Magnitude of a byte3. </summary>
    public readonly double Magnitude()
    {
        return Math.Sqrt(x * x + y * y + z * z);
    }

    // --- Static Methods ---

    public static Byte3 Lerp(Byte3 a, Byte3 b, double t)
    {
        byte r = (byte)Math.Round(a.x + (b.x - a.x) * t);
        byte g = (byte)Math.Round(a.y + (b.y - a.y) * t);
        byte bVal = (byte)Math.Round(a.z + (b.z - a.z) * t);
        return new Byte3(r, g, bVal);
    }

    /// --- Operators ---

    public static Byte3 operator +(Byte3 a, Byte3 b)
    {
        return new Byte3((byte)(a.x + b.x), (byte)(a.y + b.y), (byte)(a.z + b.z));
    }

    public static Byte3 operator -(Byte3 a, Byte3 b)
    {
        return new Byte3((byte)(a.x - b.x), (byte)(a.y - b.y), (byte)(a.z - b.z));
    }

    public static bool operator ==(Byte3 left, Byte3 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Byte3 left, Byte3 right)
    {
        return !(left == right);
    }

    // --- Overrides ---

    public override readonly string ToString()
    {
        return $"({x}, {y}, {z})";
    }

    public override readonly bool Equals(object? obj)
    {
        if (obj is not Byte3 byte3) return false;
        return x == byte3.x && y == byte3.y && z == byte3.z;
    }

    public override readonly int GetHashCode() =>
        HashCode.Combine(x, y, z);
}