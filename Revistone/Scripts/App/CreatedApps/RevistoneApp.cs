namespace Revistone
{
    namespace Apps
    {
        public class RevistoneApp : App
        {
            public RevistoneApp(string name) : base(name) {}
            public RevistoneApp(string name, ConsoleColor[] colours, int colourSpeed = 5) : base(name, colours, colourSpeed) {}
        }
    }
}