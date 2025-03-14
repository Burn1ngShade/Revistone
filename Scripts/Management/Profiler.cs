using Revistone.Console;
using Revistone.Functions;

using static Revistone.Console.ConsoleAction;
using static Revistone.Console.Data.ConsoleData;

namespace Revistone.Management;

/// <summary> Handles debugging of the console. </summary>
public static class Profiler
{
    static bool _enabled = false;
    public static bool enabled { get { return _enabled; } }

    public static List<long> tickCompletionTime = new List<long>();
    public static List<long> tickCaculationTime = new List<long>();
    public static List<long> drawTime = new List<long>();

    public static int _fps = 40;
    public static int fps { get { return _fps; } }

    /// <summary> [DO NOT CALL] Initializes Profiler. </summary>
    internal static void InitializeProfiler()
    {
        Manager.Tick += ProfileBehaviour;
    }

    /// <summary> Updates state of profiler (visual only). </summary>
    public static void SetEnabled(bool state)
    {
        ClearDebugConsole();
        if (state) SendDebugMessage("Gathering Data...");
        _enabled = state;
    }

    /// <summary> Main loop for profile behaviour. </summary>
    static void ProfileBehaviour(int tickNum)
    {
        if (tickNum % 20 == 0)
        {
            if (tickCompletionTime.Count > 0) _fps = Math.Max((int)Math.Round(tickCompletionTime.Count / ((double)tickCompletionTime.Sum() / 1000)), 0);

            if (_enabled)
            {
                string[] formattedAverages = [
                    ((int)tickCaculationTime.Average()).ToString().PadRight(2, ' '), ((int)drawTime.Average()).ToString().PadRight(2, ' '), ((int)tickCompletionTime.Average()).ToString().PadRight(2, ' ')
                    ];
                    
                string calcTicks = $"Calc Ticks (ms): Avg - {formattedAverages[0]} | {tickCaculationTime.ToElementString()} ";
                string drawTicks = $"Draw Ticks (ms): Avg - {formattedAverages[1]} | {drawTime.ToElementString()}";
                string compTicks = $"Comp Ticks (ms): Avg - {formattedAverages[2]} | {tickCompletionTime.ToElementString()}";

                UpdateDebugConsoleLine(new ConsoleLine($"[Profiler] FPS: {_fps}"), debugStartIndex + 1);
                UpdateDebugConsoleLine(new ConsoleLine($"Tick Num: {tickNum}, Lost Duration {tickCaculationTime.Where(s => s > 25).Sum(s => s - 25)} ms, Total Duration: {Math.Round((double)tickCaculationTime.Sum(), 2)} ms, Draw Duration: {Math.Round((double)drawTime.Sum(), 2)} ms"), debugStartIndex + 2);
                UpdateDebugConsoleLine(new ConsoleLine(calcTicks, ColourTickInfo(calcTicks)), debugStartIndex + 3);
                UpdateDebugConsoleLine(new ConsoleLine(drawTicks, ColourTickInfo(drawTicks)), debugStartIndex + 4);
                UpdateDebugConsoleLine(new ConsoleLine(compTicks, ColourTickInfo(compTicks, 25, 30)), debugStartIndex + 5);
            }

            tickCaculationTime.Clear();
            tickCompletionTime.Clear();
            drawTime.Clear();
        }
    }

    /// <summary> Colours ticks according to there duratio. </summary>
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

            colours[i] = ConsoleColor.White;
            for (int j = 0; j < numberString.Length; j++)
            {
                int tickDuration = int.Parse(numberString);
                colours[i + j] = tickDuration > errorThreshold ? ConsoleColor.Red : tickDuration > warningThreshold ? ConsoleColor.Yellow : ConsoleColor.White;
            }
        }

        return colours;
    }
}
