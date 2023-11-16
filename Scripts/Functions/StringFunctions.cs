using System.Collections;
using Colorful;
using Revistone.Console;
using System.Text.RegularExpressions;

namespace Revistone
{
    namespace Functions
    {
        /// <summary>
        /// Class filled with functions unrelated to console, but useful for development.
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
                    if (element != null) s += $"{element.ToString()}, ";
                }

                s = s.Substring(0, s.Length - 2) + "]";

                return s;
            }

            /// <summary> Takes list of type T and returns array as a string in format [element, element...]. </summary>
            public static string ToElementString<T>(this List<T> t) { return ToElementString(t.ToArray()); }

            /// <summary> Modifications to the captilisation of a string. </summary>
            public enum CapitalCasing { None, Upper, Lower, FirstLetterUpper }

            /// <summary> Returns a copy of given string modified to the captilisation of given casing. </summary>
            public static string AdjustCapitalisation(string input, CapitalCasing casing)
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
            public static bool MatchesCapitalisation(string input, CapitalCasing casing)
            {
                return input == AdjustCapitalisation(input, casing);
            }

            /// <summary> Checks if string is in given format, [C4] -> 4 char, [N7] -> 7 digit number, [N:] -> any digit number. Can not use [] in format. </summary>
            public static bool Formatted(string input, string format)
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
        }
    }
}