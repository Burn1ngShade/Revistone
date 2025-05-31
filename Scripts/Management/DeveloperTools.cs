using System.Runtime.CompilerServices;
using Revistone.App.BaseApps;
using Revistone.App.Command;

using static Revistone.Functions.PersistentDataFunctions;

namespace Revistone.Management;

///<summary> Set of functions and tools, for helping to debug the console. </summary>
public static class DeveloperTools
{
    static readonly SessionLog log = new();

    ///<summary> Log message to the session log. </summary>
    public static void Log<T>(T message, bool flagSession = false, bool advancedLog = false, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "", [CallerLineNumber] int callerLineNumber = 0)
    {
        if (flagSession) log.FlagSession = true;
        log.Messages.Add(new SessionLog.LogData(message?.ToString() ?? "", flagSession, callerFilePath, callerMemberName, callerLineNumber));
        if (advancedLog) DeveloperTools.advancedLog.Messages.Add(new SessionLog.LogData(message?.ToString() ?? "", flagSession, callerFilePath, callerMemberName, callerLineNumber));
    }

    ///<summary> Debug Session Log. </summary>
    public class SessionLog
    {
        public DateTime SessionStartTime { get; private set; } = DateTime.Now;
        public bool FlagSession { get; set; } = false;

        public List<LogData> Messages { get; private set; } = [];

        public class LogData(string message, bool flagSession, string callerFilePath, string callerMemberName, int callerLineNumber)
        {
            public string CallerFilePath { get; set; } = callerFilePath;
            public string CallerMemberName { get; set; } = callerMemberName;
            public int CallerLineNumber { get; set; } = callerLineNumber;

            public DateTime TimeStamp { get; set; } = DateTime.Now;
            public string Message { get; set; } = message;
            public bool FlagSession { get; set; } = flagSession;
        }

        ///<summary> Save the current session log. </summary>
        public static void Update()
        {
            SaveFileAsJSON(GeneratePath(DataLocation.ConsoleDebug, $"SessionLog.json"), log);
            if (log.FlagSession)
            {
                SaveFileAsJSON(GeneratePath(DataLocation.ConsoleDebug, "Flagged", $"SessionLog{log.SessionStartTime:yyyy-MM-dd_HH-mm-ss-fff}.json"), log);
            }
        }
    }

    // --- DEEP DEBUGGING ---

    static readonly AdvancedSessionLog advancedLog = new();

    public class AdvancedSessionLog
    {
        public static readonly string advancedSessionLogPath = GeneratePath(DataLocation.ConsoleDebug, "AdvancedSessionLog.json");

        public List<ThreadStatus> ThreadStatuses { get; set; } = [];
        public List<SessionLog.LogData> Messages { get; set; } = [];

        public class ThreadStatus(string mainThread, string mainThreadInfo, string tickThread, string tickThreadInfo, string inputThread, string inputThreadInfo, string renderThread, string renderThreadInfo)
        {
            public DateTime TimeStamp { get; set; } = DateTime.Now;

            public string MainThread { get; set; } = mainThread;
            public string MainThreadInfo { get; set; } = mainThreadInfo;
            public string TickThread { get; set; } = tickThread;
            public string TickThreadInfo { get; set; } = tickThreadInfo;
            public string InputThread { get; set; } = inputThread;
            public string InputThreadInfo { get; set; } = inputThreadInfo;
            public string RenderThread { get; set; } = renderThread;
            public string RenderThreadInfo { get; set; } = renderThreadInfo;
        }

        ///<summary> Updates the advanced session log. </summary>
        public static void Update()
        {
            SaveFileAsJSON(advancedSessionLogPath, advancedLog);
        }
    }

    ///<summary> [DO NOT CALL] Primary loop for deep debugging. </summary>
    public static void HandleAdvancedSessionLog()
    {
        int sleepDuration = int.Parse(SettingsApp.GetValue("Advanced Session Log")[..1]) * 1000;

        Log($"Advanced Session Log Enabled, {sleepDuration}ms Intervals.");

        while (true)
        {
            string[] threadStatus = Manager.GetThreadStatus();
            advancedLog.ThreadStatuses.Add(new AdvancedSessionLog.ThreadStatus(
                threadStatus[0],
                threadStatus[1],
                threadStatus[2],
                threadStatus[3],
                threadStatus[4],
                threadStatus[5],
                threadStatus[6],
                threadStatus[7]

            ));
            AdvancedSessionLog.Update();

            Thread.Sleep(10000);
        }
    }

    // --- CONSOLE CREATION FUNCTIONS ---

    // functions not used within console, but generate assets for the console

    ///<summary> Generates an updated version of the AboutRevistone.txt GPT info file. </summary>
    public static void GenerateGPTAboutFile()
    {
        List<string> aboutInfo =
        [
            "About The Console: Revistone is a virtual OS with a command-line interface, menus, and real-time input. Created by Isaac Honeyman, it serves as an all-in-one application with apps, commands, and widgets and is what YOUR integrated into. The user has access to a workspace, which allows them to create, modify, and delete files and directorys like a standard command-line interface.",
            "- Apps: Different way to interact with the console",
            "- Commands: Perform tasks",
            "- Widgets: Display information",
            "",
            "Custom file types:",
            "- .cimg (console images)",
            "- .hc (HoneyC, the system’s custom programming language)",
            "",
            "General Hotkeys:",
            .. AppCommandsData.generalHotkeys.Select(x => $"- {x.keyCombo} -> {x.description}"),
            "",
            "Input Hotkeys:",
            .. AppCommandsData.inputHotkeys.Select(x => $"- {x.keyCombo} -> {x.description}"),
            "",
            "Menu Hotkeys:",
            .. AppCommandsData.menuHotkeys.Select(x => $"- {x.keyCombo} -> {x.description}"),
            "",
            "File Hotkeys:",
            .. AppCommandsData.fileHotkeys.Select(x => $"- {x.keyCombo} -> {x.description}"),
            "",
            "Apps [- Name -> Description]:",
            "- Revistone-> Start app, used for command - line interface, workspace, and GPT interaction",
            "- Tracker -> Daily logger app",
            "- Paint -> For creating and editing.cimg files",
            "- Flash Card Manager -> For creating and using virtual flash cards",
            "- Debit Card Manager -> Test app, allowing user to create and manager virtual bank accounts",
            "- Font Showcase -> Allows user to view all the fonts available within the console, with text of their choice",
            "- Settings -> App for editing and viewing console settings",
            "- Screenshots -> App for viewing screenshots",
            "- Pong -> The classic game Pong",
            "- Tetris -> The classic game Tetris",
            "",
            "Settings [- Name -> Description]:",
            .. SettingsApp.settings.Select(x => $"- {x.settingName} -> {x.description}"),
            "",
            "Commands [- Name [Type] -> Description]:",
            .. AppCommandRegistry.baseCommands.Select(x => $"- {x.commandName} [{x.type}] -> {x.description}"),
        ];

        SaveFile(GeneratePath(DataLocation.ConsoleAssets, "GPT", "GeneratedAboutRevistone.txt"), [.. aboutInfo]);
        Log("Developer Tool Used.");

    }
}