using Revistone.App;
using Revistone.Console.Data;
using Revistone.Functions;
using Revistone.Management;

namespace Revistone.Console.Image;

/// <summary> Class pertaining all logic for creating images in the console, (0, 0) refers to the bot left of an image. </summary>
public class ConsoleImage
{
    // --- VARIABLES AND CONSTRUCTORS ---

    (int width, int height) size = (0, 0);

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

    public ConsoleImage(SerializableConsoleImage serializableConsoleImage)
    {
        SetDefaultPixel(serializableConsoleImage.DefaultPixel);
        SetImageSize(serializableConsoleImage.Width, serializableConsoleImage.Height);

        Analytics.Debug.Add($"Width: {size.width}, Height: {size.height}");

        for (int i = 0; i < serializableConsoleImage.Pixels.Length; i++)
        {
            pixels[i % size.width, i / size.width] = serializableConsoleImage.Pixels[i];
        }
    }

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
        ConsoleLine c = new(consoleLine.lineText, consoleLine.lineColour.Extend(consoleLine.lineText.Length), consoleLine.lineBGColour.Extend(consoleLine.lineText.Length));
        return SetPixelBlock(x, y, c.lineText.Length, 1, [.. c.lineText.Select((cr, index) => new ConsolePixel(cr, c.lineColour[index], c.lineBGColour[index]))]);
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

    // --- JSON ---

    ///<summary> Gets a ConsoleImage from a JSON file. </summary>
    public static ConsoleImage GetFromJSON(string filePath)
    {
        return new ConsoleImage(PersistentDataFunctions.LoadFileFromJSON<SerializableConsoleImage>(filePath) ?? new SerializableConsoleImage());
    }

    ///<summary> Saves a ConsoleImage to a JSON file. </summary>
    public static bool SaveToJson(string filePath, ConsoleImage consoleImage)
    {
        return PersistentDataFunctions.SaveFileAsJSON(filePath, new SerializableConsoleImage(consoleImage));
    }

    ///<summary> Serializable version of console image for saving and loading to JSON. </summary>
    public class SerializableConsoleImage
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public ConsolePixel[] Pixels { get; set; } = [];
        public ConsolePixel DefaultPixel { get; set; } = new ConsolePixel();

        public SerializableConsoleImage()
        {
            
        }

        public SerializableConsoleImage(ConsoleImage image)
        {
            Width = image.size.width;
            Height = image.size.height;
            DefaultPixel = image.defaultPixel;

            Pixels = new ConsolePixel[Width * Height];

            for (int i = 0; i < Pixels.Length; i++)
            {
                Pixels[i] = image.pixels[i % Width, i / Width];
            }
        }
    }
}