using System.Runtime.InteropServices;
using System.Text;
using Revistone.Console.Image;
using Revistone.Management;

using static Revistone.Console.Data.ConsoleData;

namespace Revistone.Console.Rendering;

public static class QualityConsoleRenderer
{
    const int STD_OUTPUT_HANDLE = -11;
    const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll")]
    static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll")]
    static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    /// <summary> [DO NOT CALL] Initializes renderer. </summary>
    public static void InitializeRenderer()
    {
        System.Console.OutputEncoding = Encoding.Unicode;
        System.Console.Title = "Revistone";
        System.Console.CursorVisible = false;

        // set the console mode to enable ANSI escape codes
        var handle = GetStdHandle(STD_OUTPUT_HANDLE);
        GetConsoleMode(handle, out uint mode);
        SetConsoleMode(handle, mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
        DeveloperTools.Log($"Console Mode Set From {mode} To {mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING}");
    }

    ///<summary> Render Console To Screen. </summary
    public static void RenderConsole()
    {
        StringBuilder outputAnsi = new();
        for (int i = 0; i < consoleLines.Length; i++)
        {
            if (consoleLines[i] == null || consoleLines[i].updated) continue;

            consoleLines[i].Normalise();

            outputAnsi.Append($"\x1b[{i + 1};0H");

            for (int j = 0; j < consoleLines[i].lineText.Length; j++)
            {
                outputAnsi.Append($"{new ConsoleColour(consoleLines[i].lineColour[j]).ANSIFGCode}");
                outputAnsi.Append($"{new ConsoleColour(consoleLines[i].lineBGColour[j]).ANSIBGCode}");
                outputAnsi.Append(consoleLines[i].lineText[j]);
            }

            if (consoleLinesBuffer[i].lineText.Length > consoleLines[i].lineText.Length)
            {
                outputAnsi.Append("\x1b[0m");
                outputAnsi.Append(' ', consoleLinesBuffer[i].lineText.Length - consoleLines[i].lineText.Length);
            }

            consoleLinesBuffer[i].Update(consoleLines[i]);
            consoleLines[i].MarkAsUpToDate();
        }
        if (System.Console.WindowHeight != windowSize.height || System.Console.WindowWidth != windowSize.width) return;
        System.Console.Write(outputAnsi);
    }

    /// <summary> Reloads renderer (Called on screen resize). </summary>
    public static void Reload()
    {
        ConsoleColour c = new(ConsoleColor.Black);
        System.Console.Write($"\x1b[48;2;{c.R};{c.G};{c.B}m\x1b[2J\x1b[H");
    }
}