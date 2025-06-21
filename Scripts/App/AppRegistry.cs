using System.Reflection;
using Revistone.Console.Image;
using Revistone.Console.Widget;
using Revistone.Management;

namespace Revistone.App;

/// <summary> Class pertaining the info and abilty to register app to the console. </summary>
public static class AppRegistry
{
    public static List<App> AppReg { get; private set; } = [];

    public static int ActiveAppIndex { get; private set; }
    public static App ActiveApp { get { return AppReg[ActiveAppIndex]; } }

    ///<summary> Primary colour of active app. </summary>
    public static ConsoleColour[] PrimaryCol => ActiveApp.colourScheme.primaryColour;
    ///<summary> Secondary colour of active app. </summary>
    public static ConsoleColour[] SecondaryCol => ActiveApp.colourScheme.secondaryColour;
    ///<summary> Tertiary colour of active app. </summary>
    public static ConsoleColour[] TertiaryColour => ActiveApp.colourScheme.tertiaryColour;

    // --- METHODS ---

    //unique name required to prevent user confusion, may switch to id system but will have to give an update to loading apps
    /// <summary> Register app to revistone console (name MUST be unique). </summary>
    public static bool RegisterApp(App app)
    {
        for (int i = 0; i < AppReg.Count; i++) if (AppReg[i].name == app.name) return false;

        AppReg.Add(app);
        return true;
    }

    /// <summary> Deregister app from revistone console. </summary>
    public static bool DeregisterApp(string appName)
    {
        for (int i = 0; i < AppReg.Count; i++)
        {
            if (AppReg[i].name != appName) continue;

            AppReg.RemoveAt(i);
            return true;
        }

        return false;
    }

    /// <summary> Sets active app to given index. </summary>
    public static bool SetActiveApp(int index)
    {
        if (index < 0 || index >= AppReg.Count) return false;

        Manager.Tick -= ActiveApp.OnUpdate;
        ActiveAppIndex = index;
        Manager.Tick += ActiveApp.OnUpdate;

        ConsoleWidget.UpdateWidgetHideInApp(ActiveApp.name);

        DeveloperTools.Log($"Loaded App: {ActiveApp.name}.");

        return true;
    }

    /// <summary> Sets active app to app of given name. </summary>
    public static bool SetActiveApp(string name)
    {
        for (int i = 0; i < AppReg.Count; i++)
        {
            if (AppReg[i].name.ToLower() == name.ToLower())
            {
                Analytics.App.TrackAppOpen(name);
                return SetActiveApp(i);
            }
        }

        return false;
    }

    /// <summary> Returns if app of given name is registered. </summary>
    public static bool AppExists(string name)
    {
        for (int i = 0; i < AppReg.Count; i++)
        {
            if (AppReg[i].name.ToLower() == name.ToLower()) return true;
        }

        return false;
    }

    /// <summary> [DO NOT CALL] Registers apps to console. </summary>
    internal static void InitializeAppRegistry()
    {
        // Get all types in the current assembly
        Type[] types = Assembly.GetExecutingAssembly().GetTypes();

        // Find types that are derived from BaseClass
        foreach (Type type in types)
        {
            if (type.IsSubclassOf(typeof(App)))
            {
                if (Activator.CreateInstance(type) == null) continue;

                // Call the method on the instance
                if (Activator.CreateInstance(type) is App instance)
                {
                    App[] apps = instance.OnRegister();
                    for (int i = 0; i < apps.Length; i++)
                    {
                        RegisterApp(apps[i]);
                    }
                }
            }
        }

        foreach (App app in AppReg)
        {
            app.OnRevistoneStartup();
        }
    }
}