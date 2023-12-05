using Revistone.Console;
using Revistone.Functions;
using Revistone.Apps;

using static Revistone.Console.Data.ConsoleData;
using static Revistone.Console.ConsoleAction;
using static Revistone.Functions.ColourFunctions;
using System.Runtime.InteropServices;
using Revistone.Management;
using System.Security.Cryptography.X509Certificates;

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

                int cursorIndex = 2;
                bool cursorActive = true;

                SendConsoleMessage(new ConsoleLine(">  ", cc, AdvancedHighlight(3, ConsoleColor.Black, ConsoleColor.White, (cursorIndex, 1))),
                ConsoleLineUpdate.SameLine, new ConsoleAnimatedLine(UpdateUserCursorAnimation, 20, true));

                string userInput = "";
                bool breakLoop = false;
                while (true)
                {
                    breakLoop = false;

                    (ConsoleKeyInfo keyInfo, bool interrupted) c = UserRealtimeInput.GetKey();
                    if (c.interrupted)
                    {
                        SendConsoleMessage(new ConsoleLine($"> {userInput} ", cc),
                        ConsoleLineUpdate.SameLine, new ConsoleAnimatedLine(UpdateUserCursorAnimation, 20, true));
                        continue;
                    }

                    switch (c.keyInfo.Key)
                    {
                        default:
                            if (userInput.Length > System.Console.BufferWidth - 5) continue; //stops from exceeding console buffer (-5 to be safe)
                            userInput = userInput.Insert(cursorIndex - 2, c.keyInfo.KeyChar.ToString());
                            cursorIndex++;
                            SendConsoleMessage(new ConsoleLine($"> {userInput} ", cc, UpdateUserCursor()), ConsoleLineUpdate.SameLine);
                            break;
                        case ConsoleKey.Enter:
                            breakLoop = true;
                            break;
                        case ConsoleKey.Backspace:
                            if (userInput.Length == 0 || cursorIndex == 2) continue;
                            userInput = userInput.Substring(0, cursorIndex - 3) + userInput.Substring(cursorIndex - 2);
                            cursorIndex--;
                            SendConsoleMessage(new ConsoleLine($"> {userInput} ", cc, UpdateUserCursor()), ConsoleLineUpdate.SameLine);
                            break;
                        case ConsoleKey.LeftArrow:
                            cursorIndex = Math.Clamp(cursorIndex - 1, 2, userInput.Length + 2);
                            GetConsoleLine(primaryLineIndex).Update(cc, UpdateUserCursor());
                            break;
                        case ConsoleKey.RightArrow:
                            cursorIndex = Math.Clamp(cursorIndex + 1, 2, userInput.Length + 2);
                            GetConsoleLine(primaryLineIndex).Update(cc, UpdateUserCursor());
                            break;
                    }

                    if (breakLoop) break;
                }

                SendConsoleMessage(new ConsoleLine($"> {userInput}", cc, ConsoleColor.Black.Extend(userInput.Length + 3)), ConsoleLineUpdate.SameLine, ConsoleAnimatedLine.None);
                if (clear) ClearLines(promt.lineText != "" ? 1 : 0, true);

                ShiftLine(goNextLine ? 1 : 0);

                return userInput;

                void UpdateUserCursorAnimation(ConsoleLine lineInfo, ConsoleAnimatedLine animationInfo, int tickNum)
                {
                    if (cursorActive)
                    {
                        lineInfo.Update(lineInfo.lineColour, ConsoleColor.Black.ToArray());
                    }
                    else
                    {
                        lineInfo.Update(lineInfo.lineColour, AdvancedHighlight(lineInfo.lineText.Length, ConsoleColor.Black, ConsoleColor.White, (cursorIndex, 1)));
                    }
                    cursorActive = !cursorActive;
                }

                ConsoleColor[] UpdateUserCursor()
                {
                    return cursorActive ? AdvancedHighlight(userInput.Length + 3, ConsoleColor.Black, ConsoleColor.White, (cursorIndex, 1)) : GetConsoleLine(primaryLineIndex).lineColourBG.Extend(ConsoleColor.Black, userInput.Length + 3);
                }

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
                    (ConsoleKeyInfo keyInfo, bool interrupted) c = UserRealtimeInput.GetKey();
                    if (c.interrupted)
                    {
                        SendConsoleMessage(new ConsoleLine($"Press [{key}] To Continue{new string('.', dotCount)}", colours), ConsoleLineUpdate.SameLine,
                        new ConsoleAnimatedLine(WaitForUserInputUpdate, tickMod: AppRegistry.activeApp.colourScheme.speed, enabled: true));
                        continue;
                    }
                    if (c.keyInfo.Key == key)
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

            //extras associated with menus
            static int metaOptionsLines = 0;

            /// <summary> Creates menu from given options, allowing user to pick one. </summary>
            public static int CreateOptionMenu(string title, ConsoleLine[] options, bool clear = true, int cursorStartIndex = 0)
            {
                if (options.Length < 2) return 0;

                if (title != "") SendConsoleMessage(new ConsoleLine(title, AppRegistry.activeApp.colourScheme.primaryColour));

                int[] optionLines = new int[options.Length];

                for (int i = 0; i < options.Length; i++)
                {
                    options[i].Update("> " + options[i].lineText,
                    BuildArray(AppRegistry.activeApp.colourScheme.secondaryColour.Extend(2), options[i].lineColour.Extend(options[i].lineText.Length), AppRegistry.activeApp.colourScheme.secondaryColour.Extend(3)));
                    optionLines[i] = SendConsoleMessage(options[i], ConsoleAnimatedLine.AppTheme);
                }

                for (int i = 0; i < options.Length; i++)
                {
                    optionLines[i] -= Math.Max(optionLines.Where(num => num == debugStartIndex - 1).Count() - 1, 0);
                }

                consoleLines[optionLines[0 + Math.Clamp(cursorStartIndex, 0, options.Length - 1)]].Update(options[0 + Math.Clamp(cursorStartIndex, 0, options.Length - 1)].lineText + " <-");
                int pointer = optionLines[0];

                ConsoleLine[] metaOptions = new ConsoleLine[(title != "" ? 1 : 0) + metaOptionsLines];
                for (int i = 0; i < metaOptions.Length; i++)
                {
                    metaOptions[i] = consoleLines[optionLines[0] - metaOptions.Length + i];
                }


                while (true)
                {
                    (ConsoleKeyInfo keyInfo, bool interrupted) c = UserRealtimeInput.GetKey();

                    if (c.interrupted)
                    {
                        int relativePointerPos = pointer - optionLines[0];

                        ClearLines(options.Length + metaOptions.Length, true, true);
                        for (int i = 0; i < options.Length + metaOptions.Length; i++)
                        {
                            if (i < metaOptions.Length) SendConsoleMessage(metaOptions[i]);
                            else
                            {
                                optionLines[i - metaOptions.Length] = primaryLineIndex;
                                SendConsoleMessage(options[i - metaOptions.Length]);
                            }
                        }
                        pointer = optionLines[0] + relativePointerPos;
                        consoleLines[pointer].Update(consoleLines[pointer].lineText + " <-");
                        continue;
                    }

                    if (c.keyInfo.Key == ConsoleKey.W || c.keyInfo.Key == ConsoleKey.UpArrow) pointer = Math.Clamp(pointer - 1, optionLines[0], optionLines[^1]);
                    else if (c.keyInfo.Key == ConsoleKey.S || c.keyInfo.Key == ConsoleKey.DownArrow) pointer = Math.Clamp(pointer + 1, optionLines[0], optionLines[^1]);
                    else if (c.keyInfo.Key == ConsoleKey.Enter)
                    {
                        if (clear)
                        {
                            GoToLine(optionLines[^1]);
                            ClearLines(optionLines[^1] - (optionLines[0] - 1 + (title == "" ? 1 : 0)), true, true);
                        }
                        return pointer - optionLines[0];
                    }

                    for (int i = optionLines[0]; i <= optionLines[^1]; i++)
                    {
                        consoleLines[i].Update(options[i - optionLines[0]].lineText + (pointer == i ? " <-" : ""));
                    }
                }
            }

            /// <summary> Creates menu from given consoleLine options, allowing user to pick one, and invoking the associated action. </summary>
            public static int CreateOptionMenu(string title, (ConsoleLine name, Action action)[] options, bool clear = true, int cursorStartIndex = 0)
            {
                int option = CreateOptionMenu(title, options.Select(option => option.name).ToArray(), clear, cursorStartIndex);
                options[option].action.Invoke();
                return option;
            }

            /// <summary> Creates menu from given text options, allowing user to pick one, and invoking the associated action. </summary>
            public static int CreateOptionMenu(string title, (string name, Action action)[] options, bool clear = true, int cursorStartIndex = 0)
            {
                int option = CreateOptionMenu(title, options.Select(option => new ConsoleLine(option.name)).ToArray(), clear, cursorStartIndex);
                options[option].action.Invoke();
                return option;
            }

            /// <summary> Creates menu from given text options, allowing user to pick one. </summary>
            public static int CreateOptionMenu(string title, string[] options, bool clear = true, int cursorStartIndex = 0)
            {
                return CreateOptionMenu(title, options.Select(option => new ConsoleLine(option)).ToArray(), clear, cursorStartIndex);
            }

            /// <summary> Creates menu, allowing user to select either yes or no. </summary>
            public static bool CreateOptionMenu(string title, bool clear = true, int cursorStartIndex = 0)
            {
                if (CreateOptionMenu(title, new ConsoleLine[] { new ConsoleLine("Yes"), new ConsoleLine("No") }, clear, cursorStartIndex) == 0) return true;
                return false;
            }

            /// <summary> Creates menu from given options, spliting into pages, allowing user to pick one (pinnedOptions return negative index). </summary>
            public static int CreateMultiPageOptionMenu(string title, ConsoleLine[] options, ConsoleLine[] pinnedOptions, int optionsPerPage)
            {
                if (options.Length + pinnedOptions.Length < 2) return 0;

                for (int i = 0; i < pinnedOptions.Length; i++) { pinnedOptions[i].Update(AppRegistry.activeApp.colourScheme.primaryColour.ToArray()); }

                ConsoleLine[] pgExtraOptions = new ConsoleLine[] {
                    new ConsoleLine("Next Page", AppRegistry.activeApp.colourScheme.primaryColour),
                    new ConsoleLine("Previous Page", AppRegistry.activeApp.colourScheme.primaryColour)
                }.Concat(pinnedOptions).ToArray();

                int currentPage = 0, totalPages = (options.Length - 1) / optionsPerPage;

                metaOptionsLines = 1;
                while (true)
                {
                    ConsoleLine[] pgOptions = options.Skip(currentPage * optionsPerPage).Take(Math.Min(optionsPerPage, options.Length - currentPage * optionsPerPage)).Concat(pgExtraOptions).ToArray();

                    SendConsoleMessage(new ConsoleLine($"--- {title} Page [{currentPage + 1}/{totalPages + 1}] ---",
                    BuildArray(AppRegistry.activeApp.colourScheme.primaryColour.Extend(title.Length + 10),
                    AppRegistry.activeApp.colourScheme.secondaryColour.Extend($"[{currentPage + 1}/{currentPage + 1}]".Length, true),
                    AppRegistry.activeApp.colourScheme.primaryColour.Extend(4))));

                    int result = CreateOptionMenu("", pgOptions.Select(o => new ConsoleLine(o)).ToArray());

                    if (result == pgOptions.Length - 2 - pinnedOptions.Length)
                    {
                        currentPage = currentPage < totalPages ? currentPage + 1 : 0;
                    }
                    else if (result == pgOptions.Length - 1 - pinnedOptions.Length)
                    {
                        currentPage = currentPage > 0 ? currentPage - 1 : totalPages;
                    }
                    else if (result > pgOptions.Length - 2 - pinnedOptions.Length)
                    {
                        ClearLines(updateCurrentLine: true);
                        metaOptionsLines = 0;
                        return -result + (pgOptions.Length - (pgExtraOptions.Length - 1));
                    }
                    else
                    {
                        ClearLines(updateCurrentLine: true);
                        metaOptionsLines = 0;
                        return result + currentPage * optionsPerPage;
                    }

                    ClearLines(updateCurrentLine: true);
                }
            }

            /// <summary> Creates menu from given options, spliting into pages, allowing user to pick one. </summary>
            public static int CreateMultiPageOptionMenu(string title, ConsoleLine[] options, int optionsPerPage) { return CreateMultiPageOptionMenu(title, options, new ConsoleLine[0], optionsPerPage); }

            /// <summary> Creates menu from given messages, spliting into pages, allowing user to view. </summary>
            public static void CreateReadMenu(string title, params ConsoleLine[] messages)
            {
                int page = 0, pages = messages.Length / 5, pageLength;

                while (true)
                {
                    SendConsoleMessage(new ConsoleLine($"--- {title} Page [{page + 1}/{pages + 1}] ---",
                    BuildArray(AppRegistry.activeApp.colourScheme.primaryColour.Extend(title.Length + 10),
                    AppRegistry.activeApp.colourScheme.secondaryColour.Extend($"[{page + 1}/{pages + 1}]".Length, true),
                    AppRegistry.activeApp.colourScheme.primaryColour.Extend(4))));


                    pageLength = Math.Min(page * 5 + 5, messages.Length) - page * 5;
                    metaOptionsLines = 1 + pageLength;

                    for (int i = page * 5; i < Math.Min(page * 5 + 5, messages.Length); i++)
                    {
                        SendConsoleMessage(messages[i], ConsoleAnimatedLine.AppTheme);
                    }

                    if (CreateOptionMenu("", new (ConsoleLine, Action)[] {
                    (new ConsoleLine("Next Page", AppRegistry.activeApp.colourScheme.primaryColour), () => page = page < pages ? page + 1 : 0),
                    (new ConsoleLine("Last Page", AppRegistry.activeApp.colourScheme.primaryColour), () => page = page > 0 ? page - 1 : pages),
                    (new ConsoleLine("Exit", AppRegistry.activeApp.colourScheme.primaryColour), () => {})}) == 2)
                    {
                        ClearLines(metaOptionsLines, true);
                        break;
                    }

                    ClearLines(metaOptionsLines, true);
                }

                metaOptionsLines = 0;
            }

            /// <summary> Creates menu from given messages, spliting into pages, allowing user to view. </summary>
            public static void CreateReadMenu(string title, params string[] messages)
            {
                CreateReadMenu(title, messages.Select(s => new ConsoleLine(s)).ToArray());
            }
        }
    }
}