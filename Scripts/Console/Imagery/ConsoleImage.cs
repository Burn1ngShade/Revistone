using Revistone.Console.Data;
using Revistone.Functions;
using Revistone.Management;

using static Revistone.Functions.PersistentDataFunctions;

namespace Revistone.Console.Image;

/// <summary> Class pertaining all logic for creating images in the console, (0, 0) refers to the bot left of an image. </summary>
public class ConsoleImage
{
    // --- VARIABLES AND CONSTRUCTORS ---

    (int width, int height) size = (0, 0);
    public int Width => size.width;
    public int Height => size.height;

    ConsolePixel[,] pixels = new ConsolePixel[0, 0];
    ConsolePixel defaultPixel = new ConsolePixel();

    /// <summary> Class pertaining all logic for creating images in the console. </summary>
    public ConsoleImage(int width, int height, ConsolePixel defaultPixel)
    {
        SetDefaultPixel(defaultPixel);
        SetImageSize(width, height);
    }

    /// <summary> Class pertaining all logic for creating images in the console. </summary>
    public ConsoleImage(int width = 10, int height = 10) : this(width, height, new ConsolePixel()) { }

    // --- IMAGE CONTROL ---

    ///<summary> Sets the pixel type that is used by default. </summary>
    public void SetDefaultPixel(ConsolePixel pixel)
    {
        defaultPixel = pixel;
    }

    /// <summary> Updates dimensions of the image. </summary>
    public void SetImageSize(int newWidth, int newHeight, bool perserveImage = true)
    {
        newWidth = Math.Max(1, newWidth);
        newHeight = Math.Max(1, newHeight);

        ConsolePixel[,] newPixels = new ConsolePixel[newWidth, newHeight];

        for (int x = 0; x < newWidth; x++)
        {
            for (int y = 0; y < newHeight; y++)
            {
                if (perserveImage && size.width > x && size.height > y) newPixels[x, y] = pixels[x, y];
                else newPixels[x, y] = new ConsolePixel(defaultPixel);
            }
        }

        size = (newWidth, newHeight);
        pixels = newPixels;
    }

    // --- SET PIXELS ---

    /// <summary> Set pixel at given position. </summary>
    public bool SetPixel(int x, int y, ConsolePixel newPixel)
    {
        if (!PixelExists(x, y)) return false;
        pixels[x, y].Set(newPixel);
        return true;
    }

    ///<summary> Set pixel charchter at given position. </summary>
    public bool SetPixelChar(int x, int y, char c)
    {
        if (!PixelExists(x, y)) return false;
        pixels[x, y].SetChar(c);
        return true;
    }

    ///<summary> Set pixel foreground at given position. </summary>
    public bool SetPixelForeground(int x, int y, ConsoleColor colour)
    {
        if (!PixelExists(x, y)) return false;
        pixels[x, y].SetForeground(colour);
        return true;
    }

    ///<summary> Set pixel background at given position. </summary>
    public bool SetPixelBackground(int x, int y, ConsoleColor colour)
    {
        if (!PixelExists(x, y)) return false;
        pixels[x, y].SetBackground(colour);
        return true;
    }

    /// <summary> Set given block of pixels to given blockPixel array, (array size must = width * height). </summary>
    public bool SetPixelBlock(int startX, int startY, int blockWidth, int blockHeight, ConsolePixel[] blockPixels)
    {
        if (blockPixels.Length != blockWidth * blockHeight) return false;

        for (int x = startX; x < startX + blockWidth; x++)
        {
            for (int y = startY; y < startY + blockHeight; y++)
            {
                if (!PixelExists(x, y)) continue;
                pixels[x, y].Set(blockPixels[(y - startY) * blockWidth + (x - startX)]);
            }
        }

        return true;
    }

    /// <summary> Set given block of pixels to given blockPixel array, (array size must = width * height). </summary>
    public bool SetPixelBlock(int startX, int startY, int blockWidth, int blockHeight, ConsolePixel blockPixels)
    {
        return SetPixelBlock(startX, startY, blockWidth, blockHeight, [.. Enumerable.Repeat(blockPixels, blockWidth * blockHeight)]);
    }

    /// <summary> Set given pixel positions, from a ConsleLine. </summary>
    public bool SetPixels(int x, int y, ConsoleLine consoleLine)
    {
        ConsoleLine c = new(consoleLine.lineText, consoleLine.lineColour.ExtendEnd(consoleLine.lineText.Length), consoleLine.lineBGColour.ExtendEnd(consoleLine.lineText.Length));
        return SetPixelBlock(x, y, c.lineText.Length, 1, [.. c.lineText.Select((cr, index) => new ConsolePixel(char.IsSurrogate(cr) ? ' ' : cr, c.lineColour[index], c.lineBGColour[index]))]);
    }

    /// <summary> Set given pixel positions, from A ConsoleLine array. </summary>
    public bool SetPixels(int x, int y, ConsoleLine[] consoleLines)
    {
        bool setPixels = false;
        for (int i = 0; i < consoleLines.Length; i++)
        {
            if (SetPixels(x, y + i, consoleLines[i])) setPixels = true;
        }
        return setPixels;
    }

    /// <summary> Sets all pixels to given charchter and colour. </summary>
    public bool SetAllPixels(ConsolePixel pixel) { return SetPixelBlock(0, 0, size.width, size.height, [.. Enumerable.Repeat(pixel, size.width * size.height)]); }

    // --- GET PIXELS ---

    ///<summary> Attempts to get pixel at given position, returning default pixel otherwise. </summary>
    public ConsolePixel GetPixel(int x, int y)
    {
        if (!PixelExists(x, y)) return new ConsolePixel(defaultPixel);
        return new ConsolePixel(pixels[x, y]);
    }

    ///<summary> Attempts to get pixel row, returning [] otherwise. </summary>
    public ConsolePixel[] GetPixelRow(int y)
    {
        if (y < 0 || y >= size.height) return [];
        return [.. Enumerable.Range(0, size.width).Select(x => new ConsolePixel(pixels[x, y]))];
    }

    ///<summary> Attempts to get pixel column, returning [] otherwise. </summary>
    public ConsolePixel[] GetPixelColumn(int x)
    {
        if (x < 0 || x >= size.width) return [];
        return [.. Enumerable.Range(0, size.height).Select(y => new ConsolePixel(pixels[x, y]))];
    }

    // --- GENERAL FUNCTIONS ---

    ///<summary> Verifys if given (x, y) coordinate is a pixel within the image. </summary>
    public bool PixelExists(int x, int y)
    {
        if (x < 0 || y < 0 | x >= size.width || y >= size.height) return false;
        return true;
    }

    public static ConsoleLine PixelArrayToConsoleLine(ConsolePixel[] pixels)
    {
        return new ConsoleLine(
            new string([.. pixels.Select(p => p.Character)]),
            [.. pixels.Select(p => p.FGColour)],
            [.. pixels.Select(p => p.BGColour)]);
    }

    /// <summary> Converts image to ConsoleLines array. </summary>
    public ConsoleLine[] ToConsoleLineArray()
    {
        ConsoleLine[] lines = new ConsoleLine[size.height];
        for (int y = size.height - 1; y >= 0; y--)
        {
            lines[size.height - 1 - y] = PixelArrayToConsoleLine(GetPixelRow(y));
        }
        return lines;
    }

    // --- OUTPUT ---

    /// <summary> Outputs image to console. </summary>
    public void Output(bool debugConsole = false)
    {
        if (debugConsole) ConsoleAction.SendDebugMessages(ToConsoleLineArray());
        else ConsoleAction.SendConsoleMessages(ToConsoleLineArray());
    }

    /// <summary> Outputs image to primary console at given position. </summary>
    public bool OutputAt(int x, int y)
    {
        ConsoleLine[] c = ToConsoleLineArray();

        if (c.Length == 0 || y < 1 || y >= ConsoleData.debugStartIndex) return false;

        for (int lineIndex = y; lineIndex < Math.Min(y + c.Length, ConsoleData.debugStartIndex); lineIndex++)
        {
            ConsoleAction.UpdatePrimaryConsoleLine(ConsoleLine.Overwrite(ConsoleAction.GetConsoleLine(lineIndex), c[lineIndex - y], x), lineIndex);
        }

        return true;
    }

    // --- BINARY Format ---

    public static bool SaveToCIMG(string filePath, ConsoleImage image)
    {
        if (!IsPathValid(filePath)) return false;

        CreateFile(filePath);

        using (FileStream fs = new(filePath, FileMode.Create))
        using (BinaryWriter writer = new(fs))
        {
            writer.Write("CIMG"); // file type
            writer.Write(image.Width);
            writer.Write(image.Height);
            writer.Write(image.defaultPixel.Character);
            writer.Write((byte)image.defaultPixel.FGColour);
            writer.Write((byte)image.defaultPixel.BGColour);

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    writer.Write(image.pixels[x, y].Character);
                    writer.Write((byte)image.pixels[x, y].FGColour);
                    writer.Write((byte)image.pixels[x, y].BGColour);
                }
            }
        }

        return true;
    }

    public static ConsoleImage? LoadFromCIMG(string filePath)
    {
        if (!IsPathValid(filePath)) return new ConsoleImage();

        using FileStream fs = new(filePath, FileMode.Open);
        using BinaryReader reader = new(fs);
        
        if (fs.Length <= 8) return new ConsoleImage(); // will crash trying to read .cimg

        if (reader.ReadString() != "CIMG")
        {
            DeveloperTools.Log($"Error: File At Path {filePath} Is Not A CIMG file.");
            return null;
        }

        int width = reader.ReadInt32();
        int height = reader.ReadInt32();
        ConsolePixel defaultPixel = new(reader.ReadChar(), (ConsoleColor)reader.ReadByte(), (ConsoleColor)reader.ReadByte());

        ConsoleImage image = new(width, height, defaultPixel);

        for (int x = 0; x < image.Width; x++)
        {
            for (int y = 0; y < image.Height; y++)
            {
                image.pixels[x, y] = new ConsolePixel(reader.ReadChar(), (ConsoleColor)reader.ReadByte(), (ConsoleColor)reader.ReadByte());
            }
        }

        return image;
    }
}