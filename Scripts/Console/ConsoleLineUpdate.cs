namespace Revistone.Console;

/// <summary>
/// The configuration for updating a ConsoleLine.
/// </summary>
public class ConsoleLineUpdate
{
    public bool newLine;
    public bool append;
    public bool timeStamp;

    /// <summary> The configuration for updating a ConsoleLine. </summary> 
    public ConsoleLineUpdate(bool newLine = true, bool append = false, bool timeStamp = false)
    {
        this.newLine = newLine;
        this.append = append;
        this.timeStamp = timeStamp;
    }

    /// <summary> Increments line number after update, default behaviour of ConsoleLineUpdate.</summary>
    public static ConsoleLineUpdate NewLine => new ConsoleLineUpdate();
    /// <summary> Does not increment line number after update.</summary>
    public static ConsoleLineUpdate SameLine => new ConsoleLineUpdate(newLine: false);
    /// <summary> Timestamps the line.</summary>
    public static ConsoleLineUpdate NewLineTimeStamped => new ConsoleLineUpdate(timeStamp: true);
    /// <summary> Appends the line onto current console line.</summary>
    public static ConsoleLineUpdate NewLineAppended => new ConsoleLineUpdate(append: true);

    /// <summary> Does not increment line number after update, and timestamps the line.</summary>
    public static ConsoleLineUpdate SameLineTimeStamped => new ConsoleLineUpdate(newLine: false, timeStamp: true);
    /// <summary> Does not increment line number after update, and appends onto current line.</summary>
    public static ConsoleLineUpdate SameLineAppended => new ConsoleLineUpdate(newLine: false, append: true);

    /// <summary> Appends the line onto current console line, and timestamps the line.</summary>
    public static ConsoleLineUpdate NewLineAppendedTimeStamped => new ConsoleLineUpdate(timeStamp: true, append: true);
    /// <summary> Does not increment line number after update, appends the line onto current console line, and timestamps the line.</summary>
    public static ConsoleLineUpdate SameLineAppendedTimeStamped => new ConsoleLineUpdate(timeStamp: true, append: true, newLine: false);
}
