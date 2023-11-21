using System.Reflection;

using Revistone.Functions;
using Revistone.Interaction;
using Revistone.Console;

namespace Revistone
{
    namespace Apps
    {
        /// <summary> Class pertaining the info and abilty to register app to the console. </summary>
        public static class AppRegistry
        {
            //must have Revistone assigned here so its first in list and loaded by default
            public static List<App> _appRegistry = new List<App>() {
                new RevistoneApp("Revistone", (ConsoleColor.DarkBlue, ColourFunctions.CyanGradient, 10), (ColourFunctions.CyanDarkBlueGradient.Extend(7, true), 5),
                new (UserInputProfile, Action<string>, string)[] 
                {(new UserInputProfile(UserInputProfile.InputType.FullText, "boop", caseSettings: StringFunctions.CapitalCasing.Lower, removeWhitespace: true),
                (s) => ConsoleAction.SendConsoleMessage(new ConsoleLine("Boop", activeApp.colourScheme.primaryColour)), "Boop!") })
            };
            public static List<App> appRegistry { get { return _appRegistry; } }

            static int _activeAppIndex = 0;
            public static int activeAppIndex { get { return _activeAppIndex; } }

            public static App activeApp { get { return _appRegistry[_activeAppIndex]; } }

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

                _activeAppIndex = index;
                return true;
            }

            /// <summary> Sets active app to app of given name. </summary>
            public static bool SetActiveApp(string name)
            {
                for (int i = 0; i < _appRegistry.Count; i++)
                {
                    if (_appRegistry[i].name.ToLower() == name.ToLower()) return SetActiveApp(i);
                }

                return false;
            }

            /// <summary> Registers apps to console [DO NOT CALL]. </summary>
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
            }
        }
    }
}