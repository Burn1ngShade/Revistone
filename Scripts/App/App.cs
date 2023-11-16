using Revistone.Functions;
using Revistone.Console;
using Revistone.Interaction;
using Revistone.Console.Image;
using Revistone.Console.Data;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Data;

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
                this.minHeightBuffer = Math.Max(minAppHeight, 15);

                this.baseCommands = baseCommands;
                this.appCommands = appCommands;
            }

            /// <summary> Base class for apps to inherit. </summary>
            public App() : this("app", new ConsoleColor[0], new (UserInputProfile, Action<string>, string)[0]) { }

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

            //--- Register ---
            
            /// <summary> Called on console startup, return all instances of class you want registered to console. </summary>
            public virtual App[] OnRegister()
            {
                return new App[0];    
            }
        }
    }
}