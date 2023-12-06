using Revistone.Console.Image;

namespace Revistone
{
    namespace Console
    {
        namespace Environment
        {
            /// <summary> Object within a console environment. </summary>
            public class ConsoleObject2D
            {
                static int currentID;

                public string name;
                int _id;
                public int id { get; }

                public (double x, double y) position;
                public (double x, double y) velocity;

                public ConsoleImage sprite;
                public int spriteOrder;

                // --- CONSTRUCTORS ---

                /// <summary> Object within a console environment. </summary>
                public ConsoleObject2D(string name, ConsoleImage sprite, (double x, double y) position, int spriteOrder = 0)
                {
                    this.name = name;
                    this.position = position;
                    this.sprite = sprite;
                    this.spriteOrder = spriteOrder;

                    this.velocity = (0, 0);

                    AssignID();
                }

                /// <summary> Object within a console environment. </summary>
                public ConsoleObject2D(string name, ConsoleImage sprite, double x = 0, double y = 0) : this(name, sprite, (x, y)) { }
                /// <summary> Object within a console environment. </summary>
                public ConsoleObject2D(string name = "New Object") : this(name, new ConsoleImage(bgColour: ConsoleColor.White), (0, 0)) { }

                /// <summary> Assigns unique ID to obj, should not be called after inits</summary>
                void AssignID()
                {
                    this._id = currentID++;
                }
            }
        }
    }
}