using Revistone.Console.Image;

namespace Revistone
{
    namespace Console
    {
        namespace Environment
        {
            public static class ConsoleEnvironmentRenderer
            {
                /// <summary> Renders environment to console (integer values reccomened for porporitonal to line up with physics). </summary>
                public static void Render(this ConsoleEnvironment2D env, (int x, int y) outputPosition, (double x, double y) environmentPosition, (int width, int height) size, double proportional = 1)
                {
                    ConsoleImage renderImage = new ConsoleImage(size.width, size.height, bgColour: env.bgColour);

                    ConsoleObject2D[] orderedObjs = env.objects.OrderBy(obj => obj.spriteOrder).ToArray();

                    for (int i = 0; i < orderedObjs.Length; i++)
                    {
                        ConsoleImage outputImage = new ConsoleImage(orderedObjs[i].sprite);
                        outputImage.StretchX(proportional);
                        renderImage.OverlayImage((int)Math.Round(orderedObjs[i].position.x - environmentPosition.x), (int)Math.Round(orderedObjs[i].position.y - environmentPosition.y), outputImage);
                    }

                    renderImage.SendToConsole(outputPosition.x, outputPosition.y);
                }
            }
        }
    }
}