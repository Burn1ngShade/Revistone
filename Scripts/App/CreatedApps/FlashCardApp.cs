using Revistone.Interaction;

namespace Revistone
{
    namespace Apps
    {
        public class FlashCardApp : App
        {
            public FlashCardApp() : base() {}
            public FlashCardApp(string name, ConsoleColor[] borderColours, (UserInputProfile format, Action<string> payload, string summary)[] appCommands, int borderColourSpeed = 5, int minAppWidth = 30, int minAppHeight = 30, bool baseCommands = true) : base(name, borderColours, appCommands, borderColourSpeed, minAppWidth, minAppHeight, baseCommands) {}
        }
    }
}