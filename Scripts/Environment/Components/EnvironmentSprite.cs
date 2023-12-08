using Revistone.Console.Image;

namespace Revistone
{
    namespace Environment
    {
        public class EnvironmentSprite : EnvironmentComponent
        {
            public ConsoleImage sprite;
            public int spriteOrder;

            public EnvironmentSprite(ConsoleImage sprite, int spriteOrder = 0)
            {
                this.sprite = sprite;
                this.spriteOrder = spriteOrder;
            }
        }
    }
}