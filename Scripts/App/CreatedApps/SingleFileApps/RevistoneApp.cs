using Revistone.Interaction;
using Revistone.Console;
using Revistone.Functions;
using Revistone.App.Command;
using Revistone.Modules;
using Revistone.Management;
using Revistone.Console.Image;

using static Revistone.Functions.ColourFunctions;

namespace Revistone.App.BaseApps;

public class RevistoneApp : App
{
    public RevistoneApp() : base() { }
    public RevistoneApp(string name, string description, (ConsoleColour[] primaryColour, ConsoleColour[] secondaryColour, ConsoleColour[] tertiaryColour) consoleSettings, (ConsoleColour[] colours, int speed) borderSettings, AppCommand[] appCommands, int minAppWidth = 30, int minAppHeight = 30, bool baseCommands = true) : base(name, description, consoleSettings, borderSettings, appCommands, minAppWidth, minAppHeight, baseCommands, 100) { }

    public override App[] OnRegister()
    {
        return [ new RevistoneApp("Revistone", "Hub App For The Console.", (ConsoleColour.DarkBlue.ToArray(), ConsoleColour.Cyan.ToArray(), ConsoleColour.Blue.ToArray()), (BaseBorderColours.Stretch(3).SetLength(18), 5),
                [
                    new AppCommand(
                        new UserInputProfile(["boop!", "boop"], caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                        (s) => ConsoleAction.SendConsoleMessage(new ConsoleLine("Boop!", AppRegistry.PrimaryCol)), "Boop!", "Boop.", 1),
                    new AppCommand(
                        new UserInputProfile(["1-up", "oneup", "one-up", "mario", "1up"], caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                        (s) => { AppCommandRegistry.Commands("sticker oneup"); SoundFunctions.PlaySound("OneUp"); }, "1-Up", "Nintendo Wont Be Happy."),
                ],
                98, 37) ];
    }

    static bool firstOpen = true;

    public override void OnAppInitalisation()
    {
        base.OnAppInitalisation();

        ConsoleLine[] title = TitleFunctions.CreateTitle("REVISTONE", Highlight(97, ConsoleColour.DarkBlue.ToArray(), (ConsoleColour.Cyan.ToArray(), 0, 10), (ConsoleColour.Cyan.ToArray(), 48, 10)), TitleFunctions.AsciiFont.BigMoneyNW, letterSpacing: 1);

        ConsoleAction.ShiftLine();
        ConsoleAction.SendConsoleMessages(title,
        Enumerable.Repeat(new ConsoleAnimatedLine(ConsoleAnimatedLine.ShiftForegroundColour, "", AppRegistry.ActiveApp.borderColourScheme.speed, true), title.Length).ToArray());

        ConsoleAction.ShiftLine();

        ConsoleColour[] c = Highlight(63, AppRegistry.PrimaryCol.ToArray(), (AppRegistry.SecondaryCol, 17, 10), (AppRegistry.SecondaryCol, 34, 6), (AppRegistry.SecondaryCol, 62, 20));
        ConsoleAction.SendConsoleMessage(new ConsoleLine("More Than Just A [Console!] Input 'Help' For List Of Commands. ", c),
        new ConsoleAnimatedLine(UpdateUI, 5, true));

        if (SettingsApp.GetValue("Username") != "User") ConsoleAction.SendConsoleMessage(
            new ConsoleLine($"Welcome Back {SettingsApp.GetValue("Username")}!", BuildArray(ConsoleColour.DarkBlue.ToArray(13), ConsoleColour.Cyan.ToArray())));

        for (int i = 0; i <= 11; i++)
        {
            ConsoleAction.UpdateLineExceptionStatus(true, i);
        }

        if (firstOpen && SettingsApp.GetValue("Welcome Message") == "Yes")
        {
            GPTClient.Default.Query(new GPTClient.GPTQuery($"User Has Logged In, Last Log Out Time: {Analytics.General.LastCloseDate}, Log In Time: {Analytics.General.LastOpenDate}"));
        }
        firstOpen = false;
    }

    static string[] keyword = [
                "[Console!]", "[Concept!]", "[Project!]", "[Program!]",
            ];

    static void UpdateUI(ConsoleLine lineInfo, object metaInfo, int tickNum)
    {
        string s = $"More Than Just A {keyword[tickNum / 30 % 4]} Input 'Help' For List Of Commands.";
        lineInfo.Update(s);
    }
}