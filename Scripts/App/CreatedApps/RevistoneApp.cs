using Revistone.Interaction;
using Revistone.Console;
using static Revistone.Functions.ColourFunctions;

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

                ConsoleColor[] c =
                ColourArray(new (ConsoleColor, int)[] { (ConsoleColor.DarkBlue, 17) }).Concat(
                AlternatingColours(new ConsoleColor[] { ConsoleColor.Cyan, ConsoleColor.DarkCyan }, 10)).ToArray().Concat(
                ColourArray(new (ConsoleColor, int)[] { (ConsoleColor.DarkBlue, 7) })).ToArray().Concat(
                AlternatingColours(new ConsoleColor[] { ConsoleColor.Cyan, ConsoleColor.DarkCyan }, 6)).ToArray().Concat(
                ColourArray(new (ConsoleColor, int)[] { (ConsoleColor.DarkBlue, 7) })).ToArray();

                ConsoleAction.SendConsoleMessage(new ConsoleLine("More Than Just A [Console!] Input 'Help' For List Of Commands. ", c),
                new ConsoleAnimatedLine(UpdateWelcomeMessage, "", 10, true));
            }

            static string[] keyword = new string[] {
                "[Console!]", "[Concept!]", "[Project!]", "[Program!]",
            };

            static void UpdateWelcomeMessage(ConsoleLine lineInfo, string lineMetaInfo, int tickNum)
            {
                ConsoleAnimatedLine.ConsoleTheme(lineInfo, lineMetaInfo, tickNum);
                if (tickNum % 30 == 0) lineInfo.Update($"More Than Just A {keyword[(tickNum / 30) % 4]} Input 'Help' For List Of Commands.");
            }
        }
    }
}