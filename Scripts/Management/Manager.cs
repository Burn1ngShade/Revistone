using System.Diagnostics;
using Revistone.App;
using Revistone.App.BaseApps;
using Revistone.Console;
using Revistone.Console.Data;
using Revistone.Console.Widget;
using Revistone.Functions;
using Revistone.Interaction;

using static Revistone.Functions.PersistentDataFunctions;

namespace Revistone.Management;

/// <summary> Main management class, handles initialization, Tick, and main interaction behaviour. </summary>
public static class Manager
{
    public static readonly string ConsoleVersion = "0.8.0";
    public static readonly object ConsoleLock = new(); // lock for console interaction

    public static readonly Random rng = new();

    public static event TickEventHandler Tick = new((tickNum) => { });
    public delegate void TickEventHandler(int tickNum);
    public static int ElapsedTicks { get; private set; } = 0;

    static long _deltaTime = 0; // duration of last tick in ms
    public static double DeltaTime => _deltaTime / 1000d; // duration of last tick in seconds

    static Thread? mainThread; // main thread
    static readonly Thread tickBehaviourThread = new(HandleTickBehaviour); // tick thread
    static readonly Thread realTimeInputThread = new(UserRealtimeInput.KeyRegistry); // thread for input
    static readonly Thread renderThread = new(ConsoleRendererLogic.HandleConsoleRender); // thread for rendering

    public static readonly long[] threadCycles = new long[4]; // tracks thread cycles for each threaad DO NOT CALL
    public static readonly List<string>[] threadCheckpoints =
    [
        new(), // main thread
        new(), // tick behaviour thread
        new(), // real time input thread
        new()  // render thread
    ];

    public static readonly string[] consoleTips = LoadFile(GeneratePath(DataLocation.Console, "Assets/RevistoneTips.txt")); // tips to display to user
    public static string GetConsoleTip => consoleTips[rng.Next(0, consoleTips.Length)]; // get random tip

    /// <summary> [DO NOT CALL] Calls the Tick event, occours every 25ms (40 calls per seconds). </summary>
    static void HandleTickBehaviour()
    {
        Stopwatch tickDuration = new(); //time tick starts

        while (true)
        {
            threadCheckpoints[1].Clear();

            tickDuration.Restart();

            lock (ConsoleLock)
            {
                threadCheckpoints[1].Add("Tick Invoke Start");
                Tick.Invoke(ElapsedTicks); // invoke all tick event watchers
                threadCheckpoints[1].Add("Tick Invoke End");
            }

            Profiler.CalcTime.Add(tickDuration.ElapsedMilliseconds); // how long it took for all tick event to run

            long targetThreadDelay = (int)(Math.Max(25 - Math.Max(_deltaTime - 25, 0), 0) / 1000d * Stopwatch.Frequency); // how long to wait for next tick (25ms - delta time)
            while (true) if (tickDuration.ElapsedTicks >= targetThreadDelay) break;

            _deltaTime = tickDuration.ElapsedMilliseconds;
            Profiler.TickTime.Add(_deltaTime);
            ElapsedTicks++;

            threadCycles[1]++; // increment tick behaviour thread cycle count
        }
    }

    ///<summary> Returns list of tick listeners for debugging purposes. </summary>
    public static string[] GetTickListeners()
    {
        return [.. Tick.GetInvocationList().Select(x => x.Method.Name)];
    }

    public static string[] GetThreadStatus()
    {
        return [
            $"Main Thread: {mainThread?.ThreadState} - {threadCycles[0]} Cycles",
            "Check Points: " + string.Join(", ", threadCheckpoints[0]),
            $"Tick Behaviour Thread: {tickBehaviourThread.ThreadState} - {threadCycles[1]} Cycles",
            "Check Points: " + string.Join(", ", threadCheckpoints[1]),
            $"Real Time Input Thread: {realTimeInputThread.ThreadState} - {threadCycles[2]} Cycles",
            "Check Points: " + string.Join(", ", threadCheckpoints[2]),
            $"Render Thread: {renderThread.ThreadState} - {threadCycles[3]} Cycles",
            "Check Points: " + string.Join(", ", threadCheckpoints[3])
        ];
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

            threadCycles[0]++; // increment main thread cycle count
        }
    }

    // Entry point of the entire program, initalizes all systems
    public static void Main(string[] args)
    {
        Analytics.InitalizeAnalytics(); // called to track start up process
        DeveloperTools.Log("Console Process Start.");

        AppRegistry.InitializeAppRegistry(); // init all apps

        ConsoleWidget.InitializeWidgets(); // init border widgets
        ConsoleRenderer.InitializeRenderer(); // init rendering
        ConsoleRendererLogic.InitializeConsoleRendererLogic(); // init rendering pt2
        Profiler.InitializeProfiler(); // init fps tracking (profiler)
        GPTFunctions.InitializeGPT(); // init gpt
        ConsoleData.InitalizeConsoleData(); // init some console setting values

        mainThread = Thread.CurrentThread; // set main thread
        tickBehaviourThread.Start();
        realTimeInputThread.Start();
        renderThread.Start();

        if (!SettingsApp.GetValueAsBool("Advanced Session Log", "Off"))
        {
            Thread debugThread = new(DeveloperTools.HandleAdvancedSessionLog);
            debugThread.Start();
        }
        else
        {
            DeleteFile(DeveloperTools.AdvancedSessionLog.advancedSessionLogPath); // delete advanced session log from last session if not enabled
        }

        System.Console.CancelKeyPress += new ConsoleCancelEventHandler(OnCancelKeyPress); // prevent ctrl c from closing the program
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit; // on user close
        AppDomain.CurrentDomain.UnhandledException += OnProcessCrash; // on crash

        ConsoleVolatileEnvironment.TryRestoreEnvironment();

        DeveloperTools.Log("Console Process Initialization Complete.");
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
        DeveloperTools.Log($"Console Process Exit.");
        Analytics.HandleAnalytics();
        DeveloperTools.SessionLog.Update(); // save session log
    }

    ///<summary> Called upon crash of the console. </summary>
    static void OnProcessCrash(object sender, UnhandledExceptionEventArgs e)
    {
        ConsoleVolatileEnvironment.TryRestoreEnvironment();

        Analytics.General.LastCloseDate = DateTime.Now;
        DeveloperTools.Log($"Console Unexpected (Crash D:) Process Exit.\n Crash Message: {e.ExceptionObject}", true);
        Analytics.HandleAnalytics();
        DeveloperTools.SessionLog.Update(); // save session log
    }
}