namespace Revistone
{
    namespace Console
    {
        public class ConsoleLineDynamicUpdate
        {
            public ConsoleLine consoleLine = new ConsoleLine();

            public enum UpdateType { ShiftRight, ShiftLeft }
            public UpdateType updateType;

            public int tickMod;
            public int index;

            public ConsoleLineDynamicUpdate(ConsoleLine consoleLine, UpdateType updateType, int tickMod, int index)
            {
                this.consoleLine.Update(consoleLine);
                this.updateType = updateType;

                this.tickMod = tickMod;
                this.index = index;
            }

            public ConsoleLineDynamicUpdate(ConsoleLine consoleLine, UpdateType updateType, int tickMod)
            {
                this.consoleLine = consoleLine;
                this.updateType = updateType;

                this.tickMod = tickMod;

                this.index = ConsoleAction.GetConsoleLine();
                ConsoleAction.SendConsoleMessage(consoleLine, new ConsoleLineUpdate());
            }
        }
    }
}