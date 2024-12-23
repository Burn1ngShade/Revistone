using Revistone.Console;
using Revistone.Functions;
using Revistone.Interaction;

using static Revistone.Console.ConsoleAction;
using static Revistone.Functions.ColourFunctions;

namespace Revistone.Apps.Tetris;

public class TetrisApp : App
{
    public TetrisApp() : base() { }
    public TetrisApp(string name, (ConsoleColor[] primaryColour, ConsoleColor[] secondaryColour, ConsoleColor[] tertiaryColour, int speed) consoleSettings, (ConsoleColor[] colours, int speed) borderSettings, (UserInputProfile format, Action<string> payload, string summary)[] appCommands, int minAppWidth = 30, int minAppHeight = 30, bool baseCommands = true) : base(name, consoleSettings, borderSettings, appCommands, minAppWidth, minAppHeight, baseCommands) { }

    public override App[] OnRegister()
    {
        return [ new TetrisApp("Tetris", (ConsoleColor.DarkBlue.ToArray(), ConsoleColor.Cyan.ToArray(), ConsoleColor.Blue.ToArray(), 10), (CyanDarkBlueGradient.Extend(7, true), 5),
            [], 98, 29) ];
    }

    public override void OnAppInitalisation()
    {
        base.OnAppInitalisation();

        ShiftLine();
        ConsoleLine[] title = TitleFunctions.CreateTitle("TETRIS", AdvancedHighlight(62, ConsoleColor.DarkBlue.ToArray(), (ConsoleColor.Cyan.ToArray(), 0, 10)), TitleFunctions.AsciiFont.BigMoneyNW, letterSpacing: 1);
        SendConsoleMessages(title, 
        Enumerable.Repeat(new ConsoleAnimatedLine(ConsoleAnimatedLine.ShiftColour, "", AppRegistry.activeApp.borderColourScheme.speed, true), 62).ToArray());
    }
}