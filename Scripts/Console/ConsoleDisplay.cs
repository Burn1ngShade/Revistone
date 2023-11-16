using System.Reflection;
using System.Reflection.Emit;
using Revistone.Apps;
using Revistone.Functions;
using Revistone.Management;
using static Revistone.Console.Data.ConsoleData;

namespace Revistone
{
    namespace Console
    {
        public static class ConsoleDisplay
        {
            //--- CONSOLE LOOPS ---

            /// <summary> [DO NOT CALL] Initializes ConsoleDisplay. </summary>
            internal static void InitializeConsoleDisplay()
            {
                System.Console.CursorVisible = false;
                consoleLineIndex = 1;
                debugLineIndex = debugBufferStartIndex;
                consoleReload = true;

                Management.Manager.Tick += HandleConsoleDisplayBehaviour;
            }

            /// <summary> Main loop for ConsoleDisplay, handles dynamic lines and rendering console. </summary>
            static void HandleConsoleDisplayBehaviour(int tickNum)
            {
                //if console display resized
                if (bufferSize.width != System.Console.BufferWidth || bufferSize.height != System.Console.BufferHeight || consoleReload)
                {
                    bufferSize = (System.Console.BufferWidth, System.Console.BufferHeight);
                    if (!consoleReload) SoftReloadConsoleDisplay();
                    else if (!(bufferSize.height <= AppRegistry.activeApp.minHeightBuffer || bufferSize.width <= AppRegistry.activeApp.minWidthBuffer))
                    {
                        ResetConsoleDisplay();
                        consoleReload = false;
                    }
                }

                // --- Render Console ---

                if (bufferSize.height <= AppRegistry.activeApp.minHeightBuffer || bufferSize.width <= AppRegistry.activeApp.minWidthBuffer)
                {
                    if (screenWarningUpdated || bufferSize.height == 0 || bufferSize.width == 0) return;

                    System.Console.Clear();
                    System.Console.SetCursorPosition(0, 0);
                    System.Console.ForegroundColor = ConsoleColor.Red;
                    for (int i = 0; i < bufferSize.height; i++)
                    {
                        System.Console.WriteLine($"{(bufferSize.height <= AppRegistry.activeApp.minHeightBuffer ? "[Console Height Is To Small] " : "")}{(bufferSize.width <= AppRegistry.activeApp.minWidthBuffer ? "[Console Width Is To Small] " : "")}");
                    }
                    System.Console.ForegroundColor = ConsoleColor.White;

                    screenWarningUpdated = true;
                }
                else
                {
                    UpdateAnimatedLines(tickNum);
                    RenderConsole(tickNum);
                }
            }

            //--- ANIMATED LINES METHOD ---

            /// <summary> Updates all lines marked as animated. </summary>
            static void UpdateAnimatedLines(int tickNum)
            {
                for (int i = 0; i < consoleLines.Length; i++)
                {
                    if (!consoleLineUpdates[i].enabled || tickNum % consoleLineUpdates[i].tickMod != 0) continue; //not dynamic or not right tick
                    consoleLineUpdates[i].update.Invoke(consoleLines[i], consoleLineUpdates[i], tickNum);
                }

            }

            //--- CONSOLE DISPLAY METHODS ---

            /// <summary> Resets console display to default. </summary>
            static void ResetConsoleDisplay()
            {
                screenWarningUpdated = false;

                System.Console.Clear();
                consoleLineIndex = 1;
                debugLineIndex = debugBufferStartIndex;

                consoleLines = new ConsoleLine[System.Console.BufferHeight - 1];
                consoleLinesBuffer = new ConsoleLine[System.Console.BufferHeight - 1];
                consoleLineUpdates = new ConsoleAnimatedLine[System.Console.BufferHeight - 1];
                for (int i = 0; i < consoleLines.Length; i++)
                {
                    consoleLines[i] = new ConsoleLine();
                    consoleLinesBuffer[i] = new ConsoleLine();
                    consoleLineUpdates[i] = new ConsoleAnimatedLine();
                }

                UpdateConsoleTitle();
                UpdateConsoleBorder();

                appInitalisation = true;
            }

            /// <summary> Reloads console display updating buffer size, while maintaing screen. </summary>
            static void SoftReloadConsoleDisplay()
            {
                screenWarningUpdated = false;

                if (bufferSize.height <= AppRegistry.activeApp.minHeightBuffer) return;

                System.Console.Clear();

                int debugDistanceFromEnd = consoleLines.Length - debugLineIndex;

                (ConsoleLine lineInfo, ConsoleAnimatedLine animationInfo)[] enclosedConsoleLinesDC = new (ConsoleLine, ConsoleAnimatedLine)[debugStartIndex - 1];
                (ConsoleLine lineInfo, ConsoleAnimatedLine animationInfo)[] metaConsoleLinesDC = new (ConsoleLine, ConsoleAnimatedLine)[8];

                for (int i = 1; i < consoleLines.Length; i++)
                {
                    if (i <= debugStartIndex - 1) enclosedConsoleLinesDC[i - 1] = (new ConsoleLine(consoleLines[i]), new ConsoleAnimatedLine(consoleLineUpdates[i]));
                    else
                    {
                        metaConsoleLinesDC[i - debugStartIndex] = (new ConsoleLine(consoleLines[i]), new ConsoleAnimatedLine(consoleLineUpdates[i]));
                    }
                }

                Array.Resize(ref consoleLines, bufferSize.height - 1);
                Array.Resize(ref consoleLinesBuffer, bufferSize.height - 1);
                Array.Resize(ref consoleLineUpdates, bufferSize.height - 1);

                for (int i = 1; i < consoleLines.Length; i++)
                {
                    consoleLines[i] = new ConsoleLine();
                    consoleLinesBuffer[i] = new ConsoleLine();
                    consoleLineUpdates[i] = new ConsoleAnimatedLine();
                }

                for (int i = 0; i < metaConsoleLinesDC.Length; i++)
                {
                    consoleLines[i + debugStartIndex].Update(metaConsoleLinesDC[i].lineInfo);
                    consoleLineUpdates[i + debugStartIndex].Update(metaConsoleLinesDC[i].animationInfo);
                }

                for (int i = 1; i < Math.Min(enclosedConsoleLinesDC.Length, debugStartIndex); i++)
                {
                    consoleLines[i].Update(enclosedConsoleLinesDC[i - 1].lineInfo);
                    consoleLineUpdates[i].Update(enclosedConsoleLinesDC[i - 1].animationInfo);
                }

                consoleLineIndex = Math.Clamp(consoleLineIndex, 1, debugStartIndex - 1);
                debugLineIndex = consoleLines.Length - debugDistanceFromEnd;

                UpdateConsoleTitle();
                UpdateConsoleBorder();
            }

            /// <summary> Updates console title, with current app name and colour scheme. </summary>
            static void UpdateConsoleTitle()
            {
                if (consoleLines.Length < AppRegistry.activeApp.minHeightBuffer) return;
                consoleLinesBuffer[0].Update(""); //stops buffer width
                string title = AppRegistry.activeApp.name;
                consoleLines[0].Update(new string('-', (bufferSize.width - title.Length) / 2 - 2) + $" [{title}] " + new string('-', (bufferSize.width - title.Length) / 2 - 2), ColourFunctions.Alternate(AppRegistry.activeApp.borderColours, bufferSize.width - 1, 1));
                ConsoleAction.UpdateLineAnimation(new ConsoleAnimatedLine(ConsoleAnimatedLine.ShiftColour, "", AppRegistry.activeApp.borderColourSpeed, true), 0);
            }

            /// <summary> Updates console border, with current app colour scheme. </summary>
            static void UpdateConsoleBorder()
            {
                if (consoleLines.Length < AppRegistry.activeApp.minHeightBuffer) return;
                consoleLinesBuffer[^8].Update(""); //stops buffer width
                consoleLines[^8].Update(new string('-', bufferSize.width - 1), ColourFunctions.Alternate(AppRegistry.activeApp.borderColours, bufferSize.width - 1, 1));
                ConsoleAction.UpdateLineAnimation(new ConsoleAnimatedLine(ConsoleAnimatedLine.ShiftColour, "", AppRegistry.activeApp.borderColourSpeed, true), debugStartIndex);
            }

            /// <summary> Writes given line to screen, using value of consoleLines. </summary>
            static void WriteConsoleLine(int lineIndex)
            {
                System.Console.SetCursorPosition(0, lineIndex);

                int cursorTop = System.Console.GetCursorPosition().Top;
                consoleLines[cursorTop].MarkAsUpToDate();

                ConsoleLine c = new ConsoleLine(consoleLines[cursorTop]); //copy of current console line
                ConsoleLine bc = new ConsoleLine(consoleLinesBuffer[cursorTop]); //copy of buffer console line

                if (bc.lineText.Length > c.lineText.Length) //clears line between end of currentline and buffer line
                {
                    System.Console.SetCursorPosition(c.lineText.Length, cursorTop);
                    System.Console.Write(new string(' ', System.Console.WindowWidth - c.lineText.Length));
                    System.Console.SetCursorPosition(0, System.Console.GetCursorPosition().Top);
                }

                for (int i = 0; i < c.lineText.Length; i++)
                {
                    if (c.lineColour.Length > i) System.Console.ForegroundColor = c.lineColour[i];
                    System.Console.Write(c.lineText[i]);
                }

                System.Console.ForegroundColor = ConsoleColor.White;
            }

            /// <summary> Updates the console display, based on current states of consoleLines, before updating consoleLinesBuffer. </summary>
            static void RenderConsole(int tickNum)
            {
                lock (Manager.renderLockObject)
                {
                    for (int i = 0; i < consoleLines.Length; i++)
                    {
                        if (consoleLines[i].updated || System.Console.BufferHeight <= i) continue;

                        WriteConsoleLine(i);
                        consoleLinesBuffer[i].Update(consoleLines[i]);
                    }
                }

            }
        }
    }
}
