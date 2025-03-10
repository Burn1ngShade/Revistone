using System.Diagnostics;
using Revistone.Console;
using Revistone.Functions;
using static Revistone.Console.ConsoleAction;

namespace Revistone.Apps.HoneyC.Data;

/// <summary> Class responsible for all interpreter diagnostics. </summary>
public static class Diagnostics
{
    const string ProgramLogFilePath = "HoneyC/ProgramLog/";

    static Stopwatch runtime = new();
    static string programLog = "";

    /// <summary> Init diagnostics for a HoneyC program. </summary>
    public static void Start()
    {
        runtime.Restart();

        programLog = "";
    }

    /// <summary> Finish diagnostics for a HoneyC program. </summary>
    public static void Finish()
    {
        runtime.Stop();

        if (programLog.StartsWith('\n')) programLog = programLog[1..];
        AppPersistentData.SaveFile($"{ProgramLogFilePath}{DateTime.Now:[yyyy-MM-dd_HH-mm-ss]}.txt", programLog.Split("\n"));
    }

    /// <summary> Outputs t into the program log file, and optionally to the console. </summary>
    public static void Output<T>(T t, bool header = false, bool showInConsole = false, bool isWarning = false)
    {
        programLog += $"{(header ? "\n" : "")}{t}\n";
        if (header) programLog += $"Time Elapsed - {Math.Round(ElapsedTime, 2)} ms\n";
        if (showInConsole)
        {
            SendConsoleMessage(new ConsoleLine($"{t}", header ? AppRegistry.activeApp.colourScheme.secondaryColour : isWarning ? [ConsoleColor.Yellow] : AppRegistry.activeApp.colourScheme.primaryColour));
            if (header) SendConsoleMessage(new ConsoleLine($"Time Elapsed - {Math.Round(ElapsedTime, 2)} ms", AppRegistry.activeApp.colourScheme.primaryColour));
        }
    }

    /// <summary> Throws project error, and ends diagnostics. </summary>
    public static List<T> ThrowError<T>(string errorType, string error, int line, int index, List<Token> tokens)
    {
        string[] errorStrings = [
            $"{errorType}: {error}",
            $"  {string.Join(' ', tokens.Select((t, i) => { return t.content; }))}",
            $"  At Line {(line < 0 ? "?" : line + 1)} Token {(index < 0 ? "?" : index + 1)}",
            $"  Time Elapsed - {Math.Round(ElapsedTime, 2)} ms",
        ];

        int errorStartIndex = tokens.Take(index).Select(x => x.content.Length + 1).Sum() + 2;
        SendDebugMessage(errorStartIndex);

        SendConsoleMessages(errorStrings.Select((x, i) => new ConsoleLine(x, [ConsoleColor.Red],
        i == 1 ? ColourFunctions.AdvancedHighlight(errorStrings[1].Length, ConsoleColor.Black, ConsoleColor.DarkGray, (errorStartIndex, tokens[index].content.Length)) : [ConsoleColor.Black])).ToArray());

        programLog += "\n--- Error ---\n";
        programLog += string.Join('\n', errorStrings);

        Finish();
        return [];
    }

    /// <summary> Throws project error, and ends diagnostics. </summary>
    public static T? ThrowNullError<T>(string errorType, string error, int line, int index, List<Token> tokens)
    {
        ThrowError<object>(errorType, error, line, index, tokens);
        return default;
    }

    /// <summary> Returns the elapsed time of the current program, in milliseconds. </summary>
    public static double ElapsedTime => runtime.ElapsedTicks / 10000d;

    /// <summary> Returns if diagnostics are currently running. </summary>
    public static bool Running => runtime.IsRunning;
}