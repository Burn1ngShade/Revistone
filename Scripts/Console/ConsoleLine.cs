using System.Runtime.CompilerServices;
using Revistone.Functions;

namespace Revistone
{
    namespace Console
    {
        /// <summary>
        /// Class pertaining all info for a line in the console.
        /// </summary>
        public class ConsoleLine
        {
            //--- NORMAL ---
            public string lineText { get { return _lineText; } }
            string _lineText;

            public ConsoleColor[] lineColour { get { return _lineColour; } }
            ConsoleColor[] _lineColour;

            public bool updated { get { return _updated; } }
            bool _updated;

            //--- CONSTRUCTORS ---

            /// <summary> Class pertaining all info for a line in the console. </summary>
            public ConsoleLine(string lineText, ConsoleColor[] lineColour)
            {
                this._lineText = lineText;
                this._lineColour = lineColour;
                this._updated = false;
            }

            /// <summary> Class pertaining all info for a line in the console. </summary>
            public ConsoleLine() : this("", new ConsoleColor[] { ConsoleColor.White }) { }
            /// <summary> Class pertaining all info for a line in the console. </summary>
            public ConsoleLine(string lineText) : this(lineText, new ConsoleColor[] { ConsoleColor.White }) { }
            /// <summary> Class pertaining all info for a line in the console. </summary>
            public ConsoleLine(string lineText, ConsoleColor lineColour) : this(lineText, new ConsoleColor[] { lineColour }) { }
            /// <summary> Class pertaining all info for a line in the console. </summary>
            public ConsoleLine(ConsoleLine consoleLine) : this(consoleLine.lineText, consoleLine.lineColour) { }

            //--- METHODS ---

            /// <summary> Updates ConsoleLine and marks line to be updated on console display. </summary>
            public void Update(ConsoleLine line) { Update(line.lineText, line.lineColour); }
            /// <summary> Updates ConsoleLine and marks line to be updated on console display. </summary>
            public void Update(string lineText) { Update(lineText, _lineColour); }
            /// <summary> Updates ConsoleLine and marks line to be updated on console display. </summary>
            public void Update(string lineText, ConsoleColor lineColour) { Update(lineText, new ConsoleColor[1] { lineColour }); }
            /// <summary> Updates ConsoleLine and marks line to be updated on console display. </summary>
            public void Update(ConsoleColor[] lineColour) { Update(lineText, lineColour); }

            /// <summary> Updates ConsoleLine and marks line to be updated on console display. </summary>
            public void Update(string lineText, ConsoleColor[] lineColour)
            {
                lock (Management.Manager.renderLockObject)
                {
                    this._lineText = lineText;
                    this._lineColour = lineColour;
                    this._updated = false;
                }
            }

            /// <summary> Marks ConsoleLine as updated (try to avoid). </summary>
            public void MarkAsUpToDate()
            {
                this._updated = true;
            }

            /// <summary> Marks ConsoleLine as not updated. </summary>
            public void MarkForUpdate()
            {
                this._updated = false;
            }
        }
    }
}