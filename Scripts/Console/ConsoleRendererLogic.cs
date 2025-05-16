using System.Diagnostics;
using Revistone.App.BaseApps;
using Revistone.App;
using Revistone.Console.Widget;
using Revistone.Functions;
using Revistone.Management;

using static Revistone.Console.Data.ConsoleData;

namespace Revistone.Console;

/// <summary> Class pertaining logic to update ConsoleRenderer. </summary>
public static class ConsoleRendererLogic
{
    static bool blockRender = false;

    static void HandleGlobalException(object sender, UnhandledExceptionEventArgs e)
    {
        if (SettingsApp.GetValue("Block Rendering On Crash") == "Yes") blockRender = true;
    }

    //--- CONSOLE LOOPS ---

    /// <summary> [DO NOT CALL] Initializes ConsoleRendererLogic. </summary>
    internal static void InitializeConsoleRendererLogic()
    {
        primaryLineIndex = 1;
        debugLineIndex = debugBufferStartIndex;
        consoleReload = true;

        AppDomain.CurrentDomain.UnhandledException += HandleGlobalException;
        Manager.Tick += HandleConsoleDisplayBehaviour;
    }

    ///<summary> [DO NOT CALL] Handles console rendering. </summary>
    internal static void HandleConsoleRender()
    {
        Stopwatch frameDuration = new();

        while (true)
        {
            frameDuration.Restart();

            RenderConsole();
            Profiler.RenderLogicTime.Add(frameDuration.ElapsedTicks);

            while (true)
            {
                if (frameDuration.ElapsedTicks >= displayWindowsTickInterval) break;
            }

            Profiler.RenderTime.Add(frameDuration.ElapsedTicks);
        }
    }

    /// <summary> Main loop for ConsoleDisplay, handles dynamic lines. </summary>
    static void HandleConsoleDisplayBehaviour(int tickNum)
    {
        //if console display resized
        if (windowSize.width != System.Console.WindowWidth || windowSize.height != System.Console.WindowHeight || (consoleReload && windowSize.height > AppRegistry.activeApp.minHeightBuffer && windowSize.width > AppRegistry.activeApp.minWidthBuffer))
        {
            System.Console.CursorVisible = false;
            windowSize = (System.Console.WindowWidth, System.Console.WindowHeight);
            if (!(consoleReload && windowSize.height > AppRegistry.activeApp.minHeightBuffer && windowSize.width > AppRegistry.activeApp.minWidthBuffer)) SoftReloadConsoleDisplay();
            else if (!(windowSize.height <= AppRegistry.activeApp.minHeightBuffer || windowSize.width <= AppRegistry.activeApp.minWidthBuffer))
            {
                ResetConsoleDisplay();
                consoleReload = false;
            }
        }

        // --- Render Console ---

        if (windowSize.height <= AppRegistry.activeApp.minHeightBuffer || windowSize.width <= AppRegistry.activeApp.minWidthBuffer)
        {
            if (System.Console.WindowTop != 0) try
                {
                    if (OperatingSystem.IsWindows()) System.Console.SetWindowPosition(0, 0);
                }
                catch (IOException) { } //needed as its not possible to detect user changing window size (to my knowledge)

            if (screenWarningUpdated || windowSize.height == 0 || windowSize.width == 0) return;

            //0 height and width to small, 1 height too small, 2 width too small
            int errorCase = windowSize.height <= AppRegistry.activeApp.minHeightBuffer && windowSize.width <= AppRegistry.activeApp.minWidthBuffer ? 0 :
            windowSize.height <= AppRegistry.activeApp.minHeightBuffer ? 1 : 2;
            string[] exception = new string[3];
            switch (errorCase)
            {
                case 0:
                    exception[0] = $"[Window Exception] '{AppRegistry.activeApp.name}' Window Height And Width Too Small";
                    exception[1] = $"Expected Values -> ({AppRegistry.activeApp.minWidthBuffer + 1}, {AppRegistry.activeApp.minHeightBuffer + 1})";
                    exception[2] = $"Actual Values -> ({windowSize.width}, {windowSize.height})";
                    break;
                case 1:
                    exception[0] = $"[Window Exception] '{AppRegistry.activeApp.name}' Window Height Too Small";
                    exception[1] = $"Expected Value -> {AppRegistry.activeApp.minHeightBuffer + 1}";
                    exception[2] = $"Actual Value -> {windowSize.height}";
                    break;
                case 2:
                    exception[0] = $"[Window Exception] '{AppRegistry.activeApp.name}' Window Width Too Small";
                    exception[1] = $"Expected Value -> {AppRegistry.activeApp.minWidthBuffer + 1}";
                    exception[2] = $"Actual Value -> {windowSize.width}";
                    break;
            }

            ConsoleRenderer.Reload();

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < exception[i].Length; j++) ConsoleRenderer.SetChar(j, i, exception[i][j], ConsoleColor.Red, ConsoleColor.Black);
            }

            ConsoleRenderer.DrawBuffer();

            screenWarningUpdated = true;
        }
        else
        {
            UpdateAnimatedLines(tickNum);
        }
    }

    //--- ANIMATED LINES METHOD ---

    /// <summary> Updates all lines marked as animated. </summary>
    static void UpdateAnimatedLines(int tickNum)
    {
        for (int i = 0; i < consoleLines.Length; i++)
        {
            if (!consoleLineUpdates[i].enabled || (tickNum - consoleLineUpdates[i].initTick) % consoleLineUpdates[i].tickMod != 0) continue; //not dynamic or not right tick

            consoleLineUpdates[i].update.Invoke(consoleLines[i], consoleLineUpdates[i], tickNum);
        }
    }

    //--- CONSOLE DISPLAY METHODS ---

    /// <summary> Resets console display to default. </summary>
    static void ResetConsoleDisplay()
    {
        screenWarningUpdated = false;

        ConsoleRenderer.Reload();

        primaryLineIndex = 1;
        debugLineIndex = debugBufferStartIndex;


        exceptionLines = new bool[System.Console.WindowHeight - 1];
        consoleLines = new ConsoleLine[System.Console.WindowHeight - 1];
        consoleLinesBuffer = new ConsoleLine[System.Console.WindowHeight - 1];
        consoleLineUpdates = new ConsoleAnimatedLine[System.Console.WindowHeight - 1];
        for (int i = 0; i < consoleLines.Length; i++)
        {
            exceptionLines[i] = false;
            consoleLines[i] = new ConsoleLine();
            consoleLinesBuffer[i] = new ConsoleLine();
            consoleLineUpdates[i] = new ConsoleAnimatedLine();
        }

        UpdateConsoleTitle();
        UpdateConsoleBorder();

        appInitalisation = true;
    }

    /// <summary> Reloads console display updating buffer size, while maintaing screen. </summary>
    static void SoftReloadConsoleDisplay()
    {
        screenWarningUpdated = false;

        if (windowSize.height <= AppRegistry.activeApp.minHeightBuffer) return;

        if (consoleLines.Length == 0)
        {
            ResetConsoleDisplay();
            return;
        }

        ConsoleRenderer.Reload();

        int debugDistanceFromEnd = consoleLines.Length - debugLineIndex;

        (ConsoleLine lineInfo, ConsoleAnimatedLine animationInfo, bool exceptionStatus)[] enclosedConsoleLinesDC = new (ConsoleLine, ConsoleAnimatedLine, bool)[debugStartIndex - 1];
        (ConsoleLine lineInfo, ConsoleAnimatedLine animationInfo, bool exceptionStatus)[] metaConsoleLinesDC = new (ConsoleLine, ConsoleAnimatedLine, bool)[8];

        for (int i = 1; i < consoleLines.Length; i++)
        {
            if (i <= debugStartIndex - 1) enclosedConsoleLinesDC[i - 1] = (new ConsoleLine(consoleLines[i]), new ConsoleAnimatedLine(consoleLineUpdates[i]), exceptionLines[i]);
            else
            {
                metaConsoleLinesDC[i - debugStartIndex] = (new ConsoleLine(consoleLines[i]), new ConsoleAnimatedLine(consoleLineUpdates[i]), exceptionLines[i]);
            }
        }

        Array.Resize(ref exceptionLines, windowSize.height - 1);
        Array.Resize(ref consoleLines, windowSize.height - 1);
        Array.Resize(ref consoleLinesBuffer, windowSize.height - 1);
        Array.Resize(ref consoleLineUpdates, windowSize.height - 1);

        for (int i = 1; i < consoleLines.Length; i++)
        {
            exceptionLines[i] = false;
            consoleLines[i] = new ConsoleLine();
            consoleLinesBuffer[i] = new ConsoleLine();
            consoleLineUpdates[i] = new ConsoleAnimatedLine();
        }

        for (int i = 0; i < metaConsoleLinesDC.Length; i++)
        {
            exceptionLines[i + debugStartIndex] = metaConsoleLinesDC[i].exceptionStatus;
            consoleLines[i + debugStartIndex].Update(metaConsoleLinesDC[i].lineInfo);
            consoleLineUpdates[i + debugStartIndex].Update(metaConsoleLinesDC[i].animationInfo);
        }

        for (int i = 1; i <= Math.Min(enclosedConsoleLinesDC.Length, debugStartIndex); i++)
        {
            exceptionLines[i] = enclosedConsoleLinesDC[i - 1].exceptionStatus;
            consoleLines[i].Update(enclosedConsoleLinesDC[i - 1].lineInfo);
            consoleLineUpdates[i].Update(enclosedConsoleLinesDC[i - 1].animationInfo);
        }

        primaryLineIndex = Math.Clamp(primaryLineIndex, 1, debugStartIndex);
        debugLineIndex = consoleLines.Length - debugDistanceFromEnd;

        UpdateConsoleTitle();
        UpdateConsoleBorder();

        consoleInterrupt = true;
    }

    /// <summary> Updates console title, with current app name and colour scheme. </summary>
    static void UpdateConsoleTitle()
    {
        if (consoleLines.Length < AppRegistry.activeApp.minHeightBuffer) return;
        exceptionLines[0] = true;
        consoleLinesBuffer[0].Update(""); //stops buffer width

        string title = $" [{AppRegistry.activeApp.name}] ";
        int leftBuffer = Math.Max((int)Math.Floor((windowSize.width - title.Length) / 2f), 0);
        int rightBuffer = Math.Max((int)Math.Ceiling((windowSize.width - title.Length) / 2f), 0) - 1;
        consoleLines[0].Update(new string('-', leftBuffer) + title + new string('-', rightBuffer), ColourFunctions.Alternate(AppRegistry.activeApp.borderColourScheme.colours, windowSize.width - 1, 1));
        consoleLineUpdates[0].Update(new ConsoleAnimatedLine(ConsoleAnimatedLine.ShiftForegroundColour, "1", AppRegistry.activeApp.borderColourScheme.speed, true));
    }

    /// <summary> Updates console border, with current app colour scheme. </summary>
    static void UpdateConsoleBorder()
    {
        if (consoleLines.Length < AppRegistry.activeApp.minHeightBuffer) return;
        exceptionLines[^8] = true;
        consoleLinesBuffer[^8].Update(""); //stops buffer width
        consoleLines[^8].Update(GenerateConsoleBorderString(), ColourFunctions.Alternate(AppRegistry.activeApp.borderColourScheme.colours, windowSize.width - 1, 1));
        consoleLineUpdates[^8].Update(new ConsoleAnimatedLine(UpdateConsoleBorderAnimation, "", 1, true));
    }

    /// <summary> Updates console border widgets. </summary>
    static void UpdateConsoleBorderAnimation(ConsoleLine lineInfo, ConsoleAnimatedLine animationInfo, int tickNum)
    {
        if (tickNum % AppRegistry.activeApp.borderColourScheme.speed == 0) lineInfo.Update(lineInfo.lineColour.Shift(1));
        if (tickNum % widgetTickInterval == 0) lineInfo.Update(GenerateConsoleBorderString());
    }

    /// <summary> Generates the console border string using current widget data. </summary>
    static string GenerateConsoleBorderString()
    {
        string[] widgets = ConsoleWidget.GetWidgetContents();
        string jointWidgets = string.Join("--------", widgets);
        int leftBuffer = Math.Max((int)Math.Floor((windowSize.width - jointWidgets.Length) / 2f), 0);
        int rightBuffer = Math.Max((int)Math.Ceiling((windowSize.width - jointWidgets.Length) / 2f), 1) - 1;

        return new string('-', leftBuffer) + jointWidgets + new string('-', rightBuffer);
    }

    // --- CONSOLE RENDERING ---

    /// <summary> Writes given line to screen, using value of consoleLines. </summary>
    static void WriteConsoleLine(int lineIndex)
    {
        if (consoleLines[lineIndex] == null)
        {
            Analytics.Debug.Log("Console Write Fail.");
            return;
        }

        //if user decides to set an empty array for colours (please dont do this)
        if (consoleLines[lineIndex].lineColour.Length == 0) consoleLines[lineIndex].Update(ConsoleColor.White.ToArray());
        if (consoleLines[lineIndex].lineBGColour.Length == 0) consoleLines[lineIndex].Update(consoleLines[lineIndex].lineText, consoleLines[lineIndex].lineColour, ConsoleColor.Black.ToArray());

        consoleLines[lineIndex].MarkAsUpToDate();

        ConsoleLine c = new ConsoleLine(consoleLines[lineIndex]); //copy of current console line
        ConsoleLine bc = new ConsoleLine(consoleLinesBuffer[lineIndex]); //copy of buffer console line

        if (bc.lineText.Length > c.lineText.Length) //clears line between end of currentline and buffer line
        {
            for (int i = c.lineText.Length; i < windowSize.width; i++)
            {
                ConsoleRenderer.SetChar(i, lineIndex, ' ', ConsoleColor.White, ConsoleColor.Black);
            }
        }

        for (int i = 0; i < Math.Min(c.lineText.Length, windowSize.width); i++)
        {
            ConsoleRenderer.SetChar(i, lineIndex, c.lineText[i], c.lineColour.Length > i ? c.lineColour[i] : c.lineColour[^1], c.lineBGColour.Length > i ? c.lineBGColour[i] : c.lineBGColour[^1]);
        }
    }

    /// <summary> Updates the console display, based on current states of consoleLines, before updating consoleLinesBuffer. </summary>
    public static void RenderConsole()
    {
        if (blockRender || consoleLines.Length == 0 || consoleLines[^1] == null || consoleLines.Length < AppRegistry.activeApp.minHeightBuffer) return;

        if (System.Console.WindowTop != 0)
        {
            try
            {
                if (OperatingSystem.IsWindows()) System.Console.SetWindowPosition(0, 0);
            }
            catch (IOException) { } //needed as its not possible to detect user changing window size (to my knowledge)
        }

        for (int i = 0; i < consoleLines.Length; i++)
        {
            if (consoleLines[i].updated) continue;

            if (System.Console.WindowHeight != windowSize.height || System.Console.WindowWidth != windowSize.width)
            {
                return;
            }

            WriteConsoleLine(i);
            consoleLinesBuffer[i].Update(consoleLines[i]);
        }

        ConsoleRenderer.DrawBuffer();
    }
}
