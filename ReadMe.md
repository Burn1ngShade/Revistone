# Revistone
## What Is It?
Revistone, is a console based application, capable of managing and streamlining the creation of apps for the console. Currently in development by me (Isaac) with the hopes to continue to build upon this system expanding it into a truly impressive project!
## Usage Guide
### Creating Apps
To create an app, create a new script within the project, preferabley [here!](Scripts/App/CreatedApps). All apps must use the follow the boiler plate below, where we create a class for the app, give it a empty constructor **[MUST]**, a constructor to actually create instances of the app, and an override to OnRegister(), where you return all instances of the app you wish to use in the console.

```C#
namespace Revistone
{
    namespace Apps
    {
        public class NewApp : App
        {
            public NewApp : base() {}
            public NewApp (string name) : base(name, new ConsoleColor[0], new (UserInputProfile, Action<string>, string)[0]) {}

            public override App[] OnRegister()
            {
                return new NewApp[] { new NewApp("New App") }
            } 
        }
    }
}
```
### App Behaviour
Apps currently have 3 main ways to interact with the user:

**OnAppInitalisation()** which is called when the app is first loaded in the console, use this to display intro messages, or to deivate into your own console system (such as [here!](Scripts/App/CreatedApps/DebitCardApp.cs)), if you wish to have more control over the users input.

**OnUserPromt()** which is called just before the user is asked for input, being useful to promt or inform the users input.

**OnUserInput()** which is called after the user enters his input, allowing for easy response to user input.
