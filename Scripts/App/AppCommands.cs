using Revistone.Console;
using static Revistone.Console.ConsoleAction;

namespace Revistone
{
    namespace Apps
    {
        public static class AppCommands
        {
            public static void BaseCommands(string userInput)
            {
                if (CommandValid(userInput, "load", true))
                {
                    int cindex = App.activeAppIndex;
                    string appName = userInput.Substring(5);
                    if (App.SetActiveApp(appName))
                    {
                        int closeApp = ConsoleInteraction.CreateOptionMenu($"Load {appName}?", new ConsoleLine[] { new ConsoleLine("Yes"), new ConsoleLine("No") });
                        if (closeApp == 0) ReloadConsole();
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
                else if (CommandValid(userInput, "reload", false))
                {
                    int clearConsole = ConsoleInteraction.CreateOptionMenu("Reload App?", new ConsoleLine[] { new ConsoleLine("Yes"), new ConsoleLine("No") });
                    if (clearConsole == 0) ReloadConsole();
                    else SendConsoleMessage(new ConsoleLine("App Reload Cancelled!", ConsoleColor.DarkBlue), new ConsoleLineUpdate());
                }
            }

            public static bool CommandValid(string userInput, string commandName, bool extraInfo)
            {
                if (userInput.Length < commandName.Length) return false;

                if (userInput.ToLower().Substring(0, commandName.Length) == commandName.ToLower() && (!extraInfo && userInput.Length == commandName.Length || (extraInfo && userInput.Length > commandName.Length + 1 && userInput[commandName.Length] == ' '))) return true;

                return false;
            }
        }
    }
}