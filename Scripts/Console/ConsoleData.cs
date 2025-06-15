using System.Diagnostics;
using Revistone.App.BaseApps;

namespace Revistone.Console.Data;

/// <summary> Class pertaining all the data of the console (should NOT be used by user). </summary>
internal static class ConsoleData
{
    //--- Console Lines ---
    public static ConsoleLine[] consoleLines = []; //current state of console lines
    public static ConsoleLine[] consoleLinesBuffer = []; //last tick state of console lines
    public static ConsoleAnimatedLine[] consoleLineUpdates = []; //animation data of console lines

    public static bool[] exceptionLines = []; //prevent certain methods effecting line

    //--- Console Metadata ---
    public static int primaryLineIndex; //index of current console line
    public static int debugLineIndex; //index of current debug line

    public static bool screenWarningUpdated = false; //if screen 2 small needs printing
    public static bool consoleReload = false; //console needs to be reloaded (data and visual reset)
    public static bool consoleInterrupt = false; //on console resize, need to update certian elements, allow each to carry there own behaviour
    public static bool appInitalisation = false; //app needs to be updated by manager

    //--- Console Buffer ---
    public static (int width, int height) windowSize = (-1, -1);

    //--- Shorthand values ---
    public static int debugBufferStartIndex { get { return windowSize.height - 8; } } //used in reload
    public static int debugStartIndex { get { return consoleLines.Length - 8; } } //use this one

    /// --- Console Settings --- // This has like no effect on performance, but i dont like the console GetSettings Called (im crazy)

    public static int widgetTickInterval;
    public static int analyticTickInterval;
    public static double displayWindowsTickInterval;

    public static bool showDate;
    public static int maxWorkspacePathLength = 15;

    public static void InitalizeConsoleData()
    {
        SettingsApp.OnSettingChanged += OnSettingChange; //register setting change event

        OnSettingChange("Widget Update Frequency"); //set init value
        OnSettingChange("Analytics Update Frequency"); //set init value
        OnSettingChange("Target Frame Rate"); //set init value
        OnSettingChange("Workspace Path Widget Collapsing"); // set init value
    }

    static void OnSettingChange(string settingName)
    {
        switch (settingName)
        {
            case "Widget Update Frequency":
                widgetTickInterval = (int)(float.Parse(SettingsApp.GetValue("Widget Update Frequency")[..^1]) * 40);
                break;
            case "Analytics Update Frequency":
                analyticTickInterval = int.Parse(SettingsApp.GetValue("Analytics Update Frequency")[..^1]) * 40;
                break;
            case "Target Frame Rate":
                displayWindowsTickInterval = 1d / int.Parse(SettingsApp.GetValue("Target Frame Rate")) * Stopwatch.Frequency;
                break;
            case "Workspace Path Widget Collapsing":
                var val = SettingsApp.GetValue("Workspace Path Widget Collapsing");
                if (val == "No") maxWorkspacePathLength = -1;
                else maxWorkspacePathLength = int.Parse(val);
                break;

        }
    }
}
