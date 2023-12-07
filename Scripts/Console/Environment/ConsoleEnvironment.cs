using System.Runtime.InteropServices;
using Revistone.Console.Image;
using Revistone.Management;

namespace Revistone
{
    namespace Console
    {
        namespace Environment
        {
            /// <summary> Pertains environment for console application. </summary>
            public class ConsoleEnvironment
            {
                public List<ConsoleObject> objects { get; private set; } = new List<ConsoleObject>();

                public ConsoleColor bgColour;

                // --- CONSTRUCTORS ---

                /// <summary> Pertains environment for console application. </summary>
                public ConsoleEnvironment(ConsoleColor bgColour = ConsoleColor.White)
                {
                    this.bgColour = bgColour;
                }

                // --- SIMS ---

                /// <summary> Steps the the environment forward by a given time. </summary>
                public void Step(double stepTime)
                {
                    for (int i = 0; i < objects.Count; i++)
                    {
                        if (!objects[i].HasComponent<RigidbodyComponent>()) continue;
                        objects[i].GetComponentNonNullable<RigidbodyComponent>().Step(objects[i], stepTime); 
                    }
                }

                /// <summary> Steps the the environment forward by delta time. </summary>
                public void Step()
                {
                    Step(Manager.deltaTime);
                }

                //--- OBJ MODIFICATIONS ---

                /// <summary> Adds given obj to environment. </summary>
                public void AddObject(ConsoleObject obj)
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