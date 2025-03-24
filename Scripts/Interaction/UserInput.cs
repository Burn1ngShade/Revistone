using Revistone.Console;
using Revistone.Functions;
using Revistone.App.BaseApps;
using Revistone.App;
using Revistone.Management;

using static Revistone.Console.Data.ConsoleData;
using static Revistone.Console.ConsoleAction;
using static Revistone.Functions.ColourFunctions;
using static Revistone.Functions.PersistentDataFunctions;

namespace Revistone.Interaction;

/// <summary> Handles all user input within the console. </summary>
public static class UserInput
{
    // --- GET USER INPUT ---

    // keys that will be stopped from processing in user input 
    static readonly ConsoleKey[] exceptionKeys = [
        ConsoleKey.Escape, ConsoleKey.Delete, ConsoleKey.End,
        ConsoleKey.Insert, ConsoleKey.Home, ConsoleKey.PageUp,
        ConsoleKey.PageDown, ConsoleKey.Tab, ConsoleKey.F1,
        ConsoleKey.F2, ConsoleKey.F3, ConsoleKey.F4, ConsoleKey.F5,
        ConsoleKey.F6, ConsoleKey.F7, ConsoleKey.F8, ConsoleKey.F9,
        ConsoleKey.F10, ConsoleKey.F11, ConsoleKey.F12, ConsoleKey.F13,
        ConsoleKey.F14, ConsoleKey.F15, ConsoleKey.F16, ConsoleKey.F17,
        ConsoleKey.F18, ConsoleKey.F19, ConsoleKey.F20, ConsoleKey.F21,
        ConsoleKey.F22, ConsoleKey.F23, ConsoleKey.F24,
    ];

    /// <summary> Gets input from the user (supports multiline input).  </summary>
    public static string GetUserInput(ConsoleLine prompt, bool clear = false, bool skipLine = false, string prefilledText = "", int maxLineCount = 1)
    {
        if (!prompt.IsEmpty()) SendConsoleMessage(prompt); // send out promt

        InputData data = new InputData(prefilledText, maxLineCount);
        UpdatePrimaryConsoleLineAnimation(new ConsoleAnimatedLine(data.AnimationLoop, 20, true), data.lines.startIndex); // init anim

        bool endUserInput = false;
        while (!endUserInput)
        {
            (ConsoleKeyInfo keyInfo, bool interrupted) = UserRealtimeInput.GetKey();
            (bool tab, bool shift, bool ctrl, bool alt) = (UserRealtimeInput.KeyPressed(0x09), UserRealtimeInput.KeyPressed(0x10), UserRealtimeInput.KeyPressed(0x11), UserRealtimeInput.KeyPressed(0x12));

            if (interrupted) // the console has been interrupted and line must be redrawn
            {
                data.Resize();
                continue;
            }
            if (exceptionKeys.Contains(keyInfo.Key)) continue; // keys mess with console so are just voided

            // handle key

            (int leftPointerJumpIndex, int rightPointerJumpIndex) = data.GetPointerJumpInfo(); // get the index of the last and next word based on pointer pos

            switch (keyInfo.Key) // main key handle loop
            {
                case ConsoleKey.Enter:
                    endUserInput = true;
                    break;
                case ConsoleKey.Backspace:
                    if (tab && data.pointer.extension == 0)
                    {
                        data.pointer.extension = leftPointerJumpIndex - data.pointer.index;
                        data.Remove();
                        break;
                    }

                    data.Remove();
                    break;
                case ConsoleKey.UpArrow:
                    if (data.history.index == 0) break; // reached end of history
                    if (data.history.index >= data.history.data.Count && data.input.Length > 0) data.history.data.Add(data.input); // entering history so store current input

                    data.history.index = shift ? 0 : data.history.index - 1;
                    data.pointer = (0, 0);

                    data.input = "";
                    data.Add(data.history.data[data.history.index]);
                    break;
                case ConsoleKey.DownArrow:
                    if (data.history.index >= data.history.data.Count - 1) break; // reached end of history

                    data.history.index = shift ? data.history.data.Count - 1 : data.history.index + 1;
                    data.pointer = (0, 0);

                    data.input = "";
                    data.Add(data.history.data[data.history.index]);
                    break;
                case ConsoleKey.LeftArrow:
                    if (tab) data.UpdatePointer(data.pointer.index, leftPointerJumpIndex - data.pointer.index, true);
                    else if (ctrl) data.UpdatePointer(leftPointerJumpIndex, 0, true);
                    else if (shift) data.UpdatePointer(data.pointer.index, Math.Clamp(data.pointer.extension - 1, -data.pointer.index, data.input.Length - data.pointer.index), true);
                    if (tab || ctrl || shift) break;

                    data.ShiftPointerIndex(-1);
                    break;
                case ConsoleKey.RightArrow:
                    if (tab) data.UpdatePointer(data.pointer.index, rightPointerJumpIndex - data.pointer.index, true);
                    else if (ctrl) data.UpdatePointer(rightPointerJumpIndex, 0, true);
                    else if (shift) data.UpdatePointer(data.pointer.index, Math.Clamp(data.pointer.extension + 1, -data.pointer.index, data.input.Length - data.pointer.index), true);
                    if (tab || ctrl || shift) break;

                    data.ShiftPointerIndex(1);
                    break;
                default:
                    if (ctrl) break; // ctrl blocks keys

                    if (alt)
                    {
                        switch (keyInfo.Key)
                        {
                            case ConsoleKey.S: // jump to start of line
                                data.UpdatePointer(data.GetLineInfo().startIndex, 0, true);
                                break;
                            case ConsoleKey.E: // jump to end of line
                                data.UpdatePointer(data.GetLineInfo().endIndex, 0, true);
                                break;
                            case ConsoleKey.D: // jump to end of input
                                data.UpdatePointer(data.input.Length, 0, true);
                                break;
                            case ConsoleKey.B: // jump to start of input
                                data.UpdatePointer(0, 0, true);
                                break;
                            case ConsoleKey.L: // select line
                                (int startIndex, int endIndex, int length) = data.GetLineInfo();
                                data.UpdatePointer(startIndex, length, true);
                                break;
                            case ConsoleKey.A: // select all
                                data.UpdatePointer(0, data.input.Length, true);
                                break;
                            case ConsoleKey.X: // cut
                                (int index, int length) cutPointer = data.GetPointerFromLeft();
                                StringFunctions.CopyToClipboard($"{data.input} ".Substring(cutPointer.index, cutPointer.length));
                                data.Remove();
                                SendDebugMessage(new ConsoleLine("Contents Cut To Clipboard.", AppRegistry.activeApp.colourScheme.primaryColour));
                                break;
                            case ConsoleKey.C: // copy
                                (int index, int length) copyPointer = data.GetPointerFromLeft();
                                StringFunctions.CopyToClipboard($"{data.input} ".Substring(copyPointer.index, copyPointer.length));
                                SendDebugMessage(new ConsoleLine("Contents Copied To Clipboard.", AppRegistry.activeApp.colourScheme.primaryColour));
                                break;
                            case ConsoleKey.V: // paste
                                if (data.pointer.extension != 0) data.Remove();
                                data.Add(ConsoleLine.Clean(new ConsoleLine(StringFunctions.GetClipboardText())).lineText);
                                SendDebugMessage(new ConsoleLine("Clipboard Contents Pasted.", AppRegistry.activeApp.colourScheme.primaryColour));
                                break;
                        }

                        break; //alt blocks keys
                    }

                    if (data.pointer.extension != 0) data.Remove();

                    data.Add(keyInfo.KeyChar.ToString());
                    break;
            }
            data.Output();
        }

        if (data.history.data.Count == 0 || data.history.data[^1] != data.input) data.history.data.Add(data.input);
        SaveFile(GeneratePath(DataLocation.Console, "History", "UserInput.txt"), data.history.data.TakeLast(Math.Min(data.history.data.Count, int.Parse(SettingsApp.GetValue("Input History")))).ToArray());

        data.Clean(clear);
        ShiftLine(skipLine ? 1 : 0);

        Analytics.General.LinesEntered++;

        return data.input;
    }

    /// <summary> Gets input from the user (input length capped at buffer width). </summary>
    public static string GetUserInput(string promt = "", bool clear = false, bool goNextLine = false, int maxLineCount = 1) { return GetUserInput(new ConsoleLine(promt, AppRegistry.activeApp.colourScheme.primaryColour), clear, goNextLine, "", maxLineCount); }

    /// <summary> Class responsible all of the data and it's manipulation while taking a user's input. </summary>
    class InputData
    {
        public string input = "";

        public (int index, int extension) pointer;
        public (int startIndex, int lastCount, int maxCount) lines;
        public (int index, List<string> data) history;
        public (bool show, bool skipUpdate, ConsoleColor[][] data) anim; //animation

        public int PointerLength => Math.Abs(pointer.extension) + 1;
        public int PointerRelativeIndex => pointer.index + pointer.extension;
        public int LineLength => windowSize.width - 5;

        public InputData(string input, int maxLineCount)
        {
            this.input = input;
            this.pointer = (input.Length, 0);
            this.lines = (primaryLineIndex, 1, maxLineCount);

            this.history.data = [.. LoadFile(GeneratePath(DataLocation.Console, "History", "UserInput.txt"))];
            this.history.index = history.data.Count;

            this.anim = (true, false, new ConsoleColor[maxLineCount][]);

            Output();
        }

        // --- POINTER ---

        // updates the index and extension of the pointer, and if the animation should be forced.
        public void UpdatePointer(int index, int extension, bool forceShowAnimation = false)
        {
            pointer = (index, extension);
            if (forceShowAnimation) ForceShowAnimation();
        }

        // shifts position of the pointer, reseting and applying any extension.
        public void ShiftPointerIndex(int shift)
        {
            if (pointer.extension != 0) pointer.index = Math.Clamp(PointerRelativeIndex, 0, input.Length);
            pointer = (Math.Min(Math.Max(0, pointer.index + shift), input.Length), 0);
        }

        // gets the pointer in the form of the left most index, and the total length of the pointer. 
        public (int index, int length) GetPointerFromLeft()
        {
            return (pointer.extension < 0 ? PointerRelativeIndex : pointer.index, PointerLength);
        }

        // --- INPUT --

        // Adds given string to input string, based on the current location of the pointer.
        public void Add(string s)
        {
            s = s[..Math.Min(LineLength * lines.maxCount - (input.Length + 3), s.Length)];

            input = input.Insert(pointer.index, s);
            ShiftPointerIndex(s.Length);
        }

        // Removes currently targeted text from the input string (via the pointer). 
        public void Remove()
        {
            if (pointer.extension == 0)
            {
                input = input[..(pointer.index - Math.Min(pointer.index, 1))] + input[pointer.index..];
                ShiftPointerIndex(-1);
                return;
            }

            (int index, int length) cur = GetPointerFromLeft();
            ShiftPointerIndex(-1);
            input = input[..cur.index] + ((cur.index + cur.length >= input.Length) ? "" : input[(cur.index + cur.length)..]);
            pointer.index = cur.index;
        }

        // Resizes the input to fit current terminal constraints.
        public void Resize()
        {
            int maxLength = LineLength * lines.maxCount - 3;
            if (maxLength < input.Length) input = input[..maxLength];

            pointer.index = Math.Min(pointer.index, input.Length);
            Output();
        }

        // --- OUTPUT ---

        public void Output()
        {
            string whiteSpacedInput = input + " ";
            int index = 0, lineCount = 0;

            GoToLine(lines.startIndex);
            while (index < whiteSpacedInput.Length)
            {
                int len = Math.Min(index == 0 ? LineLength - 2 : LineLength, whiteSpacedInput.Length - index);
                SendConsoleMessage(CreateConsoleLineData(whiteSpacedInput, index, len, lineCount));

                index += len;
                lineCount += 1;
            }

            if (primaryLineIndex - lineCount != lines.startIndex) lines.startIndex = primaryLineIndex - lineCount; // we hit bottom of console so need to adjust
            if (lines.lastCount > lineCount)
            {
                for (int i = 0; i < lines.lastCount - lineCount; i++) SendConsoleMessage(new ConsoleLine(), ConsoleAnimatedLine.None);
                ShiftLine(-(lines.lastCount - lineCount));
            }

            ShiftLine(-1);

            lines.lastCount = lineCount;
        }

        // Creates a console line based of the input.
        ConsoleLine CreateConsoleLineData(string whiteSpacedInput, int startIndex, int length, int index)
        {
            anim.data[index] = startIndex == 0 ? BuildArray(ConsoleColor.Black.Extend(2), CreateAnimationLineData(whiteSpacedInput, startIndex)) : CreateAnimationLineData(whiteSpacedInput, startIndex);

            return startIndex == 0 ?
            new ConsoleLine($"> {whiteSpacedInput.Substring(startIndex, length)}", BuildArray(AppRegistry.activeApp.colourScheme.secondaryColour, ConsoleColor.White.ToArray()), GetLineAnimationData(index)) :
            new ConsoleLine(whiteSpacedInput.Substring(startIndex, length), ConsoleColor.White.ToArray(), GetLineAnimationData(index));
        }

        // removes cursor animation, and clears input if required.
        public void Clean(bool clear)
        {
            GoToLine(lines.startIndex);
            for (int i = 0; i < lines.lastCount; i++)
            {
                ConsoleLine cl = GetConsoleLine(lines.startIndex + i);
                SendConsoleMessage(clear ? new ConsoleLine() : new ConsoleLine(cl.lineText, cl.lineColour, [ConsoleColor.Black]), ConsoleAnimatedLine.None);

                if (i == lines.lastCount - 1 && cl.lineText == " ") ShiftLine(-1); // edge case where no text on last line but cursor is
            }
            ShiftLine(clear ? -lines.lastCount - 1 : -1);
        }

        // --- INFO ---

        // Gets information about the locations of the left and right jump points for the pointer.
        public (int left, int right) GetPointerJumpInfo()
        {
            string spaceChars = SettingsApp.GetValue("Cursor Jump Separators");

            string s = input + " ";
            int i = PointerRelativeIndex, j = PointerRelativeIndex - 1;

            while (i < s.Length - 1)
            {
                if (!spaceChars.Contains(s[i]) && spaceChars.Contains(s[i + 1])) break;
                i++;
            }
            i++;

            while (j > 0)
            {
                if (!spaceChars.Contains(s[j]) && spaceChars.Contains(s[j - 1])) break;
                j--;
            }

            return (Math.Max(j, 0), Math.Min(i, input.Length));
        }

        // Gets information about the current line based on the pointer.
        public (int startIndex, int endIndex, int length) GetLineInfo()
        {
            int lineIndex = (PointerRelativeIndex + 2) / LineLength;
            int lineStartIndex = lineIndex == 0 ? 0 : lineIndex * LineLength - 2;
            int lineLength = lineIndex == 0 ? Math.Min(LineLength - 3, input.Length - lineStartIndex) : Math.Min(LineLength - 1, input.Length - lineStartIndex);
            return (lineStartIndex, lineStartIndex + lineLength, lineLength);
        }

        // --- ANIMATION ---

        // updates the animation of the input line(s)
        public void AnimationLoop(ConsoleLine lineInfo, ConsoleAnimatedLine animationInfo, int tickNum)
        {
            if (!anim.skipUpdate) anim.show = !anim.show;
            anim.skipUpdate = false;

            for (int i = lines.startIndex; i < lines.startIndex + lines.lastCount; i++)
            {
                ConsoleLine cl = ConsoleAction.GetConsoleLine(i);
                if (!anim.show) UpdatePrimaryConsoleLine(new ConsoleLine(cl.lineText, cl.lineColour, [ConsoleColor.Black]), i);
                else UpdatePrimaryConsoleLine(new ConsoleLine(cl.lineText, cl.lineColour, anim.data[i - lines.startIndex]), i);
            }
        }

        // Creates animation data for a given line.
        ConsoleColor[] CreateAnimationLineData(string s, int startIndex)
        {
            bool partialPointer = false; // if the pointer passes through the line without ending.

            (int index, int length) cur = GetPointerFromLeft();

            if (cur.index < startIndex && cur.index + cur.length > startIndex)
            {
                cur = (startIndex, cur.length - (startIndex - cur.index));
                partialPointer = true;
            }

            List<(ConsoleColor[] arr, int i, int len)> highlights = [];
            if (cur.length == 1) highlights = [(ConsoleColor.White.ToArray(), cur.index - startIndex, 1)];
            else if (pointer.extension < 0) highlights = [(partialPointer ? ConsoleColor.DarkGray.ToArray() : ConsoleColor.White.ToArray(), cur.index - startIndex, 1), (ConsoleColor.DarkGray.ToArray(), cur.index + 1 - startIndex, cur.length - 1)];
            else highlights = [(ConsoleColor.DarkGray.ToArray(), cur.index - startIndex, cur.length - 1), (ConsoleColor.White.ToArray(), cur.index + cur.length - 1 - startIndex, 1)];

            return AdvancedHighlight(s.Length, ConsoleColor.Black, highlights.ToArray());
        }

        // gets the current animation data for a given line
        public ConsoleColor[] GetLineAnimationData(int lineIndex)
        {
            if (anim.show) return anim.data[lineIndex];
            else return [ConsoleColor.Black];
        }

        // forces the animation to show for the rest of the current frame, and the next frame
        public void ForceShowAnimation()
        {
            anim.show = true;
            anim.skipUpdate = true;
        }
    }

    // --- ALL OTHER INPUT FUNCTIONS ---

    /// <summary> Gets input from the user, verifying it matches the conditions of given UserInputProfile. </summary>
    public static string GetValidUserInput(ConsoleLine promt, UserInputProfile inputProfile, string prefilledText = "", int maxLineCount = 1)
    {
        while (true)
        {
            string input = GetUserInput(promt, true, false, prefilledText, maxLineCount);
            if (inputProfile.InputValid(input)) return input;
        }
    }

    /// <summary> Gets input from the user, verifying it matches the conditions of given UserInputProfile. </summary>
    public static string GetValidUserInput(string promt, UserInputProfile inputProfile, string prefilledText = "", int maxLineCount = 1) { return GetValidUserInput(new ConsoleLine(promt, AppRegistry.activeApp.colourScheme.primaryColour), inputProfile, prefilledText, maxLineCount); }

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
    public static int CreateOptionMenu(ConsoleLine title, ConsoleLine[] options, bool clear = true, int cursorStartIndex = 0)
    {
        if (options.Length < 2) return 0;

        if (title.lineText != "") SendConsoleMessage(title);

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

        ConsoleLine[] metaOptions = new ConsoleLine[(title.lineText != "" ? 1 : 0) + metaOptionsLines];
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
                Analytics.General.OptionMenusUsed++;

                if (clear)
                {
                    GoToLine(optionLines[^1]);
                    ClearLines(optionLines[^1] - (optionLines[0] - 1 + (title.lineText == "" ? 1 : 0)), true, true);
                }
                return pointer - optionLines[0];
            }

            for (int i = optionLines[0]; i <= optionLines[^1]; i++)
            {
                consoleLines[i].Update(options[i - optionLines[0]].lineText + (pointer == i ? " <-" : ""));
            }
        }
    }

    /// <summary> Creates menu from given options, allowing user to pick one. </summary>
    public static int CreateOptionMenu(string title, ConsoleLine[] options, bool clear = true, int cursorStartIndex = 0)
    {
        return CreateOptionMenu(new ConsoleLine(title, AppRegistry.activeApp.colourScheme.primaryColour), options, clear, cursorStartIndex);
    }

    /// <summary> Creates menu from given consoleLine options, allowing user to pick one, and invoking the associated action. </summary>
    public static int CreateOptionMenu(string title, (ConsoleLine name, Action action)[] options, bool clear = true, int cursorStartIndex = 0)
    {
        int option = CreateOptionMenu(new ConsoleLine(title, AppRegistry.activeApp.colourScheme.primaryColour), options.Select(option => option.name).ToArray(), clear, cursorStartIndex);
        options[option].action.Invoke();
        return option;
    }

    /// <summary> Creates menu from given text options, allowing user to pick one, and invoking the associated action. </summary>
    public static int CreateOptionMenu(ConsoleLine title, (string name, Action action)[] options, bool clear = true, int cursorStartIndex = 0)
    {
        int option = CreateOptionMenu(title, options.Select(option => new ConsoleLine(option.name)).ToArray(), clear, cursorStartIndex);
        options[option].action.Invoke();
        return option;
    }

    /// <summary> Creates menu from given text options, allowing user to pick one, and invoking the associated action. </summary>
    public static int CreateOptionMenu(string title, (string name, Action action)[] options, bool clear = true, int cursorStartIndex = 0)
    {
        return CreateOptionMenu(new ConsoleLine(title, AppRegistry.activeApp.colourScheme.primaryColour), options, clear, cursorStartIndex);
    }

    /// <summary> Creates menu from given text options, allowing user to pick one. </summary>
    public static int CreateOptionMenu(string title, string[] options, bool clear = true, int cursorStartIndex = 0)
    {
        return CreateOptionMenu(new ConsoleLine(title, AppRegistry.activeApp.colourScheme.primaryColour), options.Select(option => new ConsoleLine(option)).ToArray(), clear, cursorStartIndex);
    }

    /// <summary> Creates menu, allowing user to select either yes or no. </summary>
    public static bool CreateTrueFalseOptionMenu(string title, string trueOption = "Yes", string falseOption = "No", bool clear = true, int cursorStartIndex = 0)
    {
        if (CreateOptionMenu(new ConsoleLine(title, AppRegistry.activeApp.colourScheme.primaryColour), [new ConsoleLine(trueOption, AppRegistry.activeApp.colourScheme.secondaryColour), new ConsoleLine(falseOption, AppRegistry.activeApp.colourScheme.secondaryColour)], clear, cursorStartIndex) == 0) return true;
        return false;
    }

    /// <summary> Creates menu from given options, spliting into pages, allowing user to pick one (pinnedOptions return negative index). </summary>
    public static int CreateMultiPageOptionMenu(string title, ConsoleLine[] options, ConsoleLine[] pinnedOptions, int optionsPerPage, int cursorStartIndex = 0)
    {
        if (options.Length + pinnedOptions.Length < 2) return 0;

        for (int i = 0; i < pinnedOptions.Length; i++) { pinnedOptions[i].Update(AppRegistry.activeApp.colourScheme.primaryColour.ToArray()); }

        ConsoleLine[] pgExtraOptions = new ConsoleLine[] {
                    new ConsoleLine("Next Page", AppRegistry.activeApp.colourScheme.primaryColour),
                    new ConsoleLine("Previous Page", AppRegistry.activeApp.colourScheme.primaryColour)
                }.Concat(pinnedOptions).ToArray();

        int currentPage = cursorStartIndex / optionsPerPage, totalPages = (options.Length - 1) / optionsPerPage, resultIndex = cursorStartIndex % optionsPerPage;

        metaOptionsLines = 1;
        while (true)
        {
            ConsoleLine[] pgOptions = options.Skip(currentPage * optionsPerPage).Take(Math.Min(optionsPerPage, options.Length - currentPage * optionsPerPage)).Concat(pgExtraOptions).ToArray();

            SendConsoleMessage(new ConsoleLine($"--- {title} Page [{currentPage + 1}/{totalPages + 1}] ---",
            BuildArray(AppRegistry.activeApp.colourScheme.primaryColour.Extend(title.Length + 10),
            AppRegistry.activeApp.colourScheme.secondaryColour.Extend($"[{currentPage + 1}/{currentPage + 1}]".Length, true),
            AppRegistry.activeApp.colourScheme.primaryColour.Extend(4))));

            resultIndex = CreateOptionMenu("", pgOptions.Select(o => new ConsoleLine(o)).ToArray(), cursorStartIndex: resultIndex);

            if (resultIndex == pgOptions.Length - 2 - pinnedOptions.Length)
            {
                currentPage = currentPage < totalPages ? currentPage + 1 : 0;
                resultIndex = 0;
            }
            else if (resultIndex == pgOptions.Length - 1 - pinnedOptions.Length)
            {
                currentPage = currentPage > 0 ? currentPage - 1 : totalPages;
                resultIndex = 0;
            }
            else if (resultIndex > pgOptions.Length - 2 - pinnedOptions.Length)
            {
                ClearLines(updateCurrentLine: true);
                metaOptionsLines = 0;
                return -resultIndex + (pgOptions.Length - (pgExtraOptions.Length - 1));
            }
            else
            {
                ClearLines(updateCurrentLine: true);
                metaOptionsLines = 0;
                return resultIndex + currentPage * optionsPerPage;
            }

            ClearLines(updateCurrentLine: true);
        }
    }

    /// <summary> Creates menu from given options, spliting into pages, allowing user to pick one. </summary>
    public static int CreateMultiPageOptionMenu(string title, ConsoleLine[] options, int optionsPerPage, int cursorStartIndex = 0) { return CreateMultiPageOptionMenu(title, options, new ConsoleLine[0], optionsPerPage, cursorStartIndex); }

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

    internal static int CreateOptionMenu(string v, object value, int cursorStartIndex)
    {
        throw new NotImplementedException();
    }
}
