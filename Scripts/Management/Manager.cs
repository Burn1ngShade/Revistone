using Revistone.Apps;
using Revistone.Console;
using Revistone.Console.Data;
using Revistone.Interaction;

namespace Revistone
{
    namespace Management
    {
        /// <summary>
        /// Main management class, handles initialization, Tick, and main interaction behaviour.
        /// </summary>
        public static class Manager
        {
            public static object renderLockObject = new object();

            public static int currentTick = 0;

            public static Random rnd = new Random();

            static Thread handleTickBehaviour = new Thread(HandleTickBehaviour);
            static Thread handleRealTimeInput = new Thread(UserRealtimeInput.KeyRegistry);

            public static event TickEventHandler Tick = new TickEventHandler((tickNum) => { });
            public delegate void TickEventHandler(int tickNum);

            /// <summary>
            /// Calls the Tick event, occours every 25ms (40 calls per seconds).
            /// </summary>
            static void HandleTickBehaviour() //controls tick based events
            {
                while (true)
                {
                    DateTime tickStartTime = DateTime.Now; //time tick starts
                    Tick.Invoke(currentTick);
                    Thread.Sleep(Math.Max(25 - (int)(DateTime.Now - tickStartTime).TotalMilliseconds, 0));
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

                System.Console.CursorVisible = false;
                ConsoleDisplay.InitializeConsoleDisplay();
                handleTickBehaviour.Start();
                
                handleRealTimeInput.Start();

                Thread.Sleep(50); //delay allows time for first tick to setup console
                HandleConsoleBehaviour();
            }
        }
    }
}