using Revistone.Interaction;
using Revistone.Environment;
using Revistone.Console.Image;

using static Revistone.Functions.ColourFunctions;
using Revistone.Functions;
using Revistone.Management;
using Revistone.Console;

namespace Revistone
{
    namespace Apps
    {
        public class FlappyBirdApp : App
        {
            public FlappyBirdApp() : base() { }
            public FlappyBirdApp(string name, (ConsoleColor primaryColour, ConsoleColor[] secondaryColour, int speed) consoleSettings, (ConsoleColor[] colours, int speed) borderSettings, (UserInputProfile format, Action<string> payload, string summary)[] appCommands, int minAppWidth = 30, int minAppHeight = 30, bool baseCommands = true) : base(name, consoleSettings, borderSettings, appCommands, minAppWidth, minAppHeight, baseCommands) { }

            public override App[] OnRegister()
            {
                return new FlappyBirdApp[] {
                    new FlappyBirdApp("Flappy Bird", (ConsoleColor.DarkBlue, ConsoleColor.DarkGreen.ToArray(), 10), (Alternate(DarkGreenAndDarkBlue, 6, 3), 5), new (UserInputProfile format, Action<string> payload, string summary)[0], 150, 50)
                };  
            }

            Environment.Environment game = new Environment.Environment(ConsoleColor.Cyan);
            EnvironmentObject bird = new EnvironmentObject("Flappy Bird", new EnvironmentTransform((10, 15)),
                new EnvironmentSprite(new ConsoleImage(16, 7)),
                new EnvironmentRigidbody((0, 0), (0, 0), 40, true),
                new EnvironmentHitbox((16, 7), (0, 0)));

            public override void OnAppInitialisation()
            {
                bird.GetComponent<EnvironmentSprite>().sprite.SetBGPixels(0, 0, 16, 7, ColourKey(AppPersistentData.LoadFile("FlappyBird/Art", 0).Split(':')).Stretch(2));

                game.AddObject(bird);
                game.AddObject(new EnvironmentObject("Floor", new EnvironmentTransform((0, -1)), new EnvironmentHitbox((20, 1), (0, 0))));
                game.AddObject(new EnvironmentObject("Roof", new EnvironmentTransform((0, 50)), new EnvironmentHitbox((10, 1), (0, 0))));

                while (true)
                {

                }
            }

            int ticksTillPipeSpawn = 0;
            int pipeCount = 0;
            int pipeSpacing = 15;

            public override void OnUpdate(int tickNum)
            {
                if (UserRealtimeInput.KeyPressedDown(0x01)) //player jump
                {
                    bird.GetComponent<EnvironmentRigidbody>().velocity.y = 15;
                }

                //spawn pipes
                if (ticksTillPipeSpawn == 0)
                {
                    for (int i = 0; i < game.objects.Count; i++)
                    {
                        if (game.objects[i].name.Substring(0, 4) == "Pipe" && game.objects[i].transform.position.x < -12) game.RemoveObject(game.objects[i].id);
                    }

                    int pipePoint = Manager.rng.Next(15, 36);

                    ConsoleImage pipeSprite = new ConsoleImage(24, 30);
                    pipeSprite.SetBGPixels(0, 0, 24, 30, ColourKey(AppPersistentData.LoadFile("FlappyBird/Art", 1).Split(':')).Stretch(2).Reverse().ToArray());
                    EnvironmentObject pipe = new EnvironmentObject($"Pipe {pipeCount * 2}", new EnvironmentTransform((146, pipePoint - 30 - pipeSpacing)));
                    pipe.AddComponent(new EnvironmentSprite(pipeSprite));
                    pipe.AddComponent(new EnvironmentRigidbody((-30, 0), (0, 0)));

                    ConsoleImage topPipeSprite = new ConsoleImage(24, 30);
                    topPipeSprite.SetBGPixels(0, 0, 24, 30, ColourKey(AppPersistentData.LoadFile("FlappyBird/Art", 1).Split(':')).Stretch(2));
                    EnvironmentObject topPipe = new EnvironmentObject($"Pipe {pipeCount * 2 + 1}", new EnvironmentTransform((146, pipePoint + pipeSpacing)));
                    topPipe.AddComponent(new EnvironmentSprite(topPipeSprite));
                    topPipe.AddComponent(new EnvironmentRigidbody((-30, 0), (0, 0)));

                    game.AddObject(pipe);
                    game.AddObject(topPipe);

                    pipeCount++;
                    ticksTillPipeSpawn = Math.Max(195 - pipeCount * 5, 120);
                    pipeSpacing = Math.Clamp(pipeSpacing - 1, 5, 15);
                    ConsoleAction.SendDebugMessage(ticksTillPipeSpawn);
                }

                ticksTillPipeSpawn--;

                game.Step();
                game.Render((0, 1), (0, 0), (145, 50));
            }
        }
    }
}