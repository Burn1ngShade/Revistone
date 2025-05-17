using System.Collections.Immutable;
using System.Diagnostics;
using Revistone.App;
using Revistone.Console;
using Revistone.Console.Data;
using Revistone.Console.Widget;
using Revistone.Functions;
using Revistone.Interaction;

namespace Revistone.Management;

/// <summary> Main management class, handles initialization, Tick, and main interaction behaviour. </summary>
public static class Manager
{
    public static readonly string ConsoleVersion = "0.8.0";

    public static readonly object renderLockObject = new();
    public static readonly Random rng = new();

    public static event TickEventHandler Tick = new((tickNum) => { });
    public delegate void TickEventHandler(int tickNum);
    public static int ElapsedTicks { get; private set; } = 0;

    static long _deltaTime = 0; // duration of last tick in ms
    public static double DeltaTime => _deltaTime / 1000d; // duration of last tick in seconds

    static readonly Thread tickBehaviourThread = new(HandleTickBehaviour); // tick thread
    static readonly Thread realTimeInputThread = new(UserRealtimeInput.KeyRegistry); // thread for input
    static readonly Thread renderThread = new(ConsoleRendererLogic.HandleConsoleRender); // thread for rendering

    /// <summary> [DO NOT CALL] Calls the Tick event, occours every 25ms (40 calls per seconds). </summary>
    static void HandleTickBehaviour()
    {
        Stopwatch tickDuration = new(); //time tick starts

        while (true)
        {
            tickDuration.Restart();

            Tick.Invoke(ElapsedTicks); // invoke all tick event watchers
            Profiler.CalcTime.Add(tickDuration.ElapsedMilliseconds); // how long it took for all tick event to run

            long targetThreadDelay = (int)(Math.Max(25 - Math.Max(_deltaTime - 25, 0), 0) / 1000d * Stopwatch.Frequency); // how long to wait for next tick (25ms - delta time)
            while (true) if (tickDuration.ElapsedTicks >= targetThreadDelay) break;

            _deltaTime = tickDuration.ElapsedMilliseconds;
            Profiler.TickTime.Add(_deltaTime);
            ElapsedTicks++;
        }
    }

    public static string[] GetTickListeners()
    {
        return [.. Tick.GetInvocationList().Select(x => x.Method.Name)];
    }

    /// <summary> Handles default behaviour of user interaction, can be interrupted via ConsoleInteraction methods.  </summary>
    static void HandleConsoleBehaviour()
    {
        while (true)
        {
            if (ConsoleData.consoleReload) continue;

            if (ConsoleData.appInitalisation) // load new app
            {
                Profiler.SetEnabled(Profiler.Enabled);
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

    // Entry point of the entire program, initalizes all systems
    public static void Main(string[] args)
    {
        Analytics.InitalizeAnalytics(); // called to track start up process
        Analytics.Debug.Log("Console Process Start.");

        AppRegistry.InitializeAppRegistry(); // init all apps
        AppRegistry.SetActiveApp("Revistone");

        ConsoleWidget.InitializeWidgets(); // init border widgets
        ConsoleRenderer.InitializeRenderer(); // init rendering
        ConsoleRendererLogic.InitializeConsoleRendererLogic(); // init rendering pt2
        Profiler.InitializeProfiler(); // init fps tracking (profiler)
        GPTFunctions.InitializeGPT(); // init gpt
        ConsoleData.InitalizeConsoleData(); // init some console setting values

        tickBehaviourThread.Start();
        realTimeInputThread.Start();
        renderThread.Start();

        System.Console.CancelKeyPress += new ConsoleCancelEventHandler(OnCancelKeyPress); // prevent ctrl c from closing the program
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit; // on user close
        AppDomain.CurrentDomain.UnhandledException += OnProcessCrash; // on crash

        ConsoleVolatileEnvironment.TryRestoreEnvironment();

        Analytics.Debug.Log("Console Process Initialization Complete.");
        HandleConsoleBehaviour(); // main console loop
    }

    // --- ON APPLICATION CLOSE ---

    ///<summary> Called upon attempted close keybind Ctrl+C. </summary>
    static void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs args)
    {
        ConsoleAction.SendDebugMessage(new ConsoleLine("Console Close Prevented, If Attempting To Copy Text Use Alt+C Instead.", AppRegistry.PrimaryCol));
        args.Cancel = true;
    }

    ///<summary> Called upon standard close of the console. </summary>
    static void OnProcessExit(object? sender, EventArgs e)
    {
        ConsoleVolatileEnvironment.TrySaveEnvironment();

        Analytics.General.LastCloseDate = DateTime.Now;
        Analytics.Debug.Log($"Console Process Exit.");
        Analytics.HandleAnalytics();
    }

    ///<summary> Called upon crash of the console. </summary>
    static void OnProcessCrash(object sender, UnhandledExceptionEventArgs e)
    {
        ConsoleVolatileEnvironment.TryRestoreEnvironment();

        Analytics.General.LastCloseDate = DateTime.Now;
        Analytics.Debug.Log($"Console Unexpected (Crash D:) Process Exit.\n Crash Message: {e.ExceptionObject}");
        Analytics.HandleAnalytics();
    }
}