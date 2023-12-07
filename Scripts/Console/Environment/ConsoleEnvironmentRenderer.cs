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
                public static void Render(this ConsoleEnvironment env, (int x, int y) outputPosition, (double x, double y) environmentPosition, (int width, int height) size, double proportional = 1)
                {
                    ConsoleImage renderImage = new ConsoleImage(size.width, size.height, bgColour: env.bgColour);

                    ConsoleObject[] orderedObjs = env.objects.Where(obj => obj.HasComponent<SpriteComponent>()).
                    OrderBy(obj => obj.GetComponentNonNullable<SpriteComponent>()?.spriteOrder).ToArray();

                    for (int i = 0; i < orderedObjs.Length; i++)
                    {
                        ConsoleImage outputImage = new ConsoleImage(orderedObjs[i].GetComponentNonNullable<SpriteComponent>().sprite);
                        outputImage.StretchX(proportional);

                        renderImage.OverlayImage(
                        (int)Math.Round(orderedObjs[i].transform.position.x - environmentPosition.x), 
                        (int)Math.Round(orderedObjs[i].transform.position.y - environmentPosition.y), outputImage);
                    }

                    renderImage.SendToConsole(outputPosition.x, outputPosition.y);
                }
            }
        }
    }
}