using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters;
using Revistone.Console;
using static System.ConsoleColor;

namespace Revistone
{
    namespace Functions
    {
        /// <summary>
        /// Class with methods for creating ConsoleColor arrays for ConsoleLine and ConsoleImage use.
        /// </summary>
        public static class ColourFunctions
        {
            // --- GRADIENTS ---

            /// <summary> Colour gradient, [Cyan, Dark Cyan]. </summary>
            public static ConsoleColor[] CyanGradient { get { return new ConsoleColor[2] { Cyan, DarkCyan }; } }
            /// <summary> Colour gradient, [Blue, Dark Blue]. </summary>
            public static ConsoleColor[] BlueGradient { get { return new ConsoleColor[2] { Blue, DarkBlue }; } }
            /// <summary> Colour gradient, [Cyan, Dark Cyan]. </summary>
            public static ConsoleColor[] MagentaGradient { get { return new ConsoleColor[2] { Magenta, DarkMagenta }; } }
            /// <summary> Colour gradient, [Red, Dark Red]. </summary>
            public static ConsoleColor[] RedGradient { get { return new ConsoleColor[2] { Red, DarkRed }; } }
            /// <summary> Colour gradient, [Green, Dark Green]. </summary>
            public static ConsoleColor[] GreenGradient { get { return new ConsoleColor[2] { Green, DarkGreen }; } }
            /// <summary> Colour gradient, [Yellow, Dark Yellow]. </summary>
            public static ConsoleColor[] YellowGradient { get { return new ConsoleColor[2] { Yellow, DarkYellow }; } }
            /// <summary> Colour gradient, [Grey, Dark Grey]. </summary>
            public static ConsoleColor[] GrayGradient { get { return new ConsoleColor[2] { Gray, DarkGray }; } }
            /// <summary> Colour gradient, [Red, Yellow]. </summary>
            public static ConsoleColor[] RedAndYellow { get { return new ConsoleColor[2] { Red, Yellow }; } }
            /// <summary> Colour gradient, [Red, Green]. </summary>
            public static ConsoleColor[] RedAndGreen { get { return new ConsoleColor[2] { Red, Green }; } }
             /// <summary> Colour gradient, [Red, Yellow, Green]. </summary>
            public static ConsoleColor[] RedAndYellowAndGreen { get { return new ConsoleColor[3] { Red, Yellow, Green }; } }
            /// <summary> Colour gradient, [Green, Blue]. </summary>
            public static ConsoleColor[] GreenAndBlue { get { return new ConsoleColor[2] { Green, Blue }; } }
            /// <summary> Colour gradient, [Dark Blue, Magenta]. </summary>
            public static ConsoleColor[] DarkBlueAndMagenta { get { return new ConsoleColor[2] { DarkBlue, Magenta }; } }
            /// <summary> Colour gradient, [White, Black]. </summary>
            public static ConsoleColor[] WhiteAndBlack { get { return new ConsoleColor[2] { White, Black }; } }
            /// <summary> Colour gradient, [White, Grey, Grey, Black]. </summary>
            public static ConsoleColor[] WhiteBlackGradient { get { return new ConsoleColor[4] { White, Gray, DarkGray, Black }; } }
            /// <summary> Colour gradient, [Cyan, Dark Cyan, Blue, Dark Blue]. </summary>
            public static ConsoleColor[] CyanDarkBlueGradient { get { return new ConsoleColor[4] { Cyan, DarkCyan, Blue, DarkBlue }; } }
            /// <summary> Colour gradient, [Blue, Dark Blue, Magenta, Dark Magenta]. </summary>
            public static ConsoleColor[] BlueDarkMagentaGradient { get { return new ConsoleColor[4] { Blue, DarkBlue, Magenta, DarkMagenta }; } }
            /// <summary> Colour gradient, [Cyan, Dark Cyan, Blue, Dark Blue, Magenta, Dark Magenta]. </summary>
            public static ConsoleColor[] CyanDarkMagentaGradient { get { return new ConsoleColor[6] { Cyan, DarkCyan, Blue, DarkBlue, Magenta, DarkMagenta }; } }
            /// <summary> Colour gradient, [Red, Yellow, Green, Cyan, Blue, Magenta]. </summary>
            public static ConsoleColor[] RainbowGradient { get { return new ConsoleColor[6] { Red, Yellow, Green, Cyan, Blue, Magenta }; } }
            /// <summary> Colour gradient, [Red, Green, Cyan, Blue, Magenta]. </summary>
            public static ConsoleColor[] RainbowWithoutYellowGradient { get { return new ConsoleColor[5] { Red, Green, Cyan, Blue, Magenta }; } }

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

                    if (gradientPattern)
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

                ConsoleAction.SendDebugMessage(StringFunctions.ToElementString(wordsCount));

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
    }
}