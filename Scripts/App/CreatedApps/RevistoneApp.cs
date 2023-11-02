using Revistone.Interaction;
using Revistone.Console;
using Revistone.Functions;

namespace Revistone
{
    namespace Apps
    {
        public class RevistoneApp : App
        {
            public RevistoneApp(string name, ConsoleColor[] borderColours, (UserInputProfile format, Action<string> payload, string summary)[] appCommands, int borderColourSpeed = 5, int minAppWidth = 30, int minAppHeight = 30, bool baseCommands = true) : base(name, borderColours, appCommands, borderColourSpeed, minAppWidth, minAppHeight, baseCommands) { }

            public override void OnAppInitalisation()
            {
                base.OnAppInitalisation();

                ConsoleAction.SendConsoleMessage(new ConsoleLine("More Than Just A Console! Input 'Help' For List Of Commands", ConsoleColor.DarkBlue));
            }
        }
    }
}