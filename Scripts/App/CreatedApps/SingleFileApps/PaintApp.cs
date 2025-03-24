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
        return [new PaintApp("Paint", (ConsoleColor.DarkBlue.ToArray(), ConsoleColor.Cyan.ToArray(), ConsoleColor.Blue.ToArray(), 10), (CyanDarkBlueGradient.Extend(7, true), 5), [], 70, 51)];
    }

    public override void OnAppInitalisation()
    {
        base.OnAppInitalisation();
        MainMenu();
    }

    private void MainMenu()
    {
        ClearPrimaryConsole();

        ConsoleLine[] title = TitleFunctions.CreateTitle("PAINT", AdvancedHighlight(52, ConsoleColor.DarkBlue.ToArray(), (ConsoleColor.Cyan.ToArray(), 0, 10)), TitleFunctions.AsciiFont.BigMoneyNW, letterSpacing: 1, bottomSpace: 1, topSpace: 1);
        SendConsoleMessages(title, Enumerable.Repeat(new ConsoleAnimatedLine(ConsoleAnimatedLine.ShiftForegroundColour, "", AppRegistry.activeApp.borderColourScheme.speed, true), title.Length).ToArray());

        for (int i = 0; i <= 10; i++)
        {
            UpdateLineExceptionStatus(true, i);
        }

        int option = 0;

        while (option != 4)
        {
            option = UserInput.CreateOptionMenu("--- Options ---", [(new ConsoleLine("Create New Image", ConsoleColor.Cyan), CreateImage), (new ConsoleLine("Edit Image", ConsoleColor.Cyan), SelectImage), (new ConsoleLine("Delete Image", ConsoleColor.Cyan), DeleteImage), (new ConsoleLine("Hotkeys", ConsoleColor.Cyan), Hotkeys), (new ConsoleLine("Exit", ConsoleColor.DarkBlue), ExitApp)], cursorStartIndex: option);
        }
    }

    private void CreateImage()
    {
        int width = int.Parse(UserInput.GetValidUserInput("Image Width: ", new UserInputProfile(UserInputProfile.InputType.Int, numericMin: 3, numericMax: 60)));
        int height = int.Parse(UserInput.GetValidUserInput("Image Height: ", new UserInputProfile(UserInputProfile.InputType.Int, numericMin: 3, numericMax: 20)));

        string imageName = UserInput.GetValidUserInput("Image Name: ", new UserInputProfile(UserInputProfile.InputType.FullText));

        ConsoleImage.SaveToCIMG(GeneratePath(DataLocation.Console, "Assets/Stickers", $"{imageName}.cimg"), new ConsoleImage(width, height));

        SendConsoleMessage(new ConsoleLine("Image Created! Press Enter Key To Continue.", ConsoleColor.DarkBlue));
        ShiftLine();
        UserInput.WaitForUserInput();

        EditImage($"{imageName}.cimg");
    }

    private void SelectImage()
    {
        string[] imageFiles = GetSubFiles(GeneratePath(DataLocation.Console, "Assets/Stickers"));

        if (imageFiles.Length == 0)
        {
            SendConsoleMessage(new ConsoleLine("No Images Found! Press Enter Key To Continue.", ConsoleColor.DarkBlue));
            ShiftLine();
            UserInput.WaitForUserInput();
            ClearPrimaryConsole();
            return;
        }

        int option = UserInput.CreateMultiPageOptionMenu("Images", [.. imageFiles.Select(x => new ConsoleLine(x[..^5], AppRegistry.activeApp.colourScheme.secondaryColour))], [new ConsoleLine("Exit", AppRegistry.activeApp.colourScheme.primaryColour)], 4);

        if (option == -1) return;

        EditImage(imageFiles[option]);
    }

    private void EditImage(string filePath)
    {
        ClearPrimaryConsole();

        int currentColour = 0;
        (int x, int y) = (0, 0);

        ConsoleImage? image = ConsoleImage.LoadFromCIMG(GeneratePath(DataLocation.Console, "Assets/Stickers", filePath));
        if (image == null) return;

        UpdateCursor(0, 0);
        OutputImage(true);
        UpdateImageUI(image, currentColour);

        while (true)
        {
            (ConsoleKeyInfo key, bool interrupted) = UserRealtimeInput.GetKey();

            if (interrupted)
            {
                OutputImage(true);
                UpdateImageUI(image, currentColour);
                continue;
            }

            switch (key.Key)
            {
                case ConsoleKey.A:
                    UpdateCursor(-1, 0);
                    break;
                case ConsoleKey.D:
                    UpdateCursor(1, 0);
                    break;
                case ConsoleKey.W:
                    UpdateCursor(0, 1);
                    break;
                case ConsoleKey.S:
                    UpdateCursor(0, -1);
                    break;
                case ConsoleKey.Enter:
                    image.SetPixelBackground(x, y, colours[currentColour]);
                    UpdateCursor(0, 0);
                    break;
                case ConsoleKey.J or ConsoleKey.LeftArrow:
                    currentColour = currentColour == 0 ? 15 : currentColour - 1;
                    UpdateImageUI(image, currentColour);
                    break;
                case ConsoleKey.K or ConsoleKey.RightArrow:
                    currentColour = currentColour == 15 ? 0 : currentColour + 1;
                    UpdateImageUI(image, currentColour);
                    break;
                case ConsoleKey.R:
                    int width = int.Parse(UserInput.GetValidUserInput("Image Width: ", new UserInputProfile(UserInputProfile.InputType.Int, numericMin: 5, numericMax: 60)));
                    int height = int.Parse(UserInput.GetValidUserInput("Image Height: ", new UserInputProfile(UserInputProfile.InputType.Int, numericMin: 5, numericMax: 20)));
                    image.SetImageSize(width, height, true);
                    OutputImage(true);
                    UpdateImageUI(image, currentColour);
                    break;
                case ConsoleKey.E:
                    int exitType = UserInput.CreateOptionMenu("--- Exit ---", [new ConsoleLine("Save And Exit", ConsoleColor.Cyan), new ConsoleLine("Exit Without Saving", ConsoleColor.Cyan), new ConsoleLine("Cancel", ConsoleColor.Cyan)]);
                    if (exitType == 2) break;
                    if (exitType == 0)
                    {
                        image.SetPixelChar(x, y, ' ');
                        image.SetPixelForeground(x, y, ConsoleColor.White);
                        ConsoleImage.SaveToCIMG(GeneratePath(DataLocation.Console, "Assets/Stickers", filePath), image);
                    }
                    ClearPrimaryConsole();
                    return;
            }

            OutputImage();
        }

        void UpdateCursor(int xShift, int yShift)
        {
            image.SetPixelChar(x, y, ' ');
            x = Math.Clamp(x + xShift, 0, image.Width - 1);
            y = Math.Clamp(y + yShift, 0, image.Height - 1);
            image.SetPixelChar(x, y, '\u2592');
            image.SetPixelForeground(x, y, ContrastColour[image.GetPixel(x, y).BGColour]);
        }

        void OutputImage(bool baseUI = false)
        {
            if (baseUI)
            {
                ConsoleLine[] consoleUI = [.. Enumerable.Repeat(new ConsoleLine($"|{new string(' ', image.Width)}|", ConsoleColor.DarkBlue.Extend(image.Width + 2)), image.Height)];

                consoleUI[1] = new ConsoleLine($"{consoleUI[1].lineText}     Image Width: {image.Width}", ConsoleColor.DarkBlue.Extend(100));
                consoleUI[2] = new ConsoleLine($"{consoleUI[2].lineText}     Image Height: {image.Height}", ConsoleColor.DarkBlue.Extend(100));

                ClearPrimaryConsole();
                SendConsoleMessage(new ConsoleLine($"+{new string('-', image.Width)}+     Image Name: {filePath[..^5]}", ConsoleColor.DarkBlue));
                SendConsoleMessages(consoleUI);
                SendConsoleMessage(new ConsoleLine($"+{new string('-', image.Width)}+", ConsoleColor.DarkBlue));
            }

            image.OutputAt(1, 12);
        }
    }

    static readonly ConsoleColor[] colours = [
        ConsoleColor.White,
        ConsoleColor.Gray,
        ConsoleColor.DarkGray,
        ConsoleColor.Black,
        ConsoleColor.Red,
        ConsoleColor.DarkRed,
        ConsoleColor.Yellow,
        ConsoleColor.DarkYellow,
        ConsoleColor.Green,
        ConsoleColor.DarkGreen,
        ConsoleColor.Cyan,
        ConsoleColor.DarkCyan,
        ConsoleColor.Blue,
        ConsoleColor.DarkBlue,
        ConsoleColor.Magenta,
        ConsoleColor.DarkMagenta
    ];

    private void UpdateImageUI(ConsoleImage image, int currentColour)
    {
        GoToLine(14 + image.Height);
        SendConsoleMessage(new ConsoleLine("Colour Palette:", ConsoleColor.DarkBlue));
        SendConsoleMessage(new ConsoleLine(new string(' ', 32), [ConsoleColor.White],
            BuildArray(Enumerable.Range(0, 16).Select(x => new ConsoleColor[] { colours[x], ConsoleColor.Black }).ToArray())));
        SendConsoleMessage(new ConsoleLine(new string(' ', currentColour * 2) + "^", ConsoleColor.Cyan));
        ShiftLine();
        SendConsoleMessage(new ConsoleLine("[E]: Exit, [J]: Previous Colour, [K]: Next Colour, [R]: Resize Image",
        AdvancedHighlight("[E]: Exit, [J]: Previous Colour, [K]: Next Colour, [R]: Resize Image", [ConsoleColor.DarkBlue], [ConsoleColor.Cyan], 0, 2, 5, 8)));
        ShiftLine();

    }

    private void DeleteImage()
    {
        string[] imageFiles = GetSubFiles(GeneratePath(DataLocation.Console, "Assets/Stickers"));

        if (imageFiles.Length == 0)
        {
            SendConsoleMessage(new ConsoleLine("No Images Found! Press Enter Key To Continue.", ConsoleColor.DarkBlue));
            ShiftLine();
            UserInput.WaitForUserInput();
            ClearPrimaryConsole();
            return;
        }

        int option = UserInput.CreateMultiPageOptionMenu("Images", [.. imageFiles.Select(x => new ConsoleLine(x[..^5], AppRegistry.activeApp.colourScheme.secondaryColour))], [new ConsoleLine("Exit", AppRegistry.activeApp.colourScheme.primaryColour)], 4);

        if (option == -1) return;

        DeleteFile(GeneratePath(DataLocation.Console, "Assets/Stickers", imageFiles[option]));
        SendConsoleMessage(new ConsoleLine("Image Deleted! Press Enter Key To Continue.", ConsoleColor.DarkBlue));
        ShiftLine();
        UserInput.WaitForUserInput();
        ClearPrimaryConsole();
    }

    private void Hotkeys()
    {
        ConsoleColor[] colours = BuildArray(ConsoleColor.Cyan.Extend(4), ConsoleColor.DarkBlue.ToArray());

        UserInput.CreateReadMenu("Hotkeys", 4,
            new ConsoleLine("[A]: Move Cursor Left.", colours),
            new ConsoleLine("[D]: Move Cursor Right.", colours),
            new ConsoleLine("[W]: Move Cursor Up.", colours),
            new ConsoleLine("[S]: Move Cursor Down.", colours),
            new ConsoleLine("[J]: Previous Colour.", colours),
            new ConsoleLine("[K]: Next Colour.", colours),
            new ConsoleLine("[R]: Resize Image.", colours),
            new ConsoleLine("[E]: Exit.", colours)
        );
    }
}