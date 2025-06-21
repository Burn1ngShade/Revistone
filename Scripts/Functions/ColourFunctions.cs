using Revistone.Console.Image;
using Revistone.Modules;
using static Revistone.Console.Image.ConsoleColour;

namespace Revistone.Functions;

/// <summary>
/// Class with methods for creating ConsoleColor arrays for ConsoleLine and ConsoleImage use.
/// </summary>
public static class ColourFunctions
{
    // --- GRADIENTS ---

    public readonly static ConsoleColour[] BaseColours = [
        White,Gray, DarkGray, Black, Red, DarkRed, Yellow, DarkYellow, Green, DarkGreen, Cyan, DarkCyan, Blue, DarkBlue, Magenta, DarkMagenta
    ];
    public readonly static ConsoleColour[] BaseBorderColours = [
        Cyan, DarkCyan, Blue, DarkBlue
    ];

    // --- NEW COLOUR / OUTPUT ---

    ///<summary> Lumiance of colour, based of Rec. 709 Constants. </summary>
    public static double Luminance(this ConsoleColour colour) => 0.2126 * colour.R + 0.7152 * colour.G + 0.0722 * colour.B;

    ///<summary> Returns either black or white, based on lumiance of colour. </summary>
    public static ConsoleColour GetContrastColour(this ConsoleColour colour)
    {
        return colour.Luminance() > 128 ? Black : White;
    }

    // --- NEW ARRARY ---

    ///<summary> Creates a length sized ConsoleColour[] linearly interpreting between colours. </summary>
    public static ConsoleColour[] CreateGradient(ConsoleColour startColour, ConsoleColour endColour, int length)
    {
        var c = new ConsoleColour[length];

        if (length == 1) return [startColour]; // prevent divide by 0 error

        double rStep = 1d / (length - 1);
        double t = 0;

        for (int i = 0; i < length; i++)
        {
            c[i] = new ConsoleColour(Byte3.Lerp(startColour.RGB, endColour.RGB, t), startColour.TextStyleFlags);
            t += rStep;
        }

        return c;
    }

    ///<summary> Converts ConsoleColour to ConsoleColour[] of given length. </summary>
    public static ConsoleColour[] ToArray(this ConsoleColour colour, int length = 1)
    {
        var c = new ConsoleColour[length];
        for (int i = 0; i < length; i++) { c[i] = colour; }
        return c;
    }

    ///<summary> Converts ConsoleColours to ConsoleColour[] of given lengths. </summary>
    public static ConsoleColour[] ToArray(params (ConsoleColour colour, int length)[] colours)
    {
        var c = new List<ConsoleColour>();

        for (int i = 0; i < colours.Length; i++)
        {
            c.AddRange(colours[i].colour.ToArray(colours[i].length));
        }

        return [.. c];
    }

    ///<summary> Converts ConsoleColour[] To ConsoleColour[,]. </summary>
    public static ConsoleColour[,] To2DArray(this ConsoleColour[] colours, int width, ConsoleColour defaultColour)
    {
        if (colours.Length == 0) return new ConsoleColour[0, 0];

        int height = (int)Math.Ceiling((double)colours.Length / width);
        var c = new ConsoleColour[height, width];

        for (int i = 0; i < colours.Length; i++)
        {
            c[i / width, i % width] = colours[i];
        }

        int lastRow = height - 1;
        int remainder = colours.Length % width;

        if (remainder == 0) return c;

        for (int j = remainder; j < width; j++)
        {
            c[lastRow, j] = defaultColour;
        }

        return c;
    }

    ///<summary> Build ConsoleColour[] from smaller ConsoleColour[]. </summary>
    public static ConsoleColour[] BuildArray(params ConsoleColour[][] colours)
    {
        ConsoleColour[] c = [];

        for (int i = 0; i < colours.Length; i++)
        {
            c = [.. c, .. colours[i]];
        }

        return c;
    }

    // --- ARRAY MODIFICATION ---

    ///<summary> Sets ConsoleColour[] to given length, using extensionColour. </summary>
    public static ConsoleColour[] SetLength(this ConsoleColour[] colours, ConsoleColour extensionColour, int length)
    {
        if (colours.Length >= length) return colours[..length];

        ConsoleColour[] c = new ConsoleColour[length];

        for (int i = 0; i < c.Length; i++)
        {
            c[i] = i < colours.Length ? colours[i] : extensionColour;
        }

        return c;
    }

    ///<summary> Sets ConsoleColour[] to given length, using last colour in ConsoleColour[]. </summary>
    public static ConsoleColour[] SetLength(this ConsoleColour[] colours, int length)
    {
        return colours.SetLength(colours.Length == 0 ? Black : colours[^1], length);
    }

    ///<summary> Sets ConsoleColour[] to given length, using last colour in ConsoleColour[]. </summary>
    public static ConsoleColour[] SetLength(this ConsoleColour colour, int length)
    {
        return new ConsoleColour[] { colour }.SetLength(length);
    }

    ///<summary> Replaces given ConsoleColours within the ConsoleColour[]. </summary>
    public static ConsoleColour[] Replace(this ConsoleColour[] colours, params (ConsoleColour colour, ConsoleColour replacementColour)[] r)
    {
        return ReplaceRange(colours, 0, colours.Length, r);
    }

    ///<summary> Replaces given ConsoleColours within the ConsoleColour[] and bounds. </summary>
    public static ConsoleColour[] ReplaceRange(this ConsoleColour[] colours, int startIndex, int length, params (ConsoleColour colour, ConsoleColour replacementColour)[] r)
    {
        var c = new ConsoleColour[colours.Length];

        for (int i = 0; i < colours.Length; i++)
        {
            c[i] = colours[i];

            if (i >= startIndex && i < startIndex + length)
            {
                for (int j = 0; j < r.Length; j++)
                {
                    if (colours[i] == r[j].colour)
                    {
                        c[i] = r[j].replacementColour;
                        break;
                    }
                }
            }
        }

        return c;
    }

    ///<summary> Reverses colours[] within given bounds. </summary>
    public static ConsoleColour[] ReverseRange(this ConsoleColour[] colours, int startIndex, int length)
    {
        var c = new ConsoleColour[colours.Length];

        for (int i = 0; i < colours.Length; i++)
        {
            c[i] = colours[i];
        }

        int endIndex = Math.Min(colours.Length, startIndex + length) - 1;

        for (int i = startIndex; i <= endIndex; i++)
        {
            c[i] = colours[endIndex - (i - startIndex)];
        }

        return c;
    }

    ///<summary> Cycle ConsoleColour[] by given shift. </summary>
    public static ConsoleColour[] Cycle(this ConsoleColour[] colours, int shift)
    {
        return CycleRange(colours, shift, 0, colours.Length);
    }

    ///<summary> Cycle colours within bounds in ConsoleColour[] by given shift. </summary>
    public static ConsoleColour[] CycleRange(this ConsoleColour[] colours, int shift, int startIndex, int length)
    {
        if (colours.Length == 0) return colours;

        var c = new ConsoleColour[colours.Length];
        shift %= length;

        for (int i = 0; i < c.Length; i++) { c[i] = colours[i]; }

        for (int i = Math.Max(startIndex, 0); i < Math.Min(startIndex + length, colours.Length); i++)
        {
            int shiftI = i + shift;
            shiftI += (shiftI >= (startIndex + length)) ? -length : shiftI < startIndex ? length : 0;
            c[shiftI] = colours[i];
        }

        return c;
    }

    ///<summary> Cycle colours within multiple bounds in ConsoleColour[] by given shift. </summary>
    public static ConsoleColour[] CycleRanges(this ConsoleColour[] colours, params (int shift, int startIndex, int length)[] shifts)
    {
        var c = new ConsoleColour[colours.Length];

        for (int i = 0; i < colours.Length; i++) { c[i] = colours[i]; }

        for (int i = 0; i < shifts.Length; i++)
        {
            c = CycleRange(c, shifts[i].shift, shifts[i].startIndex, shifts[i].length);
        }

        return c;
    }

    ///<summary> Repeats consoleColour array repeat times. </summary>
    public static ConsoleColour[] Repeat(this ConsoleColour[] colours, int repeats)
    {
        if (repeats <= 1) return colours;

        var c = new List<ConsoleColour>();
        for (int i = 0; i < repeats; i++) c.AddRange(colours);
        return [.. c];
    }

    /// <summary> Stretchs ConsoleColour array by given factor. </summary>
    public static ConsoleColour[] Stretch(this ConsoleColour[] colours, int factor)
    {
        if (factor <= 1) return colours;

        ConsoleColour[] c = new ConsoleColour[colours.Length * factor];

        for (int i = 0; i < c.Length; i++)
        {
            c[i] = colours[i / factor];
        }

        return c;
    }

    /// <summary> Generates a ConsoleColor array with cycling colours, changing after each given colourLength, for given length. </summary>
    public static ConsoleColour[] VariableStretch(this ConsoleColour[] colours, int length, int[] colourLength)
    {
        if (colours.Length == 0 || colourLength.Length == 0) return colours;

        var c = new ConsoleColour[length];
        int colourIndex = 0, lengthIndex = 0, colourLengthIndex = 0;

        for (int i = 0; i < length; i++)
        {
            c[i] = colours[colourIndex];

            lengthIndex++;
            if (lengthIndex >= colourLength[colourLengthIndex])
            {
                colourIndex = colourIndex >= colours.Length - 1 ? 0 : colourIndex + 1;
                lengthIndex = 0;
                colourLengthIndex = colourLength.Length - 1 > colourLengthIndex ? colourLengthIndex + 1 : 0;
            }
        }

        return c;
    }

    /// <summary> Generates a ConsoleColor array with cycing colours, changling every colourLength, for given length. </summary>
    public static ConsoleColour[] VariableStretch(ConsoleColour[] colours, int length, int colourLength = 1)
    {
        return VariableStretch(colours, length, [colourLength]);
    }

    // --- OLD STUFF ---

    // --- NEW ARRAY ---

    /// <summary> Generates a ConsoleColour[] from a base colour array, and highlight areas. </summary>
    public static ConsoleColour[] Highlight(int length, ConsoleColour[] baseColours, params (ConsoleColour[] colours, int startIndex, int length)[] highlights)
    {
        if (baseColours.Length == 0) return baseColours;

        ConsoleColour[] c = new ConsoleColour[length];

        int baseIndex = 0, highlightIndex = -1;

        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < highlights.Length; j++)
            {
                if (highlights[j].startIndex == i && highlights[j].colours.Length > 0) highlightIndex = j;
            }

            if (highlightIndex == -1) //base colours
            {
                c[i] = baseColours[baseIndex];
                baseIndex = baseIndex < baseColours.Length - 1 ? baseIndex + 1 : 0;
            }
            else
            {
                (ConsoleColour[] colours, int startIndex, int length) h = highlights[highlightIndex];
                c[i] = h.colours[(i - h.startIndex) % h.colours.Length];
                if (h.startIndex + h.length <= i + 1) highlightIndex = -1;
            }
        }

        return c;
    }

    // /// <summary> Generates a ConsoleColour array from a base colour array, highlight colour array, and highlight areas. </summary>
    // public static ConsoleColor[] AdvancedHighlight(int length, ConsoleColor[] baseColour, ConsoleColor[] highlightColour, params (int startIndex, int length)[] highlights)
    // {
    //     (ConsoleColor[] colours, int startIndex, int length)[] highlightsColoured = new (ConsoleColor[], int, int)[highlights.Length];

    //     for (int i = 0; i < highlights.Length; i++)
    //     {
    //         highlightsColoured[i] = (highlightColour, highlights[i].startIndex, highlights[i].length);
    //     }

    //     return AdvancedHighlight(length, baseColour.ToArray(), highlightsColoured);
    // }

    // /// <summary> Generates a ConsoleColour array from a base colour, highlight colour, and highlight areas. </summary>
    // public static ConsoleColor[] AdvancedHighlight(int length, ConsoleColor baseColour, ConsoleColor highlightColour, params (int startIndex, int length)[] highlights) { return AdvancedHighlight(length, baseColour.ToArray(), highlightColour.ToArray(), highlights); }

    // /// <summary> Generates a ConsoleColour array from a base colour, and highlight areas. </summary>
    // public static ConsoleColor[] AdvancedHighlight(int length, ConsoleColor baseColour, params (ConsoleColor[] colours, int startIndex, int length)[] highlights) { return AdvancedHighlight(length, baseColour.ToArray(), highlights); }

    // // <summary> Generates a ConsoleColour array from base colour array, and highlight words. </summary>
    // public static ConsoleColor[] AdvancedHighlight(string text, ConsoleColor[] baseColours, params (ConsoleColor[] colours, int index)[] wordIndexes)
    // {
    //     List<(int startIndex, int length)> words = new List<(int startIndex, int length)>();

    //     int wordStartIndex = -1;

    //     for (int i = 0; i < text.Length; i++)
    //     {
    //         if (string.IsNullOrWhiteSpace(text[i].ToString()))
    //         {
    //             if (wordStartIndex != -1)
    //             {
    //                 words.Add((wordStartIndex, i - wordStartIndex));
    //                 wordStartIndex = -1;
    //             }
    //         }
    //         else if (wordStartIndex == -1)
    //         {
    //             wordStartIndex = i;
    //         }
    //     }

    //     if (wordStartIndex != -1) words.Add((wordStartIndex, text.Length - wordStartIndex));

    //     List<(ConsoleColor[] colours, int startIndex, int length)> indexes = new List<(ConsoleColor[] colours, int startIndex, int length)>();

    //     for (int i = 0; i < words.Count; i++)
    //     {
    //         for (int j = 0; j < wordIndexes.Length; j++)
    //         {
    //             if (wordIndexes[j].index == i) indexes.Add((wordIndexes[j].colours, words[i].startIndex, words[i].length));
    //         }
    //     }

    //     return AdvancedHighlight(text.Length, baseColours, indexes.ToArray());
    // }

    // /// <summary> Generates a ConsoleColour array from a base colour array, highlight colour array, and word indexes. </summary>
    // public static ConsoleColor[] AdvancedHighlight(string text, ConsoleColor[] baseColour, ConsoleColor[] highlightColour, params int[] wordIndexes)
    // {
    //     (ConsoleColor[] colours, int index)[] highlightsColoured = new (ConsoleColor[], int)[wordIndexes.Length];

    //     for (int i = 0; i < wordIndexes.Length; i++)
    //     {
    //         highlightsColoured[i] = (highlightColour, wordIndexes[i]);
    //     }

    //     return AdvancedHighlight(text, baseColour.ToArray(), highlightsColoured);
    // }

    // /// <summary> Generates a ConsoleColour array from a base colour, highlight colour, and word indexes. </summary>
    // public static ConsoleColor[] AdvancedHighlight(string text, ConsoleColor baseColour, ConsoleColor highlightColour, params int[] wordIndexes) { return AdvancedHighlight(text, baseColour.ToArray(), highlightColour.ToArray(), wordIndexes); }

    // // <summary> Generates a ConsoleColour array from base colour array, and highlight words. </summary>
    // public static ConsoleColor[] AdvancedHighlight(string text, ConsoleColor baseColour, params (ConsoleColor[] colours, int index)[] wordIndexes) { return AdvancedHighlight(text, baseColour.ToArray(), wordIndexes); }

    // /// <summary> Generates a ConsoleColor array colourising alternating words in provided string using given colours. </summary>
    // public static ConsoleColor[] Alternate(string text, ConsoleColor[] colours)
    // {
    //     if (colours.Length == 0) return colours;

    //     List<int> wordsCount = new List<int>();

    //     int wordStartIndex = 0;
    //     bool wordFinished = false;

    //     string workText = text.TrimStart();

    //     for (int i = 0; i < workText.Length; i++)
    //     {
    //         if (string.IsNullOrWhiteSpace(workText[i].ToString()))
    //         {
    //             wordFinished = true;
    //         }
    //         else if (wordFinished)
    //         {
    //             wordsCount.Add(i - wordStartIndex);
    //             wordStartIndex = i;
    //             wordFinished = false;
    //         }
    //     }

    //     wordsCount.Add(workText.Length - wordStartIndex);

    //     if (workText.Length != text.Length) wordsCount.Insert(0, text.Length - workText.Length);

    //     return Alternate(colours, text.Length, wordsCount.ToArray());
    // }

    // /// <summary> Generates a ConsoleColor array with cycing colours, changling every colourLength, for given length. </summary>
    // public static ConsoleColor[] Alternate(ConsoleColor[] colours, int length, int colourLength = 1)
    // { return Alternate(colours, length, [colourLength]); }
}