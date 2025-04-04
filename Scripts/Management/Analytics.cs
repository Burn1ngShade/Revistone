using System.Text.Json.Serialization;
using Revistone.App;
using Revistone.App.BaseApps;
using System.Runtime.CompilerServices;
using static Revistone.Functions.PersistentDataFunctions;

namespace Revistone.Management;

///<summary> Handles tracking all data about console usage (locally stored). </summary>
public static class Analytics
{
    static readonly ManualResetEvent waitHandle = new(false);
    static readonly object saveLockObject = new();

    public static GeneralAnalyticsData General { get; set; } = new();
    public static AppAnalyticsData App { get; set; } = new();
    public static WidgetAnalyticsData Widget { get; set; } = new();
    public static DebugAnalyticsData Debug { get; set; } = new();

    ///<summary> [DO NOT CALL] Main loop for analytics. </summary> </summary>
    public static void HandleAnalytics()
    {
        General.TimesOpened++;
        General.LastOpenDate = DateTime.Now;

        RuntimeAnalytics rAnalytics = new(General);
        int lastRuntimeTicks = 0;

        while (true)
        {
            waitHandle.WaitOne((int)(float.Parse(SettingsApp.GetValue("Analytics Update Frequency")[..^1]) * 1000)); // wait for a second or until signaled

            General.TotalRuntimeTicks += Manager.ElapsedTicks - lastRuntimeTicks;
            rAnalytics.Difference(General);
            App.TrackAppRuntime(AppRegistry.activeApp.name, rAnalytics);

            SaveAnalytics();
            rAnalytics = new(General);
            lastRuntimeTicks = Manager.ElapsedTicks;
        }
    }

    // --- DATA HANDLING ---

    ///<summary> [DO NOT CALL] Initalizes analytics. </summary>
    internal static void InitalizeAnalytics()
    {
        General = LoadFileFromJSON<GeneralAnalyticsData>(GeneratePath(DataLocation.Console, "Analytics", "General.json")) ?? new GeneralAnalyticsData();
        App = LoadFileFromJSON<AppAnalyticsData>(GeneratePath(DataLocation.Console, "Analytics", "App.json")) ?? new AppAnalyticsData();
        Widget = LoadFileFromJSON<WidgetAnalyticsData>(GeneratePath(DataLocation.Console, "Analytics", "Widget.json")) ?? new WidgetAnalyticsData();
        Debug = new DebugAnalyticsData(); // we only want debug from last run

        if (!FileExists(GeneratePath(DataLocation.Console, "Analytics", "Debug.json"))) CreateFile(GeneratePath(DataLocation.Console, "Analytics", "Debug.json"));
    }

    ///<summary> Saves analytics data. </summary>
    public static void SaveAnalytics()
    {
        // using temp files prevents data being corrupted when closing console mid save so yay!

        lock (saveLockObject)
        {
            string tempGeneralFile = GeneratePath(DataLocation.Console, "Analytics", "General_temp.json");
            string tempAppFile = GeneratePath(DataLocation.Console, "Analytics", "App_temp.json");
            string tempWidgetFile = GeneratePath(DataLocation.Console, "Analytics", "Widget_temp.json");
            string tempDebugFile = GeneratePath(DataLocation.Console, "Analytics", "Debug_temp.json");

            SaveFileAsJSON(tempGeneralFile, General);
            SaveFileAsJSON(tempAppFile, App);
            SaveFileAsJSON(tempWidgetFile, Widget);
            SaveFileAsJSON(tempDebugFile, Debug);

            File.Replace(tempGeneralFile, GeneratePath(DataLocation.Console, "Analytics", "General.json"), null);
            File.Replace(tempAppFile, GeneratePath(DataLocation.Console, "Analytics", "App.json"), null);
            File.Replace(tempWidgetFile, GeneratePath(DataLocation.Console, "Analytics", "Widget.json"), null);
            File.Replace(tempDebugFile, GeneratePath(DataLocation.Console, "Analytics", "Debug.json"), null);
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
        public DateTime LastCloseDate {get; set; }

        // --- General ---
        public int TimesOpened { get; set; } = 0;
        public long TotalRuntimeTicks { get; set; } = 0;
        [JsonInclude] public TimeSpan TotalRuntime => new TimeSpan(TotalRuntimeTicks * 250000);

        public long KeyPresses { get; set; } = 0;
        public long LinesEntered { get; set; } = 0;
        public long OptionMenusUsed { get; set; } = 0;
        public long CommandsUsed { get; set; } = 0;

        public long TotalGPTPromts { get; set; } = 0;
        public long UsedGPTInputTokens { get; set; } = 0;
        public long UsedGPTOutputTokens { get; set; } = 0;
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

        public void Add(string message, [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = 0)
        {
            DebugMessages.Add(new DebugData(message, callerFilePath, callerLineNumber));
        }

        public class DebugData(string message, string callerFilePath, int callerLineNumber)
        {
            public string CallerFilePath { get; set; } = callerFilePath;
            public int CallerLineNumber { get; set; } = callerLineNumber;

            public DateTime TimeStamp { get; set; } = DateTime.Now;
            public string Message { get; set; } = message;
        }
    }
}