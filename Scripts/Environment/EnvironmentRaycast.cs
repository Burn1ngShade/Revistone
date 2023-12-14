using System.Security.Cryptography.X509Certificates;
using Revistone.Console;
using Revistone.Functions;

namespace Revistone
{
    namespace Environment
    {
        public static class EnvironmentRaycast
        {
            public static RaycastData Raycast(EnvironmentSpace environment, (double x, double y) orgin, (double x, double y) end, EnvironmentHitbox hitbox, List<int> ignoreObjectsID)
            {
                (double x, double y) dif = (end.x - orgin.x, end.y - orgin.y);
                int pointCount = (int)Math.Max(Math.Abs(dif.x) + 1, Math.Abs(dif.y) + 1);
                (double x, double y) pointDif = (dif.x / pointCount, dif.y / pointCount);

                List<(double, double)[]> bounds = new List<(double, double)[]>();

                for (int i = 1; i <= pointCount; i++)
                {
                    bounds.Add(hitbox.GetBounds(new EnvironmentTransform((orgin.x + pointDif.x * i, orgin.y + pointDif.y * i))));
                }

                int hitIndex = -1;
                List<EnvironmentObject> collisionObjects = new List<EnvironmentObject>();
                for (int i = 0; i < environment.objects.Count; i++)
                {
                    if (ignoreObjectsID.Contains(environment.objects[i].id) || !environment.objects[i].HasComponent<EnvironmentHitbox>()) continue;
                    (double, double)[] objBounds = environment.objects[i].GetComponent<EnvironmentHitbox>().GetBounds(environment.objects[i].transform);
                    for (int j = 0; j < bounds.Count; j++)
                    {
                        if (EnvironmentHitbox.HitboxOverlap(bounds[j], objBounds))
                        {
                            if (hitIndex == -1) hitIndex = j;
                            collisionObjects.Add(environment.objects[i]);
                            break;
                        }
                    }
                }

                if (hitIndex != -1) return new RaycastData(true, bounds.Select(b => b[0]).ToArray(), hitIndex, collisionObjects);

                return new RaycastData(false, bounds.Select(b => b[0]).ToArray(), pointCount - 1, new List<EnvironmentObject>());
            }

            public static RaycastData Raycast(EnvironmentSpace environment, (double x, double y) orgin, (double x, double y) end, EnvironmentHitbox hitbox)
            {
                return Raycast(environment, orgin, end, hitbox, new List<int>());
            }

            public static RaycastData Raycast(EnvironmentSpace environment, (double x, double y) orgin, (double x, double y) end)
            {
                return Raycast(environment, orgin, end, new EnvironmentHitbox((1, 1), (0, 0)), new List<int>());
            }

            public struct RaycastData
            {
                public bool hit;
                public (double x, double y)[] points;
                public int lastPointIndex;

                public List<EnvironmentObject> collidingObjects;

                public RaycastData(bool hit, (double, double)[] points, int lastPointIndex, List<EnvironmentObject> collidingObjects)
                {
                    this.hit = hit;
                    this.points = points;
                    this.lastPointIndex = lastPointIndex;

                    this.collidingObjects = collidingObjects;
                }
            }
        }
    }
}