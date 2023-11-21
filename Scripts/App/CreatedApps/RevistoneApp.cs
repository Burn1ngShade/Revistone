using Revistone.Interaction;
using Revistone.Console;
using static Revistone.Functions.ColourFunctions;
using Revistone.Functions;
using Revistone.Console.Image;
using Revistone.Console.Data;

namespace Revistone
{
    namespace Apps
    {
        public class RevistoneApp : App
        {
            public RevistoneApp() : base() { }
            public RevistoneApp(string name, (ConsoleColor primaryColour, ConsoleColor[] secondaryColour, int speed) consoleSettings, (ConsoleColor[] colours, int speed) borderSettings, (UserInputProfile format, Action<string> payload, string summary)[] appCommands, int minAppWidth = 30, int minAppHeight = 30, bool baseCommands = true) : base(name, consoleSettings, borderSettings, appCommands, minAppWidth, minAppHeight, baseCommands) { }

            public override void OnAppInitalisation()
            {
                base.OnAppInitalisation();

                ConsoleColor[] c = AdvancedHighlight(63, AppRegistry.activeApp.colourScheme.primaryColour.ToArray(), AppRegistry.activeApp.colourScheme.secondaryColour, (17, 10), (34, 6));

                ConsoleAction.SendConsoleMessage(new ConsoleLine("More Than Just A [Console!] Input 'Help' For List Of Commands. ", c),
                new ConsoleAnimatedLine(UpdateUI, AppRegistry.activeApp.colourScheme.speed, true));
                string[] s = new string[] {
                    @" o /|\/ \", @"_ o /\| \", @"       ___\o/)  | ", @"__|    \o   ( \", @"\ / | /o\", @"   |__ o/   / )   ", @"     o/__ |  (\", @"o _/\ / |"
                };

                for (int i = 0; i <= 4; i++)
                {
                    ConsoleAction.UpdateLineExceptionStatus(true, i);
                }

                ConsoleVideo stickMan = new ConsoleVideo(new ConsoleImage[0], 7);

                for (int i = 0; i <= 56; i++)
                {
                    int sIndex = i % 8;
                    int lineLength = s[sIndex].Length / 3;
                    stickMan.AddFrame(new ConsoleImage($"{new string(' ', i)}{s[sIndex].Substring(0, lineLength)}\n{new string(' ', i)}{s[sIndex].Substring(lineLength, lineLength)}\n{new string(' ', i)}{s[sIndex].Substring(lineLength * 2, lineLength)}"));
                }

                for (int i = 56; i >= 0; i--)
                {
                    int sIndex = i % 8;
                    int lineLength = s[sIndex].Length / 3;
                    stickMan.AddFrame(new ConsoleImage($"{new string(' ', i)}{s[sIndex].Substring(0, lineLength)}\n{new string(' ', i)}{s[sIndex].Substring(lineLength, lineLength)}\n{new string(' ', i)}{s[sIndex].Substring(lineLength * 2, lineLength)}"));
                }

                stickMan.SendToConsole(true);

            }

            static string[] keyword = new string[] {
                "[Console!]", "[Concept!]", "[Project!]", "[Program!]",
            };

            static void UpdateUI(ConsoleLine lineInfo, object metaInfo, int tickNum)
            {
                lineInfo.Update(lineInfo.lineColour.Shift((1, 17, 10), (1, 34, 6)));
                if (tickNum % 30 == 0) lineInfo.Update($"More Than Just A {keyword[tickNum / 30 % 4]} Input 'Help' For List Of Commands.");
            }
        }
    }
}