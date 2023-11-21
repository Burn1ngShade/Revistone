using Revistone.Console;
using Revistone.Functions;
using static Revistone.Console.ConsoleAction;
using Revistone.Interaction;

namespace Revistone
{
    namespace Apps
    {
        /// <summary> Class pertaining all logic for app commands. </summary>
        public static class AppCommands
        {
            /// <summary> Array of all built in commands. </summary>
            static (UserInputProfile format, Action<string> payload, string summary)[] baseCommands = new (UserInputProfile, Action<string>, string summary)[] {
                //useful commands
                (new UserInputProfile(UserInputProfile.InputType.FullText, "help", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => {Help(); },
                "List Of All Base Commands And Their Descriptions."),
                (new UserInputProfile(UserInputProfile.InputType.FullText, "app help", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
                (s) => {AppHelp(); },
                "List Of All App Specfic Commands And Their Descriptions."),
                (new UserInputProfile(UserInputProfile.InputType.FullText, "reload", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => { ReloadApp(s); },
                "Reloads The Current App."),
                (new UserInputProfile(new UserInputProfile.InputType[] {}, "load[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => { LoadApp(s); },
                "Loads App With The Given Name ([A:] meaning name of app)."),
                (new UserInputProfile(UserInputProfile.InputType.FullText, "hub", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => { LoadApp("LoadRevistone"); },
                "Loads Hub Revistone App."),
                (new UserInputProfile(UserInputProfile.InputType.FullText, "clear", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => { ClearPrimaryConsole(); },
                "Clears Primary Console."),
                (new UserInputProfile(UserInputProfile.InputType.FullText, "clear debug", caseSettings: StringFunctions.CapitalCasing.Lower,  removeLeadingWhitespace: true, removeTrailingWhitespace: true),
                (s) => { ClearDebugConsole(); },
                "Clears Debug Console."),
                (new UserInputProfile(UserInputProfile.InputType.FullText, "apps", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => { SendConsoleMessage(new ConsoleLine(StringFunctions.ToElementString(AppRegistry.appRegistry.Select(app => app.name).ToArray()), AppRegistry.activeApp.colourScheme.primaryColour)); },
                "Prints A List Of The Names Of All Apps."),

                //test commands
                (new UserInputProfile(new UserInputProfile.InputType[] {}, "debug[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => { SendDebugMessage(s.Substring(5).TrimStart()); }, "Sends Debug Message [A:] ([A:] being the message)"),
            };

            // --- BASE COMMANDS ---

            /// <summary> Displays list of base commands and descriptions. </summary>
            static void Help()
            {
                (string[] name, string[] summary) commands = (baseCommands.Select(cmd => StringFunctions.AdjustCapitalisation(cmd.format.inputFormat, StringFunctions.CapitalCasing.FirstLetterUpper)).ToArray(), baseCommands.Select(cmd => cmd.summary).ToArray());

                for (int i = 0; i < commands.name.Length; i++)
                {
                    SendConsoleMessage(new ConsoleLine($"{commands.name[i]}: {commands.summary[i]}",
                    ColourFunctions.AdvancedHighlight(
                    $"{commands.name[i]}: {commands.summary[i]}".Length, AppRegistry.activeApp.colourScheme.primaryColour, (AppRegistry.activeApp.colourScheme.secondaryColour, 0, commands.name[i].Length + 1))),
                    ConsoleAnimatedLine.AppTheme);
                }
            }

            /// <summary> Displays list of app specfic commands and descriptions. </summary>
            static void AppHelp()
            {
                (string[] name, string[] summary) commands = (AppRegistry.activeApp.appCommands.Select(cmd => StringFunctions.AdjustCapitalisation(cmd.format.inputFormat, StringFunctions.CapitalCasing.FirstLetterUpper)).ToArray(), AppRegistry.activeApp.appCommands.Select(cmd => cmd.summary).ToArray());

                for (int i = 0; i < commands.name.Length; i++)
                {
                    SendConsoleMessage(new ConsoleLine($"{commands.name[i]}: {commands.summary[i]}",
                    ColourFunctions.AdvancedHighlight(
                    $"{commands.name[i]}: {commands.summary[i]}".Length, AppRegistry.activeApp.colourScheme.primaryColour, (AppRegistry.activeApp.colourScheme.secondaryColour, 0, commands.name[i].Length + 1))),
                    ConsoleAnimatedLine.AppTheme);
                }

                if (commands.name.Length == 0) SendConsoleMessage(new ConsoleLine("This App Has No Custom Commands!", AppRegistry.activeApp.colourScheme.primaryColour));
            }

            /// <summary> Gives user Y/N option to reload current app. </summary>
            static void ReloadApp(string userInput)
            {
                int clearConsole = UserInput.CreateOptionMenu("Reload App?", new ConsoleLine[] { new ConsoleLine("Yes"), new ConsoleLine("No") });
                if (clearConsole == 0) ResetConsole();
                else SendConsoleMessage(new ConsoleLine("App Reload Cancelled!", AppRegistry.activeApp.colourScheme.primaryColour), new ConsoleLineUpdate());
            }

            /// <summary> Gives user Y/N option to load given app. </summary>
            static void LoadApp(string userInput)
            {
                int cindex = AppRegistry.activeAppIndex;
                string appName = userInput.Substring(4).TrimStart();
                if (AppRegistry.SetActiveApp(appName))
                {
                    int closeApp = UserInput.CreateOptionMenu($"Load {appName}?", new ConsoleLine[] { new ConsoleLine("Yes"), new ConsoleLine("No") });
                    if (closeApp == 0) ResetConsole();
                    else
                    {
                        AppRegistry.SetActiveApp(cindex);
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
            public static void Commands(string userInput)
            {
                foreach ((UserInputProfile format, Action<string> payload, string summary) in AppRegistry.activeApp.appCommands)
                {
                    format.outputFormat = UserInputProfile.OutputFormat.NoOutput; //prevent accidentley leaving output on standard from crashing

                    if (format.InputValid(userInput))
                    {
                        payload.Invoke(userInput);
                        return;
                    }
                }

                if (!AppRegistry.activeApp.baseCommands) return;
                foreach ((UserInputProfile format, Action<string> payload, string summary) in baseCommands)
                {
                    format.outputFormat = UserInputProfile.OutputFormat.NoOutput; //prevent accidentley leaving output on standard from crashing

                    if (format.InputValid(userInput))
                    {
                        payload.Invoke(userInput);
                        return;
                    }
                }
            }
        }
    }
}