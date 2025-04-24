using Revistone.Console;
using Revistone.Functions;
using Revistone.App.BaseApps;
using Revistone.App;
using Revistone.Management;

using static Revistone.Console.Data.ConsoleData;
using static Revistone.Console.ConsoleAction;
using static Revistone.Functions.ColourFunctions;
using static Revistone.Functions.PersistentDataFunctions;
using System.Security.Cryptography.X509Certificates;

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
    public static string GetUserInput(ConsoleLine prompt, bool clear = false, bool skipLine = false, string prefilledText = "", string inputPrefix = "> ", int maxLineCount = 1)
    {
        if (!prompt.IsEmpty()) SendConsoleMessage(prompt); // send out promt

        InputData data = new(prefilledText, inputPrefix, maxLineCount);
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
                                SendDebugMessage(new ConsoleLine("Contents Cut To Clipboard.", AppRegistry.PrimaryCol));
                                break;
                            case ConsoleKey.C: // copy
                                (int index, int length) copyPointer = data.GetPointerFromLeft();
                                StringFunctions.CopyToClipboard($"{data.input} ".Substring(copyPointer.index, copyPointer.length));
                                SendDebugMessage(new ConsoleLine("Contents Copied To Clipboard.", AppRegistry.PrimaryCol));
                                break;
                            case ConsoleKey.V: // paste
                                if (data.pointer.extension != 0) data.Remove();
                                data.Add(ConsoleLine.Clean(new ConsoleLine(StringFunctions.GetClipboardText())).lineText);
                                SendDebugMessage(new ConsoleLine("Clipboard Contents Pasted.", AppRegistry.PrimaryCol));
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
        SaveFile(GeneratePath(DataLocation.Console, "History", "UserInput.txt"), [.. data.history.data.TakeLast(Math.Min(data.history.data.Count, int.Parse(SettingsApp.GetValue("Input History"))))]);

        data.Clean(clear);
        ShiftLine(skipLine ? 1 : 0);

        Analytics.General.LinesEntered++;

        return data.input;
    }

    /// <summary> Gets input from the user (input length capped at buffer width). </summary>
    public static string GetUserInput(string promt = "", bool clear = false, bool goNextLine = false, string prefilledText = "", string inputPrefix = "> ", int maxLineCount = 1) { return GetUserInput(new ConsoleLine(promt, AppRegistry.PrimaryCol), clear, goNextLine, prefilledText, inputPrefix, maxLineCount); }

    /// <summary> Class responsible all of the data and it's manipulation while taking a user's input. </summary>
    private class InputData
    {
        public string input = "";
        public readonly string inputPrefix;

        public (int index, int extension) pointer;
        public (int startIndex, int lastCount, int maxCount) lines;
        public (int index, List<string> data) history;
        public (bool show, bool skipUpdate, ConsoleColor[][] data) anim; //animation

        public int PointerLength => Math.Abs(pointer.extension) + 1;
        public int PointerRelativeIndex => pointer.index + pointer.extension;
        public int LineLength => windowSize.width - 1;

        public readonly ConsoleColor[] inputColour;
        public readonly ConsoleColor[] cursorColour;
        public readonly ConsoleColor[] cursorTrailColour;

        public InputData(string input, string inputPrefix, int maxLineCount)
        {
            this.input = input;
            this.inputPrefix = inputPrefix;
            this.pointer = (input.Length, 0);
            this.lines = (primaryLineIndex, 1, maxLineCount);

            this.history.data = [.. LoadFile(GeneratePath(DataLocation.Console, "History", "UserInput.txt"))];
            this.history.index = history.data.Count;

            this.anim = (true, false, new ConsoleColor[maxLineCount][]);

            this.inputColour = SettingsApp.GetValueAsConsoleColour("Input Text Colour");
            this.cursorColour = SettingsApp.GetValueAsConsoleColour("Input Cursor Colour");
            this.cursorTrailColour = SettingsApp.GetValueAsConsoleColour("Input Cursor Trail Colour");

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
            s = s[..Math.Min(LineLength * lines.maxCount - (input.Length + inputPrefix.Length + 1), s.Length)];

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
            int maxLength = LineLength * lines.maxCount - inputPrefix.Length - 1;
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
                int len = Math.Min(index == 0 ? LineLength - inputPrefix.Length : LineLength, whiteSpacedInput.Length - index);
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
            anim.data[index] = startIndex == 0 ? BuildArray(ConsoleColor.Black.Extend(inputPrefix.Length), CreateAnimationLineData(whiteSpacedInput, startIndex)) : CreateAnimationLineData(whiteSpacedInput, startIndex);

            return startIndex == 0 ?
            new ConsoleLine($"{inputPrefix}{whiteSpacedInput.Substring(startIndex, length)}", BuildArray(AppRegistry.SecondaryCol.Extend(inputPrefix.Length), inputColour), GetLineAnimationData(index)) :
            new ConsoleLine(whiteSpacedInput.Substring(startIndex, length), inputColour, GetLineAnimationData(index));
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
            int lineIndex = (PointerRelativeIndex + inputPrefix.Length) / LineLength;
            int lineStartIndex = lineIndex == 0 ? 0 : lineIndex * LineLength - inputPrefix.Length;
            int lineLength = lineIndex == 0 ? Math.Min(LineLength - inputPrefix.Length - 1, input.Length - lineStartIndex) : Math.Min(LineLength - 1, input.Length - lineStartIndex);
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
                ConsoleLine cl = GetConsoleLine(i);
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

            List<(ConsoleColor[] arr, int i, int len)> highlights;
            if (cur.length == 1) highlights = [(cursorColour, cur.index - startIndex, 1)];
            else if (pointer.extension < 0) highlights = [(partialPointer ? cursorTrailColour : cursorColour, cur.index - startIndex, 1), (cursorTrailColour, cur.index + 1 - startIndex, cur.length - 1)];
            else highlights = [(cursorTrailColour, cur.index - startIndex, cur.length - 1), (cursorColour, cur.index + cur.length - 1 - startIndex, 1)];

            return AdvancedHighlight(s.Length, ConsoleColor.Black, [.. highlights]);
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
    public static string GetValidUserInput(ConsoleLine promt, UserInputProfile inputProfile, string prefilledText = "", string inputPrefix = "> ", int maxLineCount = 1)
    {
        while (true)
        {
            string input = GetUserInput(promt, true, false, prefilledText, inputPrefix, maxLineCount);
            if (inputProfile.InputValid(input)) return input;
        }
    }

    /// <summary> Gets input from the user, verifying it matches the conditions of given UserInputProfile. </summary>
    public static string GetValidUserInput(string promt, UserInputProfile inputProfile, string prefilledText = "", string inputPrefix = "> ", int maxLineCount = 1) { return GetValidUserInput(new ConsoleLine(promt, AppRegistry.PrimaryCol), inputProfile, prefilledText, inputPrefix, maxLineCount); }

    /// <summary> Waits for user to input given key, pausing program until pressed. Returns the key inputted. </summary>
    public static ConsoleKey WaitForUserInput(ConsoleKey[] keys, bool clear = true, bool space = false, bool goNextLine = false)
    {
        if (keys.Length == 0) return ConsoleKey.None;

        if (space) ShiftLine();

        ConsoleColor[] colours = AdvancedHighlight(30, AppRegistry.PrimaryCol, (AppRegistry.SecondaryCol, 6, 2 + keys[0].ToString().Length));

        ConsoleLine[] waitLines = [
        new ConsoleLine($"Press [{keys[0]}] To Continue", colours), new ConsoleLine($"Press [{keys[0]}] To Continue.", colours),
        new ConsoleLine($"Press [{keys[0]}] To Continue..", colours), new ConsoleLine($"Press [{keys[0]}] To Continue...", colours) ];

        SendConsoleMessage(waitLines[1], ConsoleLineUpdate.SameLine,
        new ConsoleAnimatedLine(ConsoleAnimatedLine.CycleLines, tickMod: 12, metaInfo: (1, waitLines), enabled: true));
        int index = primaryLineIndex;

        while (true)
        {
            (ConsoleKeyInfo keyInfo, bool interrupted) c = UserRealtimeInput.GetKey();

            if (c.interrupted)
            {
                SendConsoleMessage(waitLines[1], ConsoleLineUpdate.SameLine,
                new ConsoleAnimatedLine(ConsoleAnimatedLine.CycleLines, tickMod: 12, metaInfo: (1, waitLines), enabled: true));
                index = primaryLineIndex;
                continue;
            }

            foreach (ConsoleKey key in keys)
            {
                if (c.keyInfo.Key == key)
                {
                    GoToLine(index); //prevents other functions and tasks messing with clear
                    if (clear) UpdatePrimaryConsoleLine(new ConsoleLine(""), ConsoleAnimatedLine.None, index);
                    if (goNextLine) ShiftLine();
                    return key;
                }
            }
        }
    }

    /// <summary> Waits for user to input given key, pausing program until pressed. Returns the key inputted. </summary>
    public static ConsoleKey WaitForUserInput(ConsoleKey key = ConsoleKey.Enter, bool clear = true, bool space = false, bool goNextLine = false) { return WaitForUserInput([key], clear, space, goNextLine); }

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
            BuildArray(AppRegistry.SecondaryCol.Extend(2), options[i].lineColour.Extend(options[i].lineText.Length), AppRegistry.SecondaryCol.Extend(3)));
            optionLines[i] = SendConsoleMessage(options[i]);
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
            bool shift = UserRealtimeInput.KeyPressed(0x10);

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

            if ((c.keyInfo.Key >= ConsoleKey.D1 && c.keyInfo.Key <= ConsoleKey.D9) || (c.keyInfo.Key >= ConsoleKey.NumPad1 && c.keyInfo.Key <= ConsoleKey.NumPad9))
            {
                pointer = optionLines[0] + Math.Min(c.keyInfo.Key > ConsoleKey.D9 ? c.keyInfo.Key - ConsoleKey.NumPad1 : c.keyInfo.Key - ConsoleKey.D1, optionLines.Length - 1);
            }
            else
            {
                switch (c.keyInfo.Key)
                {
                    case ConsoleKey.W or ConsoleKey.UpArrow or ConsoleKey.J:
                        if (shift) pointer = optionLines[0];
                        else pointer = pointer - 1 < optionLines[0] ? optionLines[^1] : pointer - 1;
                        break;
                    case ConsoleKey.S or ConsoleKey.DownArrow or ConsoleKey.K:
                        if (shift) pointer = optionLines[^1];
                        else pointer = pointer + 1 > optionLines[^1] ? optionLines[0] : pointer + 1;
                        break;
                    case ConsoleKey.Enter:
                        Analytics.General.OptionMenusUsed++;

                        if (clear)
                        {
                            GoToLine(optionLines[^1]);
                            ClearLines(optionLines[^1] - (optionLines[0] - 1 + (title.lineText == "" ? 1 : 0)), true, true);
                        }
                        return pointer - optionLines[0];
                }
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
        return CreateOptionMenu(new ConsoleLine(title, AppRegistry.PrimaryCol), options, clear, cursorStartIndex);
    }

    /// <summary> Creates menu from given consoleLine options, allowing user to pick one, and invoking the associated action. </summary>
    public static int CreateOptionMenu(string title, (ConsoleLine name, Action action)[] options, bool clear = true, int cursorStartIndex = 0)
    {
        int option = CreateOptionMenu(new ConsoleLine(title, AppRegistry.PrimaryCol), options.Select(option => option.name).ToArray(), clear, cursorStartIndex);
        options[option].action.Invoke();
        return option;
    }

    /// <summary> Creates menu from given text options, allowing user to pick one, and invoking the associated action. </summary>
    public static int CreateOptionMenu(ConsoleLine title, (string name, Action action)[] options, bool clear = true, int cursorStartIndex = 0)
    {
        int option = CreateOptionMenu(title, options.Select(option => new ConsoleLine(option.name, AppRegistry.SecondaryCol)).ToArray(), clear, cursorStartIndex);
        options[option].action.Invoke();
        return option;
    }

    /// <summary> Creates menu from given text options, allowing user to pick one, and invoking the associated action. </summary>
    public static int CreateOptionMenu(string title, (string name, Action action)[] options, bool clear = true, int cursorStartIndex = 0)
    {
        return CreateOptionMenu(new ConsoleLine(title, AppRegistry.PrimaryCol), options, clear, cursorStartIndex);
    }

    /// <summary> Creates menu from given text options, allowing user to pick one. </summary>
    public static int CreateOptionMenu(string title, string[] options, bool clear = true, int cursorStartIndex = 0)
    {
        return CreateOptionMenu(new ConsoleLine(title, AppRegistry.PrimaryCol), options.Select(option => new ConsoleLine(option, AppRegistry.SecondaryCol)).ToArray(), clear, cursorStartIndex);
    }

    /// <summary> Creates menu, allowing user to select either yes or no. </summary>
    public static bool CreateTrueFalseOptionMenu(ConsoleLine title, string trueOption = "Yes", string falseOption = "No", bool clear = true, int cursorStartIndex = 0)
    {
        if (CreateOptionMenu(title, [new ConsoleLine(trueOption, AppRegistry.SecondaryCol), new ConsoleLine(falseOption, AppRegistry.SecondaryCol)], clear, cursorStartIndex) == 0) return true;
        return false;
    }

    /// <summary> Creates menu, allowing user to select either yes or no. </summary>
    public static bool CreateTrueFalseOptionMenu(string title, string trueOption = "Yes", string falseOption = "No", bool clear = true, int cursorStartIndex = 0)
    {
        return CreateTrueFalseOptionMenu(new ConsoleLine(title, AppRegistry.PrimaryCol), trueOption, falseOption, clear, cursorStartIndex);
    }

    /// <summary> Creates menu from given options, spliting into pages, allowing user to pick one (pinnedOptions return negative index). </summary>
    public static int CreateMultiPageOptionMenu(string title, ConsoleLine[] options, ConsoleLine[] pinnedOptions, int optionsPerPage, int cursorStartIndex = 0)
    {
        if (options.Length + pinnedOptions.Length < 2) return 0;

        cursorStartIndex = Math.Min(options.Length - 1, cursorStartIndex);

        for (int i = 0; i < pinnedOptions.Length; i++) { pinnedOptions[i].Update(AppRegistry.PrimaryCol.ToArray()); }

        ConsoleLine[] pgExtraOptions = [
                    new ConsoleLine("Next Page", AppRegistry.PrimaryCol),
                    new ConsoleLine("Last Page", AppRegistry.PrimaryCol),
                    .. pinnedOptions,
                ];

        int currentPage = cursorStartIndex / optionsPerPage, totalPages = (options.Length - 1) / optionsPerPage, resultIndex = cursorStartIndex % optionsPerPage;

        metaOptionsLines = 1;
        while (true)
        {
            ConsoleLine[] pgOptions =
            [
                .. options.Skip(currentPage * optionsPerPage).Take(Math.Min(optionsPerPage, options.Length - currentPage * optionsPerPage)),
                .. pgExtraOptions,
            ];

            SendConsoleMessage(new ConsoleLine($"--- {title} Page [{currentPage + 1}/{totalPages + 1}] ---",
            BuildArray(AppRegistry.PrimaryCol.Extend(title.Length + 10),
            AppRegistry.SecondaryCol.Extend($"[{currentPage + 1}/{currentPage + 1}]".Length, true),
            AppRegistry.PrimaryCol.Extend(4))));

            resultIndex = CreateOptionMenu("", pgOptions.Select(o => new ConsoleLine(o)).ToArray(), cursorStartIndex: resultIndex);

            if (resultIndex == pgOptions.Length - 2 - pinnedOptions.Length)
            {
                currentPage = currentPage < totalPages ? currentPage + 1 : 0;
                resultIndex = Math.Min(options.Skip(currentPage * optionsPerPage).Count(), optionsPerPage);
            }
            else if (resultIndex == pgOptions.Length - 1 - pinnedOptions.Length)
            {
                currentPage = currentPage > 0 ? currentPage - 1 : totalPages;
                resultIndex = Math.Min(options.Skip(currentPage * optionsPerPage).Count(), optionsPerPage) + 1;
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
    public static int CreateMultiPageOptionMenu(string title, ConsoleLine[] options, int optionsPerPage, int cursorStartIndex = 0) { return CreateMultiPageOptionMenu(title, options, [], optionsPerPage, cursorStartIndex); }

    /// <summary> Creates menu from given messages, spliting into pages, allowing user to view. </summary>
    public static void CreateReadMenu(string title, int messagesPerPage, (ConsoleLine name, Action action)[] extraOptions, params ConsoleLine[] messages)
    {
        int page = 0, pages = (messages.Length - 1) / messagesPerPage, pageLength;
        int cursorIndex = 0;

        while (true)
        {
            SendConsoleMessage(new ConsoleLine($"--- {title} Page [{page + 1}/{pages + 1}] ---",
            BuildArray(AppRegistry.PrimaryCol.Extend(title.Length + 10),
            AppRegistry.SecondaryCol.Extend($"[{page + 1}/{pages + 1}]".Length, true),
            AppRegistry.PrimaryCol.Extend(4))));


            pageLength = Math.Min(page * messagesPerPage + messagesPerPage, messages.Length) - page * messagesPerPage;
            metaOptionsLines = 1 + pageLength;

            for (int i = page * messagesPerPage; i < Math.Min(page * messagesPerPage + messagesPerPage, messages.Length); i++)
            {
                SendConsoleMessage(messages[i]);
            }

            cursorIndex = CreateOptionMenu("", extraOptions.Concat([
                    (new ConsoleLine("Next Page", AppRegistry.PrimaryCol), () => page = page < pages ? page + 1 : 0),
                    (new ConsoleLine("Last Page", AppRegistry.PrimaryCol), () => page = page > 0 ? page - 1 : pages),
                    (new ConsoleLine("Exit", AppRegistry.PrimaryCol), () => {})]).ToArray(), cursorStartIndex: cursorIndex);

            if (cursorIndex == 2)
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
        CreateReadMenu(title, messagesPerPage, [], messages);
    }

    /// <summary> Creates menu from given messages, spliting into pages, allowing user to view. </summary>
    public static void CreateReadMenu(string title, int messagesPerPage, params string[] messages)
    {
        CreateReadMenu(title, messagesPerPage, messages.Select(s => new ConsoleLine(s)).ToArray());
    }

    /// <summary> Creates menu from given messages, split by category, spliting into pages, allowing user to view. </summary>
    public static void CreateCategorisedReadMenu(string title, int messagesPerPage, params (string title, ConsoleLine[] messages)[] categories)
    {
        int pointer = 0;
        while (true)
        {
            (ConsoleLine, Action)[] options = [.. categories.Select(c => (new ConsoleLine(c.title, AppRegistry.SecondaryCol), (Action)(() => { CreateReadMenu($"{c.title}:", messagesPerPage, c.messages); })))];
            options = [.. options, (new ConsoleLine("Exit", AppRegistry.PrimaryCol), () => { })];

            pointer = CreateOptionMenu($"--- {title} ---", options, cursorStartIndex: pointer);

            if (pointer == options.Length - 1) return; // exit option
        }
    }

    /// <summary> Creates menu from given messages, split by category, spliting into pages, allowing user to view. </summary>
    public static void CreateCategorisedReadMenu(string title, int messagesPerPage, params (string title, string[] messages)[] categories)
    {
        CreateCategorisedReadMenu(title, messagesPerPage, categories.Select(c => (c.title, c.messages.Select(m => new ConsoleLine(m, AppRegistry.SecondaryCol)).ToArray())).ToArray());
    }

    ///<summary> Allows user to edit, remove, and append multiple lines to a given string[]. </summary>
    public static string[] GetMultiUserInput(string title, string[] content, int minDisplayLineCount = 3, int maxDisplayLineCount = 10)
    {
        MultiInputData data = new(title, content, minDisplayLineCount, maxDisplayLineCount);

        while (true)
        {
            (ConsoleKeyInfo key, bool interrupted) = UserRealtimeInput.GetKey();
            (bool tab, bool shift) = (UserRealtimeInput.KeyPressed(0x09), UserRealtimeInput.KeyPressed(0x10));

            if (interrupted)
            {
                data.RenderWindow();
                continue;
            }

            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    primaryLineIndex = data.window.top + 1 + data.curOffset;
                    data.content[data.curIndex + data.curOffset] = GetUserInput(prefilledText: data.content[data.curIndex + data.curOffset],
                    inputPrefix: $"{(data.curIndex + data.curOffset + 1).ToString().PadLeft(data.window.botLine)}. ");
                    data.RenderLine(data.curOffset);
                    break;
                case ConsoleKey.W or ConsoleKey.UpArrow or ConsoleKey.J:
                    if (tab)
                    {
                        if (data.content[data.curIndex + data.curOffset].Trim().Length == 0) data.content.RemoveAt(data.curIndex + data.curOffset);
                    }
                    else data.ShiftCur(shift ? -data.window.height : -1);
                    data.RenderWindow();
                    break;
                case ConsoleKey.S or ConsoleKey.DownArrow or ConsoleKey.K:
                    if (tab) data.content.Insert(data.curIndex + data.curOffset, "");
                    else data.ShiftCur(shift ? data.window.height : 1);
                    data.RenderWindow();
                    break;
                case ConsoleKey.E:
                    while (data.content.Count > 0 && data.content[^1].Trim().Length == 0) data.content.RemoveAt(data.content.Count - 1);
                    primaryLineIndex = data.window.top + data.window.height + 1;
                    ClearLines(data.window.height + 1, true);
                    return [.. data.content];
            }
        }
    }

    /// <summary> Class responsible all of the data and it's manipulation while taking a user's multi line input. </summary>
    private class MultiInputData
    {
        public string title;
        public List<string> content;

        public int curOffset = 0; // offset of cur from top of window
        public int curIndex = 0; // index of top of window

        public (int height, int top, int min, int max, int botLine) window;

        public ConsoleColor[] inputColour;
        public ConsoleLine[] borders = new ConsoleLine[2];

        public MultiInputData(string title, string[] content, int minDisplayLineCount, int maxDisplayLineCount)
        {
            this.title = title;
            this.content = content.Length == 0 ? [""] : [.. content];

            window = (-1, -1, minDisplayLineCount, maxDisplayLineCount, -1);
            inputColour = SettingsApp.GetValueAsConsoleColour("Input Text Colour");

            GenerateBorders();
            RenderWindow();
        }

        ///<summary> Generates the console lines for the borders of the window </summary>
        void GenerateBorders()
        {
            int borderLen = Math.Max(title.Length + 8, title.Length % 2 == 0 ? 20 : 19);
            int borderHalf = (borderLen - title.Length) / 2 - 1;
            borders[0] = new ConsoleLine($"{new string('-', borderHalf)} {title} {new string('-', borderHalf)}",
            BuildArray(AppRegistry.PrimaryCol.Extend(borderHalf), AppRegistry.SecondaryCol.Extend(title.Length + 1), AppRegistry.PrimaryCol));

            borderHalf = (borderLen - 11) / 2 - 1;
            borders[1] = new ConsoleLine($"{new string('-', borderHalf)} [E] To Exit {(title.Length % 2 == 0 ? "-" : "")}{new string('-', borderHalf)}",
            BuildArray(AppRegistry.PrimaryCol.Extend(borderHalf), AppRegistry.SecondaryCol.Extend(4), AppRegistry.PrimaryCol));
        }

        ///<summary> Renders the view window. </summary>
        public void RenderWindow()
        {
            if (window.top != -1)
            {
                GoToLine(window.top + window.height + 1); // if we are rerendering, we overwrite the old window
                ClearLines(window.height + 1, true);
            }
            window.top = primaryLineIndex;

            window.height = Math.Clamp(content.Count + 1, window.min, window.max); // looks weird to have a bunch of empty lines

            SendConsoleMessage(borders[0]);

            window.botLine = (curIndex + window.height).ToString().Length;
            for (int i = 0; i < window.height; i++)
            {
                SendConsoleMessage(new ConsoleLine($"{(curIndex + i + 1).ToString().PadLeft(window.botLine)}. " + (curIndex + i < content.Count ? content[curIndex + i] : ""),
                BuildArray(i == curOffset ? AppRegistry.SecondaryCol.Extend(window.botLine + 1) : AppRegistry.PrimaryCol.Extend(window.botLine + 1), inputColour)));
            }

            SendConsoleMessage(borders[1]);
            window.top -= window.height - (primaryLineIndex - window.top - 2);
        }

        ///<summary> Renders specific line in the window. </summary>
        public void RenderLine(int i)
        {
            UpdatePrimaryConsoleLine(new ConsoleLine($"{(curIndex + i + 1).ToString().PadLeft(window.botLine)}. " + (curIndex + i < content.Count ? content[curIndex + i] : ""), BuildArray(i == curOffset ? AppRegistry.SecondaryCol.Extend(window.botLine + 1) : AppRegistry.PrimaryCol.Extend(window.botLine + 1), inputColour)), window.top + 1 + i);
        }

        ///<summary> Shifts the cursor index. </summary>
        public void ShiftCur(int shift)
        {
            curOffset += shift;

            int dif = 0;

            if (curOffset < 0)
            {
                dif = curOffset;
                curOffset = 0;
            }
            else if (curOffset > window.height - 1)
            {
                dif = curOffset - window.height + 1;
                curOffset = window.height - 1;
            }

            curIndex = Math.Max(curIndex + dif, 0);

            while (curIndex + curOffset > content.Count - 1) content.Add("");
            while (content.Count - 1 > curIndex + curOffset && content[^1].Trim().Length == 0) content.RemoveAt(content.Count - 1);
        }
    }
}
