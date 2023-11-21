using System.Data;
using System.Runtime.InteropServices;
using Revistone.Console;

namespace Revistone
{
    namespace Interaction
    {
        /// <summary> Deal with realtime input, without the use of an external libary </summary>
        public static class UserRealtimeInput
        {
            static (ConsoleKeyInfo key, DateTime time) lastKey = new(new ConsoleKeyInfo(), DateTime.MinValue);

            /// <summary> [DO NOT CALL] Main loop for key registry. </summary>
            internal static void KeyRegistry()
            {
                while (true)
                {
                    ConsoleKeyInfo c = System.Console.ReadKey(true);
                    lastKey = (c, DateTime.Now);
                }
            }

            /// <summary> Returns the next key in the input stream. </summary>
            public static ConsoleKeyInfo GetKey()
            {
                DateTime lkTime = lastKey.time;

                while (true)
                {
                    if (lastKey.time != lkTime)
                    {
                        return lastKey.key;
                    }
                }
            }

            [DllImport("user32.dll")]
            static extern int GetAsyncKeyState(int key);

            /// <summary> Returns if key currently pressed down. </summary>
            public static bool KeyPressed(int key)
            {
                return GetAsyncKeyState((int)key) >= 1 ? true : false;
            }

            /// <summary> Returns if key currently pressed down. </summary>
            public static bool KeyPressed(ConsoleKey key) { return KeyPressed((int)key); }
        }
    }
}