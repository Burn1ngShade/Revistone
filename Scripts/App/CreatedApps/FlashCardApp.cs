using Revistone.Functions;
using Revistone.Interaction;

namespace Revistone
{
    namespace Apps
    {
        public class FlashCardApp : App
        {
            public FlashCardApp() : base() { }
            public FlashCardApp(string name, (ConsoleColor primaryColour, ConsoleColor[] secondaryColour, int speed) consoleSettings, (ConsoleColor[] colours, int speed) borderSettings, (UserInputProfile format, Action<string> payload, string summary)[] appCommands, int minAppWidth = 30, int minAppHeight = 30, bool baseCommands = true) : base(name, consoleSettings, borderSettings, appCommands, minAppWidth, minAppHeight, baseCommands) { }

            public override App[] OnRegister()
            {
                return new FlashCardApp[] {
                    new FlashCardApp("Flash Cards", (ConsoleColor.DarkBlue, ColourFunctions.CyanGradient, 1), (ColourFunctions.RainbowWithoutYellowGradient, 10), new (UserInputProfile format, Action<string> payload, string summary)[0])
                };
            }
        }
    }
}