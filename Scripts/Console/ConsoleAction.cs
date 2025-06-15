using Revistone.App;
using Revistone.Management;

using static Revistone.Console.Data.ConsoleData;

namespace Revistone.Console;

/// <summary> Handles all functions for apps to interact with the console. </summary>
public static class ConsoleAction
{
    // --- BOUNDS CHECKS ---

    /// <summary> Returns if index in bounds of primary display. </summary>
    public static bool InPrimaryConsole(int lineIndex)
    {
        return lineIndex > 0 && lineIndex < debugStartIndex;
    }

    /// <summary> Returns if index in bounds of debug console. </summary>
    public static bool InDebugConsole(int lineIndex)
    {
        return lineIndex > debugStartIndex && lineIndex < consoleLines.Length;
    }

    /// <summary> Returns if index in bounds of console display. </summary>
    public static bool InConsole(int lineIndex)
    {
        return lineIndex >= 0 && lineIndex < consoleLines.Length;
    }

    /// <summary> Clamps primary line index into primary console. </summary>
    public static void ClampPrimaryLineIndex()
    {
        primaryLineIndex = Math.Clamp(primaryLineIndex, 1, debugStartIndex - 1);
    }

    /// <summary> Clamps debug line index into debug console. </summary>
    public static void ClampDebugLineIndex()
    {
        debugLineIndex = Math.Clamp(debugLineIndex, windowSize.height - 7, windowSize.height - 2);
    }

    // --- LINE EXCEPTIONS ---

    /// <summary> Updates status of a line exception. </summary>
    public static void UpdateLineExceptionStatus(bool status, int lineIndex)
    {
        if (!InConsole(lineIndex)) return;
        exceptionLines[lineIndex] = status;
    }

    /// <summary> Gets status of a line exception. </summary>
    public static bool GetLineExceptionStatus(int lineIndex)
    {
        return InConsole(lineIndex) ? exceptionLines[lineIndex] : false;
    }

    // --- GET LINE ---

    /// <summary> Gets current value of consoleLineIndex. </summary>
    public static int GetConsoleLineIndex()
    {
        return primaryLineIndex;
    }

    /// <summary> Gets current value of debugLineIndex. </summary>
    public static int GetDebugLineIndex()
    {
        return debugLineIndex;
    }

    /// <summary> Gets ConsoleLine at given index. </summary>
    public static ConsoleLine GetConsoleLine(int lineIndex)
    {
        return InConsole(lineIndex) ? consoleLines[lineIndex] : new ConsoleLine();
    }

    /// <summary> Gets ConsoleAnimatedLine at given index. </summary>
    public static ConsoleAnimatedLine GetAnimatedConsoleLine(int lineIndex)
    {
        return InConsole(lineIndex) ? consoleLineUpdates[lineIndex] : new ConsoleAnimatedLine();
    }

    // --- CLEAR LINES ---

    /// <summary> Clears primary console area. </summary>
    public static void ClearPrimaryConsole()
    {
        int lineIndex = 1;
        for (int i = 1; i < debugStartIndex; i++)
        {
            if (exceptionLines[i])
            {
                lineIndex++;
                continue;
            }

            consoleLineUpdates[i].Update();
            consoleLines[i].Update("");
        }

        primaryLineIndex = lineIndex;
    }

    /// <summary> Clears debug console area. </summary>
    public static void ClearDebugConsole()
    {
        int lineIndex = debugStartIndex + 1;
        for (int i = debugStartIndex + 1; i < windowSize.height - 1; i++)
        {
            if (exceptionLines[i])
            {
                lineIndex++;
                continue;
            }
            consoleLineUpdates[i].Update();
            consoleLines[i].Update("");
        }

        debugLineIndex = lineIndex;
    }

    // <summary> Clears previous [count] lines. </summary>
    public static void ClearLines(int count = 1, bool updateCurrentLine = false, bool bypassClamp = false)
    {
        if (!bypassClamp) ClampPrimaryLineIndex();
        consoleLineUpdates[primaryLineIndex].Update();
        for (int i = primaryLineIndex; i >= primaryLineIndex - count; i--)
        {
            if (!InPrimaryConsole(i) || exceptionLines[i]) continue;
            consoleLineUpdates[i].Update();
            consoleLines[i].Update("");
        }

        if (updateCurrentLine) primaryLineIndex -= count;
    }

    // --- GOTO LINE ---

    /// <summary> Shifts consoleLineIndex via given shift. </summary>
    public static void ShiftLine(int shift = 1)
    {
        primaryLineIndex = Math.Clamp(primaryLineIndex + shift, 1, debugStartIndex);
    }

    /// <summary> Sets consoleLineIndex to given line. </summary>
    public static void GoToLine(int lineIndex)
    {
        primaryLineIndex = Math.Clamp(lineIndex, 1, debugStartIndex);
    }

    // --- GENERAL CONSOLE ---

    /// <summary> Updates lineInfo at given index, adjusting position of ConsoleLines within console if needed, returns index of updated line.  </summary>
    public static int UpdateEnclosedConsole(ConsoleLine lineInfo, ConsoleLineUpdate updateInfo, int consoleTop, int consoleBot, ref int consoleIndex)
    {
        if (consoleLines.Length == 0 || consoleLines[0] == null || consoleLines.Length < AppRegistry.activeApp.minHeightBuffer) return -1;

        if (consoleIndex > consoleBot) //need to move back in console
        {
            for (int i = consoleTop; i <= consoleBot; i++)
            {
                int traceBack = 1;
                while (true)
                {
                    if (i - traceBack < consoleTop - 1) break;

                    if (exceptionLines[i - traceBack])
                    {
                        traceBack++;
                        continue;
                    }

                    consoleLines[i - traceBack].Update(consoleLines[i]);
                    consoleLineUpdates[i - traceBack].Update(consoleLineUpdates[i]);

                    break;
                }
            }
            consoleLineUpdates[consoleBot] = ConsoleAnimatedLine.None; 
            consoleIndex = consoleBot;
        }

        int updatedIndex = consoleIndex;

        if (updateInfo.append)
        {
            consoleLines[consoleIndex].Update(consoleLines[consoleIndex].lineText + lineInfo.lineText, consoleLines[consoleIndex].lineColour.Concat(lineInfo.lineColour).ToArray());
        }
        else
        {
            consoleLines[consoleIndex].Update(lineInfo);
        }

        if (updateInfo.timeStamp) consoleLines[consoleIndex].Update($"[{DateTime.Now.ToString("HH:mm:ss:fff")}] {consoleLines[consoleIndex].lineText}");

        if (updateInfo.newLine) consoleIndex++;

        return updatedIndex;
    }

    /// <summary> Marks console for reload on next tick. </summary>
    public static void ReloadConsole()
    {
        consoleReload = true;
    }

    // --- PRIMARY CONSOLE UPDATES ---

    /// <summary> Updates ConsoleLine at given index, with given lineInfo in primary console. </summary>
    public static bool UpdatePrimaryConsoleLine(ConsoleLine lineInfo, int lineIndex)
    {
        if (!InPrimaryConsole(lineIndex)) return false;
        consoleLines[lineIndex].Update(lineInfo);
        return true;
    }

    /// <summary> Updates ConsoleLine at given index, with given lineInfo in primary console. </summary>
    public static bool UpdatePrimaryConsoleLine(ConsoleLine lineInfo, ConsoleAnimatedLine lineAnimationInfo, int lineIndex)
    {
        if (!InPrimaryConsole(lineIndex)) return false;
        consoleLines[lineIndex].Update(lineInfo);
        consoleLineUpdates[lineIndex].Update(lineAnimationInfo);
        return true;
    }

    /// <summary> Updates given lines animation settings, via the given ConsoleAnimatedLine in primary console. </summary>
    public static bool UpdatePrimaryConsoleLineAnimation(ConsoleAnimatedLine dynamicUpdate, int lineIndex)
    {
        if (!InPrimaryConsole(lineIndex)) return false;
        consoleLineUpdates[lineIndex].Update(dynamicUpdate);
        return true;
    }

    /// <summary> Sends lineInfo into primary console area, returns index of line updated. </summary>
    public static int SendConsoleMessage(ConsoleLine lineInfo, ConsoleLineUpdate updateInfo)
    {
        return UpdateEnclosedConsole(lineInfo, updateInfo, 1, consoleLines.Length - 9, ref primaryLineIndex);
    }

    /// <summary> Sends lineInfo into primary console area, also updating the animationInfo of the same line. </summary>
    public static int SendConsoleMessage(ConsoleLine lineInfo, ConsoleLineUpdate updateInfo, ConsoleAnimatedLine animationInfo)
    {
        int i = SendConsoleMessage(lineInfo, updateInfo);
        consoleLineUpdates[i].Update(animationInfo);
        return i;
    }

    /// <summary> Sends lineInfo into primary console area, also updating the animationInfo of the same line. </summary>
    public static int SendConsoleMessage(ConsoleLine lineInfo, ConsoleAnimatedLine animationInfo) { return SendConsoleMessage(lineInfo, new ConsoleLineUpdate(), animationInfo); }
    /// <summary> Sends lineInfo into primary console area. </summary>
    public static int SendConsoleMessage<T>(T message) { return SendConsoleMessage(message, new ConsoleLineUpdate()); } //just for ez of type
    /// <summary> Sends lineInfo into primary console area. </summary>
    public static int SendConsoleMessage(ConsoleLine lineInfo) { return SendConsoleMessage(lineInfo, new ConsoleLineUpdate()); } //just for ez of type
    /// <summary> Sends lineInfo into primary console area. </summary>
    public static int SendConsoleMessage<T>(T message, ConsoleLineUpdate updateInfo) { return SendConsoleMessage(new ConsoleLine(message?.ToString() ?? ""), updateInfo); }

    /// <summary> Sends multiple lineInfo into primary console, according to updateInfo and animationInfo. </summary>
    public static void SendConsoleMessages(ConsoleLine[] lineInfo, ConsoleLineUpdate[] updateInfo, ConsoleAnimatedLine[] animationInfo)
    {
        int length = Math.Min(lineInfo.Length, Math.Min(updateInfo.Length, animationInfo.Length));
        for (int i = 0; i < length; i++) SendConsoleMessage(lineInfo[i], updateInfo[i], animationInfo[i]);
    }

    /// <summary> Sends multiple lineInfo into primary console, according to updateInfo and animationInfo. </summary>
    public static void SendConsoleMessages(ConsoleLine[] lineInfo, ConsoleLineUpdate[] updateInfo)
    {
        int length = Math.Min(lineInfo.Length, updateInfo.Length);
        for (int i = 0; i < length; i++) SendConsoleMessage(lineInfo[i], updateInfo[i]);
    }

    /// <summary> Sends multiple lineInfo into primary console, according to updateInfo and animationInfo. </summary>
    public static void SendConsoleMessages(ConsoleLine[] messages) { SendConsoleMessages(messages, Enumerable.Repeat(new ConsoleLineUpdate(), messages.Length).ToArray()); }
    /// <summary> Sends multiple lineInfo into primary console, according to updateInfo and animationInfo. </summary>
    public static void SendConsoleMessages(ConsoleLine[] messages, ConsoleAnimatedLine[] animationInfo) { SendConsoleMessages(messages, Enumerable.Repeat(new ConsoleLineUpdate(), messages.Length).ToArray(), animationInfo); }

    // --- DEBUG CONSOLE UPDATES ---

    /// <summary> Updates ConsoleLine at given index, with given lineInfo in debug console. </summary>
    public static bool UpdateDebugConsoleLine(ConsoleLine lineInfo, int lineIndex)
    {
        if (!InDebugConsole(lineIndex)) return false;
        consoleLines[lineIndex].Update(lineInfo);
        return true;
    }

    /// <summary> Updates ConsoleLine at given index, with given lineInfo in debug console. </summary>
    public static bool UpdateDebugConsoleLine(ConsoleLine lineInfo, ConsoleAnimatedLine lineAnimationInfo, int lineIndex)
    {
        if (!InDebugConsole(lineIndex)) return false;
        consoleLines[lineIndex].Update(lineInfo);
        consoleLineUpdates[lineIndex].Update(lineAnimationInfo);
        return true;
    }

    /// <summary> Updates given lines animation settings, via the given ConsoleAnimatedLine in debug console. </summary>
    public static bool UpdateDebugConsoleLineAnimation(ConsoleAnimatedLine dynamicUpdate, int lineIndex)
    {
        if (!InDebugConsole(lineIndex)) return false;
        consoleLineUpdates[lineIndex].Update(dynamicUpdate);
        return true;
    }

    /// <summary> Sends ConsoleLine into debug console area. </summary>
    public static int SendDebugMessage(ConsoleLine lineInfo, ConsoleLineUpdate updateInfo)
    {
        if (Profiler.Enabled) return -1;
        return UpdateEnclosedConsole(lineInfo, updateInfo, windowSize.height - 7, windowSize.height - 2, ref debugLineIndex);
    }

    /// <summary> Sends ConsoleLine into debug console area. </summary>
    public static int SendDebugMessage(ConsoleLine lineInfo, ConsoleLineUpdate updateInfo, ConsoleAnimatedLine animationInfo)
    {
        int i = SendDebugMessage(lineInfo, updateInfo);
        consoleLineUpdates[i].Update(animationInfo);
        return i;
    }
    /// <summary> Sends ConsoleLine into debug console area. </summary>
    public static int SendDebugMessage(ConsoleLine lineInfo, ConsoleAnimatedLine animationInfo) { return SendDebugMessage(lineInfo, new ConsoleLineUpdate(), animationInfo); }
    /// <summary> Sends ConsoleLine into debug console area. </summary>
    public static int SendDebugMessage<T>(T message) { return SendDebugMessage(message, new ConsoleLineUpdate()); } //just for ez of type
    /// <summary> Sends ConsoleLine into debug console area. </summary>
    public static int SendDebugMessage(ConsoleLine lineInfo) { return SendDebugMessage(lineInfo, new ConsoleLineUpdate()); } //just for ez of type
    /// <summary> Sends ConsoleLine into debug console area. </summary>
    public static int SendDebugMessage<T>(T message, ConsoleLineUpdate updateInfo) { return SendDebugMessage(new ConsoleLine(message?.ToString() ?? ""), updateInfo); }

    /// <summary> Sends multiple lineInfo into primary console, according to updateInfo and animationInfo. </summary>
    public static void SendDebugMessages(ConsoleLine[] lineInfo, ConsoleLineUpdate[] updateInfo, ConsoleAnimatedLine[] animationInfo)
    {
        int length = Math.Min(lineInfo.Length, Math.Min(updateInfo.Length, animationInfo.Length));
        for (int i = 0; i < length; i++) SendDebugMessage(lineInfo[i], updateInfo[i], animationInfo[i]);
    }

    /// <summary> Sends multiple lineInfo into primary console, according to updateInfo and animationInfo. </summary>
    public static void SendDebugMessages(ConsoleLine[] lineInfo, ConsoleLineUpdate[] updateInfo)
    {
        int length = Math.Min(lineInfo.Length, updateInfo.Length);
        for (int i = 0; i < length; i++) SendDebugMessage(lineInfo[i], updateInfo[i]);
    }

    /// <summary> Sends multiple lineInfo into primary console, according to updateInfo and animationInfo. </summary>
    public static void SendDebugMessages(ConsoleLine[] messages) { SendDebugMessages(messages, Enumerable.Repeat(new ConsoleLineUpdate(), messages.Length).ToArray()); }
    /// <summary> Sends multiple lineInfo into primary console, according to updateInfo and animationInfo. </summary>
    public static void SendDebugMessages(ConsoleLine[] messages, ConsoleAnimatedLine[] animationInfo) { SendDebugMessages(messages, Enumerable.Repeat(new ConsoleLineUpdate(), messages.Length).ToArray(), animationInfo); }
}
