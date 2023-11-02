using Revistone.Functions;
using Revistone.Console;
using Revistone.Interaction;

namespace Revistone
{
    namespace Apps
    {
        /// <summary>
        /// Base class for apps to inherit.
        /// </summary>
        public abstract class App
        {
            /// <summary> Holds all apps recognised by Console, add app here to use. </summary>
            static App[] _appRegistry = {
                new RevistoneApp("Revistone",
                ColourFunctions.ColourGradient(new ConsoleColor[] { ConsoleColor.Cyan, ConsoleColor.DarkCyan, ConsoleColor.Blue, ConsoleColor.DarkBlue },
                System.Console.WindowWidth),
                new (UserInputProfile format, Action<string> payload, string summary)[] {}, 5),

                new DebitCardApp("Debit Card Manager", ColourFunctions.AlternatingColours(new ConsoleColor[] { ConsoleColor.Magenta, ConsoleColor.DarkBlue }, System.Console.WindowWidth, 3),
                new (UserInputProfile format, Action<string> payload, string summary)[] {}, 5)
                };

            public static App[] appRegistry { get { return _appRegistry; } }

            static int _activeAppIndex = 0;
            public static int activeAppIndex { get { return _activeAppIndex; } }

            public static App activeApp { get { return _appRegistry[_activeAppIndex]; } }

            public string name;
            public int appState = 0; //if app has different layers or modes

            public ConsoleColor[] borderColours;
            public int borderColourSpeed;

            public int minWidthBuffer; //app only displays with atleast this width
            public int minHeightBuffer; //app only displays with atleast this height

            public bool baseCommands; //wether or not base commands work
            public (UserInputProfile format, Action<string> payload, string summary)[] appCommands = new (UserInputProfile, Action<string>, string summary)[] { }; //custom commands

            //--- CONSTRUCTORS ---

            /// <summary> Base class for apps to inherit. </summary>
            public App(string name, ConsoleColor[] borderColours, (UserInputProfile format, Action<string> payload, string summary)[] appCommands, int borderColourSpeed = 5, int minAppWidth = 30, int minAppHeight = 30, bool baseCommands = true)
            {
                this.name = name;
                this.borderColours = borderColours;
                this.borderColourSpeed = borderColourSpeed;

                this.minWidthBuffer = minAppWidth;
                this.minHeightBuffer = minAppHeight;

                this.baseCommands = baseCommands;
                this.appCommands = appCommands;
            }

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

            //--- STATIC METHODS ---

            /// <summary> Sets active app to given index. </summary>
            public static bool SetActiveApp(int index)
            {
                if (index < 0 || index >= _appRegistry.Length) return false;

                _activeAppIndex = index;
                return true;
            }

            /// <summary> Sets active app to app of given name. </summary>
            public static bool SetActiveApp(string name)
            {
                for (int i = 0; i < _appRegistry.Length; i++)
                {
                    if (_appRegistry[i].name.ToLower() == name.ToLower()) return SetActiveApp(i);
                }

                return false;
            }
        }
    }
}