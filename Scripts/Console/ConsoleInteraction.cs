using static Revistone.Console.ConsoleLine;
using static Revistone.Console.ConsoleAction;
using Revistone.Functions;

namespace Revistone
{
    namespace Console
    {
        public static class ConsoleInteraction
        {
            public static string GetValidUserInput(ConsoleLine promt, InputType validType, int validLength = -1, int wordCount = -1, bool removeWhitespace = false)
            {
                return GetValidUserInput(promt, new InputType[] { validType }, validLength, wordCount, removeWhitespace);
            }

            public static string GetValidUserInput(ConsoleLine promt, InputType[] validType, int validLength = -1, int wordCount = -1, bool removeWhitespace = false)
            {
                string input = "";

                int startIndex = GetConsoleLine();

                while (true)
                {
                    input = GetUserInput(promt, true, false);

                    /*The exsitent of these next 2 lines means something is inherently wrong with the console system
                    but i have no idea what causes this place specific behaviour and this the only occuronce of it so
                    for now this quicj fix will have to do (THIS IS NOT GOOD PROGRAMMING :(  ))*/
                    if (startIndex == 1) GoToLine(1);
                    else GoNextLine();

                    if (InputValid(input, validType, validLength, wordCount, removeWhitespace, true)) break;

                    GoNextLine();
                    WaitForUserInput(ConsoleKey.Enter, true);

                    for (int i = GetConsoleLine() - 3; i < GetConsoleLine(); i++)
                    {
                        UpdateConsoleLine(new ConsoleLine(""), i);
                    }
                    GoNextLine(-3);
                }

                return input;
            }

            public static string GetUserInput(ConsoleLine promt, bool clear = false, bool goNextLine = false)
            {
                if (promt.lineText != "") SendConsoleMessage(new ConsoleLine(promt.lineText, promt.lineColour), new ConsoleLineUpdate());

                SendConsoleMessage(new ConsoleLine("> "), ConsoleLineUpdate.SameLine);
                string userInput = "";
                while (true)
                {
                    ConsoleKeyInfo c = System.Console.ReadKey(true);

                    if (c.Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                    else if (c.Key == ConsoleKey.Backspace)
                    {
                        if (userInput.Length > 0) userInput = userInput.Substring(0, userInput.Length - 1);
                        SendConsoleMessage(new ConsoleLine($"> {userInput}"), ConsoleLineUpdate.SameLine);
                    }
                    else
                    {
                        userInput += c.KeyChar;
                        SendConsoleMessage(new ConsoleLine($"> {userInput}"), ConsoleLineUpdate.SameLine);
                    }
                }

                if (clear)
                {
                    UpdateConsoleLine(new ConsoleLine(""), GetConsoleLine());
                    GoNextLine(-1);
                    if (promt.lineText != "")
                    {
                        UpdateConsoleLine(new ConsoleLine(""), GetConsoleLine());
                        GoNextLine(-1);
                    }
                }

                if (goNextLine) GoNextLine();

                return userInput;
            }

            public static string GetUserInput(bool clear = false, bool goNextLine = false) { return GetUserInput(new ConsoleLine(), clear, goNextLine); }
            public static string GetValidUserInput(InputType validType, int validLength = -1, int wordCount = -1, bool removeWhitespace = false) { return GetValidUserInput(new ConsoleLine(), validType, validLength, wordCount, removeWhitespace); }

            public static void WaitForUserInput(ConsoleKey key, bool clear = false, bool goNextLine = false)
            {
                SendConsoleMessage($"Press [{key}] To Continue... ");
                while (true)
                {
                    ConsoleKeyInfo c = System.Console.ReadKey(true);
                    if (c.Key == key)
                    {
                        if (clear) UpdateConsoleLine(new ConsoleLine(""), GetConsoleLine());
                        if (goNextLine) GoNextLine();
                        return;
                    }
                }
            }

            public static int CreateOptionMenu(string title, ConsoleLine[] options, bool clear = false)
            {
                if (options.Length < 2) return 0;

                SendConsoleMessage(new ConsoleLine(title, ConsoleColor.DarkBlue));

                (int min, int max) pointerRange = (consoleCurrentLine, consoleCurrentLine + options.Length);

                int distFromMax = consoleLines.Length - 8 - consoleCurrentLine;
                if (options.Length + 1 > distFromMax)
                {
                    pointerRange.min -= options.Length;
                    pointerRange.max -= options.Length;
                }
                int pointer = pointerRange.min;
                for (int i = pointerRange.min; i < pointerRange.max; i++)
                {
                    options[i - pointerRange.min].Update("> " + options[i - pointerRange.min].lineText);
                    SendConsoleMessage(options[i - pointerRange.min]);
                }

                consoleLines[pointerRange.min].Update(options[0].lineText + " <-");

                while (true)
                {
                    ConsoleKeyInfo c = System.Console.ReadKey(true);

                    if (c.Key == ConsoleKey.W || c.Key == ConsoleKey.UpArrow) pointer = Math.Clamp(pointer - 1, pointerRange.min, pointerRange.max - 1);
                    else if (c.Key == ConsoleKey.S || c.Key == ConsoleKey.DownArrow) pointer = Math.Clamp(pointer + 1, pointerRange.min, pointerRange.max - 1);
                    else if (c.Key == ConsoleKey.Enter)
                    {
                        if (clear)
                        {
                            for (int i = pointerRange.min - 1; i < pointerRange.max; i++)
                            {
                                consoleLines[i].Update("");
                                consoleCurrentLine = pointerRange.min - 1;
                                consoleUpdated = false;
                            }
                        }
                        return pointer - pointerRange.min;
                    }

                    for (int i = pointerRange.min; i < pointerRange.max; i++)
                    {
                        consoleLines[i].Update(options[i - pointerRange.min].lineText + (pointer == i ? " <-" : ""));
                        consoleUpdated = false;
                    }
                }
            }

            public static int CreateOptionMenu(string title, string[] options, bool clear = false)
            {
                ConsoleLine[] c = new ConsoleLine[options.Length];
                for (int i = 0; i < options.Length; i++)
                {
                    c[i] = new ConsoleLine(options[i]);
                }

                return CreateOptionMenu(title, c, clear);
            }

            public static bool CreateOptionMenu(string title, bool clear = false)
            {
                if (CreateOptionMenu(title, new ConsoleLine[] { new ConsoleLine("Yes"), new ConsoleLine("No") }, clear) == 0) return true;
                return false;
            }

            public enum InputType { FullText, PartialText, DateOnly, Int, Float }

            public static InputType GetInputType(string input)
            {
                if (long.TryParse(input, out long r)) return InputType.Int;
                if (float.TryParse(input, out float r2)) return InputType.Float;

                if (DateOnly.TryParse(input, out DateOnly r4)) return InputType.DateOnly;

                for (int i = 0; i < input.Length; i++)
                {
                    if (int.TryParse(input[i].ToString(), out int r3)) return InputType.PartialText;
                }

                return InputType.FullText;
            }

            public static bool InputValid(string input, InputType validType, int validLength = -1, int wordCount = -1, bool removeWhitespace = false, bool outputMismatch = false)
            {
                return InputValid(input, new InputType[] { validType }, validLength, wordCount, removeWhitespace, outputMismatch);
            }

            public static bool InputValid(string input, InputType[] validTypes, int validLength = -1, int wordCount = -1, bool removeWhitespace = false, bool outputMismatch = false)
            {
                if (removeWhitespace) input = input.Replace(" ", "");

                InputType t = GetInputType(input);
                if (validTypes.Contains(t) && (validLength == -1 || input.Length == validLength) && (wordCount == -1 || input.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == wordCount)) return true;

                if (outputMismatch)
                {
                    if (!validTypes.Contains(t)) SendConsoleMessage($"Input Recognised As [{t}], Should Be {RevistoneExtras.ArrayToString(validTypes)}!");
                    else if (validLength != -1 && input.Length < validLength) SendConsoleMessage($"Input To Short [{input.Length}], Expected Length [{validLength}]!");
                    else if (validLength != -1 && input.Length > validLength) SendConsoleMessage($"Input To Long [{input.Length}], Expected Length [{validLength}]!");
                    else SendConsoleMessage($"Input Contains [{input.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length}] Sepearate Words, Expected Words [{wordCount}]");
                }

                return false;
            }
        }
    }
}