using System.Runtime.InteropServices;
using Revistone.Console.Image;

namespace Revistone
{
    namespace Console
    {
        namespace Environment
        {
            public class ConsoleEnvironment
            {
                public List<ConsoleObject> objects { get; private set; } = new List<ConsoleObject>();

                public ConsoleColor bgColour;

                public ConsoleEnvironment(ConsoleColor bgColour)
                {
                    this.bgColour = bgColour;
                }

                //--- OBJ MODIFICATIONS ---

                public void AddObject(ConsoleObject obj)
                {
                    objects.Add(obj);
                }

                public bool RemoveObject(int objID)
                {
                    for (int i = 0; i < objects.Count; i++)
                    {
                        if (objID == objects[i].id)
                        {
                            objects.RemoveAt(i);
                            return true;
                        }
                    }

                    return false;
                }

                //--- RENDERING ---

                public void Render((int x, int y) outputPosition, (int x, int y) environmentPosition, (int width, int height) size)
                {
                    ConsoleImage renderImage = new ConsoleImage(size.width, size.height, bgColour: bgColour);

                    for (int i = 0; i < objects.Count; i++)
                    {
                        renderImage.OverlayImage(objects[i].position.x, objects[i].position.y, objects[i].sprite);
                    }

                    renderImage.SendToConsole(outputPosition.x, outputPosition.y);
                }
            }
        }
    }
}