using Revistone.Console.Widget;
using Revistone.Management;

using static Revistone.Functions.PersistentDataFunctions;

namespace Revistone.Console;

///<summary> Class representing the current state of the console. </summary>
public class ConsoleVolatileEnvironment
{
    static readonly string savePath = GeneratePath(DataLocation.Console, "VolatileEnvironment.json");

    public (string name, int order, long duration, bool paused)[] ActiveTimers { get; set; } = [];

    ///<summary> Saves the current volatile console environment. </summary>
    public static bool TrySaveEnvironment()
    {
        ConsoleVolatileEnvironment environment = new();
        environment.ActiveTimers = TimerWidget.GetActiveTimerInfo();

        if (SaveFileAsJSON(savePath, environment, true))
        {
            Analytics.Debug.Log("Volatile Environment Save - Success.");
            return true;
        }
        else
        {
            Analytics.Debug.Log("Volatile Environment Save - Failed.");
            return false;
        }
    }

    ///<summary> Attempts to restore current volatile console environemt. </summary>
    public static bool TryRestoreEnvironment()
    {
        ConsoleVolatileEnvironment? environment = LoadFileFromJSON<ConsoleVolatileEnvironment>(savePath, false, true);

        if (environment == default)
        {
            Analytics.Debug.Log("Volatile Environment Restore - Failed.");
            return false;
        }

        foreach ((string name, int order, long duration, bool paused) in environment.ActiveTimers)
        {
            ConsoleWidget.TryAddWidget(new TimerWidget(name, order, duration, paused: paused));
        }

        DeleteFile(savePath); // if no save is created prevents last save being rolled back to

        Analytics.Debug.Log("Volatile Environment Restore - Success.");
        return true;
    }
}