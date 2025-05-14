# Revistone
**Experiencing performance issues? Ensure that an exception has been added for antivirus software.**
## What Is It?
Revistone, is a console based application, capable of managing and streamlining the creation of apps for the console, using **0** dependencies and libarys (excluding dotnet). Currently in development by me (Isaac) with the hopes to continue to build upon this system expanding it into a truly impressive project!
## Future Goals!
**HoneyC (Sudo Language):** Complete and advance the HoneyC language into a fully completed weakly typed, OOP language. This will eventually be spun into it's own project, so dependencies are kept to a minimal, for rendering purposes only. 

**Spotify Integration:** Integration within in the console in the form of commands and GPT interaction, allowing spotify to be fully useable from within the console.

**Graphic Improvements:** Since seperating rendering into its own thread, I now want to focus on adding > 16 colours to the console, as well as easy and complete support for rendering square pixels. 

## Usage Guide
### Creating Apps
To create an app, create a new script within the project, preferabley [here!](Scripts/App/CreatedApps). All apps must use the follow the boiler plate below, where we create a class for the app, give it a empty constructor **[MUST]**, a constructor to actually create instances of the app, and an override to OnRegister(), where you return all instances of the app you wish to use in the console.

```C#
namespace Revistone.App;

public class NewApp : App
{
    public NewApp : base() {}
    public NewApp (string name) : base(name, ([ConsoleColour.Blue], [ConsoleColour.DarkBlue], [ConsoleColour.Magenta], 5), ([ConsoleColour.Blue], [ConsoleColour.DarkBlue], [ConsoleColour.Magenta], 5), []) {}

    public override App[] OnRegister()
    {
        return new NewApp[] { new NewApp("New App") }
    } 
}
```
### App Behaviour
Apps currently have 5 main ways to interact with the user:

**OnAppInitalisation()** which is called when the app is first loaded in the console, use this to display intro messages, or to deivate into your own console system (such as [here!](Scripts/App/CreatedApps/DebitCardApp.cs)), if you wish to have more control over the users input.

**OnUserPromt()** which is called just before the user is asked for input, being useful to promt or inform the users input.

**OnUserInput()** which is called after the user enters his input, allowing for easy response to user input.

**OnUpdate()** which is called every tick, allowing for realtime input and graphics!

**OnRevistoneStartup()** called on inital console startup.

### Creating Widgets
New to Revistone version 0.6.1, widgets are custom features that appear on the border of the application, regardless of application. To create a widget either use a base widget found [here!](Scripts/Console/Widget/DefaultWidgets.cs), or create a custom widget using the template below.

```C#
namespace Revistone.Console.Widget;

public class NewWidget : Widget
{
    public NewWidget(string name, int order, string[] widgetEnabledApps, bool canRemove = true) : base(name, order, widgetEnabledApps, canRemove) { }

    public override string GetContent(ref bool shouldRemove)
    {
        return "Your Custom Logic Here!";
    }
}
```
