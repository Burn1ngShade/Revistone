namespace Revistone
{
    namespace Console
    {
        namespace Data
        {
            /// <summary> Class pertaining all the data of the console (should NOT be used by user). </summary>
            public static class ConsoleData
            {
                //--- Console Lines ---
                public static ConsoleLine[] consoleLines = new ConsoleLine[] {}; //current state of console lines
                public static ConsoleLine[] consoleLinesBuffer = new ConsoleLine[] {}; //last tick state of console lines
                public static ConsoleAnimatedLine[] consoleLineUpdates = new ConsoleAnimatedLine[] {}; //animation data of console lines

                //--- Console Metadata ---
                public static int consoleLineIndex; //index of current console line
                public static int debugLineIndex; //index of current debug line

                public static bool consoleUpdated = false; //console needs to be updated (visually)
                public static bool consoleReload = false; //console needs to be reloaded (data and visual reset)
                public static bool appInitalisation = false; //app needs to be updated by manager

                //--- Console Buffer ---
                public static (int width, int height) bufferSize = (-1, -1);
            }
        }
    }
}