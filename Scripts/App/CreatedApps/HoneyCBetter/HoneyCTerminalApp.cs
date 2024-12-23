using Revistone.Console;
using Revistone.Interaction;

using static Revistone.Functions.ColourFunctions;
using static Revistone.Console.ConsoleAction;
using Revistone.Functions;

namespace Revistone.Apps.HoneyC;

/// <summary> Class for all calculation based functions, used via command [calc] or [c] ... </summary>
public class HoneyCTerminalApp : App
{    
    // --- APP BOILER ---

    public HoneyCTerminalApp() : base() { }
    public HoneyCTerminalApp(string name, (ConsoleColor[] primaryColour, ConsoleColor[] secondaryColour, ConsoleColor[] tertiaryColour, int speed) consoleSettings, (ConsoleColor[] colours, int speed) borderSettings, (UserInputProfile format, Action<string> payload, string summary)[] appCommands, int minAppWidth = 30, int minAppHeight = 30, bool baseCommands = true) : base(name, consoleSettings, borderSettings, appCommands, minAppWidth, minAppHeight, baseCommands) { }

    public override App[] OnRegister()
    {
        return [
            new HoneyCTerminalApp("HoneyC Terminal", (ConsoleColor.DarkBlue.ToArray(), ConsoleColor.Cyan.ToArray(), ConsoleColor.Blue.ToArray(), 10), (CyanDarkBlueGradient.Extend(7, true), 5), [], 70, 40)
        ];
    }

    public override void OnUserInput(string userInput)
    {
        if (AppCommands.Commands(userInput)) return;

        HoneyCInterpreter.Interpret(userInput);
    }

    // --- OUTPUT TO CONSOLE ---

    /// <summary> Outputs given output to console via SendConsoleMessage. </summary>
    public static void Output<T>(T output)
    {
        SendConsoleMessage(new ConsoleLine(output?.ToString() ?? "", AppRegistry.activeApp.colourScheme.primaryColour));
    }

    public static void DebugHeaderOutput<T>(T output)
    {
        SendConsoleMessage(new ConsoleLine(output?.ToString() ?? "", AppRegistry.activeApp.colourScheme.tertiaryColour));
    }

    public static void DebugOutput<T>(T output)
    {
        SendConsoleMessage(new ConsoleLine(output?.ToString() ?? "", AppRegistry.activeApp.colourScheme.secondaryColour));
    }

    public enum HoneyCError { InvalidToken, InvalidSyntax, MissingSyntax, InvalidIdentifier }

    public static HoneyCProgram? ErrorOutput(HoneyCError error, string errorMessage, InterpreterDiagnostics diagnostics, Token token, (int line, int token) position)
    {
        SendConsoleMessage(new ConsoleLine($"{error} Exception: {errorMessage}", ConsoleColor.Red));
        SendConsoleMessage(new ConsoleLine($"  TOKEN {token}", ConsoleColor.Red));
        SendConsoleMessage(new ConsoleLine($"  AT Line {position.line}, Token {position.token}", ConsoleColor.Red));
        SendConsoleMessage(new ConsoleLine($"  DURING {diagnostics.CurrentStage}, Time Elapsed {diagnostics.GetTotalElapsedTime()}ms", ConsoleColor.Red));

        return null;
    }

    public static HoneyCProgram? ErrorOutput(HoneyCError error, string errorMessage, InterpreterDiagnostics diagnostics, TokenStatement tokenStatement, (int line, int token) position)
    {
        SendConsoleMessage(new ConsoleLine($"{error} Exception: {errorMessage}", ConsoleColor.Red));
        SendConsoleMessage(new ConsoleLine($"  {tokenStatement}", ConsoleColor.Red.ToArray(), AdvancedHighlight($"  {tokenStatement}", ConsoleColor.Black, ConsoleColor.DarkGray, position.token - 1)));
        SendConsoleMessage(new ConsoleLine($"  AT Line {position.line}, Token {position.token}", ConsoleColor.Red));
        SendConsoleMessage(new ConsoleLine($"  DURING {diagnostics.CurrentStage}, Time Elapsed {diagnostics.GetTotalElapsedTime()}ms", ConsoleColor.Red));

        return null;
    }

}