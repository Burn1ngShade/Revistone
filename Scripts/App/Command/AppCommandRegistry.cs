using Revistone.App.BaseApps.Calculator;
using Revistone.App.BaseApps.HoneyC;
using Revistone.App.BaseApps;
using Revistone.Console;
using Revistone.Console.Widget;
using Revistone.Functions;
using Revistone.Interaction;
using Revistone.Management;

using static Revistone.App.Command.AppCommandsData;
using static Revistone.Console.ConsoleAction;
using static Revistone.Functions.ColourFunctions;
using static Revistone.Functions.WorkspaceFunctions;

namespace Revistone.App.Command;

/// <summary> Class pertaining all logic for app commands. </summary>
public static class AppCommands
{
    static readonly AppCommand[] newBaseCommands = [
        new AppCommand(
            new UserInputProfile("help", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
            (s) => DisplayCommands(), "Help", "Lists All Base Commands And Their Functionality.", int.MaxValue, AppCommand.CommandType.Console),
        new AppCommand(
            new UserInputProfile(["app", "apps"], caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
            (s) => LoadAppCommand(""), "Apps", "Lists All Apps And Their Functionality.", 100, AppCommand.CommandType.Apps),
        new AppCommand(
            new UserInputProfile("load[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeTrailingWhitespace: true, removeLeadingWhitespace: true),
            (s) => LoadAppCommand(s[4..]), "Load [AppName]", "Loads App Of Entered Name.", 90, AppCommand.CommandType.Apps),
        new AppCommand(
            new UserInputProfile("reload", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
            (s) => LoadAppCommand(AppRegistry.activeApp.name), "Reload", "Reloads Current App.", 60, AppCommand.CommandType.Apps),
        new AppCommand(
            new UserInputProfile(["setting", "settings"], caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
            (s) => LoadAppCommand("Settings"), "Settings", "Loads Settings App.", 70, AppCommand.CommandType.Apps),
        new AppCommand(
            new UserInputProfile(["hub", "revistone", "home"], caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
            (s) => LoadAppCommand("Revistone"), "Hub", "Loads Revistone App.", 61, AppCommand.CommandType.Apps),
        new AppCommand(
            new UserInputProfile(["hotkeys", "hotkey", "keybinds", "keybind"], caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
            (s) => DisplayHotkeysCommand(), "Hotkeys", "Displays List Of Console Hotkeys.", 100, AppCommand.CommandType.Console),
        new AppCommand(
            new UserInputProfile(["clear", "clr"], caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
            (s) => ClearPrimaryConsole(), "Clear", "Clears The Primary Console.", 90, AppCommand.CommandType.Console),
        new AppCommand(
            new UserInputProfile(["cleardebug", "clrdebug", "clrd"], caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
            (s) => ClearDebugConsole(), "Clear Debug", "Clears The Debug Console.", 80, AppCommand.CommandType.Console),
        new AppCommand(
            new UserInputProfile("debug[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeTrailingWhitespace: true, removeLeadingWhitespace: true),
            (s) => SendDebugMessage(new ConsoleLine(s[5..].TrimStart(), AppRegistry.PrimaryCol)), "Debug [Message]", "Sends Message To Debug Console.", 70, AppCommand.CommandType.Console),
        new AppCommand(
            new UserInputProfile(["profilertoggle", "toggleprofiler"], caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
            (s) => Profiler.SetEnabled(!Profiler.Enabled), "Toggle Profiler", "Toggles Profiler On Or Off.", 60, AppCommand.CommandType.Console),
        new AppCommand(
            new UserInputProfile("gpt[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
            (s) => GPTFunctions.Query(s[3..].TrimStart(), true), "GPT [Message]", "Interact With Custom Revistone ChatGPT Model.", 100, AppCommand.CommandType.ChatGPT),
        new AppCommand(
            new UserInputProfile("temp gpt[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
            (s) => GPTFunctions.Query(s[8..].TrimStart(), false), "Temp GPT [Message]", "Interact With Custom Revistone ChatGPT Model, Without Message History.", 90, AppCommand.CommandType.ChatGPT),
        new AppCommand(
            new UserInputProfile("cleargpt", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
            (s) => GPTFunctions.ClearMessageHistory(), "Clear GPT", "Wipe Message History Of ChatGPT Model.", 80, AppCommand.CommandType.ChatGPT),
        new AppCommand(
            new UserInputProfile("remember gpt[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
            (s) => GPTFunctions.AddToMemories(s[12..].TrimStart()), "Remember GPT [Message]", "Store A Permeant Memory To GPT.", 70, AppCommand.CommandType.ChatGPT),
        new AppCommand(
            new UserInputProfile(["memoriesgpt", "memgpt", "memorygpt"], caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
            (s) => GPTFunctions.ViewMemories(), "Memories GPT", "View List Of GPT Memories.", 60, AppCommand.CommandType.ChatGPT),
        new AppCommand(
            new UserInputProfile("set setting[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
            (s) => SettingInteractCommand(s[11..].TrimStart(), false), "Set Setting [Setting]", "Set The Value Of Given Setting.", 69, AppCommand.CommandType.Apps),
        new AppCommand(
            new UserInputProfile("get setting[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
            (s) => SettingInteractCommand(s[11..].TrimStart(), true), "Get Setting [Setting]", "Get The Value Of Given Setting.", 68, AppCommand.CommandType.Apps),
        new AppCommand(
            new UserInputProfile("time", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
            (s) => SendConsoleMessage(new ConsoleLine($"Current System Time - {DateTime.Now}", BuildArray(AppRegistry.PrimaryCol.Extend(22), AppRegistry.SecondaryCol))), "Time", "Displays The Current System Time.", 50, AppCommand.CommandType.Console),
        new AppCommand(
            new UserInputProfile(["runtime", "uptime"], caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
            (s) => SendConsoleMessage(new ConsoleLine($"Console Uptime - {(Manager.ElapsedTicks / 40d).ToString("0.00")}s", BuildArray(AppRegistry.PrimaryCol.Extend(17), AppRegistry.SecondaryCol))), "Runtime", "Displays The Current Console Session Uptime.", 40, AppCommand.CommandType.Console),
        new AppCommand(
            new UserInputProfile("mkdir[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
            (s) => CreateWorkspaceDirectory(s[5..].Trim()), "Mkdir [Directory]", "Create A Workspace Directory.", 100, AppCommand.CommandType.Workspace),
        new AppCommand(
            new UserInputProfile("rmdir[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
            (s) => DeleteWorkspaceDirectory(s[5..].Trim()), "Rmdir [Directory]", "Delete A Workspace Directory.", 99, AppCommand.CommandType.Workspace),
        new AppCommand(
            new UserInputProfile(["cdir[A:]", "cd[A:]"], caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
            (s) => UpdatePath(RawPath + $@"{s[(s.StartsWith("cdir")?4:2)..].Trim()}\"), "Cdir [Directory]", "Change Workspace Directory.", 98, AppCommand.CommandType.Workspace),
        new AppCommand(
            new UserInputProfile(["pdir", "pd"], caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
            (s) => UpdatePath(GetParentPath(RawPath)), "Pdir", "Go To Parent Directory.", 97, AppCommand.CommandType.Workspace),
        new AppCommand(
            new UserInputProfile(["rdir", "rd"], caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
            (s) => UpdatePath(RootPath), "Rdir", "Go To Root Directory.", 96, AppCommand.CommandType.Workspace),
        new AppCommand(
            new UserInputProfile("dir", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
            (s) => DisplayWorkspaceOverview(), "Dir", "Gets Files And Directories Within The Current Directory.", 90, AppCommand.CommandType.Workspace),
        new AppCommand(
            new UserInputProfile(["fdir", "fd"], caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
            (s) => ListWorkspaceFiles(), "Fdir", "Gets Files Within The Current Directory.", 80, AppCommand.CommandType.Workspace),
        new AppCommand(
            new UserInputProfile(["ddir", "dd"], caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
            (s) => ListWorkspaceDirectories(), "Ddir", "Gets Directories Within The Current Directory.", 70, AppCommand.CommandType.Workspace),
        new AppCommand(
            new UserInputProfile("mkfile[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
            (s) => CreateWorkspaceFile(s[6..].Trim()), "Mkfile [File]", "Create A File Within The Current Directory.", 60, AppCommand.CommandType.Workspace),
        new AppCommand(
            new UserInputProfile("rmfile[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
            (s) => DeleteWorkspaceFile(s[6..].Trim()), "Rmfile [File]", "Deletes A File Within The Current Directory.", 59, AppCommand.CommandType.Workspace),
        new AppCommand(
            new UserInputProfile("open[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
            (s) => OpenWorkspaceFile(s[4..].Trim()), "Open [File]", "Opens A File Within The Current Directory.", 58, AppCommand.CommandType.Workspace),
        new AppCommand(
            new UserInputProfile("timer[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
            (s) => TimerWidget.CreateTimer("Timer", s[5..].Trim()), "Timer [Duration]", "Creates A Timer Of Given Duration (hh:mm:ss).", 100, AppCommand.CommandType.Widget),
        new AppCommand(
            new UserInputProfile(["removetimer", "canceltimer", "endtimer"], caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
            (s) => TimerWidget.CancelTimer("Timer"), "Cancel Timer", "Removes Active Timer.", 90, AppCommand.CommandType.Widget),
        new AppCommand(
            new UserInputProfile(["pausetimer", "toggletimer"], caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
            (s) => TimerWidget.TogglePauseTimer("Timer"), "Toggle Timer", "Pauses Or Unpauses Timer.", 80, AppCommand.CommandType.Widget),
        new AppCommand(
            new UserInputProfile(["modify timer[A:]"], caseSettings: StringFunctions.CapitalCasing.Lower, removeTrailingWhitespace: true, removeLeadingWhitespace: true),
            (s) => TimerWidget.AdjustTimer("Timer", s[12..]), "Modify Timer [Duration]", "Modify The Timers Duration By Given Duration (hh:mm:ss).", 70, AppCommand.CommandType.Widget),
        new AppCommand(
            new UserInputProfile(["exit", "leave", "quit"], caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
            (s) => ExitTerminalCommand(false), "Quit", "Closes The Revistone Terminal.", -100, AppCommand.CommandType.Console),
        new AppCommand(
            new UserInputProfile(["kill", "forceexit", "forceleave", "forcequit"], caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
            (s) => ExitTerminalCommand(true), "Kill", "Force Closes The Revistone Terminal (Crashes It).", -100, AppCommand.CommandType.Console),
        new AppCommand(
            new UserInputProfile("screenshots", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
            (s) => LoadAppCommand("Screenshots"), "Screenshots", "Load Screenshots App.", 59, AppCommand.CommandType.Apps),
        new AppCommand(
            new UserInputProfile("screenshot[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
            (s) => ScreenshotsApp.TakePrimaryScreenshot(s[10..].Trim()), "Screenshot [Name]", "Take A Screenshot Of The Primary Console.", 30, AppCommand.CommandType.Console),
        new AppCommand(
            new UserInputProfile("dscreenshot[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
            (s) => ScreenshotsApp.TakeDebugScreenshot(s[11..].Trim()), "Debug Screenshot [Name]", "Take A Screenshot Of The Debug Console.", 20, AppCommand.CommandType.Console),
        new AppCommand(
            new UserInputProfile("calc[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
            (s) => CalculatorInterpreter.Intepret(s[4..].Trim()), "Calc [Query]", "Runs Given Calculator Query.", 50, AppCommand.CommandType.Apps),
        new AppCommand(
            new UserInputProfile("comp[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true), 
            (s) => HoneyCInterpreter.Interpret([s[4..].Trim()]), "Comp [Query]", "Runs Given HoneyC Query.", 51, AppCommand.CommandType.Apps),
        new AppCommand(
            new UserInputProfile("run[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
            (s) => RunWorkspaceFile(s[3..].Trim()), "Run [File]", "Runs A File Within The Current Directory.", 50, AppCommand.CommandType.Workspace),
        new AppCommand(
            new UserInputProfile("sticker[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
            (s) => PaintApp.DisplaySticker(s[7..].Trim()), "Sticker [Name]", "Displays Given Console Sticker From Base/User/Workspace Stickers.", 39, AppCommand.CommandType.Console),
        new AppCommand(
            new UserInputProfile(["liststickers", "allstickers"], caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
            (s) => PaintApp.ListStickers(), "List Stickers", "Displays List Of All Default And User Stickers (Excluding Workspace).", 38, AppCommand.CommandType.Console),
        new AppCommand(
            new UserInputProfile(["version", "releaseversion", "currentversion", "buildversion"], caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
            (s) => SendConsoleMessage(new ConsoleLine($"Build Version - {Manager.ConsoleVersion}", BuildArray(AppRegistry.PrimaryCol.Extend(16), AppRegistry.SecondaryCol))), "Version", "Displays Console Version.", 0, AppCommand.CommandType.Console)
    ];

// --- STATIC FUNCTIONS ---

/// <summary> Checks for and calls and commands if found within user input. </summary>
public static bool Commands(string userInput)
{
    foreach (AppCommand command in AppRegistry.activeApp.appCommands)
    {
        command.format.outputFormat = UserInputProfile.OutputFormat.NoOutput; //prevent accidentley leaving output on standard from crashing

        if (command.format.InputValid(userInput))
        {
            command.action.Invoke(userInput);
            Analytics.General.CommandsUsed++;
            return true;
        }
    }

    if (!AppRegistry.activeApp.useBaseCommands) return false;

    foreach (AppCommand command in newBaseCommands)
    {
        command.format.outputFormat = UserInputProfile.OutputFormat.NoOutput; //prevent accidentley leaving output on standard from crashing

        if (command.format.InputValid(userInput))
        {
            command.action.Invoke(userInput);
            Analytics.General.CommandsUsed++;
            return true;
        }
    }

    return false;
}

static void DisplayCommands()
{
    Dictionary<string, List<(ConsoleLine, int)>> commandList = [];

    foreach (AppCommand c in newBaseCommands)
    {
        if (!commandList.ContainsKey(c.type.ToString()))
        {
            commandList.Add(c.type.ToString(), []);
        }
        commandList[c.type.ToString()].Add((new ConsoleLine($"{c.name} - {c.summary}", BuildArray(AppRegistry.SecondaryCol.Extend(c.name.Length + 3), [.. AppRegistry.PrimaryCol])), c.displayPriority));
    }

    foreach (AppCommand c in AppRegistry.activeApp.appCommands)
    {
        string catName = c.type == AppCommand.CommandType.AppSpecific ? AppRegistry.activeApp.name : c.type.ToString();

        if (!commandList.ContainsKey(catName))
        {
            commandList.Add(catName, []);
        }
        commandList[catName].Add((new ConsoleLine($"{c.name} - {c.summary}", BuildArray(AppRegistry.SecondaryCol.Extend(c.name.Length + 3), [.. AppRegistry.PrimaryCol])), c.displayPriority));
    }

    UserInput.CreateCategorisedReadMenu("Help", 5, commandList.Select(x => (x.Key, x.Value.OrderByDescending(x => x.Item2).Select(x => x.Item1).ToArray())).ToArray());
}
}