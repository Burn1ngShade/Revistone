using Revistone.Interaction;
using Revistone.Environment;
using Revistone.Console.Image;

using static Revistone.Functions.ColourFunctions;
using Revistone.Functions;
using Revistone.Management;

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

            EnvironmentSpace game = new EnvironmentSpace(ConsoleColor.Cyan);
            EnvironmentObject bird = new EnvironmentObject();

            public override void OnAppInitialisation()
            {
                Init();

                while (true)
                {

                }
            }

            int ticksTillPipeSpawn = 0;
            int pipeCount = 0;
            double pipeSpacing = 10;

            int gameState = 1;

            void Init()
            {
                game = new Environment.EnvironmentSpace(ConsoleColor.Cyan);

                bird = new EnvironmentObject("Flappy Bird", new EnvironmentTransform((10, 15)),
        new EnvironmentSprite(new ConsoleImage(16, 7)),
        new EnvironmentRigidbody((0, 0), (0, 0), 40, EnvironmentRigidbody.RigidbodyType.Collision),
        new EnvironmentHitbox((16, 5), (0, 1)));

                bird.GetComponent<EnvironmentSprite>().sprite.SetBGPixels(0, 0, 16, 7, ColourKey(AppPersistentData.LoadFile("FlappyBird/Art", 0).Split(':')).Stretch(2));

                game.AddObject(bird);
                game.AddObject(new EnvironmentObject("Floor", new EnvironmentTransform((0, 1)), new EnvironmentHitbox((20, 1), (0, 0))));
                game.AddObject(new EnvironmentObject("Roof", new EnvironmentTransform((0, 48)), new EnvironmentHitbox((20, 1), (0, 0))));

                gameState = 2;

                ticksTillPipeSpawn = 0;
                pipeCount = 0;
                pipeSpacing = 10;
            }

            public override void OnUpdate(int tickNum)
            {
                if (gameState == 0)
                {
                    if (UserRealtimeInput.KeyPressedDown(0x01)) gameState = 2;
                }
                else if (gameState == 1)
                {
                    if (UserRealtimeInput.KeyPressedDown(0x01))
                    {
                        Init();
                    }
                }
                else
                {
                    foreach (EnvironmentObject e in bird.GetComponent<EnvironmentRigidbody>().collidingObj)
                    {
                        if (e.HasTag("Kill"))
                        {
                            gameState = 1;
                            return;
                        }
                    }

                    if (UserRealtimeInput.KeyPressedDown(0x01)) //player jump
                    {
                        bird.GetComponent<EnvironmentRigidbody>().velocity.y = 15;
                    }

                    if (ticksTillPipeSpawn == 0)
                    {
                        for (int i = 0; i < game.objects.Count; i++)
                        {
                            if (game.objects[i].name.Substring(0, 4) == "Pipe" && game.objects[i].transform.position.x < -12) game.RemoveObject(game.objects[i].id);
                        }

                        int pipePoint = Manager.rng.Next(15, 36);

                        ConsoleImage pipeSprite = new ConsoleImage(24, 30);
                        pipeSprite.SetBGPixels(0, 0, 24, 30, ColourKey(AppPersistentData.LoadFile("FlappyBird/Art", 1).Split(':')).Stretch(2).Reverse().ToArray());
                        EnvironmentObject pipe = new EnvironmentObject($"Pipe {pipeCount * 2}", new EnvironmentTransform((146, pipePoint - 30 - Math.Floor(pipeSpacing))));
                        pipe.AddTag("Kill");
                        pipe.AddComponent(new EnvironmentSprite(pipeSprite));
                        pipe.AddComponent(new EnvironmentRigidbody((-30, 0), (0, 0)));
                        pipe.AddComponent(new EnvironmentHitbox((24, 30), (0, 0)));

                        ConsoleImage topPipeSprite = new ConsoleImage(24, 30);
                        topPipeSprite.SetBGPixels(0, 0, 24, 30, ColourKey(AppPersistentData.LoadFile("FlappyBird/Art", 1).Split(':')).Stretch(2));
                        EnvironmentObject topPipe = new EnvironmentObject($"Pipe {pipeCount * 2 + 1}", new EnvironmentTransform((146, pipePoint + Math.Floor(pipeSpacing))));
                        topPipe.AddTag("Kill");
                        topPipe.AddComponent(new EnvironmentSprite(topPipeSprite));
                        topPipe.AddComponent(new EnvironmentRigidbody((-30, 0), (0, 0)));
                        topPipe.AddComponent(new EnvironmentHitbox((24, 30), (0, 0)));

                        game.AddObject(pipe);
                        game.AddObject(topPipe);

                        pipeCount++;
                        ticksTillPipeSpawn = Math.Max(195 - pipeCount * 5, 100);
                        pipeSpacing = Math.Clamp(pipeSpacing - 0.5, 5, 12);
                    }

                    ticksTillPipeSpawn--;

                    game.Step();
                }
                game.Render((0, 1), (0, 0), (145, 50), 9);
            }
        }
    }
}