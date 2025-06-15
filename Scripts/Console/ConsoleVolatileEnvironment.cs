using Revistone.App;
using Revistone.App.BaseApps;
using Revistone.Console.Widget;
using Revistone.Modules;
using Revistone.Management;

using static Revistone.Functions.PersistentDataFunctions;

namespace Revistone.Console;

///<summary> Class representing the current state of the console. </summary>
public class ConsoleVolatileEnvironment
{
    static readonly string savePath = GeneratePath(DataLocation.Console, "VolatileEnvironment.json");

    public (string name, int order, long duration, bool paused, bool stopWatch)[] ActiveTimers { get; set; } = [];
    public string WorkspacePath { get; set; } = "";
    public string OpenApp { get; set; } = "Revistone"; 

    ///<summary> Saves the current volatile console environment. </summary>
    public static bool TrySaveEnvironment()
    {
        ConsoleVolatileEnvironment environment = new()
        {
            ActiveTimers = TimerWidget.GetActiveTimerInfo(),
            WorkspacePath = Workspace.RawPath,
            OpenApp = AppRegistry.activeApp.name,
        };

        if (SaveFileAsJSON(savePath, environment, true))
        {
            DeveloperTools.Log("Volatile Environment Save - Success.");
            return true;
        }
        else
        {
            DeveloperTools.Log("Volatile Environment Save - Failed.");
            return false;
        }
    }

    ///<summary> Attempts to restore current volatile console environemt. </summary>
    public static bool TryRestoreEnvironment()
    {
        ConsoleVolatileEnvironment? environment = LoadFileFromJSON<ConsoleVolatileEnvironment>(savePath, false, true);

        if (environment == default)
        {
            DeveloperTools.Log("Volatile Environment Restore - Failed.");
            AppRegistry.SetActiveApp("Revistone");
            return false;
        }

        foreach ((string name, int order, long duration, bool paused, bool stopwatch) in environment.ActiveTimers)
        {
            ConsoleWidget.TryAddWidget(new TimerWidget(name, order, duration, paused: paused, stopwatch: stopwatch));
        }

        Workspace.UpdatePath(environment.WorkspacePath, false);

        if (SettingsApp.GetValueAsBool("Auto Resume")) AppRegistry.SetActiveApp(environment.OpenApp);
        else AppRegistry.SetActiveApp("Revistone");

        DeleteFile(savePath); // if no save is created prevents last save being rolled back to

        DeveloperTools.Log("Volatile Environment Restore - Success.");
        return true;
    }
}