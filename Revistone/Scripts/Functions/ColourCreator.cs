using System.Data.Common;
using System.Linq.Expressions;
using Colorful;
using Revistone.Console;

namespace Revistone
{
    namespace Functions
    {
        public static class ColourCreator
        {
            public static ConsoleColor[] CyanToDarkMagentaGradient = new ConsoleColor[6] {
            ConsoleColor.Cyan, ConsoleColor.DarkCyan, ConsoleColor.Blue, ConsoleColor.DarkBlue, ConsoleColor.Magenta, ConsoleColor.DarkMagenta
            };

            public static ConsoleColor[] CreateColourGradient(ConsoleColor[] colours, int length)
            {
                if (colours.Length == 0) return colours;

                ConsoleColor[] c = new ConsoleColor[length];
                int pointer = 0;
                bool pointerDirection = true;

                for (int i = 0; i < length; i++)
                {
                    c[i] = colours[pointer];

                    if (pointerDirection)
                    {
                        if (pointer < colours.Length - 1) pointer++;
                        else
                        {
                            pointer--;
                            pointerDirection = false;
                        }
                    }
                    else
                    {
                        if (pointer > 0) pointer--;
                        else
                        {
                            pointer++;
                            pointerDirection = true;
                        }
                    }
                }

                return c;
            }

            public static ConsoleColor[] AlternateWordColours(string text, ConsoleColor[] colours)
            {
                if (colours.Length == 0) return colours;

                ConsoleColor[] c = new ConsoleColor[text.Length];

                bool newWord = false;
                int pointer = 0;

                for (int i = 0; i < text.Length; i++)
                {
                    if (text[i] == ' ') newWord = true;
                    else if (newWord && text[i] != ' ') pointer = pointer < colours.Length - 1 ? pointer + 1 : 0;

                    c[i] = colours[pointer];
                }

                return c;
            }

            public static ConsoleColor[] HighlightWords(string text, int[] indexes, ConsoleColor[] colours, ConsoleColor baseColour)
            {
                if (colours.Length == 0) return colours;

                ConsoleColor[] c = new ConsoleColor[text.Length];

                bool newWord = false;
                bool colourWord = false;
                int wordIndex = 0;
                int pointer = 0;

                for (int i = 0; i < text.Length; i++)
                {
                    if (text[i] == ' ') newWord = true;
                    else if (newWord && text[i] != ' ')
                    {
                        wordIndex += 1;
                        ConsoleAction.SendDebugMessage($"Letter Index {i}, Word Index {wordIndex}");
                        if (indexes.Contains(wordIndex))
                        {
                            pointer = pointer < colours.Length - 1 ? pointer + 1 : 0;
                            colourWord = true;
                        }
                        else
                        {
                            colourWord = false;
                        }

                        newWord = false;
                    }

                    if (colourWord) c[i] = colours[pointer];
                    else c[i] = baseColour;
                }

                return c;
            }


            public static ConsoleColor[] AlternateColours(ConsoleColor[] colours, int length)
            {
                if (colours.Length == 0) return colours;

                ConsoleColor[] c = new ConsoleColor[length];
                int pointer = 0;
                for (int i = 0; i < length; i++)
                {
                    c[i] = colours[pointer];
                    pointer = pointer < colours.Length - 1 ? pointer + 1 : 0;
                }

                return c;
            }


            public static ConsoleColor[] AlternateColours(ConsoleColor[] colours, int length, int colourLength)
            {
                if (colours.Length == 0) return colours;

                ConsoleColor[] c = new ConsoleColor[length];
                (int c, int l) pointer = new(0, 0);
                for (int i = 0; i < length; i++)
                {
                    c[i] = colours[pointer.c];

                    pointer.l++;
                    if (pointer.l >= colourLength)
                    {
                        pointer.c = pointer.c >= colours.Length - 1 ? 0 : pointer.c + 1;
                        pointer.l = 0;
                    }
                }

                return c;
            }

            public static ConsoleColor[] ShiftColours(ConsoleColor[] colours, int shift)
            {
                if (colours.Length == 0) return colours;

                ConsoleColor[] c = new ConsoleColor[colours.Length];
                shift = shift % colours.Length;

                for (int i = 0; i < c.Length; i++)
                {
                    int newIndex = i + shift;
                    if (newIndex >= c.Length) newIndex -= c.Length;
                    else if (newIndex < 0) newIndex += c.Length;
                    c[newIndex] = colours[i];
                }

                return c;
            }
        }
    }
}