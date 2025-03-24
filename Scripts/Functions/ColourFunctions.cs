using static System.ConsoleColor;

namespace Revistone.Functions;

/// <summary>
/// Class with methods for creating ConsoleColor arrays for ConsoleLine and ConsoleImage use.
/// </summary>
public static class ColourFunctions
{
    // --- GRADIENTS ---

    /// <summary> Colour gradient, [Cyan, Dark Cyan]. </summary>
    public static ConsoleColor[] CyanGradient { get { return [Cyan, DarkCyan]; } }
    /// <summary> Colour gradient, [Blue, Dark Blue]. </summary>
    public static ConsoleColor[] BlueGradient { get { return [Blue, DarkBlue]; } }
    /// <summary> Colour gradient, [Cyan, Dark Cyan]. </summary>
    public static ConsoleColor[] MagentaGradient { get { return [Magenta, DarkMagenta]; } }
    /// <summary> Colour gradient, [Red, Dark Red]. </summary>
    public static ConsoleColor[] RedGradient { get { return [Red, DarkRed]; } }
    /// <summary> Colour gradient, [Green, Dark Green]. </summary>
    public static ConsoleColor[] GreenGradient { get { return [Green, DarkGreen]; } }
    /// <summary> Colour gradient, [Yellow, Dark Yellow]. </summary>
    public static ConsoleColor[] YellowGradient { get { return [Yellow, DarkYellow]; } }
    /// <summary> Colour gradient, [Grey, Dark Grey]. </summary>
    public static ConsoleColor[] GrayGradient { get { return [Gray, DarkGray]; } }
    /// <summary> Colour gradient, [Red, Yellow]. </summary>
    public static ConsoleColor[] RedAndYellow { get { return [Red, Yellow]; } }
    /// <summary> Colour gradient, [Red, Green]. </summary>
    public static ConsoleColor[] RedAndGreen { get { return [Red, Green]; } }
    /// <summary> Colour gradient, [Red, Yellow, Green]. </summary>
    public static ConsoleColor[] RedAndYellowAndGreen { get { return [Red, Yellow, Green]; } }
    /// <summary> Colour gradient, [Green, Blue]. </summary>
    public static ConsoleColor[] GreenAndBlue { get { return [Green, Blue]; } }
    /// <summary> Colour gradient, [Green, Blue]. </summary>
    public static ConsoleColor[] DarkGreenAndDarkBlue { get { return [DarkGreen, DarkBlue]; } }
    /// <summary> Colour gradient, [Dark Blue, Magenta]. </summary>
    public static ConsoleColor[] DarkBlueAndMagenta { get { return [DarkBlue, Magenta]; } }
    /// <summary> Colour gradient, [White, Black]. </summary>
    public static ConsoleColor[] WhiteAndBlack { get { return [White, Black]; } }
    /// <summary> Colour gradient, [White, Grey, Grey, Black]. </summary>
    public static ConsoleColor[] WhiteBlackGradient { get { return [White, Gray, DarkGray, Black]; } }
    /// <summary> Colour gradient, [Cyan, Dark Cyan, Blue, Dark Blue]. </summary>
    public static ConsoleColor[] CyanDarkBlueGradient { get { return [Cyan, DarkCyan, Blue, DarkBlue]; } }
    /// <summary> Colour gradient, [Blue, Dark Blue, Magenta, Dark Magenta]. </summary>
    public static ConsoleColor[] BlueDarkMagentaGradient { get { return [Blue, DarkBlue, Magenta, DarkMagenta]; } }
    /// <summary> Colour gradient, [Cyan, Dark Cyan, Blue, Dark Blue, Magenta, Dark Magenta]. </summary>
    public static ConsoleColor[] CyanDarkMagentaGradient { get { return [Cyan, DarkCyan, Blue, DarkBlue, Magenta, DarkMagenta]; } }
    /// <summary> Colour gradient, [Red, Yellow, Green, Cyan, Blue, Magenta]. </summary>
    public static ConsoleColor[] RainbowGradient { get { return [Red, Yellow, Green, Cyan, Blue, Magenta]; } }
    /// <summary> Colour gradient, [Red, Green, Cyan, Blue, Magenta]. </summary>
    public static ConsoleColor[] RainbowWithoutYellowGradient { get { return [Red, Green, Cyan, Blue, Magenta]; } }
    /// <summary> Colour gradient, [Cyan, Magenta, White, Magenta, Cyan]. </summary>
    public static ConsoleColor[] TransPattern { get { return [Cyan, Magenta, White, Magenta, Cyan]; } }
    /// <summary> Colour gradient, All colours in enum index order. </summary>
    public static ConsoleColor[] AllColours { get { return [Black, DarkBlue, DarkGreen, DarkCyan, DarkRed, DarkMagenta, DarkYellow, Gray, DarkGray, Blue, Green, Cyan, Red, Magenta, Yellow, White]; } }

    ///<summary> Returns the most contrasting colour to the given ConsoleColour. </summary>
    public static Dictionary<ConsoleColor, ConsoleColor> ContrastColour = new Dictionary<ConsoleColor, ConsoleColor>
    {
        {Black, White}, {DarkBlue, Yellow}, {DarkGreen, Magenta}, {DarkCyan, Red}, {DarkRed, Cyan}, {DarkMagenta, Green}, {DarkYellow, Blue}, {Gray, Black}, {DarkGray, White}, {Blue, Yellow}, {Green, Magenta}, {Cyan, Red}, {Red, Cyan}, {Magenta, Green}, {Yellow, Blue}, {White, Black}
    };

    // --- MODIFY ARRAY ---

    /// <summary> Replaces colours within ConsoleColor array, with replacement colours, within bounds of startIndex and length. </summary>
    public static ConsoleColor[] Replace(this ConsoleColor[] colours, (ConsoleColor colour, ConsoleColor replacementColour)[] replace, int startIndex, int length)
    {
        ConsoleColor[] c = new ConsoleColor[colours.Length];
        for (int i = startIndex; i < Math.Min(colours.Length, startIndex + length); i++)
        {
            bool replaced = false;
            for (int j = 0; j < replace.Length; j++)
            {
                if (colours[i] == replace[j].colour)
                {
                    c[i] = replace[j].replacementColour;
                    replaced = true;
                    break;
                }
            }
            if (!replaced) c[i] = colours[i];
        }
        return c;
    }

    /// <summary> Replaces colours within ConsoleColor array, with replacement colours. </summary>
    public static ConsoleColor[] Replace(this ConsoleColor[] colours, params (ConsoleColor colour, ConsoleColor replacementColour)[] replace) { return Replace(colours, replace, 0, colours.Length); }

    /// <summary> Reverses section of ConsoleColour array, of given startIndex and length. </summary>
    public static ConsoleColor[] Flip(this ConsoleColor[] colours, int startIndex, int length)
    {
        ConsoleColor[] c = new ConsoleColor[colours.Length];

        for (int i = 0; i < colours.Length; i++) { c[i] = colours[i]; }

        for (int i = startIndex; i < Math.Min(colours.Length, startIndex + length); i++)
        {
            c[i] = colours[startIndex + Math.Min(colours.Length, startIndex + length) - 1 - i];
        }

        return c;
    }

    /// <summary> Reverses ConsoleColour array. </summary>
    public static ConsoleColor[] Flip(this ConsoleColor[] colours) { return Flip(colours, 0, colours.Length); }

    /// <summary> Shifts ConsoleColor array by given shift value, within bounds of startIndex and length. </summary>
    public static ConsoleColor[] Shift(this ConsoleColor[] colours, int shift, int startIndex, int length)
    {
        if (colours.Length == 0) return colours;

        ConsoleColor[] c = new ConsoleColor[colours.Length];
        shift = shift % length;

        for (int i = 0; i < c.Length; i++) { c[i] = colours[i]; }

        for (int i = startIndex; i < startIndex + length; i++)
        {
            int shiftI = i + shift;
            //this line of code is worse than a normal if statment, oh well... (less lines is better right?!?!?!)
            shiftI += (shiftI >= (startIndex + length)) ? -length : shiftI < startIndex ? length : 0;
            c[shiftI] = colours[i];
        }

        return c;
    }

    /// <summary> Shifts ConsoleColor array by given shift value, within bounds of startIndex and length. </summary>
    public static ConsoleColor[] Shift(this ConsoleColor[] colours, params (int shift, int startIndex, int length)[] shifts)
    {
        ConsoleColor[] c = new ConsoleColor[colours.Length];

        for (int i = 0; i < colours.Length; i++) { c[i] = colours[i]; }

        for (int i = 0; i < shifts.Length; i++)
        {
            c = Shift(c, shifts[i].shift, shifts[i].startIndex, shifts[i].length);
        }

        return c;
    }

    /// <summary> Shifts ConsoleColor array by given shift value. </summary>
    public static ConsoleColor[] Shift(this ConsoleColor[] colours, int shift) { return Shift(colours, shift, 0, colours.Length); }

    /// <summary> Generates a ConsoleColor array out of given colours and lengths. </summary>
    public static ConsoleColor[] ToArray((ConsoleColor colour, int length)[] colours)
    {
        List<ConsoleColor> c = new List<ConsoleColor>();
        for (int i = 0; i < colours.Length; i++)
        {
            for (int j = 0; j < colours[i].length; j++)
            {
                c.Add(colours[i].colour);
            }
        }
        return c.ToArray();
    }

    /// <summary> Generates ConsoleColour to array of given length. </summary>
    public static ConsoleColor[] ToArray(this ConsoleColor colour, int length = 1) { return ToArray(new (ConsoleColor, int)[] { (colour, length) }); }

    /// <summary> Generates ConsoleColour array to a jaged array of given length. </summary>
    public static ConsoleColor[][] ToJaggedArray(this ConsoleColor[] colours, int length)
    {
        ConsoleColor[][] c = new ConsoleColor[length][];

        for (int i = 0; i < length; i++)
        {
            c[i] = colours;
        }

        return c;
    }

    /// <summary> Repeats ConsoleColour array repeats number of times. </summary>
    public static ConsoleColor[] Repeat(this ConsoleColor[] colours, int repeats)
    {
        if (colours.Length == 0) return colours;

        ConsoleColor[] c = new ConsoleColor[colours.Length * repeats];

        for (int i = 0; i < repeats; i++)
        {
            for (int j = 0; j < colours.Length; j++)
            {
                c[i * colours.Length + j] = colours[j];
            }
        }

        return c;
    }

    /// <summary> Extends ConsoleColour array to given length. </summary>
    public static ConsoleColor[] Extend(this ConsoleColor[] colours, int length, bool gradientPattern = false)
    {
        if (colours.Length == 0) return colours;

        ConsoleColor[] c = new ConsoleColor[length];

        (int index, bool direction) colour = (0, true);
        for (int i = 0; i < length; i++)
        {
            c[i] = colours[colour.index];

            if (gradientPattern && colours.Length != 1)
            {
                if ((colour.index >= colours.Length - 1 && colour.direction) || (colour.index <= 0 && !colour.direction)) colour.direction = !colour.direction;

                colour.index += colour.direction ? 1 : -1;
            }
            else
            {
                colour.index = colour.index + 1 >= colours.Length ? 0 : colour.index + 1;
            }
        }

        return c;
    }

    /// <summary> Extends ConsoleColour array to given length, by appending given extendColour. </summary>
    public static ConsoleColor[] Extend(this ConsoleColor[] colours, ConsoleColor extendColour, int length)
    {
        if (colours.Length == 0) return colours;

        ConsoleColor[] c = new ConsoleColor[length];

        for (int i = 0; i < c.Length; i++)
        {
            c[i] = colours.Length > i ? colours[i] : extendColour;
        }

        return c;
    }

    /// <summary> Extends ConsoleColour to given length. </summary>
    public static ConsoleColor[] Extend(this ConsoleColor colour, int length) { return Extend(colour.ToArray(), length, false); }

    ///<summary> Extends ConsoleColour arrary to given length, appending the final colour in the array. </summary>
    public static ConsoleColor[] ExtendEnd(this ConsoleColor[] colours, int length)
    {
        if (colours.Length == 0) return colours;

        return colours.Extend(colours[^1], length);
    }

    /// <summary> Stretchs ConsoleColour array by given factor. </summary>
    public static ConsoleColor[] Stretch(this ConsoleColor[] colours, int factor)
    {
        if (factor <= 1) return colours;

        ConsoleColor[] c = new ConsoleColor[colours.Length * factor];

        for (int i = 0; i < c.Length; i++)
        {
            c[i] = colours[i / factor];
        }

        return c;
    }

    // --- NEW ARRAY ---

    /// <summary> Generates a ConsoleColour array from a base colour array, and highlight areas. </summary>
    public static ConsoleColor[] AdvancedHighlight(int length, ConsoleColor[] baseColours, params (ConsoleColor[] colours, int startIndex, int length)[] highlights)
    {
        if (baseColours.Length == 0) return baseColours;

        ConsoleColor[] c = new ConsoleColor[length];

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
                (ConsoleColor[] colours, int startIndex, int length) h = highlights[highlightIndex];
                c[i] = h.colours[(i - h.startIndex) % h.colours.Length];
                if (h.startIndex + h.length <= i + 1) highlightIndex = -1;
            }
        }

        return c;
    }

    /// <summary> Generates a ConsoleColour array from a base colour array, highlight colour array, and highlight areas. </summary>
    public static ConsoleColor[] AdvancedHighlight(int length, ConsoleColor[] baseColour, ConsoleColor[] highlightColour, params (int startIndex, int length)[] highlights)
    {
        (ConsoleColor[] colours, int startIndex, int length)[] highlightsColoured = new (ConsoleColor[], int, int)[highlights.Length];

        for (int i = 0; i < highlights.Length; i++)
        {
            highlightsColoured[i] = (highlightColour, highlights[i].startIndex, highlights[i].length);
        }

        return AdvancedHighlight(length, baseColour.ToArray(), highlightsColoured);
    }

    /// <summary> Generates a ConsoleColour array from a base colour, highlight colour, and highlight areas. </summary>
    public static ConsoleColor[] AdvancedHighlight(int length, ConsoleColor baseColour, ConsoleColor highlightColour, params (int startIndex, int length)[] highlights) { return AdvancedHighlight(length, baseColour.ToArray(), highlightColour.ToArray(), highlights); }

    /// <summary> Generates a ConsoleColour array from a base colour, and highlight areas. </summary>
    public static ConsoleColor[] AdvancedHighlight(int length, ConsoleColor baseColour, params (ConsoleColor[] colours, int startIndex, int length)[] highlights) { return AdvancedHighlight(length, baseColour.ToArray(), highlights); }

    // <summary> Generates a ConsoleColour array from base colour array, and highlight words. </summary>
    public static ConsoleColor[] AdvancedHighlight(string text, ConsoleColor[] baseColours, params (ConsoleColor[] colours, int index)[] wordIndexes)
    {
        List<(int startIndex, int length)> words = new List<(int startIndex, int length)>();

        int wordStartIndex = -1;

        for (int i = 0; i < text.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(text[i].ToString()))
            {
                if (wordStartIndex != -1)
                {
                    words.Add((wordStartIndex, i - wordStartIndex));
                    wordStartIndex = -1;
                }
            }
            else if (wordStartIndex == -1)
            {
                wordStartIndex = i;
            }
        }

        if (wordStartIndex != -1) words.Add((wordStartIndex, text.Length - wordStartIndex));

        List<(ConsoleColor[] colours, int startIndex, int length)> indexes = new List<(ConsoleColor[] colours, int startIndex, int length)>();

        for (int i = 0; i < words.Count; i++)
        {
            for (int j = 0; j < wordIndexes.Length; j++)
            {
                if (wordIndexes[j].index == i) indexes.Add((wordIndexes[j].colours, words[i].startIndex, words[i].length));
            }
        }

        return AdvancedHighlight(text.Length, baseColours, indexes.ToArray());
    }

    /// <summary> Generates a ConsoleColour array from a base colour array, highlight colour array, and word indexes. </summary>
    public static ConsoleColor[] AdvancedHighlight(string text, ConsoleColor[] baseColour, ConsoleColor[] highlightColour, params int[] wordIndexes)
    {
        (ConsoleColor[] colours, int index)[] highlightsColoured = new (ConsoleColor[], int)[wordIndexes.Length];

        for (int i = 0; i < wordIndexes.Length; i++)
        {
            highlightsColoured[i] = (highlightColour, wordIndexes[i]);
        }

        return AdvancedHighlight(text, baseColour.ToArray(), highlightsColoured);
    }

    /// <summary> Generates a ConsoleColour array from a base colour, highlight colour, and word indexes. </summary>
    public static ConsoleColor[] AdvancedHighlight(string text, ConsoleColor baseColour, ConsoleColor highlightColour, params int[] wordIndexes) { return AdvancedHighlight(text, baseColour.ToArray(), highlightColour.ToArray(), wordIndexes); }

    // <summary> Generates a ConsoleColour array from base colour array, and highlight words. </summary>
    public static ConsoleColor[] AdvancedHighlight(string text, ConsoleColor baseColour, params (ConsoleColor[] colours, int index)[] wordIndexes) { return AdvancedHighlight(text, baseColour.ToArray(), wordIndexes); }

    /// <summary> Generates a ConsoleColor array with cycling colours, changing after each given colourLength, for given length. </summary>
    public static ConsoleColor[] Alternate(ConsoleColor[] colours, int length, int[] colourLength)
    {
        if (colours.Length == 0 || colourLength.Length == 0) return colours;

        ConsoleColor[] c = new ConsoleColor[length];
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

    /// <summary> Generates a ConsoleColor array colourising alternating words in provided string using given colours. </summary>
    public static ConsoleColor[] Alternate(string text, ConsoleColor[] colours)
    {
        if (colours.Length == 0) return colours;

        List<int> wordsCount = new List<int>();

        int wordStartIndex = 0;
        bool wordFinished = false;

        string workText = text.TrimStart();

        for (int i = 0; i < workText.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(workText[i].ToString()))
            {
                wordFinished = true;
            }
            else if (wordFinished)
            {
                wordsCount.Add(i - wordStartIndex);
                wordStartIndex = i;
                wordFinished = false;
            }
        }

        wordsCount.Add(workText.Length - wordStartIndex);

        if (workText.Length != text.Length) wordsCount.Insert(0, text.Length - workText.Length);

        return Alternate(colours, text.Length, wordsCount.ToArray());
    }

    /// <summary> Generates a ConsoleColor array with cycing colours, changling every colourLength, for given length. </summary>
    public static ConsoleColor[] Alternate(ConsoleColor[] colours, int length, int colourLength = 1)
    { return Alternate(colours, length, new int[] { colourLength }); }

    public static ConsoleColor[] BuildArray(params ConsoleColor[][] colours)
    {
        ConsoleColor[] c = new ConsoleColor[] { };

        for (int i = 0; i < colours.Length; i++)
        {
            c = c.Concat(colours[i]).ToArray();
        }

        return c;
    }
}