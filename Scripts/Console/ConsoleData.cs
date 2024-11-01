namespace Revistone.Console.Data;

/// <summary> Class pertaining all the data of the console (should NOT be used by user). </summary>
internal static class ConsoleData
{
    //--- Console Lines ---
    public static ConsoleLine[] consoleLines = new ConsoleLine[] { }; //current state of console lines
    public static ConsoleLine[] consoleLinesBuffer = new ConsoleLine[] { }; //last tick state of console lines
    public static ConsoleAnimatedLine[] consoleLineUpdates = new ConsoleAnimatedLine[] { }; //animation data of console lines

    public static bool[] exceptionLines = new bool[] { }; //prevent certain methods effecting line

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
}
