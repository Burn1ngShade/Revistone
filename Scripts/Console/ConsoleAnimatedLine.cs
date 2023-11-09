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

            public Action<ConsoleLine, string, int> update; //called on update
            public string metaInfo;

            public int tickMod; //ticks per update

            // --- CONSTRUCTORS ---

            /// <summary> The configuration for dynamically updating a ConsoleLine, every tickMod ticks. </summary>
            public ConsoleAnimatedLine(Action<ConsoleLine, string, int> update, string metaInfo = "", int tickMod = 5, bool enabled = false)
            {
                this.enabled = enabled;
                this.update = update;
                this.metaInfo = metaInfo;
                this.tickMod = tickMod;
            }

            /// <summary> The configuration for dynamically updating a ConsoleLine, every tickMod ticks. </summary>
            public ConsoleAnimatedLine(ConsoleAnimatedLine animatedLine) : this(animatedLine.update, animatedLine.metaInfo, animatedLine.tickMod, animatedLine.enabled) { }
            /// <summary> The configuration for dynamically updating a ConsoleLine, every tickMod ticks. </summary>
            public ConsoleAnimatedLine() : this(None, "", 5, false) { }

            /// <summary> Update configuration for dynamically updating a ConsoleLine, every tickMod ticks. </summary>
            public void Update(Action<ConsoleLine, string, int> update, string metaInfo = "", int tickMod = 5, bool enabled = false)
            {
                this.enabled = enabled;
                this.update = update;
                this.metaInfo = metaInfo;
                this.tickMod = tickMod;
            }

            /// <summary> Update configuration for dynamically updating a ConsoleLine, every tickMod ticks. </summary>
            public void Update(ConsoleAnimatedLine dynamicUpdate) { Update(dynamicUpdate.update, dynamicUpdate.metaInfo, dynamicUpdate.tickMod, dynamicUpdate.enabled); }
            public void Update() { Update(None, "", 5, false); }
            // --- PREMADE UPDATE TYPES ---

            /// <summary> Does nothing... </summary>
            public static void None(ConsoleLine lineInfo, string animationMetaInfo, int tickNum) { }

            /// <summary> Shift colour by given shift (within animationMetaInfo). </summary>
            public static void ShiftColour(ConsoleLine lineInfo, string animationMetaInfo, int tickNum)
            {
                int shift = int.TryParse(animationMetaInfo, out shift) ? shift : 1;
                lineInfo.Update(ColourFunctions.ShiftColours(lineInfo.lineColour, shift));
            }

            /// <summary> Shift letters by given shift (within animationMetaInfo). </summary>
            public static void ShiftLetters(ConsoleLine lineInfo, string animationMetaInfo, int tickNum)
            {
                int shift = int.TryParse(animationMetaInfo, out shift) ? shift : 1;

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
            public static void ConsoleTheme(ConsoleLine lineInfo, string animationMetaInfo, int tickNum)
            {
                lineInfo.Update(ColourFunctions.ColourReplace(lineInfo.lineColour, new (ConsoleColor, ConsoleColor)[] { (ConsoleColor.Cyan, ConsoleColor.DarkCyan), (ConsoleColor.DarkCyan, ConsoleColor.Cyan) }));
            }
        }
    }
}