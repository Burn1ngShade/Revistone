namespace Revistone
{
    namespace Environment
    {
        public class EnvironmentTransform : EnvironmentComponent
        {
            public (double x, double y) position;

            public EnvironmentTransform((double, double) position)
            {
                this.position = position;
            }
        }
    }
}