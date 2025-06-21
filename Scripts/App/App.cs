using Revistone.App.Command;
using Revistone.Console;
using Revistone.Console.Image;
using Revistone.Functions;

namespace Revistone.App;

/// <summary> Base class for apps to inherit. </summary>
public abstract class App
{
    public string name;
    public string description;

    public (ConsoleColour[] primaryColour, ConsoleColour[] secondaryColour, ConsoleColour[] tertiaryColour) colourScheme;
    public (ConsoleColour[] colours, int speed) borderColourScheme;

    public int minWidthBuffer; //app only displays with atleast this width
    public int minHeightBuffer; //app only displays with atleast this height

    public int displayPriority = 0; //used to determine order of menu display
    public bool useBaseCommands; //whether or not base commands work
    public AppCommand[] appCommands = []; //custom commands

    //--- CONSTRUCTORS ---

    /// <summary> Base class for apps to inherit. </summary>
    public App(string name, string description, (ConsoleColour[] primaryColour, ConsoleColour[] secondaryColour, ConsoleColour[] tertiaryColour) colourScheme, (ConsoleColour[] colours, int speed) borderColourScheme, AppCommand[] appCommands, int minAppWidth = 30, int minAppHeight = 30, bool baseCommands = true, int displayPriority = 0)
    {
        this.name = name;
        this.description = description;

        this.colourScheme = colourScheme;
        this.borderColourScheme = borderColourScheme;

        this.minWidthBuffer = minAppWidth;
        this.minHeightBuffer = Math.Max(minAppHeight, 15);

        this.useBaseCommands = baseCommands;
        this.appCommands = appCommands;
        this.displayPriority = displayPriority;
    }

    /// <summary> Base class for apps to inherit. </summary>
    public App() : this("app", "default app.", (ConsoleColour.DarkBlue.ToArray(), ConsoleColour.Cyan.ToArray(), ConsoleColour.DarkCyan.ToArray()), ([], 5), []) { }

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
        AppCommandRegistry.Commands(userInput);
    }

    /// <summary> Called once a tick (25ms). </summary>
    public virtual void OnUpdate(int tickNum)
    {

    }

    /// <summary> Called when revistone app is first started, just after OnRegister. </summary>
    public virtual void OnRevistoneStartup()
    {

    }

    //--- Register ---

    /// <summary> Called on console startup, return all instances of class you want registered to console. </summary>
    public virtual App[] OnRegister()
    {
        return [];
    }

    //--- Useful Functions ---

    /// <summary> Sets active app to Revistone, and resets console (must return out of function after). </summary>
    public virtual void ExitApp()
    {
        AppRegistry.SetActiveApp("Revistone");
        ConsoleAction.ReloadConsole();
    }

    /// <summary> Called when console is closed, use to clean up app. </summary>
    public virtual void OnRevistoneClose()
    {
        
    }
}