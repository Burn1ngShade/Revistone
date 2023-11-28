using Revistone.Apps;
using Revistone.Console;

namespace Revistone
{
    namespace Functions
    {
        /// <summary> Class filled with functions to help create special text. </summary>
        public static class TitleFunctions
        {
            /// <summary> Ascii Font Of Text. </summary>
            public enum AsciiFont { Standard, Big, BigMoneyNW, Graceful, Graffiti, Slant, SmallSlant, Small, AnsiRegular, Block, Colossal, Digital, Double, Merlin1, Blocks, DancingFont }

            static string validChar = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789,.!?';:()[]{}-_+=*&^<>%@£";

            /// <summary> Creates an array of strings that forms a title of given text, and given font. </summary>
            public static string[] CreateTitle(string text, AsciiFont font, int emptySpacing = 3, int letterSpacing = 0, bool removeEmptyLines = true)
            {
                string path = FontFilePath(font);

                if (!AppPersistentData.FileExists(path)) return new string[] { text };

                //textHeight is height of given font, spaceLength is the given width of spaces in the font, frontTrim is the amount that should be trimmed from the front of the font
                string[] fontMetaData = AppPersistentData.LoadFile(path, 0, 2);
                int textHeight = int.Parse(fontMetaData[0]), frontTrim = int.Parse(fontMetaData[1]);
                string[] title = new string[textHeight];

                for (int i = 0; i < text.Length; i++)
                {
                    string[] letter = new string[textHeight];
                    if (validChar.Contains(text[i]))
                    {
                        letter = AppPersistentData.LoadFile(path, 2 + validChar.IndexOf(text[i]) * textHeight, textHeight);
                        letter = letter.Select(str => string.IsNullOrWhiteSpace(str) ? new string(' ', 255) : str).ToArray(); //removes human errors
                        letter = letter.Select(s => s.Length > letter.Max(c => c.TrimEnd().Length) ? s.Substring(frontTrim, letter.Max(c => c.TrimEnd().Length - frontTrim)) + new string(' ', letterSpacing) : s.Substring(frontTrim, s.Length - frontTrim) + new string(' ', letterSpacing)).ToArray();
                    }
                    else letter = Enumerable.Repeat(new string(' ', emptySpacing), textHeight).ToArray();

                    title = title.Select((s, index) => s + letter[index]).ToArray();
                }

                if (removeEmptyLines) title = title.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                return title;
            }

            /// <summary> Creates an array of consolecolour that forms a title of given text, colour, and given font. </summary>
            public static ConsoleLine[] CreateTitle(string text, ConsoleColor[] colours, AsciiFont font, int emptySpacing = 3, int letterSpacing = 0, bool removeEmptyLines = true)
            {
                return CreateTitle(text, font, emptySpacing, letterSpacing, removeEmptyLines).Select(s => new ConsoleLine(s, colours)).ToArray();
            }

            public static string FontFilePath(AsciiFont font) { return $"TitleFont/{font}.txt"; }
        }
    }
}