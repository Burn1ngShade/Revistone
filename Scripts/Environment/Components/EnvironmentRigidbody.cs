using Revistone.Console;

namespace Revistone
{
    namespace Environment
    {
        public class EnvironmentRigidbody : EnvironmentComponent
        {
            public (double x, double y) velocity;
            public (double x, double y) acceleration;

            public double gravity = 0;

            public bool interactive;

            public List<EnvironmentObject> collidingObj = new List<EnvironmentObject>();

            public EnvironmentRigidbody((double x, double y) velocity, (double x, double y) acceleration, double gravity = 0, bool interactive = false)
            {
                this.velocity = velocity;
                this.acceleration = acceleration;
                this.gravity = gravity;

                this.interactive = interactive;
            }

            public void Step(Environment environment, EnvironmentObject obj, double stepTime)
            {
                velocity.x += acceleration.x * Management.Manager.deltaTime;
                velocity.y += acceleration.y * Management.Manager.deltaTime;

                velocity.y -= gravity * Management.Manager.deltaTime;

                (double x, double y) targetPos = (obj.transform.position.x + velocity.x * Management.Manager.deltaTime, obj.transform.position.y + velocity.y * Management.Manager.deltaTime);

                if (obj.HasComponent<EnvironmentHitbox>() && interactive)
                {
                    (double, double)[] bounds = obj.GetComponent<EnvironmentHitbox>().GetBounds(new EnvironmentTransform(targetPos));
                    List<EnvironmentObject> newCollidingObj = new List<EnvironmentObject>();

                    for (int i = 0; i < environment.objects.Count; i++)
                    {
                        if (!environment.objects[i].HasComponent<EnvironmentHitbox>() || obj.id == environment.objects[i].id) continue;

                        if (EnvironmentHitbox.HitboxOverlap(bounds, environment.objects[i].GetComponent<EnvironmentHitbox>().GetBounds(environment.objects[i].transform)))
                        {
                            newCollidingObj.Add(environment.objects[i]);   
                        }
                    }

                    collidingObj = newCollidingObj;
                }
                
                if (collidingObj.Count == 0)
                {
                    obj.transform.position = targetPos;
                }
            }
        }
    }
}