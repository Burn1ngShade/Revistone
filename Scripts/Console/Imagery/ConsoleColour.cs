using Revistone.Management;
using Revistone.Modules;

namespace Revistone.Console.Image;

public class ConsoleColour
{
    public Byte3 RGB { get; private set; }
    public byte R => RGB.x;
    public byte G => RGB.y;
    public byte B => RGB.z;

    public byte TextStyleFlags { get; private set; } // bit flags for text styles
    public ConsoleColor EquivalentConsoleColor { get; private set; }

    public string ANSIFlagCode { get; private set; } = "";
    public string ANSIFGCode => $"\u001b[{ANSIFlagCode}38;2;{R};{G};{B}m";
    public string ANSIBGCode => $"\u001b[48;2;{R};{G};{B}m";

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
    static readonly Dictionary<ConsoleColor, Byte3> ConsoleColourMap = new()
    {
        { ConsoleColor.Black, new(40, 44, 52) },
        { ConsoleColor.DarkBlue, new(86, 166, 239) },
        { ConsoleColor.DarkGreen, new(142, 194, 102) },
        { ConsoleColor.DarkCyan, new(78, 180, 194) },
        { ConsoleColor.DarkRed, new(220, 81, 98) },
        { ConsoleColor.DarkMagenta, new(191, 97, 221) },
        { ConsoleColor.DarkYellow, new(206, 142, 83) },
        { ConsoleColor.DarkGray, new(80, 86, 102) },
        { ConsoleColor.Gray, new(215, 218, 224) },
        { ConsoleColor.Blue, new(92, 197, 254) },
        { ConsoleColor.Green, new(167, 224, 118) },
        { ConsoleColor.Cyan, new(91, 210, 224) },
        { ConsoleColor.Red, new(255, 93, 113) },
        { ConsoleColor.Magenta, new(220, 113, 254) },
        { ConsoleColor.Yellow, new(236, 163, 95) },
        { ConsoleColor.White, new(230, 230, 230) },

    };

    // --- CONSTRUCTORS ---

    public ConsoleColour(Byte3 rgb, params TextStyles[] styles)
    {
        RGB = new Byte3(rgb);

        SetFlags(styles);
        SetEquivalentConsoleColour();
    }

    public ConsoleColour(Byte3 rgb, byte textStyleFlags)
    {
        RGB = new Byte3(rgb);

        SetFlags(textStyleFlags);
        SetEquivalentConsoleColour();
    }

    public ConsoleColour(byte r, byte g, byte b, params TextStyles[] styles)
    {
        RGB = new Byte3(r, g, b);

        SetFlags(styles);
        SetEquivalentConsoleColour();
    }

    public ConsoleColour(byte r, byte g, byte b, byte textStyleFlags)
    {
        RGB = new Byte3(r, g, b);

        SetFlags(textStyleFlags);
        SetEquivalentConsoleColour();
    }

    public ConsoleColour(ConsoleColor colour, params TextStyles[] styles)
    {
        RGB = ConsoleColourMap[colour];

        SetFlags(styles);
        SetEquivalentConsoleColour();
    }

    public static ConsoleColour Black => new(40, 44, 52);
    public static ConsoleColour DarkBlue => new(86, 166, 239);
    public static ConsoleColour DarkGreen => new(142, 194, 102);
    public static ConsoleColour DarkCyan => new(78, 180, 194);
    public static ConsoleColour DarkRed => new(220, 81, 98);
    public static ConsoleColour DarkMagenta => new(191, 97, 221);
    public static ConsoleColour DarkYellow => new(206, 142, 83);
    public static ConsoleColour DarkGray => new(80, 86, 102);
    public static ConsoleColour Gray => new(215, 218, 224);
    public static ConsoleColour Blue => new(92, 197, 254);
    public static ConsoleColour Green => new(167, 224, 118);
    public static ConsoleColour Cyan => new(91, 210, 224);
    public static ConsoleColour Red => new(255, 93, 113);
    public static ConsoleColour Magenta => new(220, 113, 254);
    public static ConsoleColour Yellow => new(236, 163, 95);
    public static ConsoleColour White => new(230, 230, 230);
    public static ConsoleColour ConsoleBackground => new(40, 44, 52);

    // --- Functions ---

    ///<summary> Update ANSI flag string using current flag data. </summary>
    private void SetFlags(byte textStyleFlags)
    {
        TextStyleFlags = textStyleFlags;
        ANSIFlagCode = "";

        for (int i = 0; i < 6; i++)
        {
            if ((TextStyleFlags & 1 << i) != 0) ANSIFlagCode += ANSICodeLookup[i] + ";";
        }
    }

    private void SetFlags(TextStyles[] styles)
    {
        byte tempTextStyleFlags = 0;
        foreach (TextStyles style in styles)
        {
            if (style == TextStyles.None) continue;
            tempTextStyleFlags |= (byte)style;
        }

        SetFlags(tempTextStyleFlags);
    }

    ///<summary> Converts rgb value to closest ConsoleColour equivelent. </summary>
    private void SetEquivalentConsoleColour()
    {
        (ConsoleColor colour, double distance) = (ConsoleColor.Black, double.MaxValue);

        foreach (var entry in ConsoleColourMap)
        {
            if (RGB == entry.Value)
            {
                colour = entry.Key;
                EquivalentConsoleColor = colour;
                return; // Exact match found
            }
            Byte3 b = entry.Value;

            double cDistance = Math.Pow(RGB.x - b.x, 2) +
                Math.Pow(RGB.y - b.y, 2) +
                Math.Pow(RGB.z - b.z, 2);


            if (cDistance < distance)
            {
                distance = cDistance;
                colour = entry.Key;
            }
        }

        EquivalentConsoleColor = colour;
    }
}