using System.Security.Cryptography.X509Certificates;
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

            static Thread handleTickBehaviour = new Thread(HandleTickBehaviour);

            public static event TickEventHandler Tick = new TickEventHandler((tickNum) => { });
            public delegate void TickEventHandler(int tickNum);

            /// <summary>
            /// Calls the Tick event, occours every 25ms (40 calls per seconds).
            /// </summary>
            static void HandleTickBehaviour() //controls tick based events
            {
                int tickNum = 0;

                while (true)
                {
                    Tick.Invoke(tickNum);
                    Thread.Sleep(25);
                    tickNum++;
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

                ConsoleDisplay.InitializeConsoleDisplay();
                handleTickBehaviour.Start();

                Thread.Sleep(50); //delay allows time for first tick to setup console
                HandleConsoleBehaviour();
            }
        }
    }
}