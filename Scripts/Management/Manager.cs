using System.Diagnostics;
using Revistone.Apps;
using Revistone.Console;
using Revistone.Console.Data;
using Revistone.Interaction;

namespace Revistone.Management;

/// <summary>
/// Main management class, handles initialization, Tick, and main interaction behaviour.
/// </summary>
public static class Manager
{
    public static object renderLockObject = new object();

    public static Random rng = new Random();

    static Thread handleTickBehaviour = new Thread(HandleTickBehaviour);
    static Thread handleRealTimeInput = new Thread(UserRealtimeInput.KeyRegistry);

    public static event TickEventHandler Tick = new TickEventHandler((tickNum) => { });
    public delegate void TickEventHandler(int tickNum);

    public static int currentTick = 0;

    /// <summary>
    /// Calls the Tick event, occours every 25ms (40 calls per seconds).
    /// </summary>
    static void HandleTickBehaviour() //controls tick based events
    {
        Stopwatch tickStartTime = new Stopwatch(); //time tick starts
        Stopwatch tickSleepStart = new Stopwatch(); //tick delay start
        tickStartTime.Start();

        long lastTickTime = 0;

        while (true)
        {
            Tick.Invoke(currentTick);
            Profiler.tickCaculationTime.Add(tickStartTime.ElapsedMilliseconds);

            tickSleepStart.Start();
            long targetThreadDelay = Math.Max(25 - (tickStartTime.ElapsedMilliseconds + Math.Max(lastTickTime - 25, 0)), 0);
            while (true)
            {
                if (tickSleepStart.ElapsedMilliseconds >= targetThreadDelay - 0.25) //0.25 is error miminmising
                {
                    tickSleepStart.Reset();
                    break;
                }
            }

            lastTickTime = tickStartTime.ElapsedMilliseconds;
            Profiler.tickCompletionTime.Add(lastTickTime);
            tickStartTime.Restart();
            currentTick++;
        }
    }

    /// <summary>
    /// Handles default behaviour of user interaction, can be interrupted via ConsoleInteraction methods. 
    /// </summary>
    static void HandleConsoleBehaviour()
    {
        while (true)
        {
            if (ConsoleData.consoleReload) continue;

            if (ConsoleData.appInitalisation)
            {
                Profiler.SetEnabled(Profiler.enabled);
                AppRegistry.activeApp.OnAppInitalisation();
                ConsoleData.appInitalisation = false;
            }

            AppRegistry.activeApp.OnUserPromt();

            if (ConsoleData.consoleReload) continue;

            string userInput = UserInput.GetUserInput(clear: false);

            if (userInput != "")
            {
                ConsoleAction.ShiftLine();
                AppRegistry.activeApp.OnUserInput(userInput);
            }
        }
    }

    //Initalizes main and tick threads, aswell as setting up consoleDisplay
    public static void Main(string[] args)
    {
        AppRegistry.InitializeAppRegistry();
        AppRegistry.SetActiveApp("Revistone");

        ConsoleRenderer.InitializeRenderer();
        ConsoleRendererLogic.InitializeConsoleRendererLogic();
        Profiler.InitializeProfiler();

        System.Console.CancelKeyPress += new ConsoleCancelEventHandler(OnCancelKeyPress); // prevent ctrl c from closing the program

        handleTickBehaviour.Start();
        handleRealTimeInput.Start();

        HandleConsoleBehaviour();
    }

    static void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs args)
    {
        ConsoleAction.SendDebugMessage(new ConsoleLine("Console Close Prevented, If Attempting To Copy Text Use Shift+Alt+C Instead.", ConsoleColor.DarkBlue));

        args.Cancel = true;
    }
}