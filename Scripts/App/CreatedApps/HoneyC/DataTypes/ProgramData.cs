namespace Revistone.Apps.HoneyC;

/// <summary> Class responsible for holding current program data. </summary>
public static class ProgramData
{
    static int[] loopStartIndexes = [];
    static int[] loopEndIndexes = [];

    public static void Wipe()
    {
        loopStartIndexes = [];
        loopEndIndexes = [];
    }

    public static void SetLoopData(List<(int start, int end)> loops)
    {
        loopStartIndexes = [.. loops.Select(x => x.start)];
        loopEndIndexes = [.. loops.Select(x => x.end)];
    }

    public static bool IsScopeStart(int index) => loopStartIndexes.Contains(index);
    public static bool IsScopeEnd(int index) => loopEndIndexes.Contains(index);
}