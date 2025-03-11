using Revistone.Apps;
using Revistone.Management;

namespace Revistone.Console.Widget;

///<summary> Widget for displaying current FPS. </summary>
public class FrameWidget : ConsoleWidget // displays the current frames per second
{
    public FrameWidget(string name, uint order, bool canRemove = true) : base(name, order, canRemove) { }

    public override string GetContent(ref bool shouldRemove)
    {
        return $"FPS: {Profiler.fps}";
    }
}

///<summary> Widget for displaying some form of time. </summary>
public class TimeWidget : ConsoleWidget // widget for displaying some form of time
{
    public DateTime time; // time to display
    public bool timeUntil; // should the widget display time until the given time
    public bool isNow; // returns the current time

    public TimeWidget(string name, uint order, DateTime time, bool timeUntil = false, bool isNow = false, bool canRemove = true) : base(name, order, canRemove)
    {
        this.time = time;
        this.timeUntil = timeUntil;
        this.isNow = isNow;
    }

    public override string GetContent(ref bool shouldRemove)
    {
        if (isNow) return DateTime.Now.ToString("HH:mm:ss");

        if (timeUntil)
        {
            TimeSpan ts = time - DateTime.Now;
            if (ts.TotalSeconds < 0)
            {
                shouldRemove = true;
                ConsoleAction.SendDebugMessage(new ConsoleLine("Timer Has Ended.", AppRegistry.activeApp.colourScheme.primaryColour));
                return "Timer: 00:00:00";
            }

            return $"Timer: {ts:hh\\:mm\\:ss}";
        }

        return time.ToString("HH:mm:ss");
    }
}

///<summary> Widget for displaying a message. </summary>
public class BillboardWidget : ConsoleWidget
{
    public string message;

    public BillboardWidget(string name, uint order, string message, bool canRemove = true) : base(name, order, canRemove)
    {
        this.message = message;
    }

    public override string GetContent(ref bool shouldRemove)
    {
        return message;
    }
}