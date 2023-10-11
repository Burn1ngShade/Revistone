using static Revistone.Console.ConsoleLine;
using static Revistone.Console.ConsoleDisplay;
using Revistone.Apps;

namespace Revistone
{
    namespace Console
    {
        public static class ConsoleAction
        {
            public static void ReloadConsole()
            {
                consoleReload = true;
            }

            public static void UpdateConsoleTitle(ConsoleColor[] colours)
            {
                if (consoleLines.Length < minBufferSize) return;
                string title = App.activeApp.name;
                consoleLines[0].Update(new string('-', (System.Console.WindowWidth - title.Length) / 2 - 2) + $" [{title}] " + new string('-', (System.Console.WindowWidth - title.Length) / 2 - 2), colours);
                consoleUpdated = false;
            }

            public static void UpdateConsoleBorder(ConsoleColor[] colours)
            {
                if (consoleLines.Length < minBufferSize) return;
                consoleLines[^8].Update(new string('-', System.Console.WindowWidth - 1), colours);
                consoleUpdated = false;
            }

            public static void SendConsoleMessage(ConsoleLine lineInfo, ConsoleLineUpdate updateInfo)
            {
                UpdateEnclosedConsole(lineInfo, updateInfo, 2, consoleLines.Length - 9, ref consoleCurrentLine);
            }

            public static void SendDebugMessage(ConsoleLine lineInfo, ConsoleLineUpdate updateInfo)
            {
                UpdateEnclosedConsole(lineInfo, updateInfo, bufferSize.height - 7, bufferSize.height - 2, ref debugCurrentLine);
            }

            public static void UpdateConsoleLine(ConsoleLine lineinfo, int lineIndex)
            {
                consoleLines[lineIndex].Update(lineinfo);
                consoleUpdated = false;
            }

            public static void SendConsoleMessage(string text) { SendConsoleMessage(new ConsoleLine(text), new ConsoleLineUpdate()); } //just for ez of type
            public static void SendConsoleMessage(ConsoleLine lineInfo) { SendConsoleMessage(lineInfo, new ConsoleLineUpdate()); } //just for ez of type
            public static void SendDebugMessage(string text) { SendDebugMessage(new ConsoleLine(text), new ConsoleLineUpdate()); } //just for ez of type
            public static void SendDebugMessage(ConsoleLine lineInfo) { SendDebugMessage(lineInfo, new ConsoleLineUpdate()); } //just for ez of type

            public static void UpdateEnclosedConsole(ConsoleLine lineInfo, ConsoleLineUpdate updateInfo, int consoleTop, int consoleBot, ref int consoleIndex)
            {
                if (consoleLines.Length < minBufferSize) return;

                if (consoleIndex > consoleBot)
                {
                    for (int i = consoleTop; i <= consoleBot; i++)
                    {
                        consoleLines[i - 1].Update(consoleLines[i]);
                    }
                    consoleIndex = consoleBot;
                }

                if (updateInfo.append)
                {
                    consoleLines[consoleIndex].Update(consoleLines[consoleIndex].lineText + lineInfo.lineText, consoleLines[consoleIndex].lineColour.Concat(lineInfo.lineColour).ToArray());
                }
                else
                {
                    consoleLines[consoleIndex].Update(lineInfo);
                }

                if (updateInfo.timeStamp) consoleLines[consoleIndex].Update($"[{DateTime.Now.ToString("HH:mm:ss")}] {consoleLines[consoleIndex].lineText}");

                if (updateInfo.newLine) consoleIndex++;

                consoleUpdated = false;
            }

            public static void ClearConsole()
            {
                for (int i = 1; i < consoleLines.Length - 8; i++)
                {
                    consoleLines[i].Update("");
                }

                consoleUpdated = false;

                consoleCurrentLine = 1;
            }

            public static void ClearPreviousLines(int count, bool updateCurrentLine = false)
            {
                for (int i = consoleCurrentLine; i > consoleCurrentLine - count; i--)
                {
                    consoleLines[i].Update("");
                }

                consoleUpdated = false;
                if (updateCurrentLine) consoleCurrentLine -= count;
            }

            public static void GoNextLine(int shift = 1)
            {
                consoleCurrentLine = Math.Clamp(consoleCurrentLine + shift, 1, consoleLines.Length - 9);
            }

            public static void GoToLine(int index)
            {
                consoleCurrentLine = Math.Clamp(index, 1, consoleLines.Length - 9);
            }

            public static int GetConsoleLine()
            {
                return consoleCurrentLine;
            }

        }
    }
}