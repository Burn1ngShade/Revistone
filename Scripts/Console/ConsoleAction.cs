using static Revistone.Console.Data.ConsoleData;
using Revistone.Apps;
using Revistone.Management;

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

            public static void UpdateLineExceptionStatus(bool status, int lineIndex)
            {
                if (lineIndex < 0 || lineIndex >= consoleLines.Length) return;
                exceptionLines[lineIndex] = status;
            }

            public static bool GetLineExceptionStatus(int lineIndex)
            {
                if (lineIndex < 0 || lineIndex >= consoleLines.Length) return false;
                return exceptionLines[lineIndex];
            }

            /// <summary> Updates ConsoleLine at given index, with given lineInfo </summary>
            public static bool UpdateConsoleLine(ConsoleLine lineInfo, int lineIndex)
            {
                if (lineIndex < 1 || lineIndex >= debugStartIndex) return false;
                consoleLines[lineIndex].Update(lineInfo);
                return true;
            }

            /// <summary> Updates given lines animation settings, via the given ConsoleAnimatedLine. </summary>
            public static bool UpdateLineAnimation(ConsoleAnimatedLine dynamicUpdate, int lineIndex)
            {
                if (lineIndex < 1 || lineIndex >= debugStartIndex) return false;
                consoleLineUpdates[lineIndex].Update(dynamicUpdate);
                return true;
            }

            /// <summary> Sends lineInfo into primary console area. </summary>
            public static void SendConsoleMessage(ConsoleLine lineInfo, ConsoleLineUpdate updateInfo)
            {
                UpdateEnclosedConsole(lineInfo, updateInfo, 2, consoleLines.Length - 9, ref primaryLineIndex);
            }

            /// <summary> Sends lineInfo into primary console area, also updating the animationInfo of the same line. </summary>
            public static void SendConsoleMessage(ConsoleLine lineInfo, ConsoleLineUpdate updateInfo, ConsoleAnimatedLine animationInfo)
            {
                consoleLineUpdates[primaryLineIndex].Update(animationInfo);
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
            public static void SendDebugMessage(ConsoleLine lineInfo, ConsoleLineUpdate updateInfo)
            {
                UpdateEnclosedConsole(lineInfo, updateInfo, bufferSize.height - 7, bufferSize.height - 2, ref debugLineIndex);
            }

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

                if (consoleIndex > consoleBot) //need to move back in console
                {
                    for (int i = consoleTop; i <= consoleBot; i++)
                    {
                        int traceBack = 1;
                        while (true)
                        {
                            if (i - traceBack < consoleTop - 1) break;

                            if (exceptionLines[i - traceBack])
                            {
                                traceBack++;
                                continue;
                            }

                            consoleLines[i - traceBack].Update(consoleLines[i]);
                            consoleLineUpdates[i - traceBack].Update(consoleLineUpdates[i]);

                            break;
                        }
                    }

                    consoleIndex = consoleBot;
                }

                if (updateInfo.append)
                {
                    consoleLines[consoleIndex].Update(consoleLines[consoleIndex].lineText + lineInfo.lineText, consoleLines[consoleIndex].lineColour.Concat(lineInfo.lineColour).ToArray());
                }
                else
                {
                    if (consoleIndex < 0) ConsoleAction.SendDebugMessage(consoleIndex.ToString());
                    consoleLines[consoleIndex].Update(lineInfo);
                }

                if (updateInfo.timeStamp) consoleLines[consoleIndex].Update($"[{DateTime.Now.ToString("HH:mm:ss:fff")}] {consoleLines[consoleIndex].lineText}");

                if (updateInfo.newLine) consoleIndex++;
            }

            /// <summary> Marks all consoleLines in primary console for update, useful for minimizing screen errors. </summary>
            public static void MarkPrimaryConsoleForUpdate()
            {
                for (int i = 1; i < debugStartIndex; i++) { consoleLines[i].MarkForUpdate(); }
            }

            /// <summary> Clears primary console area. </summary>
            public static void ClearPrimaryConsole()
            {
                for (int i = 1; i < debugStartIndex; i++)
                {
                    consoleLineUpdates[i].Update();
                    consoleLines[i].Update("");
                }

                primaryLineIndex = 1;
            }

            /// <summary> Clears debug console area. </summary>
            public static void ClearDebugConsole()
            {
                for (int i = debugStartIndex + 1; i < bufferSize.height - 1; i++)
                {
                    consoleLineUpdates[i].Update();
                    consoleLines[i].Update("");
                }

                debugLineIndex = debugStartIndex + 1;
            }

            // <summary> Clears previous [count] lines. </summary>
            public static void ClearLines(int count = 1, bool updateCurrentLine = false)
            {
                consoleLineUpdates[primaryLineIndex].Update();
                for (int i = primaryLineIndex; i >= primaryLineIndex - count; i--)
                {
                    if (i < 1 || i >= debugStartIndex) continue;
                    consoleLineUpdates[i].Update();
                    consoleLines[i].Update("");
                }

                if (updateCurrentLine) primaryLineIndex -= count;
            }

            /// <summary> Shifts consoleLineIndex via given shift. </summary>
            public static void ShiftLine(int shift = 1)
            {
                primaryLineIndex = Math.Clamp(primaryLineIndex + shift, 1, debugStartIndex);
            }

            /// <summary> Sets consoleLineIndex to given line. </summary>
            public static void GoToLine(int index)
            {
                primaryLineIndex = Math.Clamp(index, 1, debugStartIndex);
            }

            /// <summary> Gets current value of consoleLineIndex. </summary>
            public static int GetConsoleLineIndex()
            {
                return primaryLineIndex;
            }

            /// <summary> Gets current value of debugLineIndex. </summary>
            public static int GetDebugLineIndex()
            {
                return debugLineIndex;
            }
        }
    }
}