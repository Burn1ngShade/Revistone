using Revistone.App;

namespace Revistone.Console.Widget;

///<summary> Widget for carrying out a given function. </summary>
public class FunctionWidget : ConsoleWidget
{
    public Func<(string, bool)> function;

    public FunctionWidget(string name, int order, Func<(string content, bool shouldRemove)> function, string[] widgetEnabledApps, bool canRemove = true) : base(name, order, canRemove, widgetEnabledApps)
    {
        this.function = function;
    }

    public FunctionWidget(string name, int order, Func<(string content, bool shouldRemove)> function, bool canRemove = true) : this(name,order, function, [],canRemove) {}

    public override string GetContent(ref bool shouldRemove)
    {
        (string content, bool remove) = function.Invoke();
        shouldRemove = remove;
        return content;
    }
}

///<summary> Widget for displaying some form of time. </summary>
public class TimeWidget : ConsoleWidget // widget for displaying some form of time
{
    public DateTime time; // time to display
    public bool timeUntil; // should the widget display time until the given time
    public bool isNow; // returns the current time

    public TimeWidget(string name, int order, DateTime time, string[] widgetEnabledApps, bool timeUntil = false, bool isNow = false, bool canRemove = true) : base(name, order, canRemove, widgetEnabledApps)
    {
        this.time = time;
        this.timeUntil = timeUntil;
        this.isNow = isNow;
    }

    public TimeWidget(string name, int order, DateTime time, bool timeUntil = false, bool isNow = false, bool canRemove = true) : this(name, order, time, [], timeUntil, isNow, canRemove) {}

    public override string GetContent(ref bool shouldRemove)
    {
        if (isNow) return DateTime.Now.ToString("HH:mm:ss");

        if (timeUntil)
        {
            TimeSpan ts = time - DateTime.Now;
            if (ts.TotalSeconds < 0)
            {
                shouldRemove = true;
                ConsoleAction.SendDebugMessage(new ConsoleLine("Timer Has Ended.", AppRegistry.PrimaryCol));
                return "Timer: 00:00:00";
            }

            return $"Timer: {ts:hh\\:mm\\:ss}";
        }

        return time.ToString("HH:mm:ss");
    }
}