## Disclamer! Only works in vscode/fluent terminal (will look at)
# Revistone
## What Is It?
Revistone, is a console based application, capable of managing and streamlining the creation of apps for the console. Currently in development by me (Isaac) with the hopes to continue to build upon this system expanding it into a truly impressive project!
## Usage Guide
### Creating Apps
To create an app, create a new script within the project, preferabley [here!](Scripts/App/CreatedApps). All apps must use the follow the boiler plate below, where we create a class for the app, give it a empty constructor **[MUST]**, a constructor to actually create instances of the app, and an override to OnRegister(), where you return all instances of the app you wish to use in the console.

```
namespace Revistone
{
    namespace Apps
    {
        public class [NAME] : App
        {
            public [NAME] : base() {}
            public [NAME] (arg 1, arg 2...) : base(arg1, arg2...) {}

            public override App[] OnRegister()
            {
                return new [NAME][] { new [NAME](arg 1, arg 2...) }
            } 
        }
    }
}
```
