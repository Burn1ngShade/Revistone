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
            public class ConsoleEnvironment2D
            {
                public List<ConsoleObject2D> objects { get; private set; } = new List<ConsoleObject2D>();

                public ConsoleColor bgColour;

                // --- CONSTRUCTORS ---

                /// <summary> Pertains environment for console application. </summary>
                public ConsoleEnvironment2D(ConsoleColor bgColour = ConsoleColor.White)
                {
                    this.bgColour = bgColour;
                }

                // --- SIMS ---

                /// <summary> Steps the physics of the environment by a given time. </summary>
                public void PhysicStep(double stepTime)
                {
                    ConsoleAction.SendDebugMessage(Manager.deltaTime);
                    for (int i = 0; i < objects.Count; i++)
                    {
                        objects[i].position.x += objects[i].velocity.x * Manager.deltaTime;
                        objects[i].position.y += objects[i].velocity.y * Manager.deltaTime;
                    }
                }

                /// <summary> Steps the physics of the environment by delta time. </summary>
                public void PhysicStep()
                {
                    PhysicStep(Manager.deltaTime);
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