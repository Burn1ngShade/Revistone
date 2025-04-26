using System.Text.Json.Serialization;
using Revistone.App;
using System.Runtime.CompilerServices;

using static Revistone.Functions.PersistentDataFunctions;

namespace Revistone.Management;

///<summary> Handles tracking all data about console usage (locally stored). </summary>
public static class Analytics
{
    static readonly object saveLockObject = new(); // to stop multi thread write weirdness

    public static GeneralAnalyticsData General { get; set; } = new(); // general data
    public static AppAnalyticsData App { get; set; } = new(); // data app specific
    public static WidgetAnalyticsData Widget { get; set; } = new(); // widget stuff
    public static DebugAnalyticsData Debug { get; set; } = new(); // debug console, as debug lines dont always come through

    static readonly string path = GeneratePath(DataLocation.Console, @"Analytics\");

    static RuntimeAnalytics rAnalytics;
    static int lastRuntimeTicks = 0;

    ///<summary> Update analytics. </summary> 
    public static void HandleAnalytics()
    {
        lock (saveLockObject)
        {
            General.TotalRuntimeTicks += Manager.ElapsedTicks - lastRuntimeTicks;
            rAnalytics.Difference(General); // dif between last update and now
            App.TrackAppRuntime(AppRegistry.activeApp.name, rAnalytics); // add dif

            SaveAnalytics();
            rAnalytics = new(General);
            lastRuntimeTicks = Manager.ElapsedTicks;
        }
    }

    // --- DATA HANDLING ---

    ///<summary> [DO NOT CALL] Initalizes analytics. </summary>
    internal static void InitalizeAnalytics()
    {
        General = LoadFileFromJSON<GeneralAnalyticsData>(path + "General.json") ?? new GeneralAnalyticsData();
        App = LoadFileFromJSON<AppAnalyticsData>(path + "App.json") ?? new AppAnalyticsData();
        Widget = LoadFileFromJSON<WidgetAnalyticsData>(path + "Widget.json") ?? new WidgetAnalyticsData();
        Debug = new DebugAnalyticsData(); // we only want debug from last run

        if (!FileExists(GeneratePath(DataLocation.Console, "Analytics", "Debug.json"))) CreateFile(GeneratePath(DataLocation.Console, "Analytics", "Debug.json"));

        General.TimesOpened++;
        General.LastOpenDate = DateTime.Now;
        rAnalytics = new(General);
    }

    ///<summary> Saves analytics data. </summary>
    public static void SaveAnalytics()
    {
        lock (saveLockObject)
        {
            SaveFileAsJSON(path + "GeneralTemp.json", General);
            SaveFileAsJSON(path + "AppTemp.json", App);
            SaveFileAsJSON(path + "WidgetTemp.json", Widget);
            SaveFileAsJSON(path + "DebugTemp.json", Debug);

            if (LoadFileFromJSON<GeneralAnalyticsData>(path + "GeneralTemp.json") != null) // something went wrong during the write
            {
                File.Delete(path + "General.json");
                File.Move(path + "GeneralTemp.json", path + "General.json");
            }

            if (LoadFileFromJSON<AppAnalyticsData>(path + "AppTemp.json") != null) // something went wrong during the write
            {
                File.Delete(path + "App.json");
                File.Move(path + "AppTemp.json", path + "App.json");
            }

            if (LoadFileFromJSON<WidgetAnalyticsData>(path + "WidgetTemp.json") != null) // something went wrong during the write
            {
                File.Delete(path + "Widget.json");
                File.Move(path + "WidgetTemp.json", path + "Widget.json");
            }

            if (LoadFileFromJSON<DebugAnalyticsData>(path + "DebugTemp.json") != null) // something went wrong during the write
            {
                File.Delete(path + "Debug.json");
                File.Move(path + "DebugTemp.json", path + "Debug.json");
            }
        }
    }

    // --- ANALYTICS OBJECTS ---

    ///<summary> Generates the change in analytics within the last update to update the current App. </summary>
    public struct RuntimeAnalytics
    {
        public long runtimeTicks;
        public long keyPresses;
        public long linesEntered;
        public long menusUsed;
        public long commandsUsed;

        public RuntimeAnalytics(GeneralAnalyticsData analyticsData)
        {
            runtimeTicks = analyticsData.TotalRuntimeTicks;
            keyPresses = analyticsData.KeyPresses;
            linesEntered = analyticsData.LinesEntered;
            menusUsed = analyticsData.OptionMenusUsed;
            commandsUsed = analyticsData.CommandsUsed;
        }

        ///<summary> Calculates the difference between the current analytics Data and the given analytics Data. </summary>
        public void Difference(GeneralAnalyticsData newAnalytics)
        {
            runtimeTicks = newAnalytics.TotalRuntimeTicks - runtimeTicks;
            keyPresses = newAnalytics.KeyPresses - keyPresses;
            linesEntered = newAnalytics.LinesEntered - linesEntered;
            menusUsed = newAnalytics.OptionMenusUsed - menusUsed;
            commandsUsed = newAnalytics.CommandsUsed - commandsUsed;
        }
    }

    ///<summary> Holds all analytics pertaining to general console behaviour. </summary>
    public class GeneralAnalyticsData()
    {
        public DateTime AnalyticsCreationDate { get; set; } = DateTime.Now;
        public DateTime LastOpenDate { get; set; }
        public DateTime LastCloseDate { get; set; }

        // --- General ---
        public int TimesOpened { get; set; } = 0;
        public long TotalRuntimeTicks { get; set; } = 0;
        [JsonInclude] public TimeSpan TotalRuntime => new TimeSpan(TotalRuntimeTicks * 250000);

        public long KeyPresses { get; set; } = 0;
        public long LinesEntered { get; set; } = 0;
        public long OptionMenusUsed { get; set; } = 0;
        public long CommandsUsed { get; set; } = 0;
        public long SettingsChanged { get; set; } = 0;

        public long TotalGPTPromts { get; set; } = 0;
        public long UsedGPTInputTokens { get; set; } = 0;
        public long UsedGPTOutputTokens { get; set; } = 0;

        public long DirectoriesCreated { get; set; } = 0;
        public long DirectoriesDeleted { get; set; } = 0;
        public long DirectoriesOpened { get; set; } = 0;

        public long FilesCreated { get; set; } = 0;
        public long FilesDeleted { get; set; } = 0;
        public long FilesOpened { get; set; } = 0;
    }

    ///<summary> Holds all analytics pertaining to widget behaviour. </summary>
    public class WidgetAnalyticsData()
    {
        // --- Widgets ---
        public int WidgetsCreated { get; set; } = 0;
        public int WidgetsDestroyed { get; set; } = 0;
        public List<WidgetData> Widgets { get; set; } = [];

        ///<summary> Update widget creation analytics. </summary>
        public void TrackWidgetCreation(string widgetName)
        {
            WidgetsCreated++;
            int index = Widgets.FindIndex(w => w.WidgetName == widgetName);

            if (index != -1)
            {
                Widgets[index].TimesCreated++;
                return;
            }

            Widgets.Add(new WidgetData(widgetName));
        }

        ///<summary> Update widget deletion analytics. </summary>
        public void TrackWidgetDeletion(string widgetName)
        {
            WidgetsDestroyed++;
            int index = Widgets.FindIndex(w => w.WidgetName == widgetName);

            if (index != -1) Widgets[index].TimesDestroyed++;
        }

        ///<summary> Analytics for an individual widget. </summary>
        public class WidgetData(string widgetName)
        {
            public string WidgetName { get; set; } = widgetName;
            public int TimesCreated { get; set; } = 1;
            public int TimesDestroyed { get; set; } = 0;
        }
    }

    ///<summary> Holds all analytics pertaining to app behaviour. </summary>
    public class AppAnalyticsData()
    {
        public int AppsOpened { get; set; } = 0;
        public List<AppData> Apps { get; set; } = [];

        ///<summary> Update app open analytics. </summary>
        public void TrackAppOpen(string appName)
        {
            AppsOpened++;
            int index = Apps.FindIndex(a => a.AppName == appName);

            if (index != -1)
            {
                Apps[index].TimesOpened++;
                return;
            }

            Apps.Add(new AppData(appName));
        }

        ///<summary> Update app runtime analytics. </summary>
        public void TrackAppRuntime(string appName, RuntimeAnalytics analytics)
        {
            int index = Apps.FindIndex(a => a.AppName == appName);

            if (index != -1)
            {
                Apps[index].TotalRuntimeTicks += analytics.runtimeTicks;
                Apps[index].KeyPresses += analytics.keyPresses;
                Apps[index].LinesEntered += analytics.linesEntered;
                Apps[index].OptionMenusUsed += analytics.menusUsed;
                Apps[index].CommandsUsed += analytics.commandsUsed;
            }
        }

        ///<summary> Analytics for an individual app. </summary>
        public class AppData(string appName)
        {
            public string AppName { get; set; } = appName;
            public int TimesOpened { get; set; } = 1;

            public long TotalRuntimeTicks { get; set; } = 0;
            [JsonInclude] public TimeSpan TotalRuntime => new TimeSpan(TotalRuntimeTicks * 250000);

            public long KeyPresses { get; set; } = 0;
            public long LinesEntered { get; set; } = 0;
            public long OptionMenusUsed { get; set; } = 0;
            public long CommandsUsed { get; set; } = 0;
        }
    }

    ///<summary> Holds all analytics pertaining to app behaviour. </summary>
    public class DebugAnalyticsData()
    {
        public List<DebugData> DebugMessages { get; private set; } = [];
        public DateTime LastLogTime { get; private set; } = DateTime.Now;

        public void Log<T>(T message, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            DebugMessages.Add(new DebugData(message?.ToString() ?? "", callerFilePath, callerMemberName, callerLineNumber));
            LastLogTime = DateTime.Now;
        }

        public class DebugData(string message, string callerFilePath, string callerMemberName, int callerLineNumber)
        {
            public string CallerFilePath { get; set; } = callerFilePath;
            public string CallerMemberName { get; set; } = callerMemberName;
            public int CallerLineNumber { get; set; } = callerLineNumber;

            public DateTime TimeStamp { get; set; } = DateTime.Now;
            public string Message { get; set; } = message;
        }
    }
}