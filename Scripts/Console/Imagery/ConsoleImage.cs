using Revistone.Console.Data;
using Revistone.Functions;

namespace Revistone.Console.Image;

/// <summary> Class pertaining all logic for creating images in the console. </summary>
public class ConsoleImage
{
    // --- VARIABLES AND CONSTRUCTORS ---

    (int width, int height) _size;
    public (int width, int height) size { get { return _size; } }

    (char charchter, ConsoleColor colour)[,] _pixels;
    public (char charchter, ConsoleColor colour)[,] pixels { get { return _pixels; } }

    /// <summary> Class pertaining all logic for creating images in the console. </summary>
    public ConsoleImage(int width = 10, int height = 10, char character = ' ', ConsoleColor colour = ConsoleColor.White)
    {
        _size.width = Math.Clamp(width, 1, int.MaxValue);
        _size.height = Math.Clamp(height, 1, int.MaxValue);

        _pixels = new (char charchter, ConsoleColor colour)[width, height];

        SetPixels(character, colour);
    }

    /// <summary> Class pertaining all logic for creating images in the console. </summary>
    public ConsoleImage(string asciiArt)
    {
        //not actually needed but stops null error yapping
        _pixels = new (char charchter, ConsoleColor colour)[_size.width, _size.height];
        SetPixels(asciiArt);
    }

    // --- IMAGE CONTROL ---

    /// <summary> Resizes image to given width and height. </summary>
    public void ResizeImage(int width, int height)
    {
        _size.width = Math.Clamp(width, 1, int.MaxValue);
        _size.height = Math.Clamp(height, 1, int.MaxValue);

        (char character, ConsoleColor colour)[,] newPixels = new (char character, ConsoleColor colour)[width, height];

        for (int x = 0; x < _size.width; x++)
        {
            for (int y = 0; y < _size.width; y++)
            {
                if (x < _pixels.GetLength(0) && y < _pixels.GetLength(1)) newPixels[x, y] = _pixels[x, y];
                else newPixels[x, y] = (' ', ConsoleColor.White);
            }
        }
    }

    // --- PIXEL CONTROL ---

    /// <summary> Set given pixel position, to given charchter and colour. </summary>
    public bool SetPixel(int x, int y, char character, ConsoleColor colour = ConsoleColor.White)
    {
        if (x < 0 || x >= _size.width || y < 0 || y >= _size.height) return false;

        _pixels[x, y] = (character, colour);
        return true;
    }

    /// <summary> Set given pixel positions, to given charchters and colours (array size must = width * height). </summary>
    public bool SetPixels(int startX, int startY, int width, int height, (char character, ConsoleColor colour)[] setPixels)
    {
        if (setPixels.Length != width * height) return false;

        for (int x = startX; x < startX + width; x++)
        {
            for (int y = startY; y < startY + height; y++)
            {
                if (x < 0 || x >= _size.width || y < 0 || y >= _size.height) continue;

                _pixels[x, y] = setPixels[(y - startY) * width + (x - startX)];
            }
        }

        return true;
    }

    /// <summary> Set all pixels to gicen asciiArt </summary>
    public bool SetPixels(string asciiArt)
    {
        string[] lines = asciiArt.Split('\n');

        _size.width = lines.Max(s => s.Length);
        _size.height = lines.Length;

        _pixels = new (char charchter, ConsoleColor colour)[_size.width, _size.height];

        SetPixels(' ', ConsoleColor.White);

        for (int y = 0; y < _size.height; y++)
        {
            int lineIndex = lines.Length - 1 - y;
            for (int x = 0; x < _size.width; x++)
            {
                if (lines[lineIndex].Length > x) _pixels[x, y] = (lines[lineIndex][x], ConsoleColor.White);
            }
        }

        return true;
    }

    /// <summary> Set given pixel positions, to give ConsoleLine. </summary>
    public bool SetPixels(int x, int y, ConsoleLine consoleLine)
    {
        ConsoleLine c = new ConsoleLine(consoleLine.lineText, consoleLine.lineColour.Extend(consoleLine.lineText.Length));
        return SetPixels(x, y, c.lineText.Length, 1, c.lineText.Select((cr, index) => (cr, c.lineColour[index])).ToArray());
    }

    /// <summary> Set given pixel positions, to give ConsoleLines. </summary>
    public bool SetPixels(int x, int y, ConsoleLine[] consoleLines)
    {
        bool successfulLine = false;
        for (int i = 0; i < consoleLines.Length; i++)
        {
            if (SetPixels(x, y + i, consoleLines[i])) successfulLine = true;
        }
        return successfulLine;
    }

    /// <summary> Set given pixel positions, to give string and colour. </summary>
    public bool SetPixels(int x, int y, string text, ConsoleColor colour = ConsoleColor.White) { return SetPixels(x, y, new ConsoleLine(text, colour)); }
    /// <summary> Set given pixel positions, to given string and colours. </summary>
    public bool SetPixels(int x, int y, string text, ConsoleColor[] colours) { return SetPixels(x, y, new ConsoleLine(text, colours)); }

    /// <summary> Set given pixel positions, to given charchter and colour. </summary>
    public bool SetPixels(int startX, int startY, int width, int height, char character, ConsoleColor colour = ConsoleColor.White) { return SetPixels(startX, startY, width, height, Enumerable.Repeat((character, colour), width * height).ToArray()); }
    /// <summary> Sets all pixels to given charchter and colour. </summary>

    public bool SetPixels(char character, ConsoleColor colour) { return SetPixels(0, 0, _size.width, _size.height, character, colour); }

    // --- OUTPUT ---

    /// <summary> Outputs image to console. </summary>
    public bool SendToConsole(bool colourless = false)
    {
        ConsoleLine[] c = ToConsoleLineArray(colourless);
        if (c.Length == 0) return false;
        for (int i = 0; i < c.Length; i++) ConsoleAction.SendConsoleMessage(c[i]);
        return true;
    }

    /// <summary> Outputs image to console at given position. </summary>
    public bool SendToConsole(int x, int y, bool colourless = false)
    {
        ConsoleLine[] c = ToConsoleLineArray(colourless);
        if (c.Length == 0 || y < 1 || y >= ConsoleData.debugStartIndex) return false;

        for (int lineIndex = y; lineIndex < Math.Min(y + c.Length, ConsoleData.debugStartIndex); lineIndex++)
        {
            ConsoleAction.UpdatePrimaryConsoleLine(ConsoleLine.Overwrite(ConsoleAction.GetConsoleLine(lineIndex), c[lineIndex - y], x), lineIndex);
        }

        return true;
    }

    /// <summary> Returns a row of the image as a ConsoleLine. </summary>
    public ConsoleLine RowToConsoleLine(int rowIndex, bool colourless = false)
    {
        if (rowIndex < 0 || rowIndex >= _size.height) return new ConsoleLine();

        string s = "";
        ConsoleColor[] c = new ConsoleColor[_size.width];

        for (int x = 0; x < _size.width; x++)
        {
            s += _pixels[x, rowIndex].charchter;
            c[x] = colourless ? ConsoleColor.White : _pixels[x, rowIndex].colour;
        }

        return new ConsoleLine(s, c);
    }

    /// <summary> Returns a column of the image as a ConsoleLine. </summary>
    public ConsoleLine ColumnToConsoleLine(int columIndex, bool colourless = false)
    {
        if (columIndex < 0 || columIndex >= _size.width) return new ConsoleLine();

        string s = "";
        ConsoleColor[] c = new ConsoleColor[_size.height];

        for (int y = 0; y < _size.height; y++)
        {
            s += _pixels[columIndex, y].charchter;
            c[y] = colourless ? ConsoleColor.White : _pixels[columIndex, y].colour;
        }

        return new ConsoleLine(s, c);
    }

    /// <summary> Converts image to ConsoleLines array. </summary>
    public ConsoleLine[] ToConsoleLineArray(bool colourless = false)
    {
        ConsoleLine[] lines = new ConsoleLine[_size.height];
        for (int y = _size.height - 1; y >= 0; y--)
        {
            lines[_size.height - 1 - y] = RowToConsoleLine(y);
        }
        return lines;
    }
}