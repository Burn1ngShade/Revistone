using System.Runtime.InteropServices;
using Revistone.Console.Image;

namespace Revistone
{
    namespace Console
    {
        namespace Environment
        {
            /// <summary> Pertains environment for console application. </summary>
            public class ConsoleEnvironment2D
            {
                public List<ConsoleObject2D> objects { get; private set; } = new List<ConsoleObject2D>();

                public ConsoleColor bgColour;

                /// <summary> Pertains environment for console application. </summary>
                public ConsoleEnvironment2D(ConsoleColor bgColour)
                {
                    this.bgColour = bgColour;
                }

                //--- OBJ MODIFICATIONS ---

                /// <summary> Adds given obj to environment. </summary>
                public void AddObject(ConsoleObject2D obj)
                {
                    objects.Add(obj);
                }

                /// <summary> Attempts to remove obj of given ID from environment. </summary>
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
            }
        }
    }
}