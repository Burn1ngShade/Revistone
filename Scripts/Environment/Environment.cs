using Revistone.Management;

namespace Revistone
{
    namespace Environment
    {
        /// <summary> Pertains environment for console application. </summary>
        public class Environment
        {
            public List<EnvironmentObject> objects { get; private set; } = new List<EnvironmentObject>();

            public ConsoleColor bgColour;

            // --- CONSTRUCTORS ---

            /// <summary> Pertains environment for console application. </summary>
            public Environment(ConsoleColor bgColour = ConsoleColor.White)
            {
                this.bgColour = bgColour;
            }

            // --- SIMS ---

            /// <summary> Steps the the environment forward by a given time. </summary>
            public void Step(double stepTime)
            {
                for (int i = 0; i < objects.Count; i++)
                {
                    objects[i].TryGetComponent<EnvironmentRigidbody>()?.Step(this, objects[i], stepTime);
                }
            }

            /// <summary> Steps the the environment forward by delta time. </summary>
            public void Step()
            {
                Step(Manager.deltaTime);
            }

            //--- OBJ MODIFICATIONS ---

            /// <summary> Adds given obj to environment. </summary>
            public void AddObject(EnvironmentObject obj)
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