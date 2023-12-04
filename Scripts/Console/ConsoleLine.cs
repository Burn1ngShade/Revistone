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

            public ConsoleColor[] lineColourBG { get { return _lineBGColour; } }
            ConsoleColor[] _lineBGColour;

            public bool updated { get { return _updated; } }
            bool _updated;

            //--- CONSTRUCTORS ---

            /// <summary> Class pertaining all info for a line in the console. </summary>
            public ConsoleLine(string lineText, ConsoleColor[] lineColour, ConsoleColor[] lineColourBG)
            {
                this._lineText = lineText;
                this._lineColour = lineColour;
                this._lineBGColour = lineColourBG;
                this._updated = false;
            }
            /// <summary> Class pertaining all info for a line in the console. </summary>
            public ConsoleLine() : this("", new ConsoleColor[] { ConsoleColor.White }) { }
            /// <summary> Class pertaining all info for a line in the console. </summary>
            public ConsoleLine(string lineText) : this(lineText, new ConsoleColor[] { ConsoleColor.White }) { }
            /// <summary> Class pertaining all info for a line in the console. </summary>
            public ConsoleLine(string lineText, ConsoleColor lineColour) : this(lineText, new ConsoleColor[] { lineColour }) { }
            /// <summary> Class pertaining all info for a line in the console. </summary>
            public ConsoleLine(ConsoleLine consoleLine) : this(consoleLine.lineText, consoleLine.lineColour, consoleLine.lineColourBG) { }
            /// <summary> Class pertaining all info for a line in the console. </summary>
            public ConsoleLine(string lineText, ConsoleColor[] lineColour) : this (lineText, lineColour, new ConsoleColor[] {ConsoleColor.Black}) {}
            /// <summary> Class pertaining all info for a line in the console. </summary>
            public ConsoleLine(string lineText, ConsoleColor lineColour, ConsoleColor lineColourBG) : this(lineText, new ConsoleColor[] { lineColour }, new ConsoleColor[] { lineColourBG }) { }

            //--- METHODS ---

            /// <summary> Updates ConsoleLine and marks line to be updated on console display. </summary>
            public void Update(string lineText, ConsoleColor[] lineColour, ConsoleColor[] lineColourBG)
            {
                lock (Management.Manager.renderLockObject)
                {
                    this._lineText = lineText;
                    this._lineColour = lineColour;
                    this._lineBGColour = lineColourBG;
                    this._updated = false;
                }
            }

            /// <summary> Updates ConsoleLine and marks line to be updated on console display. </summary>
            public void Update(ConsoleLine lineInfo) { Update(lineInfo.lineText, lineInfo.lineColour, lineInfo.lineColourBG); }
            /// <summary> Updates ConsoleLine and marks line to be updated on console display. </summary>
            public void Update(string lineText) { Update(lineText, _lineColour); }
            /// <summary> Updates ConsoleLine and marks line to be updated on console display. </summary>
            public void Update(string lineText, ConsoleColor lineColour) { Update(lineText, new ConsoleColor[1] { lineColour }); }
            /// <summary> Updates ConsoleLine and marks line to be updated on console display. </summary>
            public void Update(ConsoleColor[] lineColour) { Update(lineText, lineColour); }
            /// <summary> Updates ConsoleLine and marks line to be updated on console display. </summary>
            public void Update(string lineText, ConsoleColor[] lineColour) { Update(lineText, lineColour, new ConsoleColor[] {ConsoleColor.Black}); }
            /// <summary> Updates ConsoleLine and marks line to be updated on console display. </summary>
            public void Update(string lineText, ConsoleColor lineColour, ConsoleColor lineColourBG) { Update(lineText, new ConsoleColor[1] { lineColour }, new ConsoleColor[1] { lineColourBG }); }
            /// <summary> Updates ConsoleLine and marks line to be updated on console display. </summary>
            public void Update(ConsoleColor[] lineColour, ConsoleColor[] lineColourBG) { Update(lineText, lineColour, lineColourBG); }
            /// <summary> Updates ConsoleLine and marks line to be updated on console display. </summary>
            public void Update(ConsoleColor lineColour, ConsoleColor lineColourBG) { Update(lineText, new ConsoleColor[1] { lineColour }, new ConsoleColor[1] { lineColourBG }); }

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

            //--- STATIC METHODS ---

            /// <summary> Inserts a ConsoleLine into another, overwritting overlapping chars and colours. </summary>
            public static ConsoleLine Overwrite(ConsoleLine baseLine, ConsoleLine overwriteLine, int overwriteIndex)
            {
                string s = baseLine.lineText;
                s += new string(' ', Math.Clamp(overwriteIndex + overwriteLine.lineText.Length - s.Length, 0, int.MaxValue));
                s = s.ReplaceAt(overwriteIndex, overwriteLine.lineText.Length, overwriteLine.lineText);

                ConsoleColor[] cl = baseLine.lineColour.Extend(ConsoleColor.White, s.Length);
                for (int i = overwriteIndex; i < overwriteIndex + overwriteLine.lineText.Length; i++) cl[i] = overwriteLine.lineColour[i - overwriteIndex];

                return new ConsoleLine(s, cl);
            }
        }
    }
}