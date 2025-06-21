using System.Text.Json.Serialization;
using Revistone.App;
using Revistone.Console.Data;

using static Revistone.Functions.PersistentDataFunctions;
using static Revistone.Functions.ColourFunctions;

using Revistone.Console;
using Revistone.Interaction;
using System.Diagnostics;
using Revistone.Console.Image;

namespace Revistone.Management;

///<summary> Handles tracking all data about console usage (locally stored). </summary>
public static class Analytics
{
    static readonly object saveLockObject = new(); // to stop multi thread write weirdness

    public static GeneralAnalyticsData General { get; set; } = new(); // general data
    public static AppAnalyticsData App { get; set; } = new(); // data app specific
    public static WidgetAnalyticsData Widget { get; set; } = new(); // widget stuff

    static readonly string path = GeneratePath(DataLocation.Console, @"Analytics\");

    static RuntimeAnalytics rAnalytics;
    static int lastRuntimeTicks = 0;

    public static void HandleAnalytics(int tickNum)
    {
        if (tickNum % ConsoleData.analyticTickInterval == 0)
        {
            HandleAnalytics();
            DeveloperTools.SessionLog.Update();
        }
    }

    ///<summary> Update analytics. </summary> 
    public static void HandleAnalytics()
    {
        lock (saveLockObject)
        {
            General.TotalRuntimeTicks += Manager.ElapsedTicks - lastRuntimeTicks;
            rAnalytics.Difference(General); // dif between last update and now
            App.TrackAppRuntime(AppRegistry.ActiveApp.name, rAnalytics); // add dif

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

        if (!FileExists(GeneratePath(DataLocation.Console, "Analytics", "Debug.json"))) CreateFile(GeneratePath(DataLocation.Console, "Analytics", "Debug.json"));

        General.TimesOpened++;
        General.LastOpenDate = DateTime.Now;
        rAnalytics = new(General);

        Manager.Tick += HandleAnalytics; // track tick events
    }

    ///<summary> Saves analytics data. </summary>
    public static void SaveAnalytics()
    {
        if (General.TotalRuntimeTicks != App.Apps.Sum(x => x.TotalRuntimeTicks))
        {
            ConsoleAction.SendDebugMessage(DeveloperTools.DefaultErrorMessage);
            DeveloperTools.Log($"General Ticks: {General.TotalRuntimeTicks}, App Ticks: {App.Apps.Sum(x => x.TotalRuntimeTicks)}", true);
            DeveloperTools.Log(General.CommandsUsed, true);
        }

        lock (saveLockObject)
        {
            if (LoadFileFromJSON<GeneralAnalyticsData>(path + "General.json")?.TotalRuntimeTicks > General.TotalRuntimeTicks)
            {
                ConsoleAction.SendDebugMessage(DeveloperTools.DefaultErrorMessage);
                DeveloperTools.Log(new ConsoleLine("Analytics - General.json is newer than current data, not saving.", AppRegistry.PrimaryCol), true);
                return; // something wrong with general file
            }

            SaveFileAsJSON(path + "GeneralTemp.json", General);
            SaveFileAsJSON(path + "AppTemp.json", App);
            SaveFileAsJSON(path + "WidgetTemp.json", Widget);

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
        }
    }

    ///<summary> View analytics data within console. </summary>
    public static void ViewAnalytics(int option = 0)
    {
        option = UserInput.CreateOptionMenu("---- Analytics ---", [
            new ConsoleLine("General Analytics", AppRegistry.SecondaryCol),
            new ConsoleLine("App Analytics", AppRegistry.SecondaryCol),
            new ConsoleLine("Widget Analytics", AppRegistry.SecondaryCol),
            new ConsoleLine("Exit", AppRegistry.PrimaryCol),
        ], cursorStartIndex: option);

        switch (option)
        {
            case 0:
                UserInput.GetMultiUserInput("General Analytics", LoadFile(GeneratePath(DataLocation.Console, "Analytics/General.json")), readOnly: true);
                break;
            case 1:
                UserInput.GetMultiUserInput("App Analytics", LoadFile(GeneratePath(DataLocation.Console, "Analytics/App.json")), readOnly: true);
                break;
            case 2:
                UserInput.GetMultiUserInput("Widget Analytics", LoadFile(GeneratePath(DataLocation.Console, "Analytics/Widget.json")), readOnly: true);
                break;
            default:
                return; // if exit
        }

        ViewAnalytics(option);
    }

    ///<summary> Create analytics backup. </summary>
    public static void CreateAnalyticsBackup()
    {
        if (!UserInput.CreateTrueFalseOptionMenu("Back Up Analytics? This Will Overwrite Previous Backup."))
        {
            ConsoleAction.SendConsoleMessage(new ConsoleLine("Analytics Backup Cancelled.", AppRegistry.PrimaryCol));
            return;
        }

        SaveFileAsJSON(path + "Backup/General.json", General);
        SaveFileAsJSON(path + "Backup/App.json", App);
        SaveFileAsJSON(path + "Backup/Widget.json", Widget);

        ConsoleAction.SendConsoleMessage(new ConsoleLine("Analytics Backup Created.", AppRegistry.PrimaryCol));
    }

    ///<summary> Attempts to restore corrupted data from most recent backup. </summary>
    public static void RestoreAnalyticsBackup()
    {
        ConsoleColour[][] c = [
            BuildArray(AppRegistry.PrimaryCol.SetLength(24), AppRegistry.SecondaryCol),
            BuildArray(AppRegistry.PrimaryCol.SetLength(21), AppRegistry.SecondaryCol),
            BuildArray(AppRegistry.PrimaryCol.SetLength(28), AppRegistry.SecondaryCol)
        ];

        GeneralAnalyticsData? g = LoadFileFromJSON<GeneralAnalyticsData>(path + "Backup/General.json", false);
        if (g == null) ConsoleAction.SendConsoleMessage(new ConsoleLine("Backup File Not Found - 'General.json'", c[0]));
        else if (g.TotalRuntimeTicks <= General.TotalRuntimeTicks) ConsoleAction.SendConsoleMessage(new ConsoleLine("File Not Corrupted - 'General.json'", c[1]));
        else
        {
            General = g;
            ConsoleAction.SendConsoleMessage(new ConsoleLine("File Restored From Backup - 'General.json'", c[2]));
        }

        AppAnalyticsData? a = LoadFileFromJSON<AppAnalyticsData>(path + "Backup/App.json", false);
        if (a == null) ConsoleAction.SendConsoleMessage(new ConsoleLine("Backup File Not Found - 'App.json'", c[0]));
        else if (a.AppsOpened <= App.AppsOpened) ConsoleAction.SendConsoleMessage(new ConsoleLine("File Not Corrupted - 'App.json'", c[1]));
        else
        {
            App = a;
            ConsoleAction.SendConsoleMessage(new ConsoleLine("File Restored From Backup - 'App.json'", c[2]));
        }

        WidgetAnalyticsData? w = LoadFileFromJSON<WidgetAnalyticsData>(path + "Backup/Widget.json", false);
        if (w == null) ConsoleAction.SendConsoleMessage(new ConsoleLine("Backup File Not Found - 'Widget.json'", c[0]));
        else if (w.WidgetsCreated <= Widget.WidgetsCreated) ConsoleAction.SendConsoleMessage(new ConsoleLine("File Not Corrupted - 'Widget.json'", c[1]));
        else
        {
            Widget = w;
            ConsoleAction.SendConsoleMessage(new ConsoleLine("File Restored From Backup - 'Widget.json'", c[2]));
        }

        SaveAnalytics();
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
}