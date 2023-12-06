using Revistone.Interaction;
using Revistone.Console.Environment;
using Revistone.Console.Image;
using Revistone.Console;

using static Revistone.Functions.ColourFunctions;

namespace Revistone
{
    namespace Apps
    {
        public class FlappyBirdApp : App
        {
            ConsoleEnvironment2D game = new ConsoleEnvironment2D();

            public FlappyBirdApp() : base() { }
            public FlappyBirdApp(string name, (ConsoleColor primaryColour, ConsoleColor[] secondaryColour, int speed) consoleSettings, (ConsoleColor[] colours, int speed) borderSettings, (UserInputProfile format, Action<string> payload, string summary)[] appCommands, int minAppWidth = 30, int minAppHeight = 30, bool baseCommands = true) : base(name, consoleSettings, borderSettings, appCommands, minAppWidth, minAppHeight, baseCommands) { }

            public override App[] OnRegister()
            {
                return new FlappyBirdApp[] {
                    new FlappyBirdApp("Flappy Bird", (ConsoleColor.DarkBlue, ConsoleColor.DarkGreen.ToArray(), 10), (Alternate(DarkGreenAndDarkBlue, 6, 3), 5), new (UserInputProfile format, Action<string> payload, string summary)[0], 105, 50)
                };
            }

            public override void OnAppInitalisation()
            {
                ConsoleObject2D obj = new ConsoleObject2D("Bird", new ConsoleImage(3, 3, bgColour: ConsoleColor.Yellow), 5, 19);
                obj.velocity = (0, -1);

                game = new ConsoleEnvironment2D(ConsoleColor.Cyan);
                game.AddObject(obj);

                while (true)
                {
                    if (UserRealtimeInput.KeyPressedUp(0x01)) obj.velocity.y = -obj.velocity.y;
                }
            }

            public override void OnUpdate(int tickNum)
            {
                game.PhysicStep();
                game.Render((0, 1), (0, 0), (100, 40), 2);
            }
        }
    }
}