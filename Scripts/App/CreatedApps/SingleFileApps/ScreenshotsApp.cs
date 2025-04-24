using Revistone.Interaction;
using Revistone.Functions;
using Revistone.Console;

using static Revistone.Functions.ColourFunctions;
using static Revistone.Console.ConsoleAction;
using static Revistone.Functions.PersistentDataFunctions;
using Revistone.Console.Image;
using Revistone.Console.Data;

namespace Revistone.App.BaseApps;

///<summary> App for displaying console screenshots. </summary>
public class ScreenshotsApp : App
{
    public ScreenshotsApp() : base() { }
    public ScreenshotsApp(string name, (ConsoleColor[] primaryColour, ConsoleColor[] secondaryColour, ConsoleColor[] tertiaryColour) consoleSettings, (ConsoleColor[] colours, int speed) borderSettings, (UserInputProfile format, Action<string> payload, string summary)[] appCommands, int minAppWidth = 30, int minAppHeight = 30, bool baseCommands = true) : base(name, consoleSettings, borderSettings, appCommands, minAppWidth, minAppHeight, baseCommands) { }

    public override App[] OnRegister()
    {
        return [new ScreenshotsApp("Screenshots", (ConsoleColor.DarkBlue.ToArray(), ConsoleColor.Cyan.ToArray(), ConsoleColor.Blue.ToArray()), (CyanDarkBlueGradient.Extend(7, true), 5), [], 70, 40)];
    }

    public override void OnAppInitalisation()
    {
        base.OnAppInitalisation();
        MainMenu();
    }

    private void MainMenu(int option = 0)
    {
        ClearPrimaryConsole();

        ConsoleLine[] title = TitleFunctions.CreateTitle("SCREENSHOTS", AdvancedHighlight(120, ConsoleColor.DarkBlue.ToArray(), (ConsoleColor.Cyan.ToArray(), 0, 10), (ConsoleColor.Cyan.ToArray(), 60, 10)), TitleFunctions.AsciiFont.BigMoneyNW, letterSpacing: 1, bottomSpace: 1, topSpace: 1);
        SendConsoleMessages(title, Enumerable.Repeat(new ConsoleAnimatedLine(ConsoleAnimatedLine.ShiftForegroundColour, "", AppRegistry.activeApp.borderColourScheme.speed, true), title.Length).ToArray());

        string[] screenshots = GetSubFiles(GeneratePath(DataLocation.Console, "History/Screenshots"));

        if (screenshots.Length == 0)
        {
            SendConsoleMessage(new ConsoleLine("No Screenshots Found.", ConsoleColor.Cyan));
            ShiftLine();
            UserInput.WaitForUserInput();
            ExitApp();
            return;
        }

        option = UserInput.CreateMultiPageOptionMenu("Options", Enumerable.Range(0, screenshots.Length).Select(i => new ConsoleLine(screenshots[i][..^5], ConsoleColor.Cyan)).ToArray(), [new ConsoleLine("Exit", ConsoleColor.DarkBlue)], 10, option);

        if (option != -1)
        {
            ViewScreenshot(GeneratePath(DataLocation.Console, "History/Screenshots", screenshots[option]));
            MainMenu(option);
        }
        else ExitApp();
    }

    private void ViewScreenshot(string filePath)
    {
        ConsoleImage? img = ConsoleImage.LoadFromCIMG(filePath);
        if (img == null) return;

        ClearPrimaryConsole();
        img.Output();

        if (img.Width > ConsoleData.windowSize.width || img.Height > ConsoleData.windowSize.height - 12) SendConsoleMessage(new ConsoleLine("Warning, Screenshot Is Larger Than Console Window.", ConsoleColor.Yellow));
        SendConsoleMessage(new ConsoleLine($"Viewing Screenshot: {filePath}", ConsoleColor.Cyan));
        UserInput.WaitForUserInput();
    }
}