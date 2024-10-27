using Revistone.Console;
using Revistone.Functions;
using Revistone.Apps;

using static Revistone.Console.Data.ConsoleData;
using static Revistone.Console.ConsoleAction;
using static Revistone.Functions.ColourFunctions;

namespace Revistone.Interaction;

/// <summary> Handles all user input within the console. </summary>
public static class UserInput
{
    // keys that will be stopped from processing in user input 
    static ConsoleKey[] exceptionKeys = [
        ConsoleKey.Escape, ConsoleKey.Delete, ConsoleKey.End,
                ConsoleKey.Insert, ConsoleKey.Home, ConsoleKey.PageUp,
                ConsoleKey.PageDown, ConsoleKey.Tab,
            ];

    // previous inputs from the user
    static List<string> userInputMemory = new List<string>();

    /// <summary> Gets input from the user (input length capped at buffer width). </summary>
    public static string GetUserInput(ConsoleLine promt, bool clear = false, bool goNextLine = false, string prefilledText = "")
    {
        if (promt.lineText.Length > 0) SendConsoleMessage(new ConsoleLine(promt));

        string userInput = prefilledText;

        int pointerIndex = 2 + userInput.Length;
        int pointerExtend = 0;
        int userInputPointer = userInputMemory.Count;

        ConsoleColor[] fgColour = BuildArray(AppRegistry.activeApp.colourScheme.secondaryColour, ConsoleColor.White.ToArray());
        ConsoleColor[] bgColour = GenerateCursorFrame();

        //SendConsoleMessage(new ConsoleLine($"> {userInput} ", fgColour, bgColour), ConsoleLineUpdate.SameLine, new ConsoleAnimatedLine(PointerAnimation, 20, true));
        SendConsoleMessage(new ConsoleLine($"> {userInput} ", fgColour, bgColour), ConsoleLineUpdate.SameLine, new ConsoleAnimatedLine(PointerAnimation, 20, true));

        bool endUserInput = false;
        while (!endUserInput)
        {
            int currentLine = primaryLineIndex;
            (ConsoleKeyInfo keyInfo, bool interrupted) c = UserRealtimeInput.GetKey();

            if (c.interrupted) // the console has been interrupted and line must be redrawn
            {
                SendConsoleMessage(new ConsoleLine($"> {userInput} ", fgColour, GetPointerAnimationFrame()), ConsoleLineUpdate.SameLine);
                continue;
            }
            if (exceptionKeys.Contains(c.keyInfo.Key)) continue; // keys mess with console so are just voided

            bool shiftPressed = UserRealtimeInput.KeyPressed(0x10);
            bool ctrlPressed = UserRealtimeInput.KeyPressed(0x11);
            bool altPressed = UserRealtimeInput.KeyPressed(0x12);
            (int last, int next) wordIndex = GetWordIndex();

            switch (c.keyInfo.Key)
            {
                case ConsoleKey.Enter:
                    endUserInput = true;
                    break;
                case ConsoleKey.Backspace:
                    if (pointerIndex == 2 && pointerExtend == 0) break; // we cant get rid of nothing

                    if (pointerExtend == 0)
                    {
                        userInput = userInput.Remove(pointerIndex - 3, 1);
                        pointerIndex--;
                    }
                    else
                    {
                        userInput = userInput.Remove(GetPointerStart() - 2, Math.Min(userInput.Length - (GetPointerStart() - 2), GetPointerLength()));
                        pointerIndex = pointerExtend > 0 ? pointerIndex : pointerIndex - (GetPointerLength() - 1);
                        pointerExtend = 0;
                    }

                    SendConsoleMessage(new ConsoleLine($"> {userInput} ", fgColour, GetPointerAnimationFrame()), ConsoleLineUpdate.SameLine);
                    break;
                case ConsoleKey.LeftArrow:
                    if (shiftPressed) pointerExtend = pointerIndex > 2 ? pointerExtend + 1 : pointerExtend;
                    else pointerExtend = 0;

                    if (ctrlPressed) pointerIndex = 2 + wordIndex.last;
                    else pointerIndex = Math.Max(pointerIndex - 1, 2);
                    GetConsoleLine(primaryLineIndex).Update(fgColour, GetPointerAnimationFrame());
                    break;
                case ConsoleKey.RightArrow:
                    if (shiftPressed) pointerExtend = pointerIndex < userInput.Length + 2 ? pointerExtend - 1 : pointerExtend;
                    else pointerExtend = 0;

                    if (ctrlPressed) pointerIndex = 2 + wordIndex.next;
                    else pointerIndex = Math.Min(pointerIndex + 1, userInput.Length + 2);
                    GetConsoleLine(primaryLineIndex).Update(fgColour, GetPointerAnimationFrame());

                    break;
                case ConsoleKey.UpArrow:
                    if (userInputPointer - 1 < 0) break; // reached end of user input

                    if (userInputPointer >= userInputMemory.Count && userInput.Length > 0) userInputMemory.Add(userInput); // add current userinput to memory
                    userInputPointer--;
                    userInput = userInputMemory[userInputPointer];
                    pointerIndex = 2 + userInput.Length;

                    SendConsoleMessage(new ConsoleLine($"> {userInput} ", fgColour, GetPointerAnimationFrame()), ConsoleLineUpdate.SameLine);
                    break;
                case ConsoleKey.DownArrow:
                    if (userInputPointer >= userInputMemory.Count - 1) break; // reached end of user input

                    userInputPointer++;
                    userInput = userInputMemory[userInputPointer];
                    pointerIndex = 2 + userInput.Length;

                    SendConsoleMessage(new ConsoleLine($"> {userInput} ", fgColour, GetPointerAnimationFrame()), ConsoleLineUpdate.SameLine);
                    break;
                default:
                    if (UserRealtimeInput.KeyPressed(0x11)) break; //ctrl pressed

                    if (shiftPressed && altPressed)
                    {
                        if (c.keyInfo.Key == ConsoleKey.C && pointerExtend != 0)
                        {
                            StringFunctions.CopyToClipboard(userInput.Substring(GetPointerStart() - 2, Math.Min(userInput.Length - (GetPointerStart() - 2), GetPointerLength())));
                            SendDebugMessage(new ConsoleLine("Contents Copied To Clipboard.", ConsoleColor.DarkBlue));
                            break;
                        }
                        else if (c.keyInfo.Key == ConsoleKey.X && pointerExtend != 0)
                        {
                            StringFunctions.CopyToClipboard(userInput.Substring(GetPointerStart() - 2, Math.Min(userInput.Length - (GetPointerStart() - 2), GetPointerLength())));
                            SendDebugMessage(new ConsoleLine("Contents Cut To Clipboard.", ConsoleColor.DarkBlue));
                            userInput = userInput.Remove(GetPointerStart() - 2, Math.Min(userInput.Length - (GetPointerStart() - 2), GetPointerLength()));
                            pointerIndex = pointerExtend > 0 ? pointerIndex : pointerIndex - (GetPointerLength() - 1);
                            pointerExtend = 0;
                            SendConsoleMessage(new ConsoleLine($"> {userInput} ", fgColour, GetPointerAnimationFrame()), ConsoleLineUpdate.SameLine);
                            break;
                        }
                        else if (c.keyInfo.Key == ConsoleKey.V)
                        {
                            string clipboardContents = StringFunctions.GetClipboardText().Replace('\n', ' ');
                            if (clipboardContents.Length + userInput.Length > System.Console.BufferWidth - 5)
                            {
                                SendDebugMessage(new ConsoleLine("Clipboard Contents Are To Long To Paste.", ConsoleColor.DarkBlue));
                                break; // to long to paste
                            }

                            if (pointerExtend != 0)
                            {
                                userInput = userInput.Remove(GetPointerStart() - 2, Math.Min(userInput.Length - (GetPointerStart() - 2), GetPointerLength()));
                                pointerIndex = pointerExtend > 0 ? pointerIndex : pointerIndex - (GetPointerLength() - 1);
                                pointerExtend = 0;
                            }

                            userInput = userInput.Insert(pointerIndex - 2, clipboardContents);
                            pointerIndex += clipboardContents.Length;
                            SendConsoleMessage(new ConsoleLine($"> {userInput} ", fgColour, GetPointerAnimationFrame()), ConsoleLineUpdate.SameLine);
                            SendDebugMessage(new ConsoleLine("Clipboard Contents Pasted.", ConsoleColor.DarkBlue));
                            break;
                        }
                    }

                    if (userInput.Length > System.Console.BufferWidth - 5) break; // we dont want to overflow to the next line of the console

                    if (pointerExtend != 0)
                    {
                        userInput = userInput.Remove(GetPointerStart() - 2, Math.Min(userInput.Length - (GetPointerStart() - 2), GetPointerLength()));
                        pointerIndex = pointerExtend > 0 ? pointerIndex : pointerIndex - (GetPointerLength() - 1);
                        pointerExtend = 0;
                    }

                    userInput = userInput.Insert(pointerIndex - 2, c.keyInfo.KeyChar.ToString());
                    pointerIndex++;
                    SendConsoleMessage(new ConsoleLine($"> {userInput} ", fgColour, GetPointerAnimationFrame()), ConsoleLineUpdate.SameLine);
                    break;
            }
        }

        SendConsoleMessage(new ConsoleLine($"> {userInput}", fgColour, ConsoleColor.Black.ToArray()), ConsoleLineUpdate.SameLine, ConsoleAnimatedLine.None);

        if (clear) ClearLines(promt.lineText != "" ? 1 : 0, true);
        ShiftLine(goNextLine ? 1 : 0);

        if (!(userInputPointer == userInputMemory.Count - 1 && userInput == userInputMemory[^1])) userInputMemory.Add(userInput);
        return userInput;

        // --- POINTER ANIMATION ---

        // animation for updating the pointer
        void PointerAnimation(ConsoleLine lineInfo, ConsoleAnimatedLine animationInfo, int tickNum)
        {
            if ((tickNum - animationInfo.initTick) % 40 != 0) bgColour = ConsoleColor.Black.ToArray();
            else bgColour = GenerateCursorFrame();

            lineInfo.Update(lineInfo.lineColour, bgColour);
        }

        // get the current cursor animation frame
        ConsoleColor[] GetPointerAnimationFrame()
        {
            return bgColour.Length == 1 ? bgColour : GenerateCursorFrame();
        }

        ConsoleColor[] GenerateCursorFrame()
        {
            return AdvancedHighlight(3 + userInput.Length, ConsoleColor.Black, ConsoleColor.White, (GetPointerStart(), GetPointerLength()));
        }

        // --- POINTER EXTENSION MATH ---

        int GetPointerStart() { return pointerIndex + (pointerExtend < 0 ? pointerExtend : 0); }
        int GetPointerLength() { return 1 + Math.Abs(pointerExtend); }

        // --- WORD INFO ---

        (int last, int next) GetWordIndex()
        {
            List<int> wordIndex = new List<int>();

            for (int i = 1; i < userInput.Length; i++)
            {
                if (userInput[i - 1] == ' ' && userInput[i] != ' ') wordIndex.Add(i);
            }

            int last = 0, next = userInput.Length;

            for (int i = 0; i < wordIndex.Count; i++)
            {
                if (wordIndex[i] < pointerIndex - 2) last = wordIndex[i];
                else if (wordIndex[i] > pointerIndex - 2 && next == userInput.Length) next = wordIndex[i];
            }

            return (last, next);
        }
    }

    /// <summary> Gets input from the user (input length capped at buffer width). </summary>
    public static string GetUserInput(string promt = "", bool clear = false, bool goNextLine = false) { return GetUserInput(new ConsoleLine(promt, AppRegistry.activeApp.colourScheme.primaryColour), clear, goNextLine); }

    /// <summary> Gets input from the user, verifying it matches the conditions of given UserInputProfile. </summary>
    public static string GetValidUserInput(ConsoleLine promt, UserInputProfile inputProfile, string prefilledText = "")
    {
        while (true)
        {
            string input = GetUserInput(promt, true, false, prefilledText);
            if (inputProfile.InputValid(input)) return input;
        }
    }

    /// <summary> Gets input from the user, verifying it matches the conditions of given UserInputProfile. </summary>
    public static string GetValidUserInput(string promt, UserInputProfile inputProfile, string prefilledText = "") { return GetValidUserInput(new ConsoleLine(promt, AppRegistry.activeApp.colourScheme.primaryColour), inputProfile, prefilledText); }

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
                if (clear) UpdatePrimaryConsoleLine(new ConsoleLine(""), ConsoleAnimatedLine.None, index);
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
        int pointer = optionLines[cursorStartIndex];

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
    public static bool CreateTrueFalseOptionMenu(string title, string trueOption = "Yes", string falseOption = "No", bool clear = true, int cursorStartIndex = 0)
    {
        if (CreateOptionMenu(title, new ConsoleLine[] { new ConsoleLine(trueOption), new ConsoleLine(falseOption) }, clear, cursorStartIndex) == 0) return true;
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
    public static void CreateReadMenu(string title, int messagesPerPage, (ConsoleLine name, Action action)[] extraOptions, params ConsoleLine[] messages)
    {
        int page = 0, pages = (messages.Length - 1) / messagesPerPage, pageLength;

        while (true)
        {
            SendConsoleMessage(new ConsoleLine($"--- {title} Page [{page + 1}/{pages + 1}] ---",
            BuildArray(AppRegistry.activeApp.colourScheme.primaryColour.Extend(title.Length + 10),
            AppRegistry.activeApp.colourScheme.secondaryColour.Extend($"[{page + 1}/{pages + 1}]".Length, true),
            AppRegistry.activeApp.colourScheme.primaryColour.Extend(4))));


            pageLength = Math.Min(page * messagesPerPage + messagesPerPage, messages.Length) - page * messagesPerPage;
            metaOptionsLines = 1 + pageLength;

            for (int i = page * messagesPerPage; i < Math.Min(page * messagesPerPage + messagesPerPage, messages.Length); i++)
            {
                SendConsoleMessage(messages[i], ConsoleAnimatedLine.AppTheme);
            }

            if (CreateOptionMenu("", extraOptions.Concat(new (ConsoleLine, Action)[] {
                    (new ConsoleLine("Next Page", AppRegistry.activeApp.colourScheme.primaryColour), () => page = page < pages ? page + 1 : 0),
                    (new ConsoleLine("Last Page", AppRegistry.activeApp.colourScheme.primaryColour), () => page = page > 0 ? page - 1 : pages),
                    (new ConsoleLine("Exit", AppRegistry.activeApp.colourScheme.primaryColour), () => {})}).ToArray()) == 2)
            {
                ClearLines(metaOptionsLines, true);
                break;
            }

            ClearLines(metaOptionsLines, true);
        }

        metaOptionsLines = 0;
    }

    /// <summary> Creates menu from given messages, spliting into pages, allowing user to view. </summary>
    public static void CreateReadMenu(string title, int messagesPerPage, params ConsoleLine[] messages)
    {
        CreateReadMenu(title, messagesPerPage, new (ConsoleLine, Action)[] { }, messages);
    }

    /// <summary> Creates menu from given messages, spliting into pages, allowing user to view. </summary>
    public static void CreateReadMenu(string title, int messagesPerPage, params string[] messages)
    {
        CreateReadMenu(title, messagesPerPage, messages.Select(s => new ConsoleLine(s)).ToArray());
    }
}
