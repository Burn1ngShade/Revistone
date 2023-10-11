using System.Runtime.InteropServices;
using Revistone.Apps;
using Revistone.Functions;
using static Revistone.Console.ConsoleLine;

namespace Revistone
{
    namespace Console
    {
        public static class ConsoleDisplay
        {
            //--- DYNAMIC LINES ---
            static List<ConsoleLineDynamicUpdate> dynamicLines = new List<ConsoleLineDynamicUpdate>();

            public static void AddDynamicLine(ConsoleLineDynamicUpdate d)
            {
                dynamicLines.Add(d);
            }

            public static void ForceStopDynamicLines()
            {
                dynamicLines = new List<ConsoleLineDynamicUpdate>();
            }

            //--- CONSOLE LOOPS ---

            public static void HandleConsoleFunctions()
            {
                Manager.Tick += HandleConsoleTickFunctions;

                System.Console.CursorVisible = false;

                consoleCurrentLine = 1;
                debugCurrentLine = bufferSize.height - 8;

                while (true)
                {
                    if (bufferSize.width != System.Console.BufferWidth || bufferSize.height != System.Console.BufferHeight || consoleReload)
                    {
                        bufferSize = (System.Console.BufferWidth, System.Console.BufferHeight);
                        ReloadConsoleDisplay();
                        consoleReload = false;
                    }

                    if (consoleUpdated) continue;

                    if (bufferSize.height <= minBufferSize)
                    {
                        System.Console.SetCursorPosition(0, 0);
                        System.Console.ForegroundColor = ConsoleColor.Red;
                        for (int i = 0; i < bufferSize.height; i++)
                        {
                            System.Console.WriteLine("Console To Small, Must Be Resized!");
                        }
                        System.Console.ForegroundColor = ConsoleColor.White;
                        consoleUpdated = true;
                        continue;
                    }

                    RenderConsole();

                    consoleUpdated = true;
                }
            }

            static void HandleConsoleTickFunctions(int tickNum)
            {
                if (tickNum % App.activeApp.colourSpeed == 0 && consoleLines.Length > 9)
                {
                    consoleLines[0].Update(ColourCreator.ShiftColours(consoleLines[0].lineColour, 1));
                    consoleLines[^8].Update(ColourCreator.ShiftColours(consoleLines[0].lineColour, -1));
                    consoleUpdated = false;
                }

                List<int> r = new List<int>();

                for (int i = 0; i < dynamicLines.Count; i++)
                {
                    if (tickNum % dynamicLines[i].tickMod != 0) continue;

                    if (consoleLines[dynamicLines[i].index].lineText != dynamicLines[i].consoleLine.lineText)
                    {
                        r.Add(i);
                        continue;
                    }

                    switch (dynamicLines[i].updateType)
                    {
                        case ConsoleLineDynamicUpdate.UpdateType.ShiftRight:
                            dynamicLines[i].consoleLine.Update(ColourCreator.ShiftColours(dynamicLines[i].consoleLine.lineColour, 1));
                            consoleLines[dynamicLines[i].index].Update(dynamicLines[i].consoleLine.lineColour);
                            break;
                        case ConsoleLineDynamicUpdate.UpdateType.ShiftLeft:
                            consoleLines[dynamicLines[i].index].Update(ColourCreator.ShiftColours(consoleLines[dynamicLines[i].index].lineColour, -1));
                            break;
                    }

                    consoleUpdated = false;
                }

                for (int i = r.Count - 1; i > 0; i--)
                {
                    dynamicLines.RemoveAt(r[i]);
                }
            }

            //--- CONSOLE DISPLAY METHODS ---

            static void ReloadConsoleDisplay()
            {
                System.Console.Clear();
                consoleUpdated = false;
                consoleCurrentLine = 1;
                debugCurrentLine = System.Console.BufferHeight - 8;

                ConsoleLine[] debug = new ConsoleLine[8];

                consoleLines = new ConsoleLine[System.Console.BufferHeight - 1];
                consoleLinesBuffer = new ConsoleLine[System.Console.BufferHeight - 1];
                for (int i = 0; i < consoleLines.Length; i++)
                {
                    consoleLines[i] = new ConsoleLine();
                    consoleLinesBuffer[i] = new ConsoleLine();
                }
                consoleLines[1] = new ConsoleLine("> ");
                ConsoleAction.UpdateConsoleTitle(App.activeApp.colours);
                ConsoleAction.UpdateConsoleBorder(App.activeApp.colours);
            }

            static void ClearCurrentConsoleLine()
            {
                int currentLineCursor = System.Console.CursorTop;
                System.Console.SetCursorPosition(0, System.Console.CursorTop);
                System.Console.Write(new string(' ', System.Console.WindowWidth));
                System.Console.SetCursorPosition(0, currentLineCursor);
            }

            static void WriteCurrentConsoleLine()
            {
                int cursorTop = System.Console.GetCursorPosition().Top;

                consoleLines[cursorTop].MarkAsUpToDate();

                ConsoleLine c = new ConsoleLine(consoleLines[cursorTop]);
                ConsoleLine bc = new ConsoleLine(consoleLinesBuffer[cursorTop]);

                if (bc.lineText.Length > c.lineText.Length)
                {
                    System.Console.SetCursorPosition(c.lineText.Length, cursorTop);
                    System.Console.Write(new string(' ', System.Console.WindowWidth - c.lineText.Length));
                }

                System.Console.SetCursorPosition(0, System.Console.GetCursorPosition().Top);

                for (int i = 0; i < c.lineText.Length; i++)
                {
                    if (c.lineColour.Length > i) System.Console.ForegroundColor = c.lineColour[i];
                    System.Console.Write(c.lineText[i]);
                }

                System.Console.ForegroundColor = ConsoleColor.White;
            }

            static void RenderConsole()
            {
                for (int i = 0; i < consoleLines.Length; i++)
                {
                    if (consoleLines[i].updated) continue;

                    System.Console.SetCursorPosition(0, i);
                    WriteCurrentConsoleLine();

                    consoleLinesBuffer[i].Update(consoleLines[i]);
                }
            }
        }
    }
}