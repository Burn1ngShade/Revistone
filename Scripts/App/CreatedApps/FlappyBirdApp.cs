using Revistone.Interaction;
using Revistone.Console.Environment;
using Revistone.Console.Image;
using Revistone.Console;

using static Revistone.Functions.ColourFunctions;
using static Revistone.Functions.StringFunctions;

namespace Revistone
{
    namespace Apps
    {
        public class FlappyBirdApp : App
        {
            ConsoleEnvironment game = new ConsoleEnvironment(ConsoleColor.Cyan);
            List<ConsoleObject> obj = new List<ConsoleObject>();

            public FlappyBirdApp() : base() { }
            public FlappyBirdApp(string name, (ConsoleColor primaryColour, ConsoleColor[] secondaryColour, int speed) consoleSettings, (ConsoleColor[] colours, int speed) borderSettings, (UserInputProfile format, Action<string> payload, string summary)[] appCommands, int minAppWidth = 30, int minAppHeight = 30, bool baseCommands = true) : base(name, consoleSettings, borderSettings, appCommands, minAppWidth, minAppHeight, baseCommands) { }

            public override App[] OnRegister()
            {
                return new FlappyBirdApp[] {
                    new FlappyBirdApp("Flappy Bird", (ConsoleColor.DarkBlue, ConsoleColor.DarkGreen.ToArray(), 10), (Alternate(DarkGreenAndDarkBlue, 6, 3), 5), new (UserInputProfile format, Action<string> payload, string summary)[0], 150, 30)
                };
            }

            public override void OnAppInitalisation()
            {
                //cyan = B, green = A

                ConsoleImage birdSprite = new ConsoleImage(4, 4, bgColour: ConsoleColor.Red);
                ConsoleObject bird = new ConsoleObject("Flappy Bird", new ConsoleObject.Transform((10, 15)), new SpriteComponent(birdSprite), new RigidbodyComponent((0, 0), (0, 0), 0));
                obj.Add(bird);
                game.AddObject(bird);
                ConsoleAction.SendDebugMessage(bird.HasComponent<SpriteComponent>());

                while (true)
                {

                }
            }

            public override void OnUpdate(int tickNum)
            {
                if (UserRealtimeInput.KeyPressedDown(ConsoleKey.A)) obj[0].GetComponentNonNullable<RigidbodyComponent>().velocity = (-10, 0);
                if (UserRealtimeInput.KeyPressedDown(ConsoleKey.D)) obj[0].GetComponentNonNullable<RigidbodyComponent>().velocity = (10, 0);
                if (UserRealtimeInput.KeyPressedDown(ConsoleKey.S)) obj[0].GetComponentNonNullable<RigidbodyComponent>().velocity = (0, 0);

                game.Step();
                game.Render((0, 1), (0, 0), (145, 30), 2);
            }
        }
    }
}