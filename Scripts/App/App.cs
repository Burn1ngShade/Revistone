using Revistone.Functions;

namespace Revistone
{
    namespace Apps
    {
        public abstract class App
        {
            static App[] _appRegistry = {
                new RevistoneApp("Revistone", ColourCreator.CreateColourGradient(new ConsoleColor[] { ConsoleColor.Cyan, ConsoleColor.DarkCyan, ConsoleColor.Blue, ConsoleColor.DarkBlue }, System.Console.WindowWidth), 5),
                new DebitCardApp("Debit Card Manager", ColourCreator.AlternateColours(new ConsoleColor[] { ConsoleColor.Magenta, ConsoleColor.DarkBlue }, System.Console.WindowWidth, 3), 5) };

            static int _activeAppIndex = 0;
            public static int activeAppIndex { get {return _activeAppIndex; }}
            
            public static App activeApp { get { return _appRegistry[_activeAppIndex]; } }

            public string name;
            public int appState = 0; //if app has different layers or modes

            public ConsoleColor[] colours;
            public int colourSpeed;

            //--- CONSTRUCTORS ---

            public App(string name)
            {
                this.name = name;
                this.colours = new ConsoleColor[] { ConsoleColor.White };
                this.colourSpeed = 5;
            }

            public App(string name, ConsoleColor[] colourScheme, int colourSpeed = 5)
            {
                this.name = name;
                this.colours = colourScheme;
                this.colourSpeed = colourSpeed;
            }

            //--- METHODS ---

            public virtual void HandleUserPromt()
            {
                
            }

            public virtual void HandleUserInput(string userInput)
            {
                AppCommands.BaseCommands(userInput);
            }

            //--- STATIC METHODS ---

            public static bool SetActiveApp(int index)
            {
                if (index < 0 || index >= _appRegistry.Length) return false;

                _activeAppIndex = index;
                return true;
            }

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