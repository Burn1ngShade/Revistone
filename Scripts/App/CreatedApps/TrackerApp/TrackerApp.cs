using Revistone.Functions;
using Revistone.Interaction;
using Revistone.Console;
using Revistone.Apps.Tracker;

using static Revistone.Console.ConsoleAction;
using static Revistone.Functions.ColourFunctions;
using static Revistone.Apps.Tracker.TrackerData;

namespace Revistone.Apps;

public class TrackerApp : App
{
    // --- APP BOILER ---

    public TrackerApp() : base() { }
    public TrackerApp(string name, (ConsoleColor[] primaryColour, ConsoleColor[] secondaryColour, ConsoleColor[] tertiaryColour, int speed) consoleSettings, (ConsoleColor[] colours, int speed) borderSettings, (UserInputProfile format, Action<string> payload, string summary)[] appCommands, int minAppWidth = 30, int minAppHeight = 30, bool baseCommands = true) : base(name, consoleSettings, borderSettings, appCommands, minAppWidth, minAppHeight, baseCommands) { }

    public override App[] OnRegister()
    {
        return [new TrackerApp("Tracker", (ConsoleColor.DarkBlue.ToArray(), ConsoleColor.Cyan.ToArray(), ConsoleColor.DarkCyan.ToArray(), 10), (CyanDarkBlueGradient.Extend(7, true), 5), [], 70, 50)];
    }

    public override void ExitApp()
    {
        DATA.Save();
        base.ExitApp();
    }

    public override void OnAppInitalisation()
    {
        if (AppPersistentData.FileExists("Tracker/Data")) DATA = new TrackerData(AppPersistentData.LoadFile("Tracker/Data"));
        else {
            AppPersistentData.CreateFile("Tracker/Data"); 
            DATA = new TrackerData();
        }

        for (int i = 0; i <= 10; i++) { UpdateLineExceptionStatus(true, i); }

        base.OnAppInitalisation();
        MainMenu();
    }

    // --- MAIN MENUS ---

    void MainMenu()
    {
        ShiftLine();
        ConsoleLine[] title = TitleFunctions.CreateTitle("FOCUS", AdvancedHighlight(97, ConsoleColor.DarkBlue.ToArray(), (ConsoleColor.Cyan.ToArray(), 0, 10), (ConsoleColor.Cyan.ToArray(), 48, 10)), TitleFunctions.AsciiFont.BigMoneyNW, letterSpacing: 1, bottomSpace: 1);
        SendConsoleMessages(title, Enumerable.Repeat(new ConsoleAnimatedLine(ConsoleAnimatedLine.ShiftColour, "", AppRegistry.activeApp.borderColourScheme.speed, true), title.Length).ToArray());

        DATA.GetDayData(DATA.today).Display();

        int pointer = 0;
        while (true)
        {
            pointer = UserInput.CreateOptionMenu("--- Options ---", [
                (new ConsoleLine("Log", ConsoleColor.Cyan), Log),
                (new ConsoleLine("Stats", ConsoleColor.Cyan), StatsMenu),
                (new ConsoleLine("Settings", ConsoleColor.Cyan), TrackerSettingMenus.SettingsMenu),
                (new ConsoleLine("Next Day", ConsoleColor.DarkBlue), () => DATA.ModifySelectedDate(DATA.selectedDate.AddDays(1))),
                (new ConsoleLine("Last Day", ConsoleColor.DarkBlue), () => DATA.ModifySelectedDate(DATA.selectedDate.AddDays(-1))),
                (new ConsoleLine("Exit", ConsoleColor.DarkBlue), ExitApp)], cursorStartIndex: pointer);

            if (pointer == 5) return;
        }
    }

    void Log()
    {
        ClearPrimaryConsole();

        int pointer = 0;
        TrackerDayData day = DATA.GetDayData(DATA.selectedDate);
        while (true)
        {
            day.DisplayAsMenu(ref pointer);

            if (pointer == day.stats.Count)
            {
                day.Display();
                return;
            }

            if (day.stats[pointer] is TrackerInputStat inputStat)
            {
                inputStat.value = UserInput.GetValidUserInput($"--- Enter Data For [{inputStat.statName}] ---", new UserInputProfile(inputStat.inputTypes, bannedChars: "-"), inputStat.value == "Not Tracked" ? "" : inputStat.value, 5);
            }

            else if (day.stats[pointer] is TrackerDropdownStat dropdownStat)
            {
                dropdownStat.value = dropdownStat.options[UserInput.CreateOptionMenu($"--- Enter Data For [{dropdownStat.statName}] ---",
                dropdownStat.options.Select(x => x.option).ToArray())].option;
            }
        }

    }

    // --- STATS MENUS ---

    void StatsMenu()
    {
        int pointer = 0;
        while (true)
        {
            ClearPrimaryConsole();

            TrackerDayData day = DATA.GetDayData(DATA.selectedDate);
            day.DisplayAsMenu(ref pointer);

            if (pointer == day.stats.Count)
            {
                day.Display();
                return;
            }

            List<(string name, int startDate, int length)> timePeriodData = [
                ("All Time", 0, DATA.GetDayIndex(DATA.today) + 1),
                ("Last 30 Days", Math.Max(0, DATA.GetDayIndex(DATA.today) - 30), Math.Min(30, DATA.GetDayIndex(DATA.today)) + 1),
                ("Last 7 Days", Math.Max(0, DATA.GetDayIndex(DATA.today) - 7), Math.Min(7, DATA.GetDayIndex(DATA.today)) + 1),
            ];
            timePeriodData.AddRange(AppPersistentData.LoadFile("Tracker/Arc Data", 3, true).Select(x => (x[0], DATA.GetDayIndex(System.DateOnly.Parse(x[1])), DATA.GetDayIndex(System.DateOnly.Parse(x[2])))));
            int timePeriod = 0;
            while (true)
            {
                ClearPrimaryConsole();

                TrackerStatProfile? info = TrackerStatProfile.GenerateStat(day.stats[pointer]);

                if (info == null)
                {
                    day.Display();
                    return;
                }

                info.Display(timePeriodData[timePeriod].name, timePeriodData[timePeriod].startDate, timePeriodData[timePeriod].length);

                ShiftLine();
                timePeriod = UserInput.CreateMultiPageOptionMenu("Time Period",
                timePeriodData.Select((x, i) => new ConsoleLine(x.name, i == timePeriod ? ConsoleColor.Yellow : ConsoleColor.Cyan)).ToArray(), [new ConsoleLine("Exit", ConsoleColor.DarkBlue)], 5, timePeriod);
                if (timePeriod == -1) break;
            }
        }
    }
}