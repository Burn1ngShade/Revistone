using System.Net.Http.Headers;

namespace Revistone
{
    namespace Console
    {
        public class ConsoleLineUpdate
        {
            public bool newLine;
            public bool append;
            public bool timeStamp;

            public ConsoleLineUpdate(bool newLine = true, bool append = false, bool timeStamp = false)
            {
                this.newLine = newLine;
                this.append = append;
                this.timeStamp = timeStamp;
            }

            public static ConsoleLineUpdate SameLine => new ConsoleLineUpdate(newLine: false);
            public static ConsoleLineUpdate SameLineTimeStamped => new ConsoleLineUpdate(newLine: false, timeStamp: true);
            public static ConsoleLineUpdate TimeStamped => new ConsoleLineUpdate(timeStamp: true);
        }
    }
}