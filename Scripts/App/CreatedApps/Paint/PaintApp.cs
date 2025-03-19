using Revistone.Interaction;
using Revistone.Functions;
using Revistone.Console;
using Revistone.Console.Image;

using static Revistone.Functions.ColourFunctions;
using static Revistone.Functions.PersistentDataFunctions;
using static Revistone.Console.ConsoleAction;

namespace Revistone.App.BaseApps;

///<summary> App for creating, and editing console images. </summary>
public class PaintApp : App
{
    public PaintApp() : base() { }
    public PaintApp(string name, (ConsoleColor[] primaryColour, ConsoleColor[] secondaryColour, ConsoleColor[] tertiaryColour, int speed) consoleSettings, (ConsoleColor[] colours, int speed) borderSettings, (UserInputProfile format, Action<string> payload, string summary)[] appCommands, int minAppWidth = 30, int minAppHeight = 30, bool baseCommands = true) : base(name, consoleSettings, borderSettings, appCommands, minAppWidth, minAppHeight, baseCommands) { }

    public override App[] OnRegister()
    {
        return [new PaintApp("Paint", (ConsoleColor.DarkBlue.ToArray(), ConsoleColor.Cyan.ToArray(), ConsoleColor.Blue.ToArray(), 10), (CyanDarkBlueGradient.Extend(7, true), 5), [], 70, 50)];
    }

    public override void OnAppInitalisation()
    {
        base.OnAppInitalisation();
        MainMenu();
    }

    private void MainMenu()
    {
        ClearPrimaryConsole();

        ConsoleLine[] title = TitleFunctions.CreateTitle("Paint", TransPattern.Stretch(10), TitleFunctions.AsciiFont.BigMoneyNW, letterSpacing: 1, bottomSpace: 1, topSpace: 1);
        SendConsoleMessages(title, Enumerable.Repeat(new ConsoleAnimatedLine(ConsoleAnimatedLine.ShiftForegroundColour, "", AppRegistry.activeApp.borderColourScheme.speed, true), title.Length).ToArray());

        int option = 0;

        while (option != 1)
        {
            option = UserInput.CreateOptionMenu("Options:", [("Create New Image", CreateImage), ("Exit", ExitApp)], cursorStartIndex: option);
        }
    }

    private void CreateImage()
    {
        int width = int.Parse(UserInput.GetValidUserInput("Image Width: ", new UserInputProfile(UserInputProfile.InputType.Int, numericMin: 5, numericMax: 60)));
        int height = int.Parse(UserInput.GetValidUserInput("Image Height: ", new UserInputProfile(UserInputProfile.InputType.Int, numericMin: 5, numericMax: 30)));
        string imageName = UserInput.GetValidUserInput("Image Name: ", new UserInputProfile(UserInputProfile.InputType.FullText));

        ConsoleImage.SaveToJson(GeneratePath(DataLocation.Console, "Assets/Stickers", $"{imageName}.json"), new ConsoleImage(width, height));

        SendConsoleMessage(new ConsoleLine("Image Created! Press Enter Key To Continue.", ConsoleColor.DarkBlue));
        UserInput.WaitForUserInput();

        EditImage();
    }

    private void EditImage()
    {

    }
}