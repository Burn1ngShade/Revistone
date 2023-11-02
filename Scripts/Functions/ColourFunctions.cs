using System.Data.Common;
using System.Linq.Expressions;
using Colorful;
using Revistone.Console;

namespace Revistone
{
    namespace Functions
    {
        /// <summary>
        /// Class with methods for creating ConsoleColor arrays for ConsoleLine and ConsoleImage use.
        /// </summary>
        public static class ColourFunctions
        {
            /// <summary> Colour gradient, [Cyan, Dark Cyan, Blue, Dark Blue, Magenta, Dark Magenta]. </summary>
            public static ConsoleColor[] CyanToDarkMagentaGradient = new ConsoleColor[6]
            { ConsoleColor.Cyan, ConsoleColor.DarkCyan, ConsoleColor.Blue, ConsoleColor.DarkBlue, ConsoleColor.Magenta, ConsoleColor.DarkMagenta };

            /// <summary>
            /// Generates a ConsoleColor array with a gradient pattern using the provided colours and length.
            /// </summary>
            public static ConsoleColor[] ColourGradient(ConsoleColor[] colours, int length)
            {
                if (colours.Length == 0) return colours;

                ConsoleColor[] c = new ConsoleColor[length];

                int colourIndex = 0;
                bool colourIndexDirection = true;

                for (int i = 0; i < length; i++)
                {
                    c[i] = colours[colourIndex];

                    if ((colourIndex >= colours.Length - 1 && colourIndexDirection) || (colourIndex <= 0 && !colourIndexDirection)) colourIndexDirection = !colourIndexDirection;

                    colourIndex += colourIndexDirection ? 1 : -1;
                }

                return c;
            }

            /// <summary>
            /// Generates a ConsoleColor array colourising alternating words in provided string using given colours.
            /// </summary>
            public static ConsoleColor[] ColourAlternatingWords(string text, ConsoleColor[] colours)
            {
                if (colours.Length == 0) return colours;

                ConsoleColor[] c = new ConsoleColor[text.Length];

                int colourIndex = 0;
                bool newWord = false;

                for (int i = 0; i < text.Length; i++)
                {
                    if (text[i] == ' ') newWord = true;
                    else if (newWord && text[i] != ' ')
                    {
                        colourIndex = colourIndex < colours.Length - 1 ? colourIndex + 1 : 0;
                        newWord = false;
                    }

                    c[i] = colours[colourIndex];
                }

                return c;
            }

            /// <summary>
            /// Generates a ConsoleColor array colourising words at given indexes, cycling through given colours.
            /// </summary>
            public static ConsoleColor[] ColourWords(string text, int[] indexes, ConsoleColor[] colours, ConsoleColor baseColour = ConsoleColor.White)
            {
                if (colours.Length == 0) return colours;

                ConsoleColor[] c = new ConsoleColor[text.Length];

                int wordIndex = -1, colourIndex = -1;

                for (int i = 0; i < text.Length; i++)
                {
                    if ((i == 0 || text[i - 1] == ' ') && text[i] != ' ')
                    {
                        wordIndex++;
                        if (indexes.Contains(wordIndex)) colourIndex = colourIndex >= colours.Length - 1 ? 0 : colourIndex + 1;
                    }

                    c[i] = indexes.Contains(wordIndex) ? colours[colourIndex] : baseColour;
                }

                return c;
            }

            /// <summary>
            /// Generates a ConsoleColor array with cycling colours, changing every colourLength, for given length.
            /// </summary>
            public static ConsoleColor[] AlternatingColours(ConsoleColor[] colours, int length, int colourLength = 1) 
            { return AlternatingColours(colours, length, new int[] {colourLength}); }

            /// <summary>
            /// Generates a ConsoleColor array with cycling colours, changing after each given colourLength, for given length.
            /// </summary>
            public static ConsoleColor[] AlternatingColours(ConsoleColor[] colours, int length, int[] colourLength)
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

            /// <summary>
            /// Generates a ConsoleColor array given colours, shifting it by the vector value shift.
            /// </summary>
            public static ConsoleColor[] ShiftColours(ConsoleColor[] colours, int shift)
            {
                if (colours.Length == 0) return colours;

                ConsoleColor[] c = new ConsoleColor[colours.Length];
                shift = shift % colours.Length;

                for (int i = 0; i < c.Length; i++)
                {
                    int shiftI = i + shift;
                    //this line of code is worse than a normal if statment, oh well... (less lines is better right?!?!?!)
                    shiftI += shiftI >= c.Length ? -c.Length : shiftI < 0 ? c.Length : 0;
                    c[shiftI] = colours[i];
                }

                return c;
            }
        }
    }
}