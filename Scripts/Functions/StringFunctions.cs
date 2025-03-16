using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Revistone.Functions;

/// <summary>
/// Class filled with functions unrelated to console, but useful for string manipulation.
/// </summary>
public static class StringFunctions
{
    /// <summary>
    /// Takes array of type T and returns array as a string in format [element, element...].
    /// </summary>
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
        (codePoint >= 0x1F300 && codePoint <= 0x1F64F) || // Emojis
        (codePoint >= 0x1F900 && codePoint <= 0x1FAFF) || // Supplemental Symbols
        (codePoint >= 0x20000 && codePoint <= 0x2FFFD) || // CJK Ideographs
        (codePoint >= 0x2700 && codePoint <= 0x27BF))    // Dingbats
        {
            return 2; // Wide characters
        }

        return 1; // Narrow characters
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
}
