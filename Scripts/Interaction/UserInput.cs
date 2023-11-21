using static Revistone.Console.Data.ConsoleData;
using static Revistone.Console.ConsoleAction;
using Revistone.Console;
using Revistone.Functions;
using Revistone.Apps;

namespace Revistone
{
    namespace Interaction
    {
        /// <summary> Handles all user input within the console. </summary>
        public static class UserInput
        {
            /// <summary> Gets input from the user (input length capped at buffer width). </summary>
            public static string GetUserInput(ConsoleLine promt, bool clear = false, bool goNextLine = false)
            {
                if (promt.lineText != "") SendConsoleMessage(new ConsoleLine(promt.lineText, promt.lineColour), new ConsoleLineUpdate());

                SendConsoleMessage(new ConsoleLine("> "), ConsoleLineUpdate.SameLine);

                string userInput = "";
                while (true)
                {
                    bool breakLoop = false;

                    ConsoleKeyInfo c = UserRealtimeInput.GetKey();

                    switch (c.Key)
                    {
                        default:
                            if (userInput.Length <= System.Console.BufferWidth - 5) userInput += c.KeyChar; //stops from exceeding console buffer (-5 to be safe)
                            SendConsoleMessage(new ConsoleLine($"> {userInput}"), ConsoleLineUpdate.SameLine);
                            break;
                        case ConsoleKey.Enter:
                            breakLoop = true;
                            break;
                        case ConsoleKey.Backspace:
                            if (userInput.Length > 0) userInput = userInput.Substring(0, userInput.Length - 1);
                            SendConsoleMessage(new ConsoleLine($"> {userInput}"), ConsoleLineUpdate.SameLine);
                            break;
                    }

                    if (breakLoop) break;
                }

                if (clear) ClearLines(promt.lineText != "" ? 1 : 0, true);

                ShiftLine(goNextLine ? 1 : 0);

                return userInput;
            }

            /// <summary> Gets input from the user (input length capped at buffer width). </summary>
            public static string GetUserInput(string promt = "", bool clear = false, bool goNextLine = false) { return GetUserInput(new ConsoleLine(promt), clear, goNextLine); }

            /// <summary> Gets input from the user, verifying it matches the conditions of given UserInputProfile. </summary>
            public static string GetValidUserInput(ConsoleLine promt, UserInputProfile inputProfile)
            {
                while (true)
                {
                    string input = GetUserInput(promt, true, false);
                    if (inputProfile.InputValid(input)) return input;
                }
            }

            /// <summary> Gets input from the user, verifying it matches the conditions of given UserInputProfile. </summary>
            public static string GetValidUserInput(string promt, UserInputProfile inputProfile) { return GetValidUserInput(new ConsoleLine(promt), inputProfile); }

            /// <summary> Waits for user to input given key, pausing program until pressed. </summary>
            // will update once dynamic lines get updated
            public static void WaitForUserInput(ConsoleKey key = ConsoleKey.Enter, bool clear = true, bool space = false, bool goNextLine = false)
            {
                if (space) ShiftLine();
                int dotCount = 1, index = -1;

                ConsoleColor[] colours = ColourFunctions.AdvancedHighlight(30, AppRegistry.activeApp.colourScheme.primaryColour, (AppRegistry.activeApp.colourScheme.secondaryColour, 6, 2 + key.ToString().Length));

                SendConsoleMessage(new ConsoleLine($"Press [{key}] To Continue{new string('.', dotCount)}", colours), ConsoleLineUpdate.SameLine,
                new ConsoleAnimatedLine(WaitForUserInputUpdate, tickMod: AppRegistry.activeApp.colourScheme.speed, enabled: true));

                while (true)
                {
                    ConsoleKeyInfo c = UserRealtimeInput.GetKey();
                    if (c.Key == key)
                    {
                        GoToLine(index); //prevents other functions and tasks messing with clear
                        if (clear) UpdateConsoleLine(new ConsoleLine(""), GetConsoleLineIndex());
                        if (goNextLine) ShiftLine();
                        return;
                    }
                }

                void WaitForUserInputUpdate(ConsoleLine lineInfo, ConsoleAnimatedLine animationInfo, int tickNum)
                {
                    index = primaryLineIndex;
                    SendConsoleMessage(new ConsoleLine($"Press [{key}] To Continue{new string('.', dotCount)}", colours), ConsoleLineUpdate.SameLine);
                    colours = colours.Shift(1, 6, 2 + key.ToString().Length);
                    dotCount = dotCount < 3 ? dotCount + 1 : 0;
                }
            }

            /// <summary> Creates menu from given options, allowing user to pick one. </summary>
            public static int CreateOptionMenu(string title, ConsoleLine[] options, bool clear = true)
            {
                if (options.Length < 2) return 0;

                SendConsoleMessage(new ConsoleLine(title, AppRegistry.activeApp.colourScheme.primaryColour));

                int shift = Math.Clamp(options.Length - (debugStartIndex - primaryLineIndex), 0, int.MaxValue);
                (int min, int max) pointerRange = (primaryLineIndex - shift, primaryLineIndex + options.Length - shift);

                for (int i = pointerRange.min; i < pointerRange.max; i++)
                {
                    options[i - pointerRange.min].Update("> " + options[i - pointerRange.min].lineText);
                    SendConsoleMessage(options[i - pointerRange.min]);
                }

                consoleLines[pointerRange.min].Update(options[0].lineText + " <-");

                int pointer = pointerRange.min;

                while (true)
                {
                    ConsoleKeyInfo c = UserRealtimeInput.GetKey();

                    if (c.Key == ConsoleKey.W || c.Key == ConsoleKey.UpArrow) pointer = Math.Clamp(pointer - 1, pointerRange.min, pointerRange.max - 1);
                    else if (c.Key == ConsoleKey.S || c.Key == ConsoleKey.DownArrow) pointer = Math.Clamp(pointer + 1, pointerRange.min, pointerRange.max - 1);
                    else if (c.Key == ConsoleKey.Enter)
                    {
                        if (clear)
                        {
                            GoToLine(pointerRange.max);
                            ClearLines(pointerRange.max - (pointerRange.min - 1), true);
                        }
                        return pointer - pointerRange.min;
                    }

                    for (int i = pointerRange.min; i < pointerRange.max; i++)
                    {
                        consoleLines[i].Update(options[i - pointerRange.min].lineText + (pointer == i ? " <-" : ""));
                    }
                }
            }

            /// <summary> Creates menu from given text options, allowing user to pick one, and invoking the associated action. </summary>
            public static void CreateOptionMenu(string title, (string name, Action action)[] options, bool clear = false)
            {
                ConsoleLine[] c = options.Select(option => new ConsoleLine(option.name)).ToArray();
                options[CreateOptionMenu(title, c, clear)].action.Invoke();
                return;
            }

            /// <summary> Creates menu from given text options, allowing user to pick one. </summary>
            public static int CreateOptionMenu(string title, string[] options, bool clear = false)
            {
                ConsoleLine[] c = options.Select(option => new ConsoleLine(option)).ToArray();

                return CreateOptionMenu(title, c, clear);
            }

            /// <summary> Creates menu, allowing user to select either yes or no. </summary>
            public static bool CreateOptionMenu(string title, bool clear = false)
            {
                if (CreateOptionMenu(title, new ConsoleLine[] { new ConsoleLine("Yes"), new ConsoleLine("No") }, clear) == 0) return true;
                return false;
            }
        }
    }
}