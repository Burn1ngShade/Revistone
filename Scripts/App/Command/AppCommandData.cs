using Revistone.App.BaseApps;
using Revistone.Console;
using Revistone.Functions;
using Revistone.Interaction;

using static Revistone.Console.ConsoleAction;
using static Revistone.Functions.ColourFunctions;

namespace Revistone.App.Command;

///<summary> Data and methods behind app commands. </summary>
public static class AppCommandsData
{
    // --- DATA ---

    public static readonly (string keyCombo, string description)[] generalHotkeys = [
        ("Ctrl + Shift + P", "Toggles Profiler."),
        ("F11", "Takes A Screenshot Of The Debug Console."),
        ("F12", "Takes A Screenshot Of The Primary Console."),
    ];

    public static readonly (string keyCombo, string description)[] inputHotkeys = [
        ("Up Arrow", "Jump To Previous Element In Input"),
        ("Down Arrow", "Jump To Next Element In Input History"),
        ("Shift + Up Arrow", "Jump To Top Of Input History."),
        ("Shift + Down Arrow", "Jump To End Of Input History."),
        ("Shift + Left Arrow", "Extend Selection To The Left."),
        ("Shift + Right Arrow", "Extend Selection To The Right."),
        ("Ctrl + Left Arrow", "Jump To Previous Seperator."),
        ("Ctrl + Right Arrow", "Jump To Next Seperator."),
        ("Tab + Left Arrow", "Extend Selection To The Previous Seperator."),
        ("Tab + Right Arrow", "Extend Selection To The Next Seperator."),
        ("Tab + Backspace", "Delete Text Up To The Previous Seperator."),
        ("Alt + X", "Cut Selected Text To Clipboard."),
        ("Alt + C", "Copy Selected Text To Clipboard."),
        ("Alt + V", "Paste Clipboard."),
        ("Alt + S", "Jump To Start Of Line."),
        ("Alt + E", "Jump To End Of Line."),
        ("Alt + B", "Jump To Start Of Text."),
        ("Alt + D", "Jump To End Of Text."),
        ("Alt + L", "Select Line."),
        ("Alt + A", "Select All."),
    ];

    public static readonly (string keyCombo, string description)[] menuHotkeys = [
        ("Enter", "Select Option."),
        ("W | J | Up Arrow", "Jump To Previous Option."),
        ("S | K | Down Arrow", "Jump To Next Option."),
        ("Shift + W | Shift + J | Page Up", "Jump To Top Option."),
        ("Shift + S | Shift + K | Page Down", "Jump To Bottom Option."),
    ];

    public static readonly (string keyCombo, string description)[] fileHotkeys = [
        ("Shift + Up Arrow | Page Up", "Jump A Page Up."),
        ("Shift + Down Arrow | Page Down", "Jump A Page Down."),
        ("Tab + Page Up", "Jump To Top Of File."),
        ("Tab + Page Down", "Jump To End Of File."),
        ("Tab + Up Arrow", "Removes Empty Line."),
        ("Tab + Down Arrow", "Inserts Empty Line."),
    ];

    // --- FUNCTIONS ---

    /// <summary> Displays list of console hotkeys. </summary>
    public static void DisplayHotkeysCommand()
    {
        UserInput.CreateCategorisedReadMenu("Hotkeys", 5,
        ("General", generalHotkeys.Select(x => new ConsoleLine($"[{x.keyCombo}] - {x.description}", BuildArray(AppRegistry.SecondaryCol.Extend(x.keyCombo.Length + 4), [.. AppRegistry.PrimaryCol]))).ToArray()),
        ("Input", inputHotkeys.Select(x => new ConsoleLine($"[{x.keyCombo}] - {x.description}", BuildArray(AppRegistry.SecondaryCol.Extend(x.keyCombo.Length + 4), [.. AppRegistry.PrimaryCol]))).ToArray()),
        ("Menu", menuHotkeys.Select(x => new ConsoleLine($"[{x.keyCombo}] - {x.description}", BuildArray(AppRegistry.SecondaryCol.Extend(x.keyCombo.Length + 4), [.. AppRegistry.PrimaryCol]))).ToArray()),
        ("File", fileHotkeys.Select(x => new ConsoleLine($"[{x.keyCombo}] - {x.description}", BuildArray(AppRegistry.SecondaryCol.Extend(x.keyCombo.Length + 4), [.. AppRegistry.PrimaryCol]))).ToArray()));
    }

    /// <summary> Gives user y/n option to load an app. </summary>
    public static void LoadAppCommand(string userInput)
    {
        if (userInput.Trim().Length == 0) //submitted empty load request
        {
            (string name, ConsoleLine option)[] appList = [.. AppRegistry.appRegistry.OrderByDescending(app => app.displayPriority).Select(app => (app.name, new ConsoleLine(app.name, AppRegistry.SecondaryCol)))];

            int i = UserInput.CreateMultiPageOptionMenu("Apps", [.. appList.Select(x => x.option)], [new ConsoleLine("Exit")], 5);
            if (i == -1) return;
            else userInput = appList[i].name;
        }

        string appName = userInput.TrimStart().AdjustCapitalisation(StringFunctions.CapitalCasing.FirstLetterUpper);
        if (AppRegistry.AppExists(appName))
        {
            if (UserInput.CreateTrueFalseOptionMenu(new ConsoleLine($"Load App - '{appName}'", BuildArray(AppRegistry.PrimaryCol.Extend(11), AppRegistry.SecondaryCol))))
            {
                AppRegistry.SetActiveApp(appName);
                ReloadConsole();
            }
            else SendConsoleMessage(new ConsoleLine($"App Load Cancelled!", AppRegistry.PrimaryCol));
        }
        else SendConsoleMessage(new ConsoleLine($"App Could Not Be Found - '{appName}'", BuildArray(AppRegistry.PrimaryCol.Extend(25), AppRegistry.SecondaryCol)));
    }

    ///<summary> Interact with a setting app setting. </summary>
    public static void SettingInteractCommand(string userInput, bool readOnly)
    {
        if (SettingsApp.SettingExists(userInput))
        {
            if (readOnly) SettingsApp.SettingGetMenu(userInput);
            else SettingsApp.SettingSetMenu(userInput);
        }
        else SendConsoleMessage(new ConsoleLine($"Setting Could Not Be Found - '{userInput}'", BuildArray(AppRegistry.PrimaryCol.Extend(29), AppRegistry.SecondaryCol)));
    }

    ///<summary> Confirms user choice and kills/exit terminal. </summary>
    public static void ExitTerminalCommand(bool crash)
    {
        if (crash) SendConsoleMessage(new ConsoleLine("[WARNING] This Will Force Close Console (Crash).", ConsoleColor.DarkRed));
        if (UserInput.CreateTrueFalseOptionMenu(crash ? "Kill Terminal?" : "Close Terminal?", cursorStartIndex: 1))
        {
            if (crash) throw new Exception("User Killed Terminal.");
            else Environment.Exit(0);
        }
        ClearLines(updateCurrentLine: true);
    }

    public static void RenderTestCommand()
    {
        ConsoleLine[] lines = [.. Enumerable.Range(0, 40).Select(i => new ConsoleLine(new string('a', 200), AllColours.Repeat(13), AllColours.Repeat(13)))];
        ConsoleAnimatedLine[] animation = [.. Enumerable.Range(0, 40).Select(i => new ConsoleAnimatedLine(ConsoleAnimatedLine.ShiftColour, 5, true))];
        SendConsoleMessages(lines, animation);
    }
}