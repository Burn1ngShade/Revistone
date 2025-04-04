using System.Reflection;
using Revistone.Console.Widget;
using Revistone.Management;

namespace Revistone.App;

/// <summary> Class pertaining the info and abilty to register app to the console. </summary>
public static class AppRegistry
{
    //must have Revistone assigned here so its first in list and loaded by default
    public static List<App> _appRegistry = new List<App>() { };
    public static List<App> appRegistry { get { return _appRegistry; } }

    static int _activeAppIndex = 0;
    public static int activeAppIndex { get { return _activeAppIndex; } }

    public static App activeApp { get { return _appRegistry[_activeAppIndex]; } }
    ///<summary> Primary colour of active app. </summary>
    public static ConsoleColor[] PrimaryCol => activeApp.colourScheme.primaryColour;
    ///<summary> Secondary colour of active app. </summary>
    public static ConsoleColor[] SecondaryCol => activeApp.colourScheme.secondaryColour;
    ///<summary> Tertiary colour of active app. </summary>
    public static ConsoleColor[] TertiaryColour => activeApp.colourScheme.tertiaryColour;

    // --- METHODS ---

    //unique name required to prevent user confusion, may switch to id system but will have to give an update to loading apps
    /// <summary> Register app to revistone console (name MUST be unique). </summary>
    public static bool RegisterApp(App app)
    {
        for (int i = 0; i < _appRegistry.Count; i++) if (_appRegistry[i].name == app.name) return false;

        _appRegistry.Add(app);
        return true;
    }

    /// <summary> Deregister app from revistone console. </summary>
    public static bool DeregisterApp(string appName)
    {
        for (int i = 0; i < _appRegistry.Count; i++)
        {
            if (_appRegistry[i].name != appName) continue;

            _appRegistry.RemoveAt(i);
            return true;
        }

        return false;
    }

    /// <summary> Sets active app to given index. </summary>
    public static bool SetActiveApp(int index)
    {
        if (index < 0 || index >= _appRegistry.Count) return false;

        Manager.Tick -= activeApp.OnUpdate;
        _activeAppIndex = index;
        Manager.Tick += activeApp.OnUpdate;

        ConsoleWidget.UpdateWidgetHideInApp(activeApp.name);

        Analytics.Debug.Add($"Loaded App: {activeApp.name}.");

        return true;
    }

    /// <summary> Sets active app to app of given name. </summary>
    public static bool SetActiveApp(string name)
    {
        for (int i = 0; i < _appRegistry.Count; i++)
        {
            if (_appRegistry[i].name.ToLower() == name.ToLower())
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
        for (int i = 0; i < _appRegistry.Count; i++)
        {
            if (_appRegistry[i].name.ToLower() == name.ToLower()) return true;
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

        foreach (App app in _appRegistry)
        {
            app.OnRevistoneStartup();
        }
    }
}