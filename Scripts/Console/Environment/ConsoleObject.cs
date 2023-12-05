using Revistone.Console.Image;

namespace Revistone
{
    namespace Console
    {
        namespace Environment
        {
            /// <summary> Object within a console environment. </summary>
            public class ConsoleObject
            {
                static int currentID;

                public string name;
                int _id;
                public int id { get; }

                public (int x, int y) position;

                public ConsoleImage sprite;
                public int spriteOrder;

                // --- CONSTRUCTORS ---

                /// <summary> Object within a console environment. </summary>
                public ConsoleObject(string name, ConsoleImage sprite, (int x, int y) position, int spriteOrder = 0)
                {
                    this.name = name;
                    this.position = position;
                    this.sprite = sprite;
                    this.spriteOrder = spriteOrder;

                    AssignID();
                }

                /// <summary> Object within a console environment. </summary>
                public ConsoleObject(string name, ConsoleImage sprite, int x = 0, int y = 0) : this(name, sprite, (x, y)) { }
                /// <summary> Object within a console environment. </summary>
                public ConsoleObject(string name = "New Object") : this(name, new ConsoleImage(bgColour: ConsoleColor.White), (0, 0)) { }

                /// <summary> Assigns unique ID to obj, should not be called after inits</summary>
                void AssignID()
                {
                    this._id = currentID++;
                }
            }
        }
    }
}