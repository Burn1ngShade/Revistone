using System.Diagnostics;
using Revistone.App;
using Revistone.Console;
using Revistone.Functions;

using static Revistone.Console.ConsoleAction;
using static Revistone.Console.Data.ConsoleData;
using static Revistone.Functions.ColourFunctions;

namespace Revistone.Management;

/// <summary> Handles debugging of the console. </summary>
public static class Profiler
{
    public static bool Enabled { get; private set; }
    public static int Tps { get; private set; } // number of logic updates per second (Ticks per second)
    public static int Fps { get; private set; } // number of times console is rendered per second

    public static List<long> TickTime { get; private set; } = []; // total duration of a tick
    public static List<long> CalcTime { get; private set; } = []; // time for all methods following the event Tick to run

    public static List<long> RenderTime { get; private set; } = [];
    public static List<long> RenderLogicTime { get; private set; } = [];

    /// <summary> [DO NOT CALL] Initializes Profiler. </summary>
    internal static void InitializeProfiler()
    {
        Manager.Tick += ProfileBehaviour;
    }

    /// <summary> Updates state of profiler (visual only). </summary>
    public static void SetEnabled(bool state)
    {
        if (screenWarningUpdated) return;

        ClearDebugConsole();
        if (state)
        {
            Enabled = !state;
            SendDebugMessage(new ConsoleLine("Gathering Data...", AppRegistry.SecondaryCol));
        }
        Enabled = state;
    }

    /// <summary> Main loop for profile behaviour. </summary>
    static void ProfileBehaviour(int tickNum)
    {
        if (screenWarningUpdated) return;

        if (tickNum % 20 == 0)
        {
            if (TickTime.Count > 0) Tps = Math.Max((int)Math.Round(TickTime.Count / ((double)TickTime.Sum() / 1000)), 0);
            if (RenderTime.Count > 0) Fps = Math.Max((int)Math.Round(RenderTime.Count / ((double)RenderTime.Sum() / Stopwatch.Frequency)), 0);

            if (Enabled)
            {
                if (CalcTime.Count != 20 || TickTime.Count != 20) return;

                string[] formattedAverages = [
                    ((int)CalcTime.Average()).ToString().PadRight(2, ' '), ((int)TickTime.Average()).ToString().PadRight(2, ' ')
                    ];

                string calcTicks = $"Calc Ticks (ms): Avg - {formattedAverages[0]} | {CalcTime.ToElementString()} ";
                string compTicks = $"Comp Ticks (ms): Avg - {formattedAverages[1]} | {TickTime.ToElementString()}";

                UpdateDebugConsoleLine(new ConsoleLine($"[Profiler] FPS: {Fps}, TPS: {Tps}", BuildArray(AppRegistry.SecondaryCol.Extend(10), AppRegistry.PrimaryCol)), debugStartIndex + 1);
                UpdateDebugConsoleLine(new ConsoleLine($"Tick Num: {tickNum}, Lost Duration {CalcTime.Where(s => s > 25).Sum(s => s - 25)} ms, Total Duration: {Math.Round((double)CalcTime.Sum(), 2)} ms", AppRegistry.PrimaryCol), debugStartIndex + 2);
                UpdateDebugConsoleLine(new ConsoleLine(calcTicks, ColourTickInfo(calcTicks)), debugStartIndex + 3);
                UpdateDebugConsoleLine(new ConsoleLine(compTicks, ColourTickInfo(compTicks, 25, 30)), debugStartIndex + 4);
                UpdateDebugConsoleLine(new ConsoleLine(RenderLogicTime.Count == 0 ? "Render Mode - Single Threaded" : $"Render Frames (System Ticks): Avg - {RenderLogicTime.Sum() / RenderLogicTime.Count} | Target - {Stopwatch.Frequency / Fps}", AppRegistry.PrimaryCol), debugStartIndex + 5);
            }

            CalcTime.Clear();
            TickTime.Clear();
            RenderTime.Clear();
            RenderLogicTime.Clear();
        }

        if (Enabled && GetConsoleLine(debugLineIndex + 1).lineText.Length != 0) // stats that require more realtime updates
        {
            UpdateDebugConsoleLine(new ConsoleLine($"Window Width: {windowSize.width}, Window Height: {windowSize.height}", AppRegistry.PrimaryCol), debugStartIndex + 6);
            UpdateDebugConsoleLine(new ConsoleLine($"Primary Line Index: {primaryLineIndex}, Debug Line Index: {debugLineIndex}", AppRegistry.PrimaryCol), debugStartIndex + 7);
        }
    }

    /// <summary> Colours ticks according to their duration. </summary>
    static ConsoleColor[] ColourTickInfo(string ticks, int warningThreshold = 15, int errorThreshold = 25)
    {
        ConsoleColor[] colours = new ConsoleColor[ticks.Length];
        for (int i = 0; i < ticks.Length; i++)
        {
            if (colours[i] != ConsoleColor.Black) continue;

            string numberString = "";
            int k = 0;
            while (ticks.Length > k + i)
            {
                if (char.IsDigit(ticks[k + i]))
                {
                    numberString += ticks[k + i];
                    k++;
                }
                else break;
            }

            colours[i] = AppRegistry.PrimaryCol[0];
            for (int j = 0; j < numberString.Length; j++)
            {
                int tickDuration = int.Parse(numberString);
                colours[i + j] = tickDuration > errorThreshold ? ConsoleColor.Red : tickDuration > warningThreshold ? ConsoleColor.Yellow : AppRegistry.PrimaryCol[0];
            }
        }

        return colours;
    }
}
