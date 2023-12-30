using Revistone.Console;
using Revistone.Console.Image;
using Revistone.Functions;

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

                List<(EnvironmentObject obj, ConsoleImage image, int order)> objImages = new List<(EnvironmentObject obj, ConsoleImage image, int order)>();

                foreach (EnvironmentObject obj in env.objects)
                {
                    if (obj.HasComponent<EnvironmentSprite>())
                    {
                        EnvironmentSprite s = obj.GetComponent<EnvironmentSprite>();
                        objImages.Add((obj, new ConsoleImage(s.sprite), s.spriteOrder));
                    }
                    if (obj.HasComponent<EnvironmentText>())
                    {
                        EnvironmentText t = obj.GetComponent<EnvironmentText>();
                        objImages.Add((obj, TitleFunctions.CreateTitle($"{t.text}", ConsoleColor.Black, ConsoleColor.Cyan, TitleFunctions.AsciiFont.Standard), t.textOrder));
                    }
                }

                objImages = objImages.OrderBy(obj => obj.order).ToList();

                for (int i = 0; i < objImages.Count; i++)
                {
                    renderImage.OverlayImage(
                    (int)Math.Round(objImages[i].obj.transform.position.x - environmentPosition.x),
                    (int)Math.Round(objImages[i].obj.transform.position.y - environmentPosition.y), objImages[i].image);
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