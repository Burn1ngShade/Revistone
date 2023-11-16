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
            }

            /// <summary> Sends lineInfo into primary console area, also updating the animationInfo of the same line. </summary>
            public static void SendConsoleMessage(ConsoleLine lineInfo, ConsoleLineUpdate updateInfo, ConsoleAnimatedLine animationInfo)
            {
                consoleLineUpdates[consoleLineIndex].Update(animationInfo);
                SendConsoleMessage(lineInfo, updateInfo);
            }
            /// <summary> Sends lineInfo into primary console area, also updating the animationInfo of the same line. </summary>
            public static void SendConsoleMessage(ConsoleLine lineInfo, ConsoleAnimatedLine animationInfo) { SendConsoleMessage(lineInfo, new ConsoleLineUpdate(), animationInfo); }
            /// <summary> Sends lineInfo into primary console area. </summary>
            public static void SendConsoleMessage(string text) { SendConsoleMessage(new ConsoleLine(text), new ConsoleLineUpdate()); } //just for ez of type
            /// <summary> Sends lineInfo into primary console area. </summary>
            public static void SendConsoleMessage(ConsoleLine lineInfo) { SendConsoleMessage(lineInfo, new ConsoleLineUpdate()); } //just for ez of type
            /// <summary> Sends lineInfo into primary console area. </summary>
            public static void SendConsoleMessage(string text, ConsoleLineUpdate updateIfno) { SendConsoleMessage(new ConsoleLine(text), updateIfno); } //just for ez of type


            /// <summary> Sends ConsoleLine into debug console area. </summary>
            public static void SendDebugMessage(ConsoleLine lineInfo, ConsoleLineUpdate updateInfo, ConsoleAnimatedLine animationInfo)
            {
                consoleLineUpdates[debugLineIndex].Update(animationInfo);
                SendDebugMessage(lineInfo, updateInfo);
            }
            /// <summary> Sends ConsoleLine into debug console area. </summary>
            public static void SendDebugMessage(ConsoleLine lineInfo, ConsoleAnimatedLine animationInfo) { SendDebugMessage(lineInfo, new ConsoleLineUpdate(), animationInfo); }
            /// <summary> Sends ConsoleLine into debug console area. </summary>
            public static void SendDebugMessage(string text) { SendDebugMessage(new ConsoleLine(text), new ConsoleLineUpdate()); } //just for ez of type
            /// <summary> Sends ConsoleLine into debug console area. </summary>
            public static void SendDebugMessage(ConsoleLine lineInfo) { SendDebugMessage(lineInfo, new ConsoleLineUpdate()); } //just for ez of type
            /// <summary> Sends ConsoleLine into debug console area. </summary>
            public static void SendDebugMessage(string text, ConsoleLineUpdate updateIfno) { SendDebugMessage(new ConsoleLine(text), updateIfno); } //just for ez of type

            /// <summary> Updates lineInfo at given index, adjusting position of ConsoleLines within console if needed.  </summary>
            public static void UpdateEnclosedConsole(ConsoleLine lineInfo, ConsoleLineUpdate updateInfo, int consoleTop, int consoleBot, ref int consoleIndex)
            {
                if (consoleLines.Length < AppRegistry.activeApp.minHeightBuffer) return;

                if (consoleIndex > consoleBot)
                {
                    for (int i = consoleTop; i <= consoleBot; i++)
                    {
                        consoleLines[i - 1].Update(consoleLines[i]);
                        consoleLineUpdates[i - 1].Update(consoleLineUpdates[i]);
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
            }

            /// <summary> Marks all consoleLines in primary console for update, useful for minimizing screen errors. </summary>
            public static void MarkPrimaryConsoleForUpdate()
            {
                for (int i = 1; i < consoleLines.Length - 8; i++) {consoleLines[i].MarkForUpdate();}
            }

            /// <summary> Clears primary console area. </summary>
            public static void ClearPrimaryConsole()
            {
                for (int i = 1; i < debugStartIndex; i++)
                {
                    consoleLineUpdates[i].Update();
                    consoleLines[i].Update("");
                }

                consoleLineIndex = 1;
            }

            /// <summary> Clears debug console area. </summary>
            public static void ClearDebugConsole()
            {
                for (int i = debugStartIndex; i < bufferSize.height - 1; i++)
                {
                    consoleLines[i].Update("");
                    consoleLineUpdates[i].Update();
                }

                debugLineIndex = debugStartIndex;
            }

            // <summary> Clears previous [count] lines. </summary>
            public static void ClearLines(int count = 1, bool updateCurrentLine = false)
            {
                for (int i = consoleLineIndex; i > consoleLineIndex - count; i--)
                {
                    consoleLines[i].Update("");
                }

                if (updateCurrentLine) consoleLineIndex -= count;
            }

            /// <summary> Shifts consoleLineIndex via given shift. </summary>
            public static void ShiftLine(int shift = 1)
            {
                consoleLineIndex = Math.Clamp(consoleLineIndex + shift, 1, debugStartIndex);
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

            /// <summary> Gets current value of debugLineIndex. </summary>
            public static int GetDebugLineIndex()
            {
                return debugLineIndex;
            }
        }
    }
}