using Revistone.Console.Image;

namespace Revistone
{
    namespace Environment
    {
        public static class EnvironmentRenderer
        {
            /// <summary> Renders environment to console (integer values reccomened for porporitonal to line up with physics). </summary>
            public static void Render(this Environment env, (int x, int y) outputPosition, (double x, double y) environmentPosition, (int width, int height) size)
            {
                ConsoleImage renderImage = new ConsoleImage(size.width, size.height, bgColour: env.bgColour);

                EnvironmentObject[] orderedObjs = env.objects.Where(obj => obj.HasComponent<EnvironmentSprite>()).
                OrderBy(obj => obj.GetComponent<EnvironmentSprite>()?.spriteOrder).ToArray();

                for (int i = 0; i < orderedObjs.Length; i++)
                {
                    ConsoleImage outputImage = new ConsoleImage(orderedObjs[i].GetComponent<EnvironmentSprite>().sprite);

                    renderImage.OverlayImage(
                    (int)Math.Round(orderedObjs[i].transform.position.x - environmentPosition.x),
                    (int)Math.Round(orderedObjs[i].transform.position.y - environmentPosition.y), outputImage);
                }

                renderImage.SendToConsole(outputPosition.x, outputPosition.y);
            }
        }
    }
}