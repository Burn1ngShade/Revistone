using System.Runtime.Intrinsics.Arm;

namespace Revistone.Console.Image;

public class ConsoleColour
{
    public byte R { get; private set; }
    public byte G { get; private set; }
    public byte B { get; private set; }

    byte flags;
    public ConsoleColor EquivalentConsoleColor { get; private set; }

    public string ANSIFlagCode { get; private set; } = "";
    public string ANSIFGCode => $"\u001b[{ANSIFlagCode}38;2;{R};{G};{B}m";
    public string ANSIBGCode => $"\u001b[{ANSIFlagCode}48;2;{R};{G};{B}m";

    public enum TextStyles
    {
        None = 0,
        Bold = 1 << 0,
        Faint = 1 << 1,
        Italic = 1 << 2,
        Underline = 1 << 3,
        StrikeThrough = 1 << 4,
        Overline = 1 << 5,
    }

    static readonly byte[] ANSICodeLookup = [1, 2, 3, 4, 9, 53]; // bold, faint, italic, underline, strike-through, overline
    static readonly Dictionary<ConsoleColor, (byte r, byte g, byte b)> ConsoleColourMap = new()
    {
        { ConsoleColor.Black, (40, 44, 52) },
        { ConsoleColor.DarkBlue, (86, 166, 239) },
        { ConsoleColor.DarkGreen, (142, 194, 102) },
        { ConsoleColor.DarkCyan, (78, 180, 194) },
        { ConsoleColor.DarkRed, (220, 81, 98) },
        { ConsoleColor.DarkMagenta, (191, 97, 221) },
        { ConsoleColor.DarkYellow, (206, 142, 83) },
        { ConsoleColor.DarkGray, (80, 86, 102) },
        { ConsoleColor.Gray, (215, 218, 224) },
        { ConsoleColor.Blue, (92, 197, 254) },
        { ConsoleColor.Green, (167, 224, 118) },
        { ConsoleColor.Cyan, (91, 210, 224) },
        { ConsoleColor.Red, (255, 93, 113) },
        { ConsoleColor.Magenta, (220, 113, 254) },
        { ConsoleColor.Yellow, (236, 163, 95) },
        { ConsoleColor.White, (230, 230, 230) },

    };

    // --- CONSTRUCTORS ---

    public ConsoleColour(byte r, byte g, byte b, params TextStyles[] styles)
    {
        R = r;
        G = g;
        B = b;

        SetFlags(styles);
        SetEquivalentConsoleColour();
    }

    public ConsoleColour(ConsoleColor colour, params TextStyles[] styles)
    {
        (byte r, byte g, byte b) = ConsoleColourMap[colour];
        R = r;
        G = g;
        B = b;

        SetFlags(styles);
        SetEquivalentConsoleColour();
    }

    public static ConsoleColour Black => new(40, 44, 52);
    public static ConsoleColour DarkBlue => new(0, 0, 128);
    public static ConsoleColour DarkGreen => new(0, 128, 0);
    public static ConsoleColour DarkCyan => new(0, 128, 128);
    public static ConsoleColour DarkRed => new(128, 0, 0);
    public static ConsoleColour DarkMagenta => new(128, 0, 128);
    public static ConsoleColour DarkYellow => new(128, 128, 0);
    public static ConsoleColour Gray => new(192, 192, 192);
    public static ConsoleColour DarkGray => new(128, 128, 128);
    public static ConsoleColour Blue => new(0, 0, 255);
    public static ConsoleColour Green => new(0, 255, 0);
    public static ConsoleColour Cyan => new(0, 255, 255);
    public static ConsoleColour Red => new(255, 0, 0);
    public static ConsoleColour Magenta => new(255, 0, 255);
    public static ConsoleColour Yellow => new(255, 255, 0);
    public static ConsoleColour White => new(255, 255, 255);

    // --- Functions ---

    ///<summary> Update ANSI flag string using current flag data. </summary>
    private void SetFlags(TextStyles[] styles)
    {
        flags = 0;
        foreach (TextStyles style in styles)
        {
            if (style == TextStyles.None) continue;
            flags |= (byte)style;
        }

        ANSIFlagCode = "";

        for (int i = 0; i < 6; i++)
        {
            if ((flags & 1 << i) != 0) ANSIFlagCode += ANSICodeLookup[i] + ";";
        }
    }

    ///<summary> Converts rgb value to closest ConsoleColour equivelent. </summary>
    private void SetEquivalentConsoleColour()
    {
        (ConsoleColor colour, double distance) = (ConsoleColor.Black, double.MaxValue);

        foreach (var entry in ConsoleColourMap)
        {
            var (r, g, b) = entry.Value;
            double cDistance = Math.Pow(R - r, 2) + Math.Pow(G - g, 2) + Math.Pow(B - b, 2); // dont have to sqrt, as comparision

            if (cDistance < distance)
            {
                distance = cDistance;
                colour = entry.Key;
            }
        }

        EquivalentConsoleColor = colour;
    }
}