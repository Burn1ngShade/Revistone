using Revistone.Console.Image;

namespace Revistone
{
    namespace Environment
    {
        public static class EnvironmentRenderer
        {
            /// <summary> Renders environment to console (integer values reccomened for porporitonal to line up with physics). </summary>
            public static void Render(this EnvironmentSpace env, (int x, int y) outputPosition, (double x, double y) environmentPosition, (int width, int height) size, int borderColour = -1)
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

                if (borderColour >= 0 && borderColour < 16)
                {
                    renderImage.SetBGPixels(0, 0, renderImage.size.width, 1, (ConsoleColor)borderColour);
                    renderImage.SetBGPixels(0, renderImage.size.height - 1, renderImage.size.width, 1, (ConsoleColor)borderColour);
                    renderImage.SetBGPixels(0, 0, 2, renderImage.size.height, (ConsoleColor)borderColour);
                    renderImage.SetBGPixels(renderImage.size.width - 2, 0, 2, renderImage.size.height, (ConsoleColor)borderColour);
                    
                }

                renderImage.SendToConsole(outputPosition.x, outputPosition.y);
            }
        }
    }
}