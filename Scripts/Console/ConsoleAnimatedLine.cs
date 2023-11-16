using Revistone.Functions;

namespace Revistone
{
    namespace Console
    {
        /// <summary>
        /// The configuration for dynamically updating a ConsoleLine.
        /// </summary>
        public class ConsoleAnimatedLine
        {
            public bool enabled;

            public Action<ConsoleLine, ConsoleAnimatedLine, int> update; //called on update
            public object metaInfo;

            public int tickMod; //ticks per update

            // --- CONSTRUCTORS ---

            /// <summary> The configuration for dynamically updating a ConsoleLine, every tickMod ticks. </summary>
            public ConsoleAnimatedLine(Action<ConsoleLine, ConsoleAnimatedLine, int> update, object metaInfo, int tickMod = 5, bool enabled = false)
            {
                this.enabled = enabled;
                this.update = update;
                this.metaInfo = metaInfo;
                this.tickMod = tickMod;
            }

            /// <summary> The configuration for dynamically updating a ConsoleLine, every tickMod ticks. </summary>
            public ConsoleAnimatedLine(Action<ConsoleLine, ConsoleAnimatedLine, int> update, int tickMod = 5, bool enabled = false) : this(update, new object(), tickMod, enabled) { }
            /// <summary> The configuration for dynamically updating a ConsoleLine, every tickMod ticks. </summary>
            public ConsoleAnimatedLine(ConsoleAnimatedLine animatedLine) : this(animatedLine.update, animatedLine.metaInfo, animatedLine.tickMod, animatedLine.enabled) { }
            /// <summary> The configuration for dynamically updating a ConsoleLine, every tickMod ticks. </summary>
            public ConsoleAnimatedLine() : this(None, "", 5, false) { }

            /// <summary> Update configuration for dynamically updating a ConsoleLine, every tickMod ticks. </summary>
            public void Update(Action<ConsoleLine, ConsoleAnimatedLine, int> update, object metaInfo, int tickMod = 5, bool enabled = false)
            {
                lock (Management.Manager.renderLockObject)
                {
                    this.enabled = enabled;
                    this.update = update;
                    this.metaInfo = metaInfo;
                    this.tickMod = tickMod;
                }
            }

            /// <summary> Update configuration for dynamically updating a ConsoleLine, every tickMod ticks. </summary>
            public void Update(ConsoleAnimatedLine dynamicUpdate) { Update(dynamicUpdate.update, dynamicUpdate.metaInfo, dynamicUpdate.tickMod, dynamicUpdate.enabled); }
            public void Update() { Update(None, "", 5, false); }
            // --- PREMADE UPDATE TYPES ---

            /// <summary> Does nothing... </summary>
            public static void None(ConsoleLine lineInfo, ConsoleAnimatedLine animationInfo, int tickNum) { }

            /// <summary> Shift colour by given shift (within animationMetaInfo). </summary>
            public static void ShiftColour(ConsoleLine lineInfo, ConsoleAnimatedLine animationInfo, int tickNum)
            {
                int shift = 1;
                if (animationInfo.metaInfo as int? != null) shift = (int)animationInfo.metaInfo;
                lineInfo.Update(lineInfo.lineColour.Shift(shift));
            }

            /// <summary> Shift letters by given shift (within metaInfo). </summary>
            public static void ShiftLetters(ConsoleLine lineInfo, ConsoleAnimatedLine animationInfo, int tickNum)
            {
                int shift = 1;
                if (animationInfo.metaInfo as int? != null) shift = (int)animationInfo.metaInfo;

                char[] c = new char[lineInfo.lineText.Length];
                shift = shift % lineInfo.lineText.Length;

                for (int i = 0; i < c.Length; i++)
                {
                    int shiftI = i + shift;
                    //this line of code is worse than a normal if statment, oh well... (less lines is better right?!?!?!)
                    shiftI += shiftI >= c.Length ? -c.Length : shiftI < 0 ? c.Length : 0;
                    c[shiftI] = lineInfo.lineText[i];
                }

                lineInfo.Update(new string(c));
            }

            /// <summary> Updates text colours, via switching cyan and dark cyan. </summary>
            public static void ConsoleTheme(ConsoleLine lineInfo, ConsoleAnimatedLine animationInfo, int tickNum)
            {
                lineInfo.Update(ColourFunctions.Replace(lineInfo.lineColour, new (ConsoleColor, ConsoleColor)[] { (ConsoleColor.Cyan, ConsoleColor.DarkCyan), (ConsoleColor.DarkCyan, ConsoleColor.Cyan) }));
            }
        }
    }
}