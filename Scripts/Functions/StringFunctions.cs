using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Revistone.Console.Data;

namespace Revistone.Functions;

/// <summary> Class filled with functions unrelated to console, but useful for string manipulation. </summary>
public static class StringFunctions
{
    /// <summary> Takes array of type T and returns array as a string in format [element, element...]. </summary>
    public static string ToElementString<T>(this T[] t)
    {
        if (t.Length == 0) return "[]";

        string s = "[";

        foreach (T element in t)
        {
            if (element != null) s += $"{element}, ";
        }

        s = s.Substring(0, s.Length - 2) + "]";

        return s;
    }

    /// <summary> Takes list of type T and returns array as a string in format [element, element...]. </summary>
    public static string ToElementString<T>(this List<T> t) { return ToElementString(t.ToArray()); }

    /// <summary> Remove part of string at given index and length, and replace with replacementString. </summary>
    public static string ReplaceAt(this string str, int index, int length, string replace)
    {
        return str.Remove(index, Math.Min(length, str.Length - index))
                .Insert(index, replace);
    }

    ///<summary> Converts a string to array format, split to fit into the console. </summary>
    public static string[] FitToConsole(this string str)
    {
        List<string> lines = [];
        string currentLine = "";

        foreach (char c in str)
        {
            if (c == '\n')
            {
                if (currentLine.Length != 0) lines.Add(currentLine);
                currentLine = "";
                continue;
            }

            if (currentLine.Length >= ConsoleData.windowSize.width - 1)
            {
                int lastWordIndex = currentLine.LastIndexOf(' ');

                if (lastWordIndex == -1 || lastWordIndex == currentLine.Length - 1)
                {
                    lines.Add(currentLine);
                    currentLine = "";
                }
                else
                {
                    lines.Add(currentLine[..(lastWordIndex + 1)]);
                    currentLine = currentLine[(lastWordIndex + 1)..];
                }
            }

            currentLine += c;
        }
        if (currentLine.Length != 0) lines.Add(currentLine);

        return [.. lines];
    }

    /// <summary> Modifications to the captilisation of a string. </summary>
    public enum CapitalCasing { None, Upper, Lower, FirstLetterUpper }

    /// <summary> Returns a copy of given string modified to the captilisation of given casing. </summary>
    public static string AdjustCapitalisation(this string input, CapitalCasing casing)
    {
        switch (casing)
        {
            case CapitalCasing.Upper:
                return input.ToUpper();
            case CapitalCasing.Lower:
                return input.ToLower();
            case CapitalCasing.FirstLetterUpper: //capitalises first letter of each word
                input = input[0].ToString().ToUpper() + input.Substring(1); //captilises first letter
                for (int i = 1; i < input.Length; i++) //cant use split string as loses size of gaps between words
                {
                    if (input[i - 1] == ' ' && input[i] != ' ')
                    {
                        //edits string to captialise first letter of each word
                        input = input.Substring(0, i) + input[i].ToString().ToUpper() + (i + 1 < input.Length ? input.Substring(i + 1) : "");
                    }
                }
                return input;
        }

        return input; //if using CapitalCasing.None
    }

    /// <summary> Determines if given string matches CapitalCasing preset given. </summary>
    public static bool MatchesCapitalisation(this string input, CapitalCasing casing)
    {
        return input == AdjustCapitalisation(input, casing);
    }

    /// <summary> Splits at string at each capital letter, and inserts a space. </summary>
    public static string SplitAtCapitalisation(this string input)
    {
        return Regex.Replace(input, "([a-z])([A-Z])", "$1 $2");
    }

    /// <summary> Checks if string is in given format, [C4] -> 4 char, [N7] -> 7 digit number, [N:] -> any digit number. Can not use [] in format. </summary>
    public static bool InFormat(this string input, string format)
    {
        // Replace [N4] with \d{4} for numeric checks and [C2] with [A-Za-z]{2} for character checks
        format = Regex.Replace(format, @"\[N(\d+)]", m =>
        {
            if (int.TryParse(m.Groups[1].Value, out int numberOfDigits))
            {
                return @"\d{" + numberOfDigits + "}";
            }
            return m.Value; // Return the original string if parsing fails
        });
        format = Regex.Replace(format, @"\[C(\d+)]", m =>
        {
            if (int.TryParse(m.Groups[1].Value, out int numberOfDigits))
            {
                return @"[A-Za-z]{" + numberOfDigits + "}";
            }
            return m.Value; // Return the original string if parsing fails
        });
        format = Regex.Replace(format, @"\[A(\d+)]", m =>
        {
            if (int.TryParse(m.Groups[1].Value, out int numberOfDigits))
            {
                return @".{" + numberOfDigits + "}";
            }
            return m.Value; // Return the original string if parsing fails
        });

        format = Regex.Replace(format, @"\[N:\]", @"\d+");
        format = Regex.Replace(format, @"\[C:\]", @"[A-Za-z]+");
        format = Regex.Replace(format, @"\[A:\]", ".*");

        return Regex.IsMatch(input, $"^{format}$");
    }

    /// <summary> Returns the standard CharWidth of given char. </summary>
    public static int GetCharWidth(string c)
    {
        int codePoint = char.ConvertToUtf32(c, 0);

        if (codePoint == 0x2060 || // Word Joiner
        codePoint == 0xFEFF || // Zero Width No-Break Space
        (codePoint >= 0xFE00 && codePoint <= 0xFE0F) || // Variation Selectors
        (codePoint >= 0xE0000 && codePoint <= 0xE007F) || // Tags
        (codePoint >= 0x200B && codePoint <= 0x200F) || // ZWSP, ZWNJ, ZWJ, L-t-R Mark, R-t-L Mark
        (codePoint >= 0x2066 && codePoint <= 0x2069) || // Directional Isolates
        (codePoint >= 0xFFF9 && codePoint <= 0xFFFB) || // Interlinear Annotation Characters.
        codePoint == 0x2062 || // Invisible Times
        codePoint == 0x2063) // Invisible Separator
        {
            return 0; // Zero-width character
        }

        // Check for wide characters
        if ((codePoint >= 0x1100 && codePoint <= 0x115F) || // Hangul Jamo
            (codePoint >= 0x2E80 && codePoint <= 0xA4CF) || // CJK Radicals
            (codePoint >= 0xAC00 && codePoint <= 0xD7A3) || // Hangul Syllables
            (codePoint >= 0x1F300 && codePoint <= 0x1F6FF) || // Emojis + Transport & Map Symbols
            (codePoint >= 0x1F900 && codePoint <= 0x1FAFF) || // Supplemental Symbols
            (codePoint >= 0x20000 && codePoint <= 0x2FFFD) || // CJK Ideographs
            (codePoint >= 0x2700 && codePoint <= 0x27BF) ||   // Dingbats
            (codePoint >= 0x3000 && codePoint <= 0x303F) ||   // CJK Punctuation, Spaces
            (codePoint >= 0xFF00 && codePoint <= 0xFF60) ||   // Fullwidth Forms
            (codePoint >= 0x1F000 && codePoint <= 0x1F02F) || // Mahjong & Domino Tiles
            (codePoint >= 0x2300 && codePoint <= 0x23FF)) // Miscellaneous Technical
        {
            return 2; // Wide characters
        }

        return 1; // Narrow characters
    }

    public static bool IsEmoji(string s)
    {
        return Regex.Match(s, emojiRegexPattern).Success;
    }

    /// <summary> Returns the standard CharWidth of given char. </summary>
    public static int GetCharWidth(char c) { return GetCharWidth(c.ToString()); }

    // --- CLIPBOARD LOGIC --- (LOW LEVEL STUFF YAY, im NOT importing window forms)

    [DllImport("user32.dll")] static extern bool OpenClipboard(IntPtr clipboardOwner);
    [DllImport("user32.dll")] static extern bool CloseClipboard();
    [DllImport("user32.dll")] static extern IntPtr SetClipboardData(uint format, IntPtr memoryHandle);
    [DllImport("user32.dll")] static extern bool EmptyClipboard();
    [DllImport("user32.dll")] static extern IntPtr GetClipboardData(uint format);
    [DllImport("user32.dll")] static extern bool IsClipboardFormatAvailable(uint format);

    [DllImport("kernel32.dll")] static extern IntPtr GlobalAlloc(uint flags, UIntPtr numOfBytes);
    [DllImport("kernel32.dll")] static extern IntPtr GlobalLock(IntPtr memoryHandle);
    [DllImport("kernel32.dll")] static extern bool GlobalUnlock(IntPtr memoryHandle);
    [DllImport("kernel32.dll", SetLastError = true)] static extern UIntPtr GlobalSize(IntPtr memoryHandle);

    const int UNICODEFORMAT = 13;
    const int ANSIFORMAT = 1;

    /// <summary> Attempts to copy given text to system clipboard. </summary>
    public static bool CopyToClipboard(string text)
    {
        if (!OpenClipboard(IntPtr.Zero)) return false;

        try
        {
            EmptyClipboard();

            // assign memory in the heap for the text, 0x0002 -> memory aloc flag for moveable memory
            IntPtr memoryHandle = GlobalAlloc(0x0002, (UIntPtr)((text.Length + 1) * Marshal.SystemDefaultCharSize));
            if (memoryHandle == IntPtr.Zero) return false; // alloc failed

            // assign pointer to newly created memory
            IntPtr memoryPointer = GlobalLock(memoryHandle);
            if (memoryPointer == IntPtr.Zero) return false;

            Marshal.Copy(text.ToCharArray(), 0, memoryPointer, text.Length); //copy text to memory
            Marshal.WriteByte(memoryPointer, text.Length * Marshal.SystemDefaultCharSize, 0); // mark memory end
            GlobalUnlock(memoryHandle); // unlock the memory

            if (SetClipboardData(UNICODEFORMAT, memoryHandle) == IntPtr.Zero) return false;
        }
        finally
        {
            CloseClipboard();
        }

        return true;
    }

    /// <summary> Attempts to get text from system clipboard. </summary>
    public static string GetClipboardText()
    {
        if (!OpenClipboard(IntPtr.Zero)) return "";

        try
        {
            if (IsClipboardFormatAvailable(UNICODEFORMAT)) // in unicode format
            {
                // get memory from the heap
                IntPtr memoryHandle = GetClipboardData(UNICODEFORMAT);
                if (memoryHandle == IntPtr.Zero) return "";

                // create pointer to memory from the heap
                IntPtr memoryPointer = GlobalLock(memoryHandle);
                if (memoryPointer == IntPtr.Zero) return "";

                try
                {
                    // Get the size of the clipboard text data
                    UIntPtr size = GlobalSize(memoryHandle);
                    byte[] buffer = new byte[size.ToUInt32()];

                    // Copy the clipboard text data into the buffer and return
                    Marshal.Copy(memoryPointer, buffer, 0, (int)size);
                    return System.Text.Encoding.Unicode.GetString(buffer).TrimEnd('\0');
                }
                finally
                {
                    GlobalUnlock(memoryHandle);
                }
            }
            else if (IsClipboardFormatAvailable(ANSIFORMAT)) // in ANSI format
            {
                // get memory from the heap
                IntPtr memoryHandle = GetClipboardData(ANSIFORMAT);
                if (memoryHandle == IntPtr.Zero) return "";

                // create pointer to memory from the heap
                IntPtr memoryPointer = GlobalLock(memoryHandle);
                if (memoryPointer == IntPtr.Zero) return "";

                try
                {
                    // Get the size of the clipboard text data
                    UIntPtr size = GlobalSize(memoryHandle);
                    byte[] buffer = new byte[size.ToUInt32()];

                    // Copy the clipboard text data into the buffer and return
                    Marshal.Copy(memoryPointer, buffer, 0, (int)size);
                    return System.Text.Encoding.ASCII.GetString(buffer).TrimEnd('\0');
                }
                finally
                {
                    GlobalUnlock(memoryHandle);
                }
            }

            // No supported text format found in the clipboard
            return "";
        }
        finally
        {
            // Always close the clipboard
            CloseClipboard();
        }
    }

    // DATA

    static string emojiRegexPattern = @"[#*0-9]\uFE0F?\u20E3|\u00A9\uFE0F?|[\u00AE\u203C\u2049\u2122\u2139\u2194-\u2199\u21A9\u21AA]\uFE0F?|[\u231A\u231B]|[\u2328\u23CF]\uFE0F?|[\u23E9-\u23EC]|[\u23ED-\u23EF]\uFE0F?|\u23F0|[\u23F1\u23F2]\uFE0F?|\u23F3|[\u23F8-\u23FA\u24C2\u25AA\u25AB\u25B6\u25C0\u25FB\u25FC]\uFE0F?|[\u25FD\u25FE]|[\u2600-\u2604\u260E\u2611]\uFE0F?|[\u2614\u2615]|\u2618\uFE0F?|\u261D(?:\uD83C[\uDFFB-\uDFFF]|\uFE0F)?|[\u2620\u2622\u2623\u2626\u262A\u262E\u262F\u2638-\u263A\u2640\u2642]\uFE0F?|[\u2648-\u2653]|[\u265F\u2660\u2663\u2665\u2666\u2668\u267B\u267E]\uFE0F?|\u267F|\u2692\uFE0F?|\u2693|[\u2694-\u2697\u2699\u269B\u269C\u26A0]\uFE0F?|\u26A1|\u26A7\uFE0F?|[\u26AA\u26AB]|[\u26B0\u26B1]\uFE0F?|[\u26BD\u26BE\u26C4\u26C5]|\u26C8\uFE0F?|\u26CE|[\u26CF\u26D1]\uFE0F?|\u26D3(?:\u200D\uD83D\uDCA5|\uFE0F(?:\u200D\uD83D\uDCA5)?)?|\u26D4|\u26E9\uFE0F?|\u26EA|[\u26F0\u26F1]\uFE0F?|[\u26F2\u26F3]|\u26F4\uFE0F?|\u26F5|[\u26F7\u26F8]\uFE0F?|\u26F9(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?|\uFE0F(?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\u26FA\u26FD]|\u2702\uFE0F?|\u2705|[\u2708\u2709]\uFE0F?|[\u270A\u270B](?:\uD83C[\uDFFB-\uDFFF])?|[\u270C\u270D](?:\uD83C[\uDFFB-\uDFFF]|\uFE0F)?|\u270F\uFE0F?|[\u2712\u2714\u2716\u271D\u2721]\uFE0F?|\u2728|[\u2733\u2734\u2744\u2747]\uFE0F?|[\u274C\u274E\u2753-\u2755\u2757]|\u2763\uFE0F?|\u2764(?:\u200D(?:\uD83D\uDD25|\uD83E\uDE79)|\uFE0F(?:\u200D(?:\uD83D\uDD25|\uD83E\uDE79))?)?|[\u2795-\u2797]|\u27A1\uFE0F?|[\u27B0\u27BF]|[\u2934\u2935\u2B05-\u2B07]\uFE0F?|[\u2B1B\u2B1C\u2B50\u2B55]|[\u3030\u303D\u3297\u3299]\uFE0F?|\uD83C(?:[\uDC04\uDCCF]|[\uDD70\uDD71\uDD7E\uDD7F]\uFE0F?|[\uDD8E\uDD91-\uDD9A]|\uDDE6\uD83C[\uDDE8-\uDDEC\uDDEE\uDDF1\uDDF2\uDDF4\uDDF6-\uDDFA\uDDFC\uDDFD\uDDFF]|\uDDE7\uD83C[\uDDE6\uDDE7\uDDE9-\uDDEF\uDDF1-\uDDF4\uDDF6-\uDDF9\uDDFB\uDDFC\uDDFE\uDDFF]|\uDDE8\uD83C[\uDDE6\uDDE8\uDDE9\uDDEB-\uDDEE\uDDF0-\uDDF7\uDDFA-\uDDFF]|\uDDE9\uD83C[\uDDEA\uDDEC\uDDEF\uDDF0\uDDF2\uDDF4\uDDFF]|\uDDEA\uD83C[\uDDE6\uDDE8\uDDEA\uDDEC\uDDED\uDDF7-\uDDFA]|\uDDEB\uD83C[\uDDEE-\uDDF0\uDDF2\uDDF4\uDDF7]|\uDDEC\uD83C[\uDDE6\uDDE7\uDDE9-\uDDEE\uDDF1-\uDDF3\uDDF5-\uDDFA\uDDFC\uDDFE]|\uDDED\uD83C[\uDDF0\uDDF2\uDDF3\uDDF7\uDDF9\uDDFA]|\uDDEE\uD83C[\uDDE8-\uDDEA\uDDF1-\uDDF4\uDDF6-\uDDF9]|\uDDEF\uD83C[\uDDEA\uDDF2\uDDF4\uDDF5]|\uDDF0\uD83C[\uDDEA\uDDEC-\uDDEE\uDDF2\uDDF3\uDDF5\uDDF7\uDDFC\uDDFE\uDDFF]|\uDDF1\uD83C[\uDDE6-\uDDE8\uDDEE\uDDF0\uDDF7-\uDDFB\uDDFE]|\uDDF2\uD83C[\uDDE6\uDDE8-\uDDED\uDDF0-\uDDFF]|\uDDF3\uD83C[\uDDE6\uDDE8\uDDEA-\uDDEC\uDDEE\uDDF1\uDDF4\uDDF5\uDDF7\uDDFA\uDDFF]|\uDDF4\uD83C\uDDF2|\uDDF5\uD83C[\uDDE6\uDDEA-\uDDED\uDDF0-\uDDF3\uDDF7-\uDDF9\uDDFC\uDDFE]|\uDDF6\uD83C\uDDE6|\uDDF7\uD83C[\uDDEA\uDDF4\uDDF8\uDDFA\uDDFC]|\uDDF8\uD83C[\uDDE6-\uDDEA\uDDEC-\uDDF4\uDDF7-\uDDF9\uDDFB\uDDFD-\uDDFF]|\uDDF9\uD83C[\uDDE6\uDDE8\uDDE9\uDDEB-\uDDED\uDDEF-\uDDF4\uDDF7\uDDF9\uDDFB\uDDFC\uDDFF]|\uDDFA\uD83C[\uDDE6\uDDEC\uDDF2\uDDF3\uDDF8\uDDFE\uDDFF]|\uDDFB\uD83C[\uDDE6\uDDE8\uDDEA\uDDEC\uDDEE\uDDF3\uDDFA]|\uDDFC\uD83C[\uDDEB\uDDF8]|\uDDFD\uD83C\uDDF0|\uDDFE\uD83C[\uDDEA\uDDF9]|\uDDFF\uD83C[\uDDE6\uDDF2\uDDFC]|\uDE01|\uDE02\uFE0F?|[\uDE1A\uDE2F\uDE32-\uDE36]|\uDE37\uFE0F?|[\uDE38-\uDE3A\uDE50\uDE51\uDF00-\uDF20]|[\uDF21\uDF24-\uDF2C]\uFE0F?|[\uDF2D-\uDF35]|\uDF36\uFE0F?|[\uDF37-\uDF43]|\uDF44(?:\u200D\uD83D\uDFEB)?|[\uDF45-\uDF4A]|\uDF4B(?:\u200D\uD83D\uDFE9)?|[\uDF4C-\uDF7C]|\uDF7D\uFE0F?|[\uDF7E-\uDF84]|\uDF85(?:\uD83C[\uDFFB-\uDFFF])?|[\uDF86-\uDF93]|[\uDF96\uDF97\uDF99-\uDF9B\uDF9E\uDF9F]\uFE0F?|[\uDFA0-\uDFC1]|\uDFC2(?:\uD83C[\uDFFB-\uDFFF])?|\uDFC3(?:\u200D(?:[\u2640\u2642](?:\u200D\u27A1\uFE0F?|\uFE0F(?:\u200D\u27A1\uFE0F?)?)?|\u27A1\uFE0F?)|\uD83C[\uDFFB-\uDFFF](?:\u200D(?:[\u2640\u2642](?:\u200D\u27A1\uFE0F?|\uFE0F(?:\u200D\u27A1\uFE0F?)?)?|\u27A1\uFE0F?))?)?|\uDFC4(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDFC5\uDFC6]|\uDFC7(?:\uD83C[\uDFFB-\uDFFF])?|[\uDFC8\uDFC9]|\uDFCA(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDFCB\uDFCC](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?|\uFE0F(?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDFCD\uDFCE]\uFE0F?|[\uDFCF-\uDFD3]|[\uDFD4-\uDFDF]\uFE0F?|[\uDFE0-\uDFF0]|\uDFF3(?:\u200D(?:\u26A7\uFE0F?|\uD83C\uDF08)|\uFE0F(?:\u200D(?:\u26A7\uFE0F?|\uD83C\uDF08))?)?|\uDFF4(?:\u200D\u2620\uFE0F?|\uDB40\uDC67\uDB40\uDC62\uDB40(?:\uDC65\uDB40\uDC6E\uDB40\uDC67|\uDC73\uDB40\uDC63\uDB40\uDC74|\uDC77\uDB40\uDC6C\uDB40\uDC73)\uDB40\uDC7F)?|[\uDFF5\uDFF7]\uFE0F?|[\uDFF8-\uDFFF])|\uD83D(?:[\uDC00-\uDC07]|\uDC08(?:\u200D\u2B1B)?|[\uDC09-\uDC14]|\uDC15(?:\u200D\uD83E\uDDBA)?|[\uDC16-\uDC25]|\uDC26(?:\u200D(?:\u2B1B|\uD83D\uDD25))?|[\uDC27-\uDC3A]|\uDC3B(?:\u200D\u2744\uFE0F?)?|[\uDC3C-\uDC3E]|\uDC3F\uFE0F?|\uDC40|\uDC41(?:\u200D\uD83D\uDDE8\uFE0F?|\uFE0F(?:\u200D\uD83D\uDDE8\uFE0F?)?)?|[\uDC42\uDC43](?:\uD83C[\uDFFB-\uDFFF])?|[\uDC44\uDC45]|[\uDC46-\uDC50](?:\uD83C[\uDFFB-\uDFFF])?|[\uDC51-\uDC65]|[\uDC66\uDC67](?:\uD83C[\uDFFB-\uDFFF])?|\uDC68(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:\uDC8B\u200D\uD83D)?\uDC68|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D(?:\uDC66(?:\u200D\uD83D\uDC66)?|\uDC67(?:\u200D\uD83D[\uDC66\uDC67])?|[\uDC68\uDC69]\u200D\uD83D(?:\uDC66(?:\u200D\uD83D\uDC66)?|\uDC67(?:\u200D\uD83D[\uDC66\uDC67])?)|[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92])|\uD83E(?:\uDDAF(?:\u200D\u27A1\uFE0F?)?|[\uDDB0-\uDDB3]|[\uDDBC\uDDBD](?:\u200D\u27A1\uFE0F?)?))|\uD83C(?:\uDFFB(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:\uDC8B\u200D\uD83D)?\uDC68\uD83C[\uDFFB-\uDFFF]|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83D\uDC68\uD83C[\uDFFC-\uDFFF]|\uDDAF(?:\u200D\u27A1\uFE0F?)?|[\uDDB0-\uDDB3]|[\uDDBC\uDDBD](?:\u200D\u27A1\uFE0F?)?)))?|\uDFFC(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:\uDC8B\u200D\uD83D)?\uDC68\uD83C[\uDFFB-\uDFFF]|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83D\uDC68\uD83C[\uDFFB\uDFFD-\uDFFF]|\uDDAF(?:\u200D\u27A1\uFE0F?)?|[\uDDB0-\uDDB3]|[\uDDBC\uDDBD](?:\u200D\u27A1\uFE0F?)?)))?|\uDFFD(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:\uDC8B\u200D\uD83D)?\uDC68\uD83C[\uDFFB-\uDFFF]|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83D\uDC68\uD83C[\uDFFB\uDFFC\uDFFE\uDFFF]|\uDDAF(?:\u200D\u27A1\uFE0F?)?|[\uDDB0-\uDDB3]|[\uDDBC\uDDBD](?:\u200D\u27A1\uFE0F?)?)))?|\uDFFE(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:\uDC8B\u200D\uD83D)?\uDC68\uD83C[\uDFFB-\uDFFF]|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83D\uDC68\uD83C[\uDFFB-\uDFFD\uDFFF]|\uDDAF(?:\u200D\u27A1\uFE0F?)?|[\uDDB0-\uDDB3]|[\uDDBC\uDDBD](?:\u200D\u27A1\uFE0F?)?)))?|\uDFFF(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:\uDC8B\u200D\uD83D)?\uDC68\uD83C[\uDFFB-\uDFFF]|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83D\uDC68\uD83C[\uDFFB-\uDFFE]|\uDDAF(?:\u200D\u27A1\uFE0F?)?|[\uDDB0-\uDDB3]|[\uDDBC\uDDBD](?:\u200D\u27A1\uFE0F?)?)))?))?|\uDC69(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:\uDC8B\u200D\uD83D)?[\uDC68\uDC69]|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D(?:\uDC66(?:\u200D\uD83D\uDC66)?|\uDC67(?:\u200D\uD83D[\uDC66\uDC67])?|\uDC69\u200D\uD83D(?:\uDC66(?:\u200D\uD83D\uDC66)?|\uDC67(?:\u200D\uD83D[\uDC66\uDC67])?)|[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92])|\uD83E(?:\uDDAF(?:\u200D\u27A1\uFE0F?)?|[\uDDB0-\uDDB3]|[\uDDBC\uDDBD](?:\u200D\u27A1\uFE0F?)?))|\uD83C(?:\uDFFB(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFF]|\uDC8B\u200D\uD83D[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFF])|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83D[\uDC68\uDC69]\uD83C[\uDFFC-\uDFFF]|\uDDAF(?:\u200D\u27A1\uFE0F?)?|[\uDDB0-\uDDB3]|[\uDDBC\uDDBD](?:\u200D\u27A1\uFE0F?)?)))?|\uDFFC(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFF]|\uDC8B\u200D\uD83D[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFF])|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83D[\uDC68\uDC69]\uD83C[\uDFFB\uDFFD-\uDFFF]|\uDDAF(?:\u200D\u27A1\uFE0F?)?|[\uDDB0-\uDDB3]|[\uDDBC\uDDBD](?:\u200D\u27A1\uFE0F?)?)))?|\uDFFD(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFF]|\uDC8B\u200D\uD83D[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFF])|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83D[\uDC68\uDC69]\uD83C[\uDFFB\uDFFC\uDFFE\uDFFF]|\uDDAF(?:\u200D\u27A1\uFE0F?)?|[\uDDB0-\uDDB3]|[\uDDBC\uDDBD](?:\u200D\u27A1\uFE0F?)?)))?|\uDFFE(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFF]|\uDC8B\u200D\uD83D[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFF])|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83D[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFD\uDFFF]|\uDDAF(?:\u200D\u27A1\uFE0F?)?|[\uDDB0-\uDDB3]|[\uDDBC\uDDBD](?:\u200D\u27A1\uFE0F?)?)))?|\uDFFF(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFF]|\uDC8B\u200D\uD83D[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFF])|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83D[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFE]|\uDDAF(?:\u200D\u27A1\uFE0F?)?|[\uDDB0-\uDDB3]|[\uDDBC\uDDBD](?:\u200D\u27A1\uFE0F?)?)))?))?|\uDC6A|[\uDC6B-\uDC6D](?:\uD83C[\uDFFB-\uDFFF])?|\uDC6E(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDC6F(?:\u200D[\u2640\u2642]\uFE0F?)?|[\uDC70\uDC71](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDC72(?:\uD83C[\uDFFB-\uDFFF])?|\uDC73(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDC74-\uDC76](?:\uD83C[\uDFFB-\uDFFF])?|\uDC77(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDC78(?:\uD83C[\uDFFB-\uDFFF])?|[\uDC79-\uDC7B]|\uDC7C(?:\uD83C[\uDFFB-\uDFFF])?|[\uDC7D-\uDC80]|[\uDC81\uDC82](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDC83(?:\uD83C[\uDFFB-\uDFFF])?|\uDC84|\uDC85(?:\uD83C[\uDFFB-\uDFFF])?|[\uDC86\uDC87](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDC88-\uDC8E]|\uDC8F(?:\uD83C[\uDFFB-\uDFFF])?|\uDC90|\uDC91(?:\uD83C[\uDFFB-\uDFFF])?|[\uDC92-\uDCA9]|\uDCAA(?:\uD83C[\uDFFB-\uDFFF])?|[\uDCAB-\uDCFC]|\uDCFD\uFE0F?|[\uDCFF-\uDD3D]|[\uDD49\uDD4A]\uFE0F?|[\uDD4B-\uDD4E\uDD50-\uDD67]|[\uDD6F\uDD70\uDD73]\uFE0F?|\uDD74(?:\uD83C[\uDFFB-\uDFFF]|\uFE0F)?|\uDD75(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?|\uFE0F(?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDD76-\uDD79]\uFE0F?|\uDD7A(?:\uD83C[\uDFFB-\uDFFF])?|[\uDD87\uDD8A-\uDD8D]\uFE0F?|\uDD90(?:\uD83C[\uDFFB-\uDFFF]|\uFE0F)?|[\uDD95\uDD96](?:\uD83C[\uDFFB-\uDFFF])?|\uDDA4|[\uDDA5\uDDA8\uDDB1\uDDB2\uDDBC\uDDC2-\uDDC4\uDDD1-\uDDD3\uDDDC-\uDDDE\uDDE1\uDDE3\uDDE8\uDDEF\uDDF3\uDDFA]\uFE0F?|[\uDDFB-\uDE2D]|\uDE2E(?:\u200D\uD83D\uDCA8)?|[\uDE2F-\uDE34]|\uDE35(?:\u200D\uD83D\uDCAB)?|\uDE36(?:\u200D\uD83C\uDF2B\uFE0F?)?|[\uDE37-\uDE41]|\uDE42(?:\u200D[\u2194\u2195]\uFE0F?)?|[\uDE43\uDE44]|[\uDE45-\uDE47](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDE48-\uDE4A]|\uDE4B(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDE4C(?:\uD83C[\uDFFB-\uDFFF])?|[\uDE4D\uDE4E](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDE4F(?:\uD83C[\uDFFB-\uDFFF])?|[\uDE80-\uDEA2]|\uDEA3(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDEA4-\uDEB3]|[\uDEB4\uDEB5](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDEB6(?:\u200D(?:[\u2640\u2642](?:\u200D\u27A1\uFE0F?|\uFE0F(?:\u200D\u27A1\uFE0F?)?)?|\u27A1\uFE0F?)|\uD83C[\uDFFB-\uDFFF](?:\u200D(?:[\u2640\u2642](?:\u200D\u27A1\uFE0F?|\uFE0F(?:\u200D\u27A1\uFE0F?)?)?|\u27A1\uFE0F?))?)?|[\uDEB7-\uDEBF]|\uDEC0(?:\uD83C[\uDFFB-\uDFFF])?|[\uDEC1-\uDEC5]|\uDECB\uFE0F?|\uDECC(?:\uD83C[\uDFFB-\uDFFF])?|[\uDECD-\uDECF]\uFE0F?|[\uDED0-\uDED2\uDED5-\uDED7\uDEDC-\uDEDF]|[\uDEE0-\uDEE5\uDEE9]\uFE0F?|[\uDEEB\uDEEC]|[\uDEF0\uDEF3]\uFE0F?|[\uDEF4-\uDEFC\uDFE0-\uDFEB\uDFF0])|\uD83E(?:\uDD0C(?:\uD83C[\uDFFB-\uDFFF])?|[\uDD0D\uDD0E]|\uDD0F(?:\uD83C[\uDFFB-\uDFFF])?|[\uDD10-\uDD17]|[\uDD18-\uDD1F](?:\uD83C[\uDFFB-\uDFFF])?|[\uDD20-\uDD25]|\uDD26(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDD27-\uDD2F]|[\uDD30-\uDD34](?:\uD83C[\uDFFB-\uDFFF])?|\uDD35(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDD36(?:\uD83C[\uDFFB-\uDFFF])?|[\uDD37-\uDD39](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDD3A|\uDD3C(?:\u200D[\u2640\u2642]\uFE0F?)?|[\uDD3D\uDD3E](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDD3F-\uDD45\uDD47-\uDD76]|\uDD77(?:\uD83C[\uDFFB-\uDFFF])?|[\uDD78-\uDDB4]|[\uDDB5\uDDB6](?:\uD83C[\uDFFB-\uDFFF])?|\uDDB7|[\uDDB8\uDDB9](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDDBA|\uDDBB(?:\uD83C[\uDFFB-\uDFFF])?|[\uDDBC-\uDDCC]|\uDDCD(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDDCE(?:\u200D(?:[\u2640\u2642](?:\u200D\u27A1\uFE0F?|\uFE0F(?:\u200D\u27A1\uFE0F?)?)?|\u27A1\uFE0F?)|\uD83C[\uDFFB-\uDFFF](?:\u200D(?:[\u2640\u2642](?:\u200D\u27A1\uFE0F?|\uFE0F(?:\u200D\u27A1\uFE0F?)?)?|\u27A1\uFE0F?))?)?|\uDDCF(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDDD0|\uDDD1(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\uD83C[\uDF3E\uDF73\uDF7C\uDF84\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83E\uDDD1|\uDDAF(?:\u200D\u27A1\uFE0F?)?|[\uDDB0-\uDDB3]|[\uDDBC\uDDBD](?:\u200D\u27A1\uFE0F?)?|(?:\uDDD1\u200D\uD83E)?\uDDD2(?:\u200D\uD83E\uDDD2)?))|\uD83C(?:\uDFFB(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D(?:\uD83D\uDC8B\u200D)?\uD83E\uDDD1\uD83C[\uDFFC-\uDFFF]|\uD83C[\uDF3E\uDF73\uDF7C\uDF84\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83E\uDDD1\uD83C[\uDFFB-\uDFFF]|\uDDAF(?:\u200D\u27A1\uFE0F?)?|[\uDDB0-\uDDB3]|[\uDDBC\uDDBD](?:\u200D\u27A1\uFE0F?)?)))?|\uDFFC(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D(?:\uD83D\uDC8B\u200D)?\uD83E\uDDD1\uD83C[\uDFFB\uDFFD-\uDFFF]|\uD83C[\uDF3E\uDF73\uDF7C\uDF84\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83E\uDDD1\uD83C[\uDFFB-\uDFFF]|\uDDAF(?:\u200D\u27A1\uFE0F?)?|[\uDDB0-\uDDB3]|[\uDDBC\uDDBD](?:\u200D\u27A1\uFE0F?)?)))?|\uDFFD(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D(?:\uD83D\uDC8B\u200D)?\uD83E\uDDD1\uD83C[\uDFFB\uDFFC\uDFFE\uDFFF]|\uD83C[\uDF3E\uDF73\uDF7C\uDF84\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83E\uDDD1\uD83C[\uDFFB-\uDFFF]|\uDDAF(?:\u200D\u27A1\uFE0F?)?|[\uDDB0-\uDDB3]|[\uDDBC\uDDBD](?:\u200D\u27A1\uFE0F?)?)))?|\uDFFE(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D(?:\uD83D\uDC8B\u200D)?\uD83E\uDDD1\uD83C[\uDFFB-\uDFFD\uDFFF]|\uD83C[\uDF3E\uDF73\uDF7C\uDF84\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83E\uDDD1\uD83C[\uDFFB-\uDFFF]|\uDDAF(?:\u200D\u27A1\uFE0F?)?|[\uDDB0-\uDDB3]|[\uDDBC\uDDBD](?:\u200D\u27A1\uFE0F?)?)))?|\uDFFF(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D(?:\uD83D\uDC8B\u200D)?\uD83E\uDDD1\uD83C[\uDFFB-\uDFFE]|\uD83C[\uDF3E\uDF73\uDF7C\uDF84\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83E\uDDD1\uD83C[\uDFFB-\uDFFF]|\uDDAF(?:\u200D\u27A1\uFE0F?)?|[\uDDB0-\uDDB3]|[\uDDBC\uDDBD](?:\u200D\u27A1\uFE0F?)?)))?))?|[\uDDD2\uDDD3](?:\uD83C[\uDFFB-\uDFFF])?|\uDDD4(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDDD5(?:\uD83C[\uDFFB-\uDFFF])?|[\uDDD6-\uDDDD](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDDDE\uDDDF](?:\u200D[\u2640\u2642]\uFE0F?)?|[\uDDE0-\uDDFF\uDE70-\uDE7C\uDE80-\uDE89\uDE8F-\uDEC2]|[\uDEC3-\uDEC5](?:\uD83C[\uDFFB-\uDFFF])?|[\uDEC6\uDECE-\uDEDC\uDEDF-\uDEE9]|\uDEF0(?:\uD83C[\uDFFB-\uDFFF])?|\uDEF1(?:\uD83C(?:\uDFFB(?:\u200D\uD83E\uDEF2\uD83C[\uDFFC-\uDFFF])?|\uDFFC(?:\u200D\uD83E\uDEF2\uD83C[\uDFFB\uDFFD-\uDFFF])?|\uDFFD(?:\u200D\uD83E\uDEF2\uD83C[\uDFFB\uDFFC\uDFFE\uDFFF])?|\uDFFE(?:\u200D\uD83E\uDEF2\uD83C[\uDFFB-\uDFFD\uDFFF])?|\uDFFF(?:\u200D\uD83E\uDEF2\uD83C[\uDFFB-\uDFFE])?))?|[\uDEF2-\uDEF8](?:\uD83C[\uDFFB-\uDFFF])?)";
}
