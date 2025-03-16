using System.Diagnostics;
using Revistone.App;
using Revistone.Console;
using Revistone.Console.Data;
using Revistone.Console.Widget;
using Revistone.Functions;
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
    static Thread handleAnalytics = new Thread(Analytics.HandleAnalytics);

    public static event TickEventHandler Tick = new TickEventHandler((tickNum) => { });
    public delegate void TickEventHandler(int tickNum);

    public static int currentTick = 0;

    static long _deltaTime = 0; // duration of last tick in ms
    public static double deltaTime => _deltaTime / 1000d; // duration of last tick in seconds

    /// <summary>
    /// [DO NOT CALL] Calls the Tick event, occours every 25ms (40 calls per seconds).
    /// </summary>
    static void HandleTickBehaviour() //controls tick based events
    {
        Stopwatch tickStartTime = new Stopwatch(); //time tick starts
        Stopwatch tickSleepStart = new Stopwatch(); //tick delay start
        tickStartTime.Start();

        while (true)
        {
            Tick.Invoke(currentTick);
            Profiler.tickCaculationTime.Add(tickStartTime.ElapsedMilliseconds);

            tickSleepStart.Start();
            long targetThreadDelay = Math.Max(25 - (tickStartTime.ElapsedMilliseconds + Math.Max(_deltaTime - 25, 0)), 0);
            while (true)
            {
                if (tickSleepStart.ElapsedMilliseconds >= targetThreadDelay - 0.25) //0.25 is error miminmising
                {
                    tickSleepStart.Reset();
                    break;
                }
            }

            _deltaTime = tickStartTime.ElapsedMilliseconds;
            Profiler.tickCompletionTime.Add(_deltaTime);
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

            string userInput = UserInput.GetUserInput(maxLineCount: 3);

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
        // called first so init functions can still be tracked
        Analytics.InitalizeAnalytics();

        AppRegistry.InitializeAppRegistry();
        AppRegistry.SetActiveApp("Revistone");

        ConsoleWidget.InitializeWidgets();
        ConsoleRenderer.InitializeRenderer();
        ConsoleRendererLogic.InitializeConsoleRendererLogic();
        Profiler.InitializeProfiler();
        GPTFunctions.InitializeGPT();

        System.Console.CancelKeyPress += new ConsoleCancelEventHandler(OnCancelKeyPress); // prevent ctrl c from closing the program

        handleAnalytics.Start();
        handleTickBehaviour.Start();
        handleRealTimeInput.Start(); 

        HandleConsoleBehaviour();

    }

    static void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs args)
    {
        ConsoleAction.SendDebugMessage(new ConsoleLine("Console Close Prevented, If Attempting To Copy Text Use Alt+C Instead.", ConsoleColor.DarkBlue));

        args.Cancel = true;
    }
}