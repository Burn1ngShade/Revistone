using System.Text.Json.Serialization;
using Revistone.App;
using Revistone.Console;

namespace Revistone.Management;

///<summary> Class responsible for tracking all data about console usage (locally stored). </summary>
public static class Analytics
{
    static readonly ManualResetEvent waitHandle = new(false);

    public static GeneralAnalyticsData General { get; set; } = new();
    public static AppAnalyticsData App { get; set; } = new();
    public static WidgetAnalyticsData Widget { get; set; } = new();

    ///<summary> [DO NOT CALL] Main loop for analytics. </summary> </summary>
    public static void HandleAnalytics()
    {
        General.TimesOpened++;
        General.LastOpenDate = DateTime.Now;

        int lastUpdateTick = Manager.currentTick;
        (long keyPresses, long inputsProcessed, long optionMenusUsed) = (General.TotalKeyPresses, General.TotalInputsProcessed, General.TotalOptionMenusUsed);

        while (true)
        {
            waitHandle.WaitOne(1000); // wait for a second or until signaled

            General.TotalRuntimeTicks += Manager.currentTick - lastUpdateTick;
            App.TrackAppRuntime(AppRegistry.activeApp.name, Manager.currentTick - lastUpdateTick, General.TotalKeyPresses - keyPresses, General.TotalInputsProcessed - inputsProcessed, General.TotalOptionMenusUsed - optionMenusUsed);

            SaveAnalytics();
            lastUpdateTick = Manager.currentTick;
            (keyPresses, inputsProcessed, optionMenusUsed) = (General.TotalKeyPresses, General.TotalInputsProcessed, General.TotalOptionMenusUsed);
        }
    }

    // --- DATA HANDLING ---

    ///<summary> [DO NOT CALL] Initalizes analytics. </summary>
    public static void InitalizeAnalytics()
    {
        General = AppPersistentData.LoadFileFromJSON<GeneralAnalyticsData>("Analytics/General.json") ?? new GeneralAnalyticsData();
        App = AppPersistentData.LoadFileFromJSON<AppAnalyticsData>("Analytics/App.json") ?? new AppAnalyticsData();
        Widget = AppPersistentData.LoadFileFromJSON<WidgetAnalyticsData>("Analytics/Widget.json") ?? new WidgetAnalyticsData();
    }

    ///<summary> Saves analytics data. </summary>
    static void SaveAnalytics()
    {
        AppPersistentData.SaveFileAsJSON("Analytics/General.json", General);
        AppPersistentData.SaveFileAsJSON("Analytics/App.json", App);
        AppPersistentData.SaveFileAsJSON("Analytics/Widget.json", Widget);
    }

    // --- ANALYTICS OBJECTS ---

    ///<summary> Holds all analytics pertaining to general console behaviour. </summary>
    public class GeneralAnalyticsData()
    {
        public DateTime AnalyticsCreationDate { get; set; } = DateTime.Now;
        public DateTime LastOpenDate { get; set; }

        // --- General ---
        public int TimesOpened { get; set; } = 0;
        public long TotalRuntimeTicks { get; set; } = 0;
        [JsonInclude] public TimeSpan TotalRuntime => new TimeSpan(TotalRuntimeTicks * 250000);

        public long TotalKeyPresses { get; set; } = 0;
        public long TotalInputsProcessed { get; set; } = 0;
        public long TotalOptionMenusUsed { get; set; } = 0;

        public long TotalGPTPromts { get; set; } = 0;
        public long TotalGPTInputTokens { get; set; } = 0;
        public long TotalGPTOutputTokens { get; set; } = 0;
    }

    ///<summary> Holds all analytics pertaining to widget behaviour. </summary>
    public class WidgetAnalyticsData()
    {
        // --- Widgets ---
        public int WidgetsCreated { get; set; } = 0;
        public int WidgetsDestroyed { get; set; } = 0;
        public List<WidgetData> Widgets { get; set; } = [];

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

        public void TrackWidgetDeletion(string widgetName)
        {
            WidgetsDestroyed++;
            int index = Widgets.FindIndex(w => w.WidgetName == widgetName);

            if (index != -1) Widgets[index].TimesDestroyed++;
        }

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

        public void TrackAppRuntime(string appName, long ticks, long totalKeyPresses, long totalInputsProcessed, long totalOptionMenusUsed)
        {
            int index = Apps.FindIndex(a => a.AppName == appName);

            if (index != -1)
            {
                Apps[index].TotalRuntimeTicks += ticks;
                Apps[index].TotalKeyPresses += totalKeyPresses;
                Apps[index].TotalInputsProcessed += totalInputsProcessed;
                Apps[index].TotalOptionMenusUsed += totalOptionMenusUsed;
            }
        }

        public class AppData(string appName)
        {
            public string AppName { get; set; } = appName;
            public int TimesOpened { get; set; } = 1;

            public long TotalRuntimeTicks { get; set; } = 0;
            [JsonInclude]
            public TimeSpan TotalRuntime => new TimeSpan(TotalRuntimeTicks * 250000);

            public long TotalKeyPresses { get; set; } = 0;
            public long TotalInputsProcessed { get; set; } = 0;
            public long TotalOptionMenusUsed { get; set; } = 0;
        }
    }
}