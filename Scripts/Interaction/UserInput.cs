using Revistone.Console;
using Revistone.Functions;
using Revistone.Apps;

using static Revistone.Console.Data.ConsoleData;
using static Revistone.Console.ConsoleAction;
using static Revistone.Functions.ColourFunctions;

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

                ConsoleColor[] cc = BuildArray(AppRegistry.activeApp.colourScheme.secondaryColour, ConsoleColor.White.ToArray());

                SendConsoleMessage(new ConsoleLine("> ", cc), ConsoleLineUpdate.SameLine);

                string userInput = "";
                while (true)
                {
                    bool breakLoop = false;

                    ConsoleKeyInfo c = UserRealtimeInput.GetKey();

                    switch (c.Key)
                    {
                        default:
                            if (userInput.Length <= System.Console.BufferWidth - 5) userInput += c.KeyChar; //stops from exceeding console buffer (-5 to be safe)
                            SendConsoleMessage(new ConsoleLine($"> {userInput}", cc), ConsoleLineUpdate.SameLine);
                            break;
                        case ConsoleKey.Enter:
                            breakLoop = true;
                            break;
                        case ConsoleKey.Backspace:
                            if (userInput.Length > 0) userInput = userInput.Substring(0, userInput.Length - 1);
                            SendConsoleMessage(new ConsoleLine($"> {userInput}", cc), ConsoleLineUpdate.SameLine);
                            break;
                    }

                    if (breakLoop) break;
                }

                if (clear) ClearLines(promt.lineText != "" ? 1 : 0, true);

                ShiftLine(goNextLine ? 1 : 0);

                return userInput;
            }

            /// <summary> Gets input from the user (input length capped at buffer width). </summary>
            public static string GetUserInput(string promt = "", bool clear = false, bool goNextLine = false) { return GetUserInput(new ConsoleLine(promt, AppRegistry.activeApp.colourScheme.primaryColour), clear, goNextLine); }

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
            public static string GetValidUserInput(string promt, UserInputProfile inputProfile) { return GetValidUserInput(new ConsoleLine(promt, AppRegistry.activeApp.colourScheme.primaryColour), inputProfile); }

            /// <summary> Waits for user to input given key, pausing program until pressed. </summary>
            // will update once dynamic lines get updated
            public static void WaitForUserInput(ConsoleKey key = ConsoleKey.Enter, bool clear = true, bool space = false, bool goNextLine = false)
            {
                if (space) ShiftLine();
                int dotCount = 1, index = -1;

                ConsoleColor[] colours = AdvancedHighlight(30, AppRegistry.activeApp.colourScheme.primaryColour, (AppRegistry.activeApp.colourScheme.secondaryColour, 6, 2 + key.ToString().Length));

                SendConsoleMessage(new ConsoleLine($"Press [{key}] To Continue{new string('.', dotCount)}", colours), ConsoleLineUpdate.SameLine,
                new ConsoleAnimatedLine(WaitForUserInputUpdate, tickMod: AppRegistry.activeApp.colourScheme.speed, enabled: true));

                while (true)
                {
                    ConsoleKeyInfo c = UserRealtimeInput.GetKey();
                    if (c.Key == key)
                    {
                        GoToLine(index); //prevents other functions and tasks messing with clear
                        if (clear) UpdatePrimaryConsoleLine(new ConsoleLine(""), GetConsoleLineIndex());
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
                    int j = i - pointerRange.min;
                    options[j].Update("> " + options[j].lineText,
                    BuildArray(AppRegistry.activeApp.colourScheme.secondaryColour.Extend(2), options[j].lineColour.Extend(options[j].lineText.Length), AppRegistry.activeApp.colourScheme.secondaryColour.Extend(3)));
                    SendConsoleMessage(options[j], ConsoleAnimatedLine.AppTheme);
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

            /// <summary> Creates menu from given consoleLine options, allowing user to pick one, and invoking the associated action. </summary>
            public static int CreateOptionMenu(string title, (ConsoleLine name, Action action)[] options, bool clear = true)
            {
                int option = CreateOptionMenu(title, options.Select(option => option.name).ToArray(), clear);
                options[option].action.Invoke();
                return option;
            }

            /// <summary> Creates menu from given text options, allowing user to pick one, and invoking the associated action. </summary>
            public static int CreateOptionMenu(string title, (string name, Action action)[] options, bool clear = true)
            {
                int option = CreateOptionMenu(title, options.Select(option => new ConsoleLine(option.name)).ToArray(), clear);
                options[option].action.Invoke();
                return option;
            }

            /// <summary> Creates menu from given text options, allowing user to pick one. </summary>
            public static int CreateOptionMenu(string title, string[] options, bool clear = true)
            {
                return CreateOptionMenu(title, options.Select(option => new ConsoleLine(option)).ToArray(), clear);
            }

            /// <summary> Creates menu, allowing user to select either yes or no. </summary>
            public static bool CreateOptionMenu(string title, bool clear = true)
            {
                if (CreateOptionMenu(title, new ConsoleLine[] { new ConsoleLine("Yes"), new ConsoleLine("No") }, clear) == 0) return true;
                return false;
            }

            /// <summary> Creates menu from given options, spliting into pages, allowing user to pick one (pinnedOptions return negative index). </summary>
            public static int CreateMultiPageOptionMenu(string title, ConsoleLine[] options, ConsoleLine[] pinnedOptions, int optionsPerPage)
            {
                if (options.Length + pinnedOptions.Length < 2) return 0;

                for (int i = 0; i < pinnedOptions.Length; i++) {pinnedOptions[i].Update(AppRegistry.activeApp.colourScheme.primaryColour.ToArray());}

                ConsoleLine[] pgExtraOptions = new ConsoleLine[] {
                    new ConsoleLine("Next Page", AppRegistry.activeApp.colourScheme.primaryColour),
                    new ConsoleLine("Previous Page", AppRegistry.activeApp.colourScheme.primaryColour)
                }.Concat(pinnedOptions).ToArray();

                int currentPage = 0, totalPages = (options.Length - 1) / optionsPerPage;
                while (true)
                {
                    string pgTitle = $"{title} [{currentPage + 1}/{totalPages + 1}]";
                    ConsoleLine[] pgOptions = options.Skip(currentPage * optionsPerPage).Take(Math.Min(optionsPerPage, options.Length - currentPage * optionsPerPage)).Concat(pgExtraOptions).ToArray();

                    int result = CreateOptionMenu(pgTitle, pgOptions.Select(o => new ConsoleLine(o)).ToArray());

                    if (result == pgOptions.Length - 2 - pinnedOptions.Length) currentPage = currentPage < totalPages ? currentPage + 1 : 0;
                    else if (result == pgOptions.Length - 1 - pinnedOptions.Length) currentPage = currentPage > 0 ? currentPage - 1 : totalPages;
                    else if (result > pgOptions.Length - 2 - pinnedOptions.Length)
                    {
                        return -result + (pgOptions.Length - (pgExtraOptions.Length - 1));
                    }
                    else return result + currentPage * optionsPerPage;
                }
            }

            /// <summary> Creates menu from given options, spliting into pages, allowing user to pick one. </summary>
            public static int CreateMultiPageOptionMenu(string title, ConsoleLine[] options, int optionsPerPage) { return CreateMultiPageOptionMenu(title, options, new ConsoleLine[0], optionsPerPage); }
        }
    }
}