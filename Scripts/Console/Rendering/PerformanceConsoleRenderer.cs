using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Revistone.Functions;
using static Revistone.Console.Data.ConsoleData;
using static Revistone.Functions.ColourFunctions;

namespace Revistone.Console.Rendering;

//Implementation based off https://github.com/crowfingers/FastConsole/blob/master/FastConsole.cs (tyyyyyyyyy :))
/// <summary> Class pertaining custom logic to write to screen buffer. </summary>
public static class PerformanceConsoleRenderer
{
    /// <summary> Struct representing left and top position of a char in console. </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct Coord
    {
        public short x, y;
        public Coord(short x, short y)
        {
            this.x = x; this.y = y;
        }
    };

    /// <summary> Struct representing the unicode value, and colour data and grid set of a char in console. </summary>
    [StructLayout(LayoutKind.Explicit)]
    struct CharInfo
    {
        [FieldOffset(0)] public ushort Char;
        [FieldOffset(2)] public short Attributes;
    }

    /// <summary> Struct specifying region of console to be written to. </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct Rectangle
    {
        public short left, top, right, bottom;
        public Rectangle(short left, short top, short right, short bottom)
        {
            this.left = left; this.top = top; this.right = right; this.bottom = bottom;
        }
    }

    static SafeFileHandle outputHandle = new();

    // Output buffer
    static CharInfo[] buffer = Array.Empty<CharInfo>();
    static short width, height, left, top;
    static short right => (short)(left + width - 1);
    static short bottom => (short)(top + height - 1);

    /// <summary> [DO NOT CALL] Initializes renderer. </summary>
    internal static void InitializeRenderer(ConsoleColor foreground = ConsoleColor.Gray, ConsoleColor background = ConsoleColor.Black)
    {
        System.Console.OutputEncoding = System.Text.Encoding.Unicode;
        System.Console.Title = "Revistone";
        System.Console.CursorVisible = false;

        GetOutputHandle();

        width = (short)System.Console.WindowWidth;
        height = (short)System.Console.WindowHeight;
        left = 0;
        top = 0;

        buffer = new CharInfo[width * height];

        ForegroundColor = foreground;
        BackgroundColor = background;
        Clear(ForegroundColor, BackgroundColor);
    }

    /// <summary> Reloads renderer (Called on screen resize). </summary>
    public static void Reload()
    {
        width = (short)System.Console.WindowWidth;
        height = (short)System.Console.WindowHeight;
        left = 0;
        top = 0;

        buffer = new CharInfo[width * height];

        ForegroundColor = ConsoleColor.White;
        BackgroundColor = ConsoleColor.Black;
        Clear(ForegroundColor, BackgroundColor);
    }

    /// <summary> Attempts to obtain a file handle for the console output. </summary>
    static void GetOutputHandle()
    {
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern SafeFileHandle CreateFile(
            string fileName,
            [MarshalAs(UnmanagedType.U4)] uint fileAccess,
            [MarshalAs(UnmanagedType.U4)] uint fileShare,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] int flags,
            IntPtr template);

        outputHandle = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
        if (outputHandle.IsInvalid) throw new Exception("outputHandle is invalid!");
    }

    /// <summary> Represents a foreground and background as one short value. </summary>
    static short Colorset(ConsoleColor foreground, ConsoleColor background)
        => (short)(foreground + ((short)background << 4));

    //hex values of overline, leftline and rightline bits
    const short overlineBit = 0x0400;
    const short leftlineBit = 0x0800;
    const short rightlineBit = 0x1000;

    /// <summary> Represents the gridset of a char. </summary>
    static short Gridset(bool overline, bool leftline, bool rightline)
    {
        return (short)(
            (overline ? overlineBit : 0) + (leftline ? leftlineBit : 0) + (rightline ? rightlineBit : 0)
        );
    }

    // --- PUBLIC METHODS ---


    static ConsoleColor _ForegroundColor, _BackgroundColor;
    /// <summary> Colour of char. </summary>
    public static ConsoleColor ForegroundColor
    {
        get => _ForegroundColor;
        set
        {
            _ForegroundColor = value;
            System.Console.ForegroundColor = value;
        }
    }
    /// <summary> Colour of char background. </summary>
    public static ConsoleColor BackgroundColor
    {
        get => _BackgroundColor;
        set
        {
            _BackgroundColor = value;
            System.Console.BackgroundColor = value;
        }
    }

    /// <summary> Set char at given point in console. </summary>
    public static void SetChar((int x, int y) coords, char c, ConsoleColor foreground, ConsoleColor background, bool overline = false, bool leftline = false, bool rightline = false, bool underline = false)
    {
        SetChar((short)coords.x, (short)coords.y, c, foreground, background, overline, leftline, rightline, underline);
    }

    /// <summary> Set char at given point in console. </summary>
    public static void SetChar(int x, int y, char c, ConsoleColor foreground, ConsoleColor background, bool overline = false, bool leftline = false, bool rightline = false, bool underline = false)
    {
        SetChar((short)x, (short)y, c, foreground, background, overline, leftline, rightline, underline);
    }

    /// <summary> Set char at given point in console. </summary>
    public static void SetChar(short x, short y, char c, ConsoleColor foreground, ConsoleColor background, bool overline = false, bool leftline = false, bool rightline = false, bool underline = false)
    {
        short address = (short)(width * y + x);
        short colorset = Colorset(foreground, background);
        short gridset = Gridset(overline, leftline, rightline);

        if (address < 0 || address >= buffer.Length) return; //throw new Exception("Can't write to address (" + x + "," + y + ").");
        buffer[address].Char = c;
        buffer[address].Attributes = (short)(colorset + gridset);

        if (underline)
        {
            if (y == height - 1) return;
            address = (short)(width * (y + 1) + x);
            buffer[address].Attributes |= overlineBit;
        }
    }

    /// <summary> Fills buffer with given char, and colour. </summary>
    public static void FillBuffer(char c, ConsoleColor foreground, ConsoleColor background)
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i].Attributes = Colorset(foreground, background);
            buffer[i].Char = c;
        }
    }
    /// <summary> Replacement for System.Console.Clear(). </summary>
    public static void Clear(ConsoleColor foreground = ConsoleColor.Gray, ConsoleColor background = ConsoleColor.Black)
    {
        FillBuffer(' ', foreground, background);
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool WriteConsoleOutputW(
          SafeFileHandle hConsoleOutput,
          CharInfo[] lpBuffer,
          Coord dwBufferSize,
          Coord dwBufferCoord,
          ref Rectangle lpWriteRegion);

    /// <summary> Draws buffer to console. </summary>
    public static void DrawBuffer()
    {
        Rectangle rect = new(left, top, right, bottom);
        WriteConsoleOutputW(outputHandle, buffer, new Coord(width, height), new Coord(0, 0), ref rect);
    }

    /// <summary> Gets char at given point in the buffer. </summary>
    public static (char character, ConsoleColor foreground, ConsoleColor background) ReadChar((int x, int y) coords)
    {
        short address = (short)(width * coords.y + coords.x);
        char character = (char)buffer[address].Char;
        short attributes = buffer[address].Attributes;
        attributes &= 0x0FF;
        ConsoleColor foreground = (ConsoleColor)(attributes & 0x0F);
        ConsoleColor background = (ConsoleColor)(attributes >> 4);
        return (character, foreground, background);
    }

    ///<summary> Render Console To Screen. </summary>
    public static void RenderConsole()
    {
        for (int i = 0; i < consoleLines.Length; i++)
        {
            if (consoleLines[i] == null || consoleLines[i].Updated) continue;

            // mabye move this out??? i dont know if itll break something
            if (System.Console.WindowHeight != windowSize.height || System.Console.WindowWidth != windowSize.width)
            {
                return;
            }

            WriteConsoleLine(i);
            consoleLinesBuffer[i].Update(consoleLines[i]);
        }
    }

    /// <summary> Writes given line to screen, using value of consoleLines. </summary>
    static void WriteConsoleLine(int lineIndex)
    {
        consoleLines[lineIndex].MarkAsUpToDate();
        consoleLines[lineIndex].Normalise();
        
        if (consoleLinesBuffer[lineIndex].LineText.Length > consoleLines[lineIndex].LineText.Length) //clears line between end of currentline and buffer line
        {
            for (int i = consoleLines[lineIndex].LineText.Length; i < windowSize.width; i++)
            {
                SetChar(i, lineIndex, ' ', ConsoleColor.White, ConsoleColor.Black);
            }
        }

        for (int i = 0; i < Math.Min(consoleLines[lineIndex].LineText.Length, windowSize.width); i++)
        {
            SetChar(i, lineIndex, consoleLines[lineIndex].LineText[i], consoleLines[lineIndex].LineColour[i].EquivalentConsoleColor, consoleLines[lineIndex].LineBGColour[i].EquivalentConsoleColor);
        }
    }
}
