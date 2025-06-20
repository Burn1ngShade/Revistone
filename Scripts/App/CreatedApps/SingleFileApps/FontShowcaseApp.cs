using Revistone.Console;
using Revistone.Functions;
using Revistone.Interaction;
using Revistone.App.Command;

using static Revistone.Functions.ColourFunctions;
using static Revistone.Functions.TitleFunctions;
using static Revistone.Console.ConsoleAction;

namespace Revistone.App.BaseApps;

public class FontShowcaseApp : App
{
    public FontShowcaseApp() : base() { }
    public FontShowcaseApp(string name, string description, (ConsoleColor[] primaryColour, ConsoleColor[] secondaryColour, ConsoleColor[] tertiaryColour) consoleSettings, (ConsoleColor[] colours, int speed) borderSettings, AppCommand[] appCommands, int minAppWidth = 30, int minAppHeight = 30, bool baseCommands = true) : base(name, description, consoleSettings, borderSettings, appCommands, minAppWidth, minAppHeight, baseCommands, 20) { }

    public override App[] OnRegister()
    {
        return [new FontShowcaseApp("Font Showcase", "View All The ASCII Fonts The Console Supports.", (ConsoleColor.DarkBlue.ToArray(), ConsoleColor.Cyan.ToArray(), ConsoleColor.Blue.ToArray()), (CyanDarkBlueGradient.Stretch(3).Extend(18, true), 5), [], 70, 40)];
    }

    string exampleText = "Revistone!";
    AsciiFont exampleFont = AsciiFont.AnsiRegular;
    int fontCount = Enum.GetValues(typeof(AsciiFont)).Length;

    public override void OnAppInitalisation()
    {
        base.OnAppInitalisation();

        Display();
    }

    public void Display(int pointerIndex = 0)
    {
        ClearPrimaryConsole();

        ConsoleLine[] title = CreateTitle(exampleText, AdvancedHighlight(exampleText.Length * 15, ConsoleColor.DarkBlue.ToArray(), (ConsoleColor.Cyan.ToArray(), 0, 10), (ConsoleColor.Cyan.ToArray(), exampleText.Length * 5, 10), (ConsoleColor.Cyan.ToArray(), exampleText.Length * 10, 10)), exampleFont, letterSpacing: 1, bottomSpace: 1, topSpace: 1);
        SendConsoleMessages(title, Enumerable.Repeat(new ConsoleAnimatedLine(ConsoleAnimatedLine.ShiftForegroundColour, "", AppRegistry.activeApp.borderColourScheme.speed, true), title.Length).ToArray());

        GoToLine(14);
        int option = UserInput.CreateOptionMenu($"--- Font {exampleFont} [{(int)exampleFont + 1} / {fontCount}] ---", ["Next Font", "Previous Font", "Edit Example Text", "Exit"], cursorStartIndex: pointerIndex);

        switch (option)
        {
            case 0:
                IncrementFont(1);
                break;
            case 1:
                IncrementFont(-1);
                break;
            case 2:
                exampleText = UserInput.GetValidUserInput(new ConsoleLine("--- Example Text ---", ConsoleColor.DarkBlue), new UserInputProfile(), exampleText);
                break;
            case 3:
                ExitApp();
                return;
        }

        Display(option);
    }

    public void IncrementFont(int increment)
    {
        int newFont = ((int)exampleFont + increment);
        exampleFont = (AsciiFont)(newFont < 0 ? fontCount - 1 : newFont % fontCount);
        
    }


}