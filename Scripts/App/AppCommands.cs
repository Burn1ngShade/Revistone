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
                (new UserInputProfile(UserInputProfile.InputType.FullText, "app help", caseSettings: StringFunctions.CapitalCasing.Lower),
                (s) => {AppHelp(); },
                "List Of All App Specfic Commands And Their Descriptions."),
                (new UserInputProfile(UserInputProfile.InputType.FullText, "reload", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => { ReloadApp(s); },
                "Reloads The Current App."),
                (new UserInputProfile(UserInputProfile.InputType.FullText, "load[C:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => { LoadApp(s); },
                "Loads App With The Given Name ([C:] meaning name of app)."),
                (new UserInputProfile(UserInputProfile.InputType.FullText, "clear", caseSettings: StringFunctions.CapitalCasing.Lower),
                (s) => { ClearPrimaryConsole(); },
                "Clears Primary Console."),
                (new UserInputProfile(UserInputProfile.InputType.FullText, "debug clear", caseSettings: StringFunctions.CapitalCasing.Lower),
                (s) => { ClearDebugConsole(); },
                "Clears Debug Console."),
                (new UserInputProfile(UserInputProfile.InputType.FullText, "apps", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => { SendConsoleMessage(new ConsoleLine(StringFunctions.ArrayToElementString(App.appRegistry.Select(app => app.name).ToArray()), ConsoleColor.DarkBlue)); },
                "Prints A List Of The Names Of All Apps."),

                
                //fun commands
                (new UserInputProfile(UserInputProfile.InputType.FullText, "boop", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => SendConsoleMessage(new ConsoleLine("Boop!", ConsoleColor.DarkBlue)),
                "Boop!")
            };

            // --- BASE COMMANDS ---

            /// <summary> Displays list of base commands and descriptions. </summary>
            static void Help()
            {
                (string[] name, string[] summary) commands = (baseCommands.Select(cmd => StringFunctions.AdjustCapitalisation(cmd.format.inputFormat, StringFunctions.CapitalCasing.FirstLetterUpper)).ToArray(), baseCommands.Select(cmd => cmd.summary).ToArray());

                for (int i = 0; i < commands.name.Length; i++)
                {
                    SendConsoleMessage(new ConsoleLine($"{commands.name[i]} -> {commands.summary[i]}", ConsoleColor.DarkBlue));
                }
            }

            /// <summary> Displays list of app specfic commands and descriptions. </summary>
            static void AppHelp()
            {
                (string[] name, string[] summary) commands = (App.activeApp.appCommands.Select(cmd => StringFunctions.AdjustCapitalisation(cmd.format.inputFormat, StringFunctions.CapitalCasing.FirstLetterUpper)).ToArray(), App.activeApp.appCommands.Select(cmd => cmd.summary).ToArray());

                for (int i = 0; i < commands.name.Length; i++)
                {
                    SendConsoleMessage(new ConsoleLine($"{commands.name[i]} -> {commands.summary[i]}", ConsoleColor.DarkBlue));
                }

                if (commands.name.Length == 0) SendConsoleMessage(new ConsoleLine("This App Has No Custom Commands!", ConsoleColor.DarkBlue));
            }

            /// <summary> Gives user Y/N option to reload current app. </summary>
            static void ReloadApp(string userInput)
            {
                int clearConsole = UserInput.CreateOptionMenu("Reload App?", new ConsoleLine[] { new ConsoleLine("Yes"), new ConsoleLine("No") });
                if (clearConsole == 0) ResetConsole();
                else SendConsoleMessage(new ConsoleLine("App Reload Cancelled!", ConsoleColor.DarkBlue), new ConsoleLineUpdate());
            }

            /// <summary> Gives user Y/N option to load given app. </summary>
            static void LoadApp(string userInput)
            {
                int cindex = App.activeAppIndex;
                string appName = userInput.Substring(5);
                if (App.SetActiveApp(appName))
                {
                    int closeApp = UserInput.CreateOptionMenu($"Load {appName}?", new ConsoleLine[] { new ConsoleLine("Yes"), new ConsoleLine("No") });
                    if (closeApp == 0) ResetConsole();
                    else
                    {
                        App.SetActiveApp(cindex);
                        SendConsoleMessage(new ConsoleLine($"App Load Cancelled!", ConsoleColor.DarkBlue), new ConsoleLineUpdate());
                    }
                }
                else
                {
                    SendConsoleMessage(new ConsoleLine($"App Named [{appName}] Could Not Be Found!", ConsoleColor.DarkBlue), new ConsoleLineUpdate());
                }
            }

            // --- STATIC FUNCTIONS ---

            /// <summary> Checks for and calls and commands if found within user input. </summary>
            public static void Commands(string userInput)
            {
                foreach ((UserInputProfile format, Action<string> payload, string summary) in App.activeApp.appCommands)
                {
                    format.outputFormat = UserInputProfile.OutputFormat.NoOutput; //prevent accidentley leaving output on standard from crashing

                    if (format.InputValid(userInput))
                    {
                        payload.Invoke(userInput);
                        return;
                    }
                }

                if (!App.activeApp.baseCommands) return;
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