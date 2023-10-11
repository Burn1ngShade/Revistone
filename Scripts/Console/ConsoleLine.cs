using System.Runtime.CompilerServices;

namespace Revistone
{
    namespace Console
    {
        public class ConsoleLine
        {
            //--- STATIC ---
            public static ConsoleLine[] consoleLines = new ConsoleLine[100];
            public static ConsoleLine[] consoleLinesBuffer = new ConsoleLine[100];

            public static int consoleCurrentLine;
            public static int debugCurrentLine;

            public static bool consoleUpdated = false;
            public static bool consoleReload = false;

            public const int minBufferSize = 20;
            public static (int width, int height) bufferSize = (-1, -1);

            //--- NORMAL ---
            public string lineText { get { return _lineText; } }
            public bool updated { get { return _updated; } }

            public ConsoleColor[] lineColour { get { return _lineColour; } }

            string _lineText;
            bool _updated;

            ConsoleColor[] _lineColour;

            //--- CONSTRUCTORS ---

            public ConsoleLine()
            {
                this._lineText = "";
                this._lineColour = new ConsoleColor[1] { ConsoleColor.White };
                this._updated = false;
            }

            public ConsoleLine(string lineText)
            {
                this._lineText = lineText;
                this._lineColour = new ConsoleColor[1] { ConsoleColor.White };
                this._updated = false;
            }

            public ConsoleLine(string lineText, ConsoleColor lineColour)
            {
                this._lineText = lineText;
                this._lineColour = new ConsoleColor[1] { lineColour };
                this._updated = false;
            }

            public ConsoleLine(string lineText, ConsoleColor[] lineColour)
            {
                this._lineText = lineText;
                this._lineColour = lineColour;
                this._updated = false;
            }

            public ConsoleLine(ConsoleLine consoleLine)
            {
                this._lineText = consoleLine.lineText;
                this._lineColour = consoleLine.lineColour == null ? new ConsoleColor[] {ConsoleColor.White} : consoleLine.lineColour;
                this._updated = false;
            }

            //--- METHODS ---

            public void Update(ConsoleLine line) { Update(line.lineText, line.lineColour); }
            public void Update(string lineText) { Update(lineText, _lineColour); }
            public void Update(string lineText, ConsoleColor lineColour) { Update(lineText, new ConsoleColor[1] { lineColour }); }
            public void Update(ConsoleColor[] lineColour) { Update(lineText, lineColour); }

            public void Update(string lineText, ConsoleColor[] lineColour)
            {
                this._lineText = lineText;
                this._lineColour = lineColour;
                this._updated = false;
            }

            public void MarkAsUpToDate()
            {
                this._updated = true;
            }
        }
    }
}