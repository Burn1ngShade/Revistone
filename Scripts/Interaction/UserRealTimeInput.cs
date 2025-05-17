using System.Runtime.InteropServices;
using Revistone.App.BaseApps;
using Revistone.Console.Data;
using Revistone.Management;

using static Revistone.Functions.PersistentDataFunctions;

namespace Revistone.Interaction;

/// <summary> Deal with realtime input, without the use of an external libary </summary>
public static class UserRealtimeInput
{
    static readonly (bool lastState, bool currentState)[] keyInfo = new (bool, bool)[256];
    static (ConsoleKeyInfo key, DateTime time) lastKey = new(new ConsoleKeyInfo(), DateTime.MinValue);

    // --- Get Key Alternative ---

    /// <summary> [DO NOT CALL] Main loop for key registry. </summary>
    internal static void KeyRegistry()
    {
        Manager.Tick += UpdateKeyStates;

        while (true)
        {
            ConsoleKeyInfo c = System.Console.ReadKey(true);
            Analytics.General.KeyPresses++;
            lastKey = (c, DateTime.Now);
        }
    }

    /// <summary> Returns the next key in the input stream. </summary>
    public static (ConsoleKeyInfo keyInfo, bool interrupted) GetKey()
    {
        //was doing something else on console readjustment, custom logic is not really my issue :)
        if (ConsoleData.consoleInterrupt) ConsoleData.consoleInterrupt = false;

        DateTime lkTime = lastKey.time;

        while (true)
        {
            if (lastKey.time != lkTime)
            {
                if (KeyPressed(0x10) && KeyPressed(0x11))
                {
                    lkTime = lastKey.time;
                    continue;
                }
                return (lastKey.key, false);
            }
            if (ConsoleData.consoleInterrupt)
            {
                ConsoleData.consoleInterrupt = false;
                return (new ConsoleKeyInfo(), true);
            }
        }
    }

    // --- Realtime Keys ---

    [DllImport("user32.dll")]
    static extern int GetAsyncKeyState(int key);

    /// <summary> Updates key state of all keys. </summary>
    internal static void UpdateKeyStates(int tickNum)
    {
        for (int i = 0; i < 256; i++)
        {
            keyInfo[i] = (keyInfo[i].currentState, Math.Abs(GetAsyncKeyState(i)) > 1);
        }

        // --- Permanent Hotkeys ---

        if (KeyPressed(0x11) && KeyPressed(0x10) && KeyPressedDown(80)) Profiler.SetEnabled(!Profiler.Enabled); // ctrl + shift + p
        if (KeyPressedDown(123)) ScreenshotsApp.TakePrimaryScreenshot(); // f12
        if (KeyPressedDown(122)) ScreenshotsApp.TakeDebugScreenshot(); // f11
    }

    /// <summary> Returns if key currently pressed down. </summary>
    public static bool KeyPressed(byte key)
    {
        return keyInfo[key].currentState;
    }

    ///<summary> Was the key pressed within the last tick. </summary>
    public static bool KeyPressedDown(byte key)
    {
        if (keyInfo[key].currentState && !keyInfo[key].lastState) return true;
        return false;
    }

    ///<summary> Was the key was released within the last tick. </summary>
    public static bool KeyReleased(byte key)
    {
        if (!keyInfo[key].currentState && keyInfo[key].lastState) return true;
        return false;
    }

    /// <summary> Returns if key currently pressed down. </summary>
    public static bool KeyPressed(ConsoleKey key) { return KeyPressed((byte)key); }

    ///<summary> Was the key pressed within the last tick. </summary>
    public static bool KeyPressedDown(ConsoleKey key) { return KeyPressedDown((byte)key); }

    ///<summary> Was the key was released within the last tick. </summary>
    public static bool KeyReleased(ConsoleKey key) { return KeyReleased((byte)key); }
}
