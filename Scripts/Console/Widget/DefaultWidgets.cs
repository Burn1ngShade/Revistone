using Revistone.App;
using Revistone.Functions;
using Revistone.Management;

using static Revistone.Console.ConsoleAction;
using static Revistone.Functions.ColourFunctions;

namespace Revistone.Console.Widget;

///<summary> Widget for carrying out a given function. </summary>
public class FunctionWidget : ConsoleWidget
{
    public Func<(string, bool)> function;

    public FunctionWidget(string name, int order, Func<(string content, bool shouldRemove)> function, string[] widgetEnabledApps, bool canRemove = true) : base(name, order, canRemove, widgetEnabledApps)
    {
        this.function = function;
    }

    public FunctionWidget(string name, int order, Func<(string content, bool shouldRemove)> function, bool canRemove = true) : this(name, order, function, [], canRemove) { }

    public override string GetContent(ref bool shouldRemove)
    {
        (string content, bool remove) = function.Invoke();
        shouldRemove = remove;
        return content;
    }
}

///<summary> Widget for displaying some form of time. </summary>
public class TimerWidget : ConsoleWidget // widget for displaying some form of time
{
    long duration; // timer tick duration
    long lastUpdate; // tick

    public bool paused = false;
    public bool stopwatch;

    public bool persistent = true; // if timer should be persistent across sesssions

    public TimerWidget(string name, int order, long duration, string[] widgetEnabledApps, bool canRemove = true, bool paused = false, bool stopwatch = false, bool persistent = true) : base(name, order, canRemove, widgetEnabledApps)
    {
        this.duration = duration;
        this.lastUpdate = Manager.ElapsedTicks;
        this.paused = paused;
        this.stopwatch = stopwatch;
        this.persistent = persistent;
    }

    public TimerWidget(string name, int order, long duration, bool canRemove = true, bool paused = false, bool stopwatch = false, bool persistent = true) : this(name, order, duration, [], canRemove, paused, stopwatch, persistent) { }

    public override string GetContent(ref bool shouldRemove)
    {
        if (!paused)
        {
            if (stopwatch) duration += Manager.ElapsedTicks - lastUpdate; // stop watch adds time.
            else duration -= Manager.ElapsedTicks - lastUpdate; // timer removes time.
            lastUpdate = Manager.ElapsedTicks;
        }

        if (duration <= 0 && !stopwatch)
        {
            shouldRemove = true;
            SoundFunctions.PlaySound("TimerEnd");
            SendDebugMessage(new ConsoleLine($"Timer Has Ended - '{name}'", BuildArray(AppRegistry.PrimaryCol.Extend(18), AppRegistry.SecondaryCol)));
            return $"{name}: 00:00:00";
        }

        TimeSpan time = TimeSpan.FromSeconds(duration / 40);
        return $"{name}: {time:hh\\:mm\\:ss}";
    }

    /// <summary> Create Timer Widget from time. </summary>
    public static void CreateTimer(string name, string time, bool stopwatch = false, int order = 100, bool persistent = true)
    {
        if (WidgetExists(name))
        {
            SendConsoleMessage(new ConsoleLine($"Timer Already Exists - '{name}'", BuildArray(AppRegistry.PrimaryCol.Extend(23), AppRegistry.SecondaryCol)));
            return;
        }

        if (!NumericalFunctions.TryParseTime(time, out double timerInSeconds))
        {
            SendConsoleMessage(new ConsoleLine("Invalid Time Format Use Format - hh:mm:ss.", BuildArray(AppRegistry.PrimaryCol.Extend(33), AppRegistry.SecondaryCol)));
            return;
        }

        if (timerInSeconds > 86400)
        {
            SendConsoleMessage(new ConsoleLine("Timer Can Not Exceed Duration - 24 Hours", BuildArray(AppRegistry.PrimaryCol.Extend(32), AppRegistry.SecondaryCol)));
            return;
        }

        TryAddWidget(new TimerWidget(name, order, (long)(timerInSeconds * 40), true, stopwatch: stopwatch, persistent: persistent));
        SendConsoleMessage(new ConsoleLine($"Timer Created - '{name}'", BuildArray(AppRegistry.PrimaryCol.Extend(16), AppRegistry.SecondaryCol)));
    }

    /// <summary> Delete timer widget. </summary>
    public static void CancelTimer(string name)
    {
        if (TryRemoveWidget(name)) SendConsoleMessage(new ConsoleLine($"Timer Cancelled - '{name}'", BuildArray(AppRegistry.PrimaryCol.Extend(18), AppRegistry.SecondaryCol)));
        else SendConsoleMessage(new ConsoleLine($"Timer Does Not Exist - '{name}'", BuildArray(AppRegistry.PrimaryCol.Extend(23), AppRegistry.SecondaryCol)));
    }

    ///<summary> Toggles timer pause status. </summary>
    public static void TogglePauseTimer(string name)
    {
        ConsoleWidget? consoleWidget = TryGetWidget(name);
        if (consoleWidget is TimerWidget timer)
        {
            timer.paused = !timer.paused;
            timer.lastUpdate = Manager.ElapsedTicks;
            SendConsoleMessage(new ConsoleLine($"Timer {(timer.paused ? "Paused" : "Unpaused")} - '{name}'", BuildArray(AppRegistry.PrimaryCol.Extend(timer.paused ? 15 : 17), AppRegistry.SecondaryCol)));
        }
        else SendConsoleMessage(new ConsoleLine($"Timer Does Not Exist - '{name}'", BuildArray(AppRegistry.PrimaryCol.Extend(23), AppRegistry.SecondaryCol)));
    }

    ///<summary> Adjust the duration of timer. </summary>
    public static void AdjustTimer(string name, string time)
    {
        ConsoleWidget? consoleWidget = TryGetWidget(name);
        if (consoleWidget is TimerWidget timer)
        {
            if (!NumericalFunctions.TryParseTime(time, out double timerInSeconds))
            {
                SendConsoleMessage(new ConsoleLine("Invalid Time Format Use Format - hh:mm:ss.", BuildArray(AppRegistry.PrimaryCol.Extend(33), AppRegistry.SecondaryCol)));
                return;
            }

            if (timerInSeconds + (timer.duration / 40) > 86400)
            {
                SendConsoleMessage(new ConsoleLine("Timer Can Not Exceed Duration - 24 Hours", BuildArray(AppRegistry.PrimaryCol.Extend(32), AppRegistry.SecondaryCol)));
                return;
            }

            timer.duration += (long)timerInSeconds * 40;
            timer.lastUpdate = Manager.ElapsedTicks;
            SendConsoleMessage(new ConsoleLine($"Timer Duration Modified - '{name}'", BuildArray(AppRegistry.PrimaryCol.Extend(26), AppRegistry.SecondaryCol)));

            if (timer.duration <= 0)
            {
                CancelTimer(name);
            }
        }
        else SendConsoleMessage(new ConsoleLine($"Timer Does Not Exist - '{name}'", BuildArray(AppRegistry.PrimaryCol.Extend(23), AppRegistry.SecondaryCol)));
    }

    ///<summary> Get A List Of Timers And There Remaining Duration. </summary>
    public static (string name, int order, long duration, bool paused, bool stopwatch)[] GetActiveTimerInfo()
    {
        List<(string name, int order, long duration, bool paused, bool stopwatch)> timers = [];

        foreach (ConsoleWidget w in widgets)
        {
            if (w is TimerWidget tw && tw.persistent) timers.Add((tw.name, tw.order, tw.duration, tw.paused, tw.stopwatch));
        }

        return [.. timers];
    }
}