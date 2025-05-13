using Revistone.Interaction;
using Revistone.Functions;
using Revistone.Console;
using Revistone.App.Command;
using Revistone.Console.Image;
using Revistone.Console.Data;

using static Revistone.Functions.ColourFunctions;
using static Revistone.Console.ConsoleAction;
using static Revistone.Functions.PersistentDataFunctions;


namespace Revistone.App.BaseApps;

///<summary> App for displaying console screenshots. </summary>
public class ScreenshotsApp : App
{
    public ScreenshotsApp() : base() { }
    public ScreenshotsApp(string name, (ConsoleColor[] primaryColour, ConsoleColor[] secondaryColour, ConsoleColor[] tertiaryColour) consoleSettings, (ConsoleColor[] colours, int speed) borderSettings, AppCommand[] appCommands, int minAppWidth = 30, int minAppHeight = 30, bool baseCommands = true) : base(name, consoleSettings, borderSettings, appCommands, minAppWidth, minAppHeight, baseCommands, 30) { }

    public override App[] OnRegister()
    {
        return [new ScreenshotsApp("Screenshots", (ConsoleColor.DarkBlue.ToArray(), ConsoleColor.Cyan.ToArray(), ConsoleColor.Blue.ToArray()), (CyanDarkBlueGradient.Stretch(3).Extend(18, true), 5), [], 70, 40)];
    }

    public override void OnAppInitalisation()
    {
        base.OnAppInitalisation();
        MainMenu();
    }

    public override void ExitApp()
    {
        base.ExitApp();
        init = false;
    }

    static ConsoleLine[] titleGraphic = [];
    static bool init = false;

    private void MainMenu(int option = 0)
    {
        if (init == false)
        {
            titleGraphic = TitleFunctions.CreateTitle("SCREENSHOTS", AdvancedHighlight(120, ConsoleColor.DarkBlue.ToArray(), (ConsoleColor.Cyan.ToArray(), 0, 10), (ConsoleColor.Cyan.ToArray(), 60, 10)), TitleFunctions.AsciiFont.BigMoneyNW, letterSpacing: 1, bottomSpace: 1, topSpace: 1);
            SendConsoleMessages(titleGraphic, Enumerable.Repeat(new ConsoleAnimatedLine(ConsoleAnimatedLine.ShiftForegroundColour, "", AppRegistry.activeApp.borderColourScheme.speed, true), titleGraphic.Length).ToArray());
            init = true;
        }

        option = UserInput.CreateOptionMenu("--- Options ---", [new ConsoleLine("Primary Screenshots", AppRegistry.SecondaryCol), new ConsoleLine("Debug Screenshots", AppRegistry.SecondaryCol), new ConsoleLine("Exit", AppRegistry.PrimaryCol)], cursorStartIndex: option);

        if (option == 0) ListScreenshots("--- Primary Screenshots ---", GeneratePath(DataLocation.App, "Screenshots/Primary"), 0);
        else if (option == 1) ListScreenshots("--- Debug Screenshots ---", GeneratePath(DataLocation.App, "Screenshots/Debug"), 0);
        else
        {
            ExitApp();
            return;
        }

        MainMenu(option);
    }

    private static void ListScreenshots(string title, string filePath, int option)
    {
        string[] screenshots = GetSubFiles(filePath);

        if (screenshots.Length == 0)
        {
            SendConsoleMessage(new ConsoleLine("No Screenshots Found.", AppRegistry.PrimaryCol));
            ShiftLine();
            UserInput.WaitForUserInput();
            ClearLines(2, true);
            return;
        }

        option = UserInput.CreateMultiPageOptionMenu("Options", [.. Enumerable.Range(0, screenshots.Length).Select(i => new ConsoleLine(screenshots[i][..^5], ConsoleColor.Cyan))], [new ConsoleLine("Exit", ConsoleColor.DarkBlue)], 10, option);

        if (option != -1)
        {
            ViewScreenshot($"{filePath}/{screenshots[option]}", 0);
            ListScreenshots(title, filePath, option);
        }
    }

    private static void ViewScreenshot(string filePath, int option)
    {
        ConsoleImage? img = ConsoleImage.LoadFromCIMG(filePath);
        if (img == null) return;

        ClearPrimaryConsole();
        img.Output();

        string title = $" Viewing Screenshot - {filePath} ";
        int leftBuffer = Math.Max((int)Math.Floor((ConsoleData.windowSize.width - title.Length) / 2f), 0);
        int rightBuffer = Math.Max((int)Math.Ceiling((ConsoleData.windowSize.width - title.Length) / 2f), 0) - 1;
        SendConsoleMessage(new ConsoleLine(new string('=', leftBuffer) + title + new string('=', rightBuffer), CyanDarkBlueGradient.Stretch(3).Extend(ConsoleData.windowSize.width, true)), new ConsoleAnimatedLine(ConsoleAnimatedLine.ShiftForegroundColour, enabled: true));
        if (img.Width > ConsoleData.windowSize.width || img.Height > ConsoleData.windowSize.height - 16) SendConsoleMessage(new ConsoleLine("Warning, Screenshot Is Larger Than Console Window.", ConsoleColor.Yellow));
        else ShiftLine();

        option = UserInput.CreateOptionMenu("--- Options ---", [new ConsoleLine("Reload Screenshot", AppRegistry.SecondaryCol), new ConsoleLine("Delete Screenshot", AppRegistry.SecondaryCol), new ConsoleLine("Exit", AppRegistry.PrimaryCol)]);

        if (option == 0)
        {
            ViewScreenshot(filePath, option);
            return;
        }
        else if (option == 1)
        {
            if (!UserInput.CreateTrueFalseOptionMenu("Are You Sure You Want To Delete Screenshot?"))
            {
                ViewScreenshot(filePath, option);
                return;
            }

            DeleteFile(filePath);
            SendConsoleMessage(new ConsoleLine("Screenshot Deleted.", AppRegistry.PrimaryCol));
            ShiftLine();
            UserInput.WaitForUserInput();
        }

        ClearPrimaryConsole();
        SendConsoleMessages(titleGraphic, Enumerable.Repeat(new ConsoleAnimatedLine(ConsoleAnimatedLine.ShiftForegroundColour, "", AppRegistry.activeApp.borderColourScheme.speed, true), title.Length).ToArray());
    }

    // --- GENERAL CONSOLE ---

    ///<summary> Takes a screenshot of the primary console. </summary>
    public static void TakePrimaryScreenshot(string name = "")
    {
        if (!IsNameValid(name, true) || ConsoleData.screenWarningUpdated) return;

        name = name.Length == 0 ? $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}.cimg" : $"{name}.cimg";
        string path = GeneratePath(DataLocation.App, "Screenshots/Primary", name);

        if (FileExists(path))
        {
            SendConsoleMessage(new ConsoleLine($"Screenshot With Name Already Exists - '{name}'", BuildArray(AppRegistry.PrimaryCol.Extend(38), AppRegistry.SecondaryCol)));
            return;
        }

        int primarySize = ConsoleData.debugStartIndex - 1;
        ConsoleLine[] c = new ConsoleLine[primarySize];

        for (int i = 1; i < primarySize + 1; i++)
        {
            c[primarySize - i] = GetConsoleLine(i);
        }

        ConsoleImage image = new(ConsoleData.windowSize.width, primarySize);
        image.SetPixels(0, 0, c);
        ConsoleImage.SaveToCIMG(path, image);

        SendDebugMessage(new ConsoleLine($"Screenshot Taken - '{name}'", BuildArray(AppRegistry.PrimaryCol.Extend(19), AppRegistry.SecondaryCol)));
    }

    ///<summary> Takes a screenshot of the debug console. </summary>
    public static void TakeDebugScreenshot(string name = "")
    {
        if (!IsNameValid(name, true) || ConsoleData.screenWarningUpdated) return;

        name = name.Length == 0 ? $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}.cimg" : $"{name}.cimg";
        string path = GeneratePath(DataLocation.App, "Screenshots/Debug", name);

        if (FileExists(path))
        {
            SendConsoleMessage(new ConsoleLine($"Screenshot With Name Already Exists - '{name}'", BuildArray(AppRegistry.PrimaryCol.Extend(38), AppRegistry.SecondaryCol)));
            return;
        }

        ConsoleLine[] c = new ConsoleLine[7];

        for (int i = ConsoleData.debugStartIndex + 1; i < ConsoleData.debugStartIndex + 8; i++)
        {
            c[ConsoleData.debugBufferStartIndex - i + 6] = GetConsoleLine(i);
        }

        ConsoleImage image = new(ConsoleData.windowSize.width, 8);
        image.SetPixels(0, 0, c);
        ConsoleImage.SaveToCIMG(path, image);

        SendDebugMessage(new ConsoleLine($"Debug Screenshot Taken - '{name}'", BuildArray(AppRegistry.PrimaryCol.Extend(25), AppRegistry.SecondaryCol)));
    }
}