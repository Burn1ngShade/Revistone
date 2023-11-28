using Revistone.Interaction;
using Revistone.Console.Image;
using Revistone.Functions;
using Revistone.Console;
using Revistone.Management;

using static Revistone.Functions.ColourFunctions;
using static Revistone.Console.ConsoleAction;

namespace Revistone
{
    namespace Apps
    {
        public class PongApp : App
        {
            int gameState = 0;
            Ball ball;

            (int score, int pos) player1;
            (int score, int pos) player2;

            public PongApp() : base() { }
            public PongApp(string name, (ConsoleColor primaryColour, ConsoleColor[] secondaryColour, int speed) consoleSettings, (ConsoleColor[] colours, int speed) borderSettings, (UserInputProfile format, Action<string> payload, string summary)[] appCommands, int minAppWidth = 30, int minAppHeight = 30, bool baseCommands = true) : base(name, consoleSettings, borderSettings, appCommands, minAppWidth, minAppHeight, baseCommands) { }

            public override App[] OnRegister()
            {
                return new PongApp[] {
                    new PongApp("Pong", (ConsoleColor.DarkBlue, CyanGradient, 10), (CyanDarkBlueGradient, 5), new (UserInputProfile format, Action<string> payload, string summary)[0], 60, 40)
                };
            }

            public override void OnAppInitalisation()
            {
                base.OnAppInitalisation();
                MainMenu();
            }

            public override void OnUpdate(int tickNum)
            {
                base.OnUpdate(tickNum);

                if (gameState == 0) return;

                if (UserRealtimeInput.KeyPressed(ConsoleKey.W)) player1.pos = Math.Clamp(player1.pos + 1, 0, 12);
                if (UserRealtimeInput.KeyPressed(ConsoleKey.S)) player1.pos = Math.Clamp(player1.pos - 1, 0, 12);

                if (UserRealtimeInput.KeyPressed(ConsoleKey.UpArrow) && gameState == 2) player2.pos = Math.Clamp(player2.pos + 1, 0, 12);
                if (UserRealtimeInput.KeyPressed(ConsoleKey.DownArrow) && gameState == 2) player2.pos = Math.Clamp(player2.pos - 1, 0, 12);

                if (gameState == 1 && tickNum % 2 == 0 && ball.cooldown < 3)
                {
                    if (player2.pos + 1 < ball.NextPos.y) player2.pos = Math.Clamp(player2.pos + 1, 0, 12);
                    if (player2.pos + 1 > ball.NextPos.y) player2.pos = Math.Clamp(player2.pos - 1, 0, 12);
                }

                if (ball.cooldown == 0)
                {
                    //win checks
                    if (ball.NextPos.x <= 0)
                    {
                        player2.score++;
                        ResetGame();
                    }
                    if (ball.NextPos.x >= 58)
                    {
                        player1.score++;
                        ResetGame();
                    }

                    //wall check
                    if (ball.NextPos.y < 0 | ball.NextPos.y > 14) ball.velocity.y = -ball.velocity.y;

                    //bumper check
                    if ((ball.NextPos.x == 1 && ball.NextPos.y - player1.pos < 3 && ball.NextPos.y - player1.pos >= 0) || (ball.NextPos.x == 57 && ball.NextPos.y - player2.pos < 3 && ball.NextPos.y - player2.pos >= 0))
                    {
                        ball.speed = Math.Clamp(ball.speed - 1, 0, int.MaxValue);
                        ball.velocity.x = -ball.velocity.x;
                    }

                    ball.pos = ball.NextPos;
                    ball.cooldown = ball.speed;
                }
                else
                {
                    ball.cooldown--;
                }

                DrawFrame();

                if (player1.score == 3 || player2.score == 3)
                {
                    gameState = 0;
                    ClearPrimaryConsole();
                }
            }

            /// <summary> Main menu for pong, allows user to select game mode or return to menu</summary>
            void MainMenu()
            {
                gameState = 0;

                ClearPrimaryConsole();
                ShiftLine();
                SendConsoleMessages(
                    TitleFunctions.CreateTitle("PONG!", AdvancedHighlight(48, ConsoleColor.DarkBlue.ToArray(), (ConsoleColor.Cyan.ToArray(), 0, 10)), TitleFunctions.AsciiFont.BigMoneyNW, letterSpacing: 1),
                    Enumerable.Repeat(new ConsoleAnimatedLine(ConsoleAnimatedLine.ShiftColour, "", AppRegistry.activeApp.borderColourScheme.speed, true), 48).ToArray());
                ShiftLine();
                int i = UserInput.CreateOptionMenu("Options:", new (string, Action)[] {
                    ("1 Player", () => GameLoad(1)),
                    ("2 Player", () => GameLoad(2)),
                    ("Exit", ExitApp)});
                if (i == 2) return;

                while (gameState != 0) { }
                SendConsoleMessages(TitleFunctions.CreateTitle($"PLAYER {(player1.score == 3 ? '1' : '2')} WINS!", TitleFunctions.AsciiFont.Big, letterSpacing: 1).Select(s => new ConsoleLine(s)).ToArray());
                UserInput.WaitForUserInput(space: true);

                MainMenu();
            }

            /// <summary> Load game of pong, setting base conditions, and updating game state.</summary>
            void GameLoad(int state)
            {
                gameState = state;
                player1.score = 0;
                player2.score = 0;
                ResetGame();
                ShiftLine();
                SendConsoleMessage(new ConsoleLine(new string('-', 60), CyanDarkBlueGradient.Extend(60, true)), new ConsoleAnimatedLine(ConsoleAnimatedLine.ShiftColour, "1", 5, true));
                UpdatePrimaryConsoleLine(new ConsoleLine(new string('-', 60), CyanDarkBlueGradient.Extend(60, true)), new ConsoleAnimatedLine(ConsoleAnimatedLine.ShiftColour, "1", 5, true), 28);
            }

            /// <summary> Reset game at start and after each point.</summary>
            void ResetGame()
            {
                player1.pos = 7;
                player2.pos = 7;
                ball.pos = (29, 7);
                ball.speed = 1;
                ball.velocity = new (int, int)[] { (-1, -1), (-1, 1), (1, -1), (1, 1) }[Manager.rnd.Next(0, 4)];
                ball.cooldown = 20;
            }

            /// <summary> Draws frame of pong game.</summary>
            void DrawFrame()
            {
                ConsoleImage frame = new ConsoleImage(59, 15);
                frame.SetPixels(1, player1.pos, 1, 3, '|', ConsoleColor.White);
                frame.SetPixels(57, player2.pos, 1, 3, '|', ConsoleColor.White);
                frame.SetPixels(10, 9, TitleFunctions.CreateTitle($"{player1.score}", ConsoleColor.White.ToArray(), TitleFunctions.AsciiFont.Small).Reverse().ToArray());
                frame.SetPixels(47, 9, TitleFunctions.CreateTitle($"{player2.score}", ConsoleColor.White.ToArray(), TitleFunctions.AsciiFont.Small).Reverse().ToArray());
                frame.SetPixel(ball.pos.x, ball.pos.y, '@', ConsoleColor.White);
                frame.SendToConsole(0, 13);
            }

            /// <summary> All logic for the ball.</summary>
            struct Ball
            {
                public (int x, int y) pos;
                public (int x, int y) velocity;
                public int speed;
                public int cooldown;

                public (int x, int y) NextPos { get { return (pos.x + velocity.x, pos.y + velocity.y); } }
            }
        }
    }
}