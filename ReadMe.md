# Revistone
## What Is It?
Revistone, is a console based application, capable of managing and streamlining the creation of apps for the console, using **0** dependencies and libarys (excluding dotnet). Currently in development by me (Isaac) with the hopes to continue to build upon this system expanding it into a truly impressive project!
## Large Future Goals!
**HoneyC (Sudo Language):** Built in sudo language: somewhere between a caculator and its own language, capable of interacting with code in the console through it (Call it HoneyC) [0.5-0.6]

**Game Engine:** game enginge to easily create scenes and render quickly, very ambitious but i think this would make the project really cool, possibly **3D** in future [0.5-0.6]

**Complete Code Refactor:** complete code rewrite (in parts) and cleanup [0.6]

**GitHub Transfer:** move github repositry to restart commit history, as im improving at github, while preserving importnat versions [1.0]
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
Apps currently have 4 main ways to interact with the user:

**OnAppInitalisation()** which is called when the app is first loaded in the console, use this to display intro messages, or to deivate into your own console system (such as [here!](Scripts/App/CreatedApps/DebitCardApp.cs)), if you wish to have more control over the users input.

**OnUserPromt()** which is called just before the user is asked for input, being useful to promt or inform the users input.

**OnUserInput()** which is called after the user enters his input, allowing for easy response to user input.

**OnUpdate()** which is called every tick, allowing for realtime input and graphics!
