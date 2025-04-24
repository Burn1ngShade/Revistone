using Revistone.App.BaseApps.Calculator;
using Revistone.App.BaseApps.HoneyC;
using Revistone.App.BaseApps;
using Revistone.Console;
using Revistone.Console.Image;
using Revistone.Console.Widget;
using Revistone.Functions;
using Revistone.Interaction;
using Revistone.Management;

using static Revistone.Console.ConsoleAction;
using static Revistone.Functions.ColourFunctions;
using static Revistone.Functions.PersistentDataFunctions;
using static Revistone.Functions.WorkspaceFunctions;

namespace Revistone.App;

/// <summary> Class pertaining all logic for app commands. </summary>
public static class AppCommands
{
    /// <summary> Array of all built in commands. </summary>
    public static (UserInputProfile format, Action<string> payload, string summary)[] baseCommands = [
                //useful commands
                (new UserInputProfile(UserInputProfile.InputType.FullText, "help", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => {if (baseCommands != null) Help((baseCommands.Select(cmd => StringFunctions.AdjustCapitalisation(cmd.format.inputFormat, StringFunctions.CapitalCasing.FirstLetterUpper)).ToArray(), baseCommands.Select(cmd => cmd.summary).ToArray())); },
                "List Of All Base Commands And Their Descriptions."),
                (new UserInputProfile(UserInputProfile.InputType.FullText, "app help", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
                (s) => {AppHelp(); },
                "List Of All App Specfic Commands And Their Descriptions."),
                (new UserInputProfile(UserInputProfile.InputType.FullText, "apps", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => {
                    int i = UserInput.CreateMultiPageOptionMenu("Apps:", AppRegistry.appRegistry.Select(app => new ConsoleLine(app.name, AppRegistry.SecondaryCol)).ToArray(), [new ConsoleLine("Exit")], 4);
                    if (i >= 0) LoadApp($"Load{AppRegistry.appRegistry[i].name}");
                },
                "Menu To Load Apps."),
                (new UserInputProfile(UserInputProfile.InputType.FullText, "hotkeys", caseSettings: StringFunctions.CapitalCasing.Lower,  removeLeadingWhitespace: true, removeTrailingWhitespace: true),
                (s) => { Hotkeys(); }, "Displays List Of Console Hotkeys."),
                (new UserInputProfile("load[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => { LoadApp(s); },
                "Loads App With The Given Name ([A:] meaning name of app)."),
                (new UserInputProfile(UserInputProfile.InputType.FullText, "reload", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => { ReloadApp(s); },
                "Reloads The Current App."),
                (new UserInputProfile(UserInputProfile.InputType.FullText, "hub", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => { LoadApp("LoadRevistone"); },
                "Loads Hub Revistone App."),
                (new UserInputProfile(UserInputProfile.InputType.FullText, "settings", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => { LoadApp("LoadSettings"); },
                "Loads The Setting App."),
                (new UserInputProfile(UserInputProfile.InputType.FullText, "clear", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => { ClearPrimaryConsole(); },
                "Clears Primary Console."),
                (new UserInputProfile(UserInputProfile.InputType.FullText, "clear debug", caseSettings: StringFunctions.CapitalCasing.Lower,  removeLeadingWhitespace: true, removeTrailingWhitespace: true),
                (s) => { ClearDebugConsole(); },
                "Clears Debug Console."),
                (new UserInputProfile(UserInputProfile.InputType.FullText, "time", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => { SendConsoleMessage(new ConsoleLine($"{DateTime.Now}.", AppRegistry.PrimaryCol)); },
                "Prints The Current System Time."),
                (new UserInputProfile(UserInputProfile.InputType.FullText, "runtime", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => { SendConsoleMessage(new ConsoleLine($"Revistone Has Been Running For {(Manager.ElapsedTicks / 40d).ToString("0.00")} Seconds.", AppRegistry.PrimaryCol)); },
                "Prints The Runtime Of The Current Session."),
                (new UserInputProfile("comp[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true), (s) => {HoneyCInterpreter.Interpret([s[4..]]); }, "Carries Out Given HoneyC Promt."),
                (new UserInputProfile("calc[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true), (s) => {CalculatorInterpreter.Intepret(s[4..]); }, "Carries Out Given Calculator Promt."),
                (new UserInputProfile("file comp[A:]", caseSettings: StringFunctions.CapitalCasing.Lower), (s) => {LoadHoneyCFile(s[9..]); }, "Loads And Carries Out Given HoneyCFile"),
                (new UserInputProfile("debug[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => { SendDebugMessage(s.Substring(5).TrimStart()); }, "Sends Debug Message [A:] ([A:] being the message)."),
                (new UserInputProfile("profiler toggle", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
                (s) => { Profiler.SetEnabled(!Profiler.Enabled); }, "Toggles Profiler On Or Off."),
                (new UserInputProfile("gpt[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
                (s) => { GPTFunctions.Query(s[3..].TrimStart(), true); }, "Interact With Custom Revistone ChatGPT Model, In A Back And Forth Conversation."),
                (new UserInputProfile("snap gpt[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
                (s) => { GPTFunctions.Query(s[8..].TrimStart(), false); }, "Interact With Custom Revistone ChatGPT Model, In A Single Message."),
                (new UserInputProfile("clear gpt", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
                (s) => { GPTFunctions.ClearMessageHistory(); }, "Wipe Message History Of ChatGPT Model."),
                (new UserInputProfile("store gpt[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
                (s) => { GPTFunctions.AddToMemories(s[9..].TrimStart()); }, "Store A Permeant Memory To GPT."),
                (new UserInputProfile("memory gpt", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
                (s) => { GPTFunctions.ViewMemories(); }, "View List Of GPT Memories."),
                (new UserInputProfile("get setting[A:]", removeLeadingWhitespace: true, removeTrailingWhitespace: true),
                (s) => { GetSetting(s[11..]); }, "Get The Value Of Given Setting."),
                (new UserInputProfile("set setting[A:]", removeLeadingWhitespace: true, removeTrailingWhitespace: true),
                (s) => { SetSetting(s[11..]); }, "Get The Value Of Given Setting."),
                (new UserInputProfile("timer[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => CreateTimer(s[5..].TrimStart()), "Creates Timer."),
                (new UserInputProfile("cancel timer", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
                (s) => CancelTimer(), "Cancels Active Timer."),
                (new UserInputProfile("sticker[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
                (s) => { DisplaySticker(s[7..].TrimStart()); }, "Output A Given Sticker To The Console."),
                (new UserInputProfile("list stickers", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
                (s) => StickerList(), "List All Default Stickers."),
                (new UserInputProfile("screenshot[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
                (s) => { ConsoleImage.TakePrimaryScreenshot(s[10..].Trim()); }, "Take A Screenshot Of The Primary Console."),
                (new UserInputProfile("dscreenshot[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
                (s) => { ConsoleImage.TakeDebugScreenshot(s[11..].Trim()); }, "Take A Screenshot Of The Debug Console."),
                (new UserInputProfile("kill", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => {KillTerminal();}, "Force Closes The Revistone Terminal."),
                (new UserInputProfile("mkdir[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
                (s) => {CreateWorkspaceDirectory(s[5..].Trim());}, "Create A Directory Within The Current Workspace Path."),
                (new UserInputProfile("rmdir[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
                (s) => {DeleteWorkspaceDirectory(s[5..].Trim());}, "Deletes A Directory Within The Current Workspace Path."),
                (new UserInputProfile("cdir[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
                (s) => {UpdatePath(RawPath + $@"{s[4..].Trim()}\");}, "Changes Directory To A Directory Within The Current Workspace Path."),
                (new UserInputProfile("pdir", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => {UpdatePath(GetParentPath(RawPath));}, "Goes To Parent Directory."),
                (new UserInputProfile("rdir", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => {UpdatePath(RootPath);}, "Changes To Root Directory."),
                (new UserInputProfile("dir", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => {DisplayWorkspaceOverview();}, "Gets Files And Directories Within The Current Workspace Directory."),
                (new UserInputProfile("mkfile[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
                (s) => {CreateWorkspaceFile(s[6..].Trim());}, "Create A File Within The Current Workspace Path."),
                (new UserInputProfile("rmfile[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
                (s) => {DeleteWorkspaceFile(s[6..].Trim());}, "Deletes A File Within The Current Workspace Path."),
                (new UserInputProfile("open[A:]", caseSettings: StringFunctions.CapitalCasing.Lower, removeLeadingWhitespace: true, removeTrailingWhitespace: true),
                (s) => {OpenWorkspaceFile(s[4..].Trim());}, "Opens A File Within The Current Workspace Path."),
            ];

    // --- BASE COMMANDS ---

    /// <summary> Displays list of base commands and descriptions. </summary>
    static void Help((string[] name, string[] summary) commands)
    {
        UserInput.CreateReadMenu("Help", 5,
        commands.name.Select((s, i) => new ConsoleLine($"{s}: {commands.summary[i]}",
        AdvancedHighlight($"{commands.name[i]}: {commands.summary[i]}".Length, AppRegistry.PrimaryCol,
        (AppRegistry.SecondaryCol, 0, commands.name[i].Length + 1)))).ToArray());
    }

    /// <summary> Displays list of app specfic commands and descriptions. </summary>
    static void AppHelp()
    {
        (string[] name, string[] summary) commands = (AppRegistry.activeApp.appCommands.Select(cmd => StringFunctions.AdjustCapitalisation(cmd.format.inputFormat, StringFunctions.CapitalCasing.FirstLetterUpper)).ToArray(), AppRegistry.activeApp.appCommands.Select(cmd => cmd.summary).ToArray());
        if (commands.name.Length == 0) SendConsoleMessage(new ConsoleLine("This App Has No Custom Commands!", AppRegistry.PrimaryCol));
        else Help(commands);
    }

    static readonly (string keyCombo, string description)[] hotkeys = [
        ("Ctrl + Shift + P", "Toggles Profiler."),
        ("Shift + Upwards Arrow", "Jump To Top Of Input History."),
        ("Shift + Downwards Arrow", "Jump To End Of Input History."),
        ("Shift + Leftwards Arrow", "Extend Selection To The Left."),
        ("Shift + Rightwards Arrow", "Extend Selection To The Right."),
        ("Ctrl + Leftwards Arrow", "Jump To Previous Seperator."),
        ("Ctrl + Rightwards Arrow", "Jump To Next Seperator."),
        ("Tab + Leftwards Arrow", "Extend Selection To The Previous Seperator."),
        ("Tab + Rightwards Arrow", "Extend Selection To The Next Seperator."),
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
        ("F11", "Takes A Screenshot Of The Debug Console."),
        ("F12", "Takes A Screenshot Of The Primary Console.")
    ];

    static readonly (string keyCombo, string description)[] generalHotkeys = [
        ("Ctrl + Shift + P", "Toggles Profiler."),
        ("F11", "Takes A Screenshot Of The Debug Console."),
        ("F12", "Takes A Screenshot Of The Primary Console."),
    ];

    static readonly (string keyCombo, string description)[] inputHotkeys = [
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

    static readonly (string keyCombo, string description)[] fileHotkeys = [
        ("Shift + Up Arrow", "Jump A Page Up."),
        ("Shift + Down Arrow", "Jump A Page Down."),
        ("Tab + Up Arrow", "Removes Empty Line."),
        ("Tab + Down Arrow", "Inserts Empty Line."),
    ];

    /// <summary> Displays list of console hotkeys. </summary>
    static void Hotkeys()
    {
        UserInput.CreateCategorisedReadMenu("Hotkeys", 5,
        ("General", generalHotkeys.Select(x => new ConsoleLine($"[{x.keyCombo}]: {x.description}", BuildArray(AppRegistry.SecondaryCol.Extend(x.keyCombo.Length + 3), [.. AppRegistry.PrimaryCol]))).ToArray()),
        ("Input", inputHotkeys.Select(x => new ConsoleLine($"[{x.keyCombo}]: {x.description}", BuildArray(AppRegistry.SecondaryCol.Extend(x.keyCombo.Length + 3), [.. AppRegistry.PrimaryCol]))).ToArray()),
        ("File", fileHotkeys.Select(x => new ConsoleLine($"[{x.keyCombo}]: {x.description}", BuildArray(AppRegistry.SecondaryCol.Extend(x.keyCombo.Length + 3), [.. AppRegistry.PrimaryCol]))).ToArray()));

        // UserInput.CreateReadMenu("Hotkeys", 5,
        // hotkeys.Select(x => new ConsoleLine($"[{x.keyCombo}]: {x.description}",
        // BuildArray(AppRegistry.SecondaryCol.Extend(x.keyCombo.Length + 3), AppRegistry.PrimaryCol.ToArray()))).ToArray());
    }

    /// <summary> Gives user Y/N option to reload current app. </summary>
    static void ReloadApp(string userInput)
    {
        if (UserInput.CreateTrueFalseOptionMenu("Reload App?")) ReloadConsole();
        else SendConsoleMessage(new ConsoleLine("App Reload Cancelled!", AppRegistry.PrimaryCol), new ConsoleLineUpdate());
    }

    /// <summary> Gives user Y/N option to load given app. </summary>
    static void LoadApp(string userInput)
    {
        if (userInput.Trim().Length == 4) //submitted empty load request
        {
            int i = UserInput.CreateMultiPageOptionMenu("Apps:", AppRegistry.appRegistry.Select(app => new ConsoleLine(app.name, AppRegistry.SecondaryCol)).ToArray(), new ConsoleLine[] { new ConsoleLine("Exit") }, 4);
            if (i >= 0) LoadApp($"Load{AppRegistry.appRegistry[i].name}");
            return;
        }
        string appName = userInput[4..].TrimStart().AdjustCapitalisation(StringFunctions.CapitalCasing.FirstLetterUpper);
        if (AppRegistry.AppExists(appName))
        {
            if (UserInput.CreateTrueFalseOptionMenu($"Load {appName}?"))
            {
                AppRegistry.SetActiveApp(appName);
                ReloadConsole();
            }
            else
            {
                SendConsoleMessage(new ConsoleLine($"App Load Cancelled!", AppRegistry.PrimaryCol), new ConsoleLineUpdate());
            }
        }
        else
        {
            SendConsoleMessage(new ConsoleLine($"App Named [{appName}] Could Not Be Found!", AppRegistry.PrimaryCol), new ConsoleLineUpdate());
        }
    }

    /// <summary> Returns value of setting with a given name. </summary>
    static void GetSetting(string userInput)
    {
        userInput = userInput.TrimStart();
        if (SettingsApp.SettingExists(userInput))
        {
            SettingsApp.HandleSettingGet(userInput);
        }
        else SendConsoleMessage(new ConsoleLine($"Setting [{userInput}] Could Not Be Found!", AppRegistry.PrimaryCol));
    }

    /// <summary> Allows user to set setting with a given name. </summary>
    static void SetSetting(string userInput)
    {
        userInput = userInput.TrimStart();
        if (SettingsApp.SettingExists(userInput))
        {
            SettingsApp.HandleSettingSet(userInput);
        }
        else SendConsoleMessage(new ConsoleLine($"Setting [{userInput}] Could Not Be Found!", AppRegistry.PrimaryCol));
    }

    /// <summary> Loads HoneyC File. </summary>
    static void LoadHoneyCFile(string userInput)
    {
        userInput = userInput.Trim();

        if (FileExists(GeneratePath(DataLocation.Workspace, $"HoneyC/{userInput}")))
        {
            HoneyCInterpreter.Interpret(LoadFile(GeneratePath(DataLocation.Workspace, $"HoneyC/{userInput}")));
        }
        else
        {
            SendConsoleMessage(new ConsoleLine($"File [{userInput}] Could Not Be Found!", AppRegistry.PrimaryCol));
        }
    }

    /// <summary> Create Timer Widget. </summary>
    static void CreateTimer(string time)
    {
        if (ConsoleWidget.WidgetExists("Timer"))
        {
            SendConsoleMessage(new ConsoleLine("Timer Already Active! Use Cancel Timer To Remove Active Timer.", AppRegistry.PrimaryCol));
            return;
        }

        double timerInSeconds = 0;

        if (double.TryParse(time, out double dDuration)) timerInSeconds = dDuration;
        else if (TimeSpan.TryParse(time, out TimeSpan duration)) timerInSeconds = duration.TotalSeconds;
        else
        {
            SendConsoleMessage(new ConsoleLine("Invalid Time Format! Use Format hh:mm:ss.", AppRegistry.PrimaryCol));
            return;
        }

        if (timerInSeconds > 86400) // seconds in a day
        {
            SendConsoleMessage(new ConsoleLine("Timer Can Not Be Longer Than 24 Hours!", AppRegistry.PrimaryCol));
            return;
        }

        ConsoleWidget.TryAddWidget(new TimeWidget("Timer", 100, DateTime.Now.AddSeconds(timerInSeconds), true));
        SendConsoleMessage(new ConsoleLine($"Timer Has Been Created!", AppRegistry.PrimaryCol));
    }

    /// <summary> Delete Timer Widget. </summary>
    static void CancelTimer()
    {
        if (!ConsoleWidget.WidgetExists("Timer"))
        {
            SendConsoleMessage(new ConsoleLine("No Active Timer Exists!", AppRegistry.PrimaryCol));
            return;
        }

        ConsoleWidget.TryRemoveWidget("Timer");
        SendConsoleMessage(new ConsoleLine($"Timer Cancelled!", AppRegistry.PrimaryCol));
    }

    /// <summary> Displays Sticker. </summary>
    static void DisplaySticker(string sticker)
    {
        if (File.Exists(GeneratePath(DataLocation.Console, "Assets", $"Stickers/{sticker}.cimg")))
        {
            ConsoleImage? stickerImage = ConsoleImage.LoadFromCIMG(GeneratePath(DataLocation.Console, "Assets", $"Stickers/{sticker}.cimg"));
            stickerImage?.Output();
        }
        else
        {
            SendConsoleMessage(new ConsoleLine($"Sticker [{sticker}] Could Not Be Found!", AppRegistry.PrimaryCol));
        }
    }

    ///<summary> Displays list of console stickers. </summary>
    static void StickerList()
    {
        string[] stickers = GetSubFiles(GeneratePath(DataLocation.Console, "Assets", "Stickers"));
        UserInput.CreateReadMenu("Stickers", 4, stickers.Select(x => new ConsoleLine(x[..^5], AppRegistry.SecondaryCol)).ToArray());
    }

    ///<summary> Confirms user choice and kills terminal. </summary>
    static void KillTerminal()
    {
        SendConsoleMessage(new ConsoleLine("[WARNING] This Will Force Close Console (Crash).", ConsoleColor.DarkRed));
        if (UserInput.CreateTrueFalseOptionMenu("Kill Terminal?", cursorStartIndex: 1))
        {
            throw new Exception("User Killed Terminal.");
        }
        ClearLines(updateCurrentLine: true);
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
                Analytics.General.CommandsUsed++;
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
                Analytics.General.CommandsUsed++;
                return true;
            }
        }

        return false;
    }
}