using System.ComponentModel.Design;
using System.Globalization;
using Revistone.Console.Image;

namespace Revistone
{
    namespace Console
    {
        namespace Environment
        {
            public abstract class ConsoleComponent
            {
            }

            public class SpriteComponent : ConsoleComponent
            {
                public ConsoleImage sprite;
                public int spriteOrder;

                public SpriteComponent(ConsoleImage sprite, int spriteOrder = 0)
                {

                    this.sprite = sprite;
                    this.spriteOrder = spriteOrder;
                }
            }

            public class HitboxComponent : ConsoleComponent
            {
                public (double x, double y) size;
                public (double x, double y) offset;

                public HitboxComponent((double x, double y) size, (double x, double y) offset)
                {
                    this.size = size;
                    this.offset = offset;
                }
            }

            public class RigidbodyComponent : ConsoleComponent
            {
                public (double x, double y) velocity;
                public (double x, double y) acceleration;

                public double gravity = 0;

                public RigidbodyComponent((double x, double y) velocity, (double x, double y) acceleration, double gravity = 0)
                {
                    this.velocity = velocity;
                    this.acceleration = acceleration;
                    this.gravity = gravity;
                }

                public void Step(ConsoleObject obj, double stepTime)
                {
                    velocity.x += acceleration.x * Management.Manager.deltaTime;
                    velocity.y += acceleration.y * Management.Manager.deltaTime;

                    velocity.y -= gravity * Management.Manager.deltaTime;

                    obj.transform.position.x += velocity.x * Management.Manager.deltaTime;
                    obj.transform.position.y += velocity.y * Management.Manager.deltaTime;
                }
            }
        }
    }
}