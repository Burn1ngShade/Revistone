using Revistone.Console.Data;
using Revistone.Functions;

namespace Revistone
{
    namespace Console
    {
        namespace Image
        {
            /// <summary> Class pertaining all logic for creating images in the console. </summary>
            public class ConsoleImage
            {
                // --- VARIABLES AND CONSTRUCTORS ---

                (int width, int height) _size;
                public (int width, int height) size { get { return _size; } }

                (char charchter, ConsoleColor colour)[,] _pixels;
                public (char charchter, ConsoleColor colour)[,] pixels { get { return _pixels; } }

                ConsoleColor[,] _bgPixels;
                public ConsoleColor[,] bgPixels { get { return _bgPixels; } }

                /// <summary> Class pertaining all logic for creating images in the console. </summary>
                public ConsoleImage(int width = 10, int height = 10, char character = ' ', ConsoleColor colour = ConsoleColor.White, ConsoleColor bgColour = ConsoleColor.Black)
                {
                    _size.width = Math.Clamp(width, 1, int.MaxValue);
                    _size.height = Math.Clamp(height, 1, int.MaxValue);

                    _pixels = new (char charchter, ConsoleColor colour)[width, height];
                    _bgPixels = new ConsoleColor[width, height];

                    SetPixels(character, colour);
                    SetBGPixels(bgColour);
                }

                /// <summary> Class pertaining all logic for creating images in the console. </summary>
                public ConsoleImage(ConsoleImage image)
                {
                    _size = image.size;
                    _pixels = image.pixels;
                    _bgPixels = image._bgPixels;
                }

                /// <summary> Class pertaining all logic for creating images in the console. </summary>
                public ConsoleImage(string asciiArt, ConsoleColor colour = ConsoleColor.White, ConsoleColor bgColour = ConsoleColor.Black)
                {
                    //not actually needed but stops null error yapping
                    _pixels = new (char charchter, ConsoleColor colour)[_size.width, _size.height];
                    _bgPixels = new ConsoleColor[_size.width, _size.height];
                    SetPixels(asciiArt, colour);
                    SetBGPixels(bgColour);
                }

                // --- IMAGE CONTROL ---

                /// <summary> Resizes image to given width and height. </summary>
                public void Resize(int width, int height)
                {
                    _size.width = Math.Clamp(width, 1, int.MaxValue);
                    _size.height = Math.Clamp(height, 1, int.MaxValue);

                    (char character, ConsoleColor colour)[,] newPixels = new (char character, ConsoleColor colour)[width, height];
                    ConsoleColor[,] newBGPixels = new ConsoleColor[width, height];

                    for (int x = 0; x < _size.width; x++)
                    {
                        for (int y = 0; y < _size.width; y++)
                        {
                            if (x < _pixels.GetLength(0) && y < _pixels.GetLength(1)) newPixels[x, y] = _pixels[x, y];
                            else newPixels[x, y] = (' ', ConsoleColor.White);

                            if (x < _bgPixels.GetLength(0) && y < _bgPixels.GetLength(1)) newBGPixels[x, y] = _bgPixels[x, y];
                            else newBGPixels[x, y] = ConsoleColor.Black;
                        }
                    }
                }

                /// <summary> Stretch image in x axis by given scale factor. </summary>
                public void StretchX(double scaleFactor)
                {
                    if (scaleFactor < 1) return;

                    int newWidth = (int)(_size.width * scaleFactor);

                    (char character, ConsoleColor colour)[,] newPixels = new (char character, ConsoleColor colour)[newWidth, _size.height];
                    ConsoleColor[,] newBGPixels = new ConsoleColor[newWidth, _size.height];

                    for (int x = 0; x < newWidth; x++)
                    {
                        for (int y = 0; y < _size.height; y++)
                        {
                            newPixels[x, y] = pixels[(int)(x / scaleFactor), y];
                            newBGPixels[x, y] = bgPixels[(int)(x / scaleFactor), y];
                        }
                    }

                    _size.width = newWidth;
                    _pixels = newPixels;
                    _bgPixels = newBGPixels;
                }

                /// <summary> Stretch image in x axis by given scale factor. </summary>
                public void StretchY(double scaleFactor)
                {
                    if (scaleFactor < 1) return;

                    int newHeight = (int)(_size.height * scaleFactor);

                    (char character, ConsoleColor colour)[,] newPixels = new (char character, ConsoleColor colour)[_size.width, newHeight];
                    ConsoleColor[,] newBGPixels = new ConsoleColor[_size.width, newHeight];

                    for (int x = 0; x < _size.width; x++)
                    {
                        for (int y = 0; y < newHeight; y++)
                        {
                            newPixels[x, y] = pixels[(int)(x / scaleFactor), y];
                            newBGPixels[x, y] = bgPixels[(int)(x / scaleFactor), y];
                        }
                    }

                    _size.height = newHeight;
                    _pixels = newPixels;
                    _bgPixels = newBGPixels;
                }


                public void OverlayImage(int x, int y, ConsoleImage image)
                {
                    SetPixels(x, y, image.size.width, image.size.height, image.pixels.Cast<(char, ConsoleColor)>().ToArray());
                    SetBGPixels(x, y, image.size.width, image.size.height, image.bgPixels.Cast<ConsoleColor>().ToArray());
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

                /// <summary> Set all pixels to given asciiArt </summary>
                public bool SetPixels(string asciiArt, ConsoleColor colour = ConsoleColor.White)
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
                            if (lines[lineIndex].Length > x) _pixels[x, y] = (lines[lineIndex][x], colour);
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

                // --- BG PIXEL CONTROL ---

                /// <summary> Set given BG pixel position, to given charchter and colour. </summary>
                public bool SetBGPixel(int x, int y, ConsoleColor colour = ConsoleColor.White)
                {
                    if (x < 0 || x >= _size.width || y < 0 || y >= _size.height) return false;

                    _bgPixels[x, y] = colour;
                    return true;
                }

                /// <summary> Set given BG pixel positions, to given charchters and colours (array size must = width * height). </summary>
                public bool SetBGPixels(int startX, int startY, int width, int height, ConsoleColor[] setPixels)
                {
                    if (setPixels.Length != width * height) return false;

                    for (int x = startX; x < startX + width; x++)
                    {
                        for (int y = startY; y < startY + height; y++)
                        {
                            if (x < 0 || x >= _size.width || y < 0 || y >= _size.height) continue;
                            _bgPixels[x, y] = setPixels[(y - startY) * width + (x - startX)];
                        }
                    }

                    return true;
                }

                /// <summary> Set given BG pixel positions, to give ConsoleLine. </summary>
                public bool SetBGPixels(int x, int y, ConsoleLine consoleLine)
                {
                    return SetBGPixels(x, y, consoleLine.lineColourBG.Length, 1, consoleLine.lineColourBG);
                }

                /// <summary> Set given BG pixel positions, to give ConsoleLines. </summary>
                public bool SetBGPixels(int x, int y, ConsoleLine[] consoleLines)
                {
                    bool successfulLine = false;
                    for (int i = 0; i < consoleLines.Length; i++)
                    {
                        if (SetBGPixels(x, y + i, consoleLines[i])) successfulLine = true;
                    }
                    return successfulLine;
                }

                /// <summary> Set given BG pixel positions, to given colour. </summary>
                public bool SetBGPixels(int startX, int startY, int width, int height, ConsoleColor colour = ConsoleColor.White) { return SetBGPixels(startX, startY, width, height, Enumerable.Repeat(colour, width * height).ToArray()); }
                /// <summary> Sets all BG pixels to given colour. </summary>
                public bool SetBGPixels(ConsoleColor colour) { return SetBGPixels(0, 0, _size.width, _size.height, colour); }

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
                        ConsoleAction.UpdatePrimaryConsoleLine(ConsoleLine.Overlay(ConsoleAction.GetConsoleLine(lineIndex), c[lineIndex - y], x), lineIndex);
                    }

                    return true;
                }

                /// <summary> Returns a row of the image as a ConsoleLine. </summary>
                public ConsoleLine RowToConsoleLine(int rowIndex, bool colourless = false)
                {
                    if (rowIndex < 0 || rowIndex >= _size.height) return new ConsoleLine();

                    string s = "";
                    ConsoleColor[] c = new ConsoleColor[_size.width];
                    ConsoleColor[] bgC = new ConsoleColor[_size.width];

                    for (int x = 0; x < _size.width; x++)
                    {
                        s += _pixels[x, rowIndex].charchter;
                        c[x] = colourless ? ConsoleColor.White : _pixels[x, rowIndex].colour;
                        bgC[x] = _bgPixels[x, rowIndex];
                    }

                    return new ConsoleLine(s, c, bgC);
                }

                /// <summary> Returns a column of the image as a ConsoleLine. </summary>
                public ConsoleLine ColumnToConsoleLine(int columIndex, bool colourless = false)
                {
                    if (columIndex < 0 || columIndex >= _size.width) return new ConsoleLine();

                    string s = "";
                    ConsoleColor[] c = new ConsoleColor[_size.height];
                    ConsoleColor[] bgC = new ConsoleColor[_size.height];

                    for (int y = 0; y < _size.height; y++)
                    {
                        s += _pixels[columIndex, y].charchter;
                        c[y] = colourless ? ConsoleColor.White : _pixels[columIndex, y].colour;
                        bgC[y] = _bgPixels[columIndex, y];
                    }

                    return new ConsoleLine(s, c, bgC);
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
        }
    }
}