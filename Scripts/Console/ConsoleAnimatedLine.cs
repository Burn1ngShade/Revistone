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

            public enum UpdateType { ShiftRight, ShiftLeft }
            public UpdateType updateType;

            public int tickMod; //ticks per update

            /// <summary>
            /// The configuration for dynamically updating a ConsoleLine, every tickMod ticks.
            /// </summary>
            public ConsoleAnimatedLine(bool enabled = false, UpdateType updateType = UpdateType.ShiftRight, int tickMod = 5)
            {
                this.enabled = enabled;
                this.updateType = updateType;
                this.tickMod = tickMod;
            }

            /// <summary>
            /// Update configuration for dynamically updating a ConsoleLine, every tickMod ticks.
            /// </summary>
            public void Update(ConsoleAnimatedLine dynamicUpdate) { Update(dynamicUpdate.enabled, dynamicUpdate.updateType, dynamicUpdate.tickMod); }

            public void Update(bool enabled = false, UpdateType updateType = UpdateType.ShiftRight, int tickMod = 5)
            {
                this.enabled = enabled;
                this.updateType = updateType;
                this.tickMod = tickMod;
            }
        }
    }
}