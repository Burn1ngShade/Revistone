using Revistone.Console;
using Revistone.Management;

namespace Revistone
{
    namespace Environment
    {
        public class EnvironmentRigidbody : EnvironmentComponent
        {
            public (double x, double y) velocity;
            public (double x, double y) acceleration;

            public double gravity = 0;

            public enum RigidbodyType { Simple, Collision }
            public RigidbodyType bodyType;

            public List<EnvironmentObject> collidingObj = new List<EnvironmentObject>();

            public EnvironmentRigidbody((double x, double y) velocity, (double x, double y) acceleration, double gravity = 0, RigidbodyType bodyType = RigidbodyType.Simple)
            {
                this.velocity = velocity;
                this.acceleration = acceleration;
                this.gravity = gravity;

                this.bodyType = bodyType;
            }

            public void Step(EnvironmentSpace environment, EnvironmentObject obj, double stepTime)
            {
                velocity.x += acceleration.x * Management.Manager.deltaTime;
                velocity.y += acceleration.y * Management.Manager.deltaTime;

                velocity.y -= gravity * Management.Manager.deltaTime;

                (double x, double y) targetPos = (obj.transform.position.x + velocity.x * Management.Manager.deltaTime, obj.transform.position.y + velocity.y * Management.Manager.deltaTime);

                if (obj.HasComponent<EnvironmentHitbox>() && bodyType != RigidbodyType.Simple)
                {
                    EnvironmentRaycast.RaycastData r = EnvironmentRaycast.Raycast(environment, obj.transform.position, targetPos, obj.GetComponent<EnvironmentHitbox>(), new List<int>() { obj.id });
                    collidingObj = r.collidingObjects;

                    if (collidingObj.Count == 0) obj.transform.position = targetPos;
                    else if (r.lastPointIndex != 0) obj.transform.position = r.points[r.lastPointIndex - 1];
                }
                else
                {
                    obj.transform.position = targetPos;
                }

                ConsoleAction.SendDebugMessage(obj.transform.position.y);


            }
        }
    }
}