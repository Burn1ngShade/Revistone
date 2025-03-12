using Revistone.Interaction;
using Revistone.Console;
using Revistone.Functions;

using static Revistone.Functions.ColourFunctions;

namespace Revistone.App;

public class RevistoneApp : App
{
    public RevistoneApp() : base() { }
    public RevistoneApp(string name, (ConsoleColor[] primaryColour, ConsoleColor[] secondaryColour, ConsoleColor[] tertiaryColour, int speed) consoleSettings, (ConsoleColor[] colours, int speed) borderSettings, (UserInputProfile format, Action<string> payload, string summary)[] appCommands, int minAppWidth = 30, int minAppHeight = 30, bool baseCommands = true) : base(name, consoleSettings, borderSettings, appCommands, minAppWidth, minAppHeight, baseCommands) { }

    public override App[] OnRegister()
    {
        return [ new RevistoneApp("Revistone", (ConsoleColor.DarkBlue.ToArray(), ConsoleColor.Cyan.ToArray(), ConsoleColor.Blue.ToArray(), 10), (CyanDarkBlueGradient.Extend(7, true), 5),
                [(new UserInputProfile(UserInputProfile.InputType.FullText, "boop", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => ConsoleAction.SendConsoleMessage(new ConsoleLine("Boop!", AppRegistry.activeApp.colourScheme.primaryColour)), "Boop!"),
                (new UserInputProfile(UserInputProfile.InputType.FullText, "pride", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => ConsoleAction.SendConsoleMessages(Enumerable.Range(0, 10).Select(i => new ConsoleLine(new string(' ', 41), ConsoleColor.White, TransPattern[i / 2])).ToArray()), "Outputs Trans Flag!"),
                ],
                98, 29) ];
    }

    public override void OnAppInitalisation()
    {
        base.OnAppInitalisation();

        ConsoleLine[] title = TitleFunctions.CreateTitle("REVISTONE", AdvancedHighlight(97, ConsoleColor.DarkBlue.ToArray(), (ConsoleColor.Cyan.ToArray(), 0, 10), (ConsoleColor.Cyan.ToArray(), 48, 10)), TitleFunctions.AsciiFont.BigMoneyNW, letterSpacing: 1);

        ConsoleAction.ShiftLine();
        ConsoleAction.SendConsoleMessages(title,
        Enumerable.Repeat(new ConsoleAnimatedLine(ConsoleAnimatedLine.ShiftColour, "", AppRegistry.activeApp.borderColourScheme.speed, true), title.Length).ToArray());

        ConsoleAction.ShiftLine();

        ConsoleColor[] c = AdvancedHighlight(63, AppRegistry.activeApp.colourScheme.primaryColour.ToArray(), AppRegistry.activeApp.colourScheme.secondaryColour, (17, 10), (34, 6), (62, 20));
        ConsoleAction.SendConsoleMessage(new ConsoleLine("More Than Just A [Console!] Input 'Help' For List Of Commands. ", c),
        new ConsoleAnimatedLine(UpdateUI, 5, true));

        if (SettingsApp.GetValue("Username") != "User") ConsoleAction.SendConsoleMessage(
            new ConsoleLine($"Welcome Back {SettingsApp.GetValue("Username")}!", BuildArray(ConsoleColor.DarkBlue.Extend(13), ConsoleColor.Cyan.ToArray())));

        for (int i = 0; i <= 11; i++)
        {
            ConsoleAction.UpdateLineExceptionStatus(true, i);
        }
    }

    static string[] keyword = new string[] {
                "[Console!]", "[Concept!]", "[Project!]", "[Program!]",
            };

    static void UpdateUI(ConsoleLine lineInfo, object metaInfo, int tickNum)
    {
        string s = $"More Than Just A {keyword[tickNum / 30 % 4]} Input 'Help' For List Of Commands.";
        lineInfo.Update(s);
    }
}