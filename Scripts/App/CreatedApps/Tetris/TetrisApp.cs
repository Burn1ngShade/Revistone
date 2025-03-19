using Revistone.Console;
using Revistone.Functions;
using Revistone.Interaction;
using Revistone.Management;
using static Revistone.Console.ConsoleAction;
using static Revistone.Functions.ColourFunctions;

namespace Revistone.App.BaseApps.Tetris;

public class TetrisApp : App
{
    public TetrisApp() : base() { }
    public TetrisApp(string name, (ConsoleColor[] primaryColour, ConsoleColor[] secondaryColour, ConsoleColor[] tertiaryColour, int speed) consoleSettings, (ConsoleColor[] colours, int speed) borderSettings, (UserInputProfile format, Action<string> payload, string summary)[] appCommands, int minAppWidth = 30, int minAppHeight = 30, bool baseCommands = true) : base(name, consoleSettings, borderSettings, appCommands, minAppWidth, minAppHeight, baseCommands) { }

    public override App[] OnRegister()
    {
        return [ new TetrisApp("Tetris", (ConsoleColor.DarkBlue.ToArray(), ConsoleColor.Cyan.ToArray(), ConsoleColor.Blue.ToArray(), 10), (CyanDarkBlueGradient.Extend(7, true), 5),
            [], 98, 40) ];
    }

    public override void OnAppInitalisation()
    {
        base.OnAppInitalisation();

        ConsoleLine[] title = TitleFunctions.CreateTitle("TETRIS", AdvancedHighlight(62, ConsoleColor.DarkBlue.ToArray(), (ConsoleColor.Cyan.ToArray(), 0, 10)), TitleFunctions.AsciiFont.BigMoneyNW, letterSpacing: 1, bottomSpace: 1, topSpace: 1);
        SendConsoleMessages(title,
        Enumerable.Repeat(new ConsoleAnimatedLine(ConsoleAnimatedLine.ShiftForegroundColour, "", AppRegistry.activeApp.borderColourScheme.speed, true), title.Length).ToArray());

        for (int i = 0; i <= 10; i++)
        {
            UpdateLineExceptionStatus(true, i);
        }


        int menuIndex = 0;
        while (true)
        {
            menuIndex = UserInput.CreateOptionMenu("Options", [
            (new ConsoleLine("Play", ConsoleColor.Cyan), Play),
            (new ConsoleLine("Controls", ConsoleColor.Cyan), () => {UserInput.CreateReadMenu("Controls", 4,
                new ConsoleLine("[A]: Move Piece Left.", ConsoleColor.Cyan),
                new ConsoleLine("[D]: Move Piece Right.", ConsoleColor.Cyan),
                new ConsoleLine("[Q]: Rotate Piece Left.", ConsoleColor.Cyan),
                new ConsoleLine("[E]: Rotate Piece Right.", ConsoleColor.Cyan),
                new ConsoleLine("[S]: Quick Drop Piece.", ConsoleColor.Cyan),
                new ConsoleLine("[Space]: Slam Drop Piece.", ConsoleColor.Cyan),
                new ConsoleLine("[R]: Hold Piece.", ConsoleColor.Cyan),
                new ConsoleLine("[P]: Pause Game.", ConsoleColor.Cyan),
                new ConsoleLine("[E]: End Game (If Paused)", ConsoleColor.Cyan));}),
            (new ConsoleLine("Exit", ConsoleColor.Cyan), () => { ExitApp(); })], cursorStartIndex: menuIndex);

            if (menuIndex == 2) return;
        }
    }

    static TetrisGameboard gb = new();

    public static void Play()
    {
        gb = new();

        gb.startTick = Manager.currentTick;
        gb.Output();

        while (true)
        {
            (ConsoleKeyInfo key, bool interrupted) = UserRealtimeInput.GetKey();

            if (gb.gameOver)
            {
                ClearPrimaryConsole();
                return;
            }

            if (interrupted)
            {
                ClearPrimaryConsole();
                gb.Output();
                continue;
            }

            switch (key.Key)
            {
                case ConsoleKey.A:
                    if (gb.PieceIsColliding(gb.currentPiece.type, gb.currentPiece.rotation, gb.currentPiece.x - 1, gb.currentPiece.y) || !gb.running) break;
                    gb.currentPiece.x--;
                    gb.Output();
                    break;
                case ConsoleKey.D:
                    if (gb.PieceIsColliding(gb.currentPiece.type, gb.currentPiece.rotation, gb.currentPiece.x + 1, gb.currentPiece.y) || !gb.running) break;
                    gb.currentPiece.x++;
                    gb.Output();
                    break;
                case ConsoleKey.Q:
                    if (gb.PieceIsColliding(gb.currentPiece.type, gb.currentPiece.Rotate(false), gb.currentPiece.x, gb.currentPiece.y) || !gb.running) break;
                    gb.currentPiece.rotation = gb.currentPiece.Rotate(false);
                    gb.Output();
                    break;
                case ConsoleKey.E:
                    if (gb.paused) gb.EndGame();

                    if (gb.PieceIsColliding(gb.currentPiece.type, gb.currentPiece.Rotate(true), gb.currentPiece.x, gb.currentPiece.y) || !gb.running) break;
                    gb.currentPiece.rotation = gb.currentPiece.Rotate(true);
                    gb.Output();
                    break;
                case ConsoleKey.Spacebar:
                    while (!gb.PieceIsColliding(gb.currentPiece.type, gb.currentPiece.rotation, gb.currentPiece.x, gb.currentPiece.y + 1) || !gb.running) gb.currentPiece.y++;
                    gb.Tick();
                    gb.Output();
                    break;
                case ConsoleKey.R:
                    if (!gb.running) continue;
                    gb.ToggleHeldPiece();
                    break;
                case ConsoleKey.P:
                    gb.TogglePause();
                    break;
            }
        }
    }

    public override void OnUpdate(int tickNum)
    {
        if (gb.startTick == -1) return;

        if (((tickNum - gb.startTick) % 16 == 0 || ((tickNum - gb.startTick) % 2 == 0 && UserRealtimeInput.KeyPressed(0x53))) && !gb.paused && !gb.gameOver)
        {
            gb.Tick();
        }
    }
}