using static Revistone.Console.Data.ConsoleData;
using Revistone.Apps;

namespace Revistone
{
    namespace Console
    {
        /// <summary> Handles all functions for apps to interact with the console. </summary>
        public static class ConsoleAction
        {
            /// <summary> Marks the console for reset on next tick. </summary>
            public static void ResetConsole()
            {
                consoleReload = true;
            }

            /// <summary> Updates given lines animation settings, via the given ConsoleAnimatedLine. </summary>
            public static void UpdateLineAnimation(ConsoleAnimatedLine dynamicUpdate, int lineIndex)
            {
                consoleLineUpdates[lineIndex].Update(dynamicUpdate);
            }

            /// <summary> Sends lineInfo into primary console area. </summary>
            public static void SendConsoleMessage(ConsoleLine lineInfo, ConsoleLineUpdate updateInfo)
            {
                UpdateEnclosedConsole(lineInfo, updateInfo, 2, consoleLines.Length - 9, ref consoleLineIndex);
            }

            /// <summary> Sends ConsoleLine into debug console area. </summary>
            public static void SendDebugMessage(ConsoleLine lineInfo, ConsoleLineUpdate updateInfo)
            {
                UpdateEnclosedConsole(lineInfo, updateInfo, bufferSize.height - 7, bufferSize.height - 2, ref debugLineIndex);
            }

            /// <summary> Updates ConsoleLine at given index, with given lineInfo </summary>
            public static void UpdateConsoleLine(ConsoleLine lineInfo, int lineIndex)
            {
                consoleLines[lineIndex].Update(lineInfo);
                consoleUpdated = false;
            }

            /// <summary> Sends lineInfo into primary console area. </summary>
            public static void SendConsoleMessage(string text) { SendConsoleMessage(new ConsoleLine(text), new ConsoleLineUpdate()); } //just for ez of type
            /// <summary> Sends lineInfo into primary console area. </summary>
            public static void SendConsoleMessage(ConsoleLine lineInfo) { SendConsoleMessage(lineInfo, new ConsoleLineUpdate()); } //just for ez of type
            /// <summary> Updates ConsoleLine at given index, with given lineInfo </summary>
            public static void SendDebugMessage(string text) { SendDebugMessage(new ConsoleLine(text), new ConsoleLineUpdate()); } //just for ez of type
            /// <summary> Updates ConsoleLine at given index, with given lineInfo </summary>
            public static void SendDebugMessage(ConsoleLine lineInfo) { SendDebugMessage(lineInfo, new ConsoleLineUpdate()); } //just for ez of type

            /// <summary> Updates lineInfo at given index, adjusting position of ConsoleLines within console if needed.  </summary>
            public static void UpdateEnclosedConsole(ConsoleLine lineInfo, ConsoleLineUpdate updateInfo, int consoleTop, int consoleBot, ref int consoleIndex)
            {
                if (consoleLines.Length < App.activeApp.minHeightBuffer) return;

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

            /// <summary> Clears primary console area. </summary>
            public static void ClearPrimaryConsole()
            {
                for (int i = 1; i < consoleLines.Length - 8; i++)
                {
                    consoleLines[i].Update("");
                    consoleLineUpdates[i].Update();
                }

                consoleUpdated = false;

                consoleLineIndex = 1;
            }

            /// <summary> Clears debug console area. </summary>
            public static void ClearDebugConsole()
            {
                for (int i = bufferSize.height - 8; i < bufferSize.height - 2; i++)
                {
                    consoleLines[i].Update("");
                    consoleLineUpdates[i].Update();
                }

                consoleUpdated = false;

                debugLineIndex = bufferSize.height - 8;
            }

            // <summary> Clears previous [count] lines. </summary>
            public static void ClearLines(int count = 1, bool updateCurrentLine = false)
            {
                for (int i = consoleLineIndex; i > consoleLineIndex - count; i--)
                {
                    consoleLines[i].Update("");
                }

                consoleUpdated = false;
                if (updateCurrentLine) consoleLineIndex -= count;
            }

            /// <summary> Shifts consoleLineIndex via given shift. </summary>
            public static void ShiftLine(int shift = 1)
            {
                consoleLineIndex = Math.Clamp(consoleLineIndex + shift, 1, consoleLines.Length - 8);
            }

            /// <summary> Sets consoleLineIndex to given line. </summary>
            public static void GoToLine(int index)
            {
                consoleLineIndex = Math.Clamp(index, 1, consoleLines.Length - 9);
            }

            /// <summary> Gets current value of consoleLineIndex. </summary>
            public static int GetConsoleLineIndex()
            {
                return consoleLineIndex;
            }
        }
    }
}