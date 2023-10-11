using Revistone.Apps;
using Revistone.Console;
using Revistone.Functions;

namespace Revistone
{
    namespace Functions
    {
        public static class Manager
    {
        static Thread displayThread = new Thread(ConsoleDisplay.HandleConsoleFunctions);
        static Thread tickThread = new Thread(TickCall);

        public static event TickEventHandler Tick = new TickEventHandler((tickNum) => { });
        public delegate void TickEventHandler(int tickNum);

        static void TickCall()
        {
            int tickNum = 0;

            while (true)
            {
                Tick.Invoke(tickNum);
                Thread.Sleep(25);
                tickNum++;
            }
        }

        public static void Main(string[] args)
        {
            System.Console.CursorVisible = false;

            displayThread.Start();
            tickThread.Start();

            Thread.Sleep(100);

            HandleConsoleBehaviour();
        }

        static void HandleConsoleBehaviour()
        {
            while (true)
            {
                App.activeApp.HandleUserPromt();

                string userInput = ConsoleInteraction.GetUserInput();

                if (userInput != "")
                {
                    ConsoleAction.GoNextLine();
                    App.activeApp.HandleUserInput(userInput);
                }
            }
        }
    }
    }
}