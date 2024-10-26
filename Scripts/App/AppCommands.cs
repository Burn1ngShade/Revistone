using Revistone.Apps.HoneyC;
using Revistone.Console;
using Revistone.Functions;
using Revistone.Interaction;
using Revistone.Management;

using static Revistone.Console.ConsoleAction;

namespace Revistone.Apps;

/// <summary> Class pertaining all logic for app commands. </summary>
public static class AppCommands
{
    /// <summary> Array of all built in commands. </summary>
    static (UserInputProfile format, Action<string> payload, string summary)[] baseCommands = new (UserInputProfile, Action<string>, string summary)[] {
                //useful commands
                (new UserInputProfile(UserInputProfile.InputType.FullText, "help", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => {if (baseCommands != null) Help((baseCommands.Select(cmd => StringFunctions.AdjustCapitalisation(cmd.format.inputFormat, StringFunctions.CapitalCasing.FirstLetterUpper)).ToArray(), baseCommands.Select(cmd => cmd.summary).ToArray())); },
                "List Of All Base Commands And Their Descriptions."),
                (new UserInputProfile(UserInputProfile.InputType.FullText, "app help", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
                (s) => {AppHelp(); },
                "List Of All App Specfic Commands And Their Descriptions."),
                (new UserInputProfile(UserInputProfile.InputType.FullText, "apps", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => {
                    int i = UserInput.CreateMultiPageOptionMenu("Apps:", AppRegistry.appRegistry.Select(app => new ConsoleLine(app.name, AppRegistry.activeApp.colourScheme.secondaryColour)).ToArray(), new ConsoleLine[] {new ConsoleLine("Exit")}, 3);
                    if (i >= 0) LoadApp($"Load{AppRegistry.appRegistry[i].name}");
                },
                "Menu To Load Apps."),
                (new UserInputProfile(new UserInputProfile.InputType[] {}, "load[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => { LoadApp(s); },
                "Loads App With The Given Name ([A:] meaning name of app)."),
                (new UserInputProfile(UserInputProfile.InputType.FullText, "reload", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => { ReloadApp(s); },
                "Reloads The Current App."),
                (new UserInputProfile(UserInputProfile.InputType.FullText, "hub", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => { LoadApp("LoadRevistone"); },
                "Loads Hub Revistone App."),
                (new UserInputProfile(UserInputProfile.InputType.FullText, "clear", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => { ClearPrimaryConsole(); },
                "Clears Primary Console."),
                (new UserInputProfile(UserInputProfile.InputType.FullText, "clear debug", caseSettings: StringFunctions.CapitalCasing.Lower,  removeLeadingWhitespace: true, removeTrailingWhitespace: true),
                (s) => { ClearDebugConsole(); },
                "Clears Debug Console."),
                (new UserInputProfile(UserInputProfile.InputType.FullText, "time", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => { SendConsoleMessage(new ConsoleLine($"{DateTime.Now.ToString()}.", AppRegistry.activeApp.colourScheme.primaryColour)); },
                "Prints The Current System Time."),
                (new UserInputProfile(UserInputProfile.InputType.FullText, "runtime", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => { SendConsoleMessage(new ConsoleLine($"Revistone Has Been Running For {(Manager.currentTick / 40d).ToString("0.00")} Seconds.", AppRegistry.activeApp.colourScheme.primaryColour)); },
                "Prints The Runtime Of The Current Session."),
                (new UserInputProfile("comp[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true), (s) => {HoneyCInterpreter.Intepret(s[4..]); }, "Carries Out Given HoneyC Promt."),
                //test commands
                (new UserInputProfile(new UserInputProfile.InputType[] {}, "debug[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => { SendDebugMessage(s.Substring(5).TrimStart()); }, "Sends Debug Message [A:] ([A:] being the message)."),
                (new UserInputProfile(new UserInputProfile.InputType[] {}, "profiler toggle", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
                (s) => { Profiler.SetEnabled(!Profiler.enabled); }, "Toggles Profiler On Or Off."),

            };

    // --- BASE COMMANDS ---

    /// <summary> Displays list of base commands and descriptions. </summary>
    static void Help((string[] name, string[] summary) commands)
    {
        UserInput.CreateReadMenu("Help", 5,
        commands.name.Select((s, i) => new ConsoleLine($"{s}: {commands.summary[i]}",
        ColourFunctions.AdvancedHighlight($"{commands.name[i]}: {commands.summary[i]}".Length, AppRegistry.activeApp.colourScheme.primaryColour,
        (AppRegistry.activeApp.colourScheme.secondaryColour, 0, commands.name[i].Length + 1)))).ToArray());
    }

    /// <summary> Displays list of app specfic commands and descriptions. </summary>
    static void AppHelp()
    {
        (string[] name, string[] summary) commands = (AppRegistry.activeApp.appCommands.Select(cmd => StringFunctions.AdjustCapitalisation(cmd.format.inputFormat, StringFunctions.CapitalCasing.FirstLetterUpper)).ToArray(), AppRegistry.activeApp.appCommands.Select(cmd => cmd.summary).ToArray());
        if (commands.name.Length == 0) SendConsoleMessage(new ConsoleLine("This App Has No Custom Commands!", AppRegistry.activeApp.colourScheme.primaryColour));
        else Help(commands);
    }

    /// <summary> Gives user Y/N option to reload current app. </summary>
    static void ReloadApp(string userInput)
    {
        int clearConsole = UserInput.CreateOptionMenu("Reload App?", new ConsoleLine[] { new ConsoleLine("Yes"), new ConsoleLine("No") });
        if (clearConsole == 0) ReloadConsole();
        else SendConsoleMessage(new ConsoleLine("App Reload Cancelled!", AppRegistry.activeApp.colourScheme.primaryColour), new ConsoleLineUpdate());
    }

    /// <summary> Gives user Y/N option to load given app. </summary>
    static void LoadApp(string userInput)
    {
        if (userInput.Trim().Length == 4) //submitted empty load request
        {
            int i = UserInput.CreateMultiPageOptionMenu("Apps:", AppRegistry.appRegistry.Select(app => new ConsoleLine(app.name, AppRegistry.activeApp.colourScheme.secondaryColour)).ToArray(), new ConsoleLine[] { new ConsoleLine("Exit") }, 3);
            if (i >= 0) LoadApp($"Load{AppRegistry.appRegistry[i].name}");
            return;
        }
        string appName = userInput.Substring(4).TrimStart().AdjustCapitalisation(StringFunctions.CapitalCasing.FirstLetterUpper);
        if (AppRegistry.AppExists(appName))
        {
            int closeApp = UserInput.CreateOptionMenu($"Load {appName}?", new ConsoleLine[] { new ConsoleLine("Yes"), new ConsoleLine("No") });
            if (closeApp == 0)
            {
                AppRegistry.SetActiveApp(appName);
                ReloadConsole();
            }
            else
            {
                SendConsoleMessage(new ConsoleLine($"App Load Cancelled!", AppRegistry.activeApp.colourScheme.primaryColour), new ConsoleLineUpdate());
            }
        }
        else
        {
            SendConsoleMessage(new ConsoleLine($"App Named [{appName}] Could Not Be Found!", AppRegistry.activeApp.colourScheme.primaryColour), new ConsoleLineUpdate());
        }
    }

    // --- STATIC FUNCTIONS ---

    /// <summary> Checks for and calls and commands if found within user input. </summary>
    public static bool Commands(string userInput)
    {
        foreach ((UserInputProfile format, Action<string> payload, string summary) in AppRegistry.activeApp.appCommands)
        {
            format.outputFormat = UserInputProfile.OutputFormat.NoOutput; //prevent accidentley leaving output on standard from crashing

            if (format.InputValid(userInput))
            {
                payload.Invoke(userInput);
                return true;
            }
        }

        if (!AppRegistry.activeApp.baseCommands) return false;
        foreach ((UserInputProfile format, Action<string> payload, string summary) in baseCommands)
        {
            format.outputFormat = UserInputProfile.OutputFormat.NoOutput; //prevent accidentley leaving output on standard from crashing

            if (format.InputValid(userInput))
            {
                payload.Invoke(userInput);
                return true;
            }
        }

        return false;
    }
}