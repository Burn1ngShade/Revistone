using Revistone.Console;
using Revistone.Interaction;
using Revistone.Management;

namespace Revistone
{
    namespace Apps
    {
        /// <summary>
        /// Base class for apps to inherit.
        /// </summary>
        public abstract class App
        {
            public string name;

            public (ConsoleColor primaryColour, ConsoleColor[] secondaryColour, int speed) colourScheme;
            public (ConsoleColor[] colours, int speed) borderColourScheme;

            public int minWidthBuffer; //app only displays with atleast this width
            public int minHeightBuffer; //app only displays with atleast this height

            public bool baseCommands; //wether or not base commands work
            public (UserInputProfile format, Action<string> payload, string summary)[] appCommands = new (UserInputProfile, Action<string>, string summary)[] { }; //custom commands

            //--- CONSTRUCTORS ---

            /// <summary> Base class for apps to inherit. </summary>
            public App(string name, (ConsoleColor primaryColour, ConsoleColor[] secondaryColours, int speed) colourScheme, (ConsoleColor[] colours, int speed) borderColourScheme, (UserInputProfile format, Action<string> payload, string summary)[] appCommands, int minAppWidth = 30, int minAppHeight = 30, bool baseCommands = true)
            {
                this.name = name;

                this.colourScheme = colourScheme;
                this.borderColourScheme = borderColourScheme;

                this.minWidthBuffer = minAppWidth;
                this.minHeightBuffer = Math.Max(minAppHeight, 15);

                this.baseCommands = baseCommands;
                this.appCommands = appCommands;
            }

            /// <summary> Base class for apps to inherit. </summary>
            public App() : this("app", (ConsoleColor.DarkBlue, new ConsoleColor[0], 5), (new ConsoleColor[0], 5), new (UserInputProfile, Action<string>, string)[0]) { }

            //--- METHODS ---

            /// <summary> Called on app initalisation. </summary>
            public virtual void OnAppInitalisation()
            {

            }

            /// <summary> Called just before user is asked for input, use to interact with user. </summary>
            public virtual void OnUserPromt()
            {
                
            }

            /// <summary> Called after user input, use to respond to user input. </summary>
            public virtual void OnUserInput(string userInput)
            {
                AppCommands.Commands(userInput);
            }

            /// <summary> Called once a tick (25ms). </summary>
            public virtual void OnUpdate(int tickNum)
            {
                if (UserRealtimeInput.KeyPressed(0x11) && UserRealtimeInput.KeyPressed(0x10) && UserRealtimeInput.KeyPressedDown(80)) Profiler.SetEnabled(!Profiler.enabled);
            }

            //--- Register ---

            /// <summary> Called on console startup, return all instances of class you want registered to console. </summary>
            public virtual App[] OnRegister()
            {
                return new App[0];
            }

            //--- Useful Functions ---

            /// <summary> Sets active app to Revistone, and resets console (must return out of function after). </summary>
            public void ExitApp()
            {
                AppRegistry.SetActiveApp("Revistone");
                ConsoleAction.ReloadConsole();
            }
        }
    }
}