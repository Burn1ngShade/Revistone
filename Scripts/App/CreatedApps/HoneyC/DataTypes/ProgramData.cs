namespace Revistone.App.BaseApps.HoneyC.Data;

/// <summary> Class responsible for holding running program data. </summary>
public static class ProgramData
{
    static int[] scopeStartIndexes = []; // lines that mark the start of loops
    static int[] scopeEndIndexes = []; // lines that mark the end of loops

    ///<summary> Wipes the data  </summary>
    public static void Wipe()
    {
        scopeStartIndexes = [];
        scopeEndIndexes = [];
    }

    ///<summary> Sets the start and end indexes for all scopes in a program.  </summary>
    public static void SetScopeData(List<(int start, int end)> loops)
    {
        scopeStartIndexes = [.. loops.Select(x => x.start)];
        scopeEndIndexes = [.. loops.Select(x => x.end)];
    }

    /// <summary> Checks if given lineIndex is the start of a scope. </summary>
    public static bool IsScopeStart(int lineIndex) => scopeStartIndexes.Contains(lineIndex);
    /// <summary> Checks if given lineIndex is the end of a scope. </summary>
    public static bool IsScopeEnd(int lineIndex) => scopeEndIndexes.Contains(lineIndex);
}