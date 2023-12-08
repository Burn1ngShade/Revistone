using Revistone.Console;

namespace Revistone
{
    namespace Environment
    {
        public class EnvironmentHitbox : EnvironmentComponent
        {
            public (double x, double y) size;
            public (double x, double y) offset;

            public EnvironmentHitbox((double x, double y) size, (double x, double y) offset)
            {
                this.size = (Math.Max(size.x, 0), Math.Max(size.y, 0));
                this.offset = offset;
            }

            public (double x, double y)[] GetBounds(EnvironmentTransform transform)
            {
                return new (double x, double y)[] {
                        (transform.position.x + offset.x, transform.position.y + offset.y),
                        (transform.position.x + offset.x, transform.position.y + offset.y + size.y),
                        (transform.position.x + offset.x + size.x, transform.position.y + offset.y + size.y),
                        (transform.position.x + offset.x + size.x, transform.position.y + offset.y),
                    };
            }

            public static bool HitboxOverlap((double x, double y)[] h1, (double x, double y)[] h2)
            {
                if (h1.Length != 4 || h2.Length != 4) return false;

                for (int i = 0; i < 4; i++)
                {
                    if (h1[i].x >= h2[0].x && h1[i].x <= h2[2].x && h1[i].y >= h2[0].y && h1[i].y <= h2[2].y) return true;
                    if (h2[i].x >= h1[0].x && h2[i].x <= h1[2].x && h2[i].y >= h1[0].y && h2[i].y <= h1[2].y) return true;
                }
                return false;
            }
        }
    }
}